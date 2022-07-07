using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AI {
    [RequireComponent(typeof(Unit))]
    public class Vision : MonoBehaviour {
        public VisionSettings settings;
        public SituationRatingSettings sitSettings;
        Unit unit;
        WeaponHandler weap;

        public Dictionary<GameObject, UnitInfo> EnemiesInfoPermanent { get; private set; }
        public Dictionary<GameObject, UnitInfo> AlliesInfoPermanent { get; private set; }
        public List<UnitInfo> EnemiesInfo { get; private set; }
        public List<UnitInfo> AlliesInfo { get; private set; }
        public bool EnemySpotted {
            get {
                return EnemiesInfo.Count != 0;
            }
        }
        public Action<bool> OnEnemyCheck;
        public float LastEnemyTimestamp { get; private set; }

        public float SituationRating { get; private set; }
        public float DiffHealthRating { get; private set; }
        public float OwnHealthRating { get; private set; }
        public float CooldownRating { get; private set; }

        /*
         * Functions
         */

        void Awake() {
            if (settings == null) {
                Debug.LogError("Unit is missing VisionSettings!");
                Debug.Break();
            }

            unit = GetComponent<Unit>();
            weap = GetComponent<WeaponHandler>();

            AlliesInfoPermanent = new Dictionary<GameObject, UnitInfo>();
            EnemiesInfoPermanent = new Dictionary<GameObject, UnitInfo>();
            AlliesInfo = new List<UnitInfo>();
            EnemiesInfo = new List<UnitInfo>();
        }

        void Start() {
            StartCoroutine(UpdateVision());
        }

        IEnumerator UpdateVision() {
            while (true) {
                yield return new WaitForSeconds(settings.updateRate);

                AlliesInfo.Clear();
                EnemiesInfo.Clear();

                float range;
                if (settings.ignoreWalls) {
                    range = 100f;
                } else {
                    range = settings.sightRange;
                }
                // Within range?
                int hittableMask = 1 << LayerMask.NameToLayer("Hittable");
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, hittableMask);

                for (int i = 0; i < colliders.Length; i++) {
                    IHittable hittable = colliders[i].GetComponent<IHittable>();
                    if (hittable == null) {
                            Debug.LogError("Object on Hittable layer does not have an IHittable component!");
                        continue;
                    }

                    Factions.Relationship rl = Factions.GetRelationship(hittable.Faction, unit.Faction);

                    // Not neutral?
                    if (rl == Factions.Relationship.Neutral)
                        continue;

                    if (!settings.ignoreAngle) {
                        // Within sight angle?
                        float sightAngle = Vector2.SignedAngle(transform.up, colliders[i].transform.position - transform.position);
                        if (Mathf.Abs(sightAngle)*2 > settings.sightAngle)
                            continue;
                    }

                    if (!settings.ignoreWalls) {
                        // Unobstructed view?
                        int terrainMask = 1 << LayerMask.NameToLayer("Terrain");
                        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                            colliders[i].transform.position - transform.position,
                            (colliders[i].transform.position - transform.position).magnitude,
                            terrainMask);

                        if (hit.collider != null)
                            continue;
                    }

                    // Is self?
                    if (ReferenceEquals(colliders[i].gameObject, gameObject))
                        continue;

                    // Create UnitInfo! TODO: Don't always create new UnitInfos. We can edit them.
                    UnitInfo unitInfo = new UnitInfo().FromGameObject(colliders[i].gameObject);

                    // Add to dict/list, if it's not in there already!
                    if (rl == Factions.Relationship.Friendly) {
                        AlliesInfoPermanent[colliders[i].gameObject] = unitInfo;
                        AlliesInfo.Add(unitInfo);
                    } else {
                        EnemiesInfoPermanent[colliders[i].gameObject] = unitInfo;
                        EnemiesInfo.Add(unitInfo);
                        LastEnemyTimestamp = Time.time;
                    }
                }

                if (OnEnemyCheck != null) {
                    OnEnemyCheck(EnemySpotted);
                }
            }
        }

        /// <summary>
        /// The situation rating is a number between -1 and 1, that expresses how well the current
        /// situation is for the AI. Positive numbers are good, negative are bad.
        /// The situation rating depends on the own health, allies health and enemies health.
        /// </summary>
        public float GetSituationRating() {

            if (sitSettings.fixSituationRating) { // Is situation rating fixed?
                SituationRating = sitSettings.fixedSituationRating;
                return SituationRating;
            }

            if (sitSettings == null) {
                Debug.LogWarning("No situation rating settings set.");
                return 0;
            }

            int numAttendants = EnemiesInfo.Count + AlliesInfo.Count;
            SituationRating = 0;
            DiffHealthRating = 0;
            for (int i = 0; i < EnemiesInfo.Count; i++) {
                DiffHealthRating -= EnemiesInfo[i].HealthFraction;
            }
            for (int i = 0; i < AlliesInfo.Count; i++) {
                DiffHealthRating += AlliesInfo[i].HealthFraction;
            }
            DiffHealthRating += unit.Health / unit.MaxHealth;
            DiffHealthRating /= 1f - sitSettings.healthThreshold; // https://www.wolframalpha.com/input/?i=Clip((x)+%2F+(1-0.3)+)+with+x+%3D+0+to+1
            DiffHealthRating = Mathf.Clamp(DiffHealthRating / numAttendants, -1f, 1f); // Scale to -1 to 1

            OwnHealthRating = unit.Health / unit.MaxHealth;
            OwnHealthRating = Mathf.Clamp01(OwnHealthRating / (1f - sitSettings.healthThreshold)); // https://www.wolframalpha.com/input/?i=Clip((x)+%2F+(1-0.3)+)+with+x+%3D+0+to+1
            OwnHealthRating = 2 * OwnHealthRating - 1f; // Scale to -1 to 1

            if (weap != null) {
                CooldownRating = 1-Mathf.Clamp01(weap.Cooldown / (sitSettings.cooldownThreshold * 2));
                CooldownRating = CooldownRating - 1; // Scale to -1 to 0 (!)
            } else {
                CooldownRating = 0f;
            }
            SituationRating = OwnHealthRating * sitSettings.ownHealthWeight + DiffHealthRating * sitSettings.diffHealthWeight + CooldownRating * sitSettings.cooldownWeight;
            SituationRating /= sitSettings.ownHealthWeight + sitSettings.diffHealthWeight + sitSettings.cooldownWeight; // Scale to -1 to 1
            SituationRating = Mathf.Clamp(SituationRating * 3, -1f, 1f);
            return SituationRating;
        }

        void OnDrawGizmosSelected() {
            if (settings != null) {
                Handles.color = new Color(1f, 1f, 0f, 0.1f);
                float range;
                float angle;
                range = settings.ignoreRange ? 100f : settings.sightRange;
                angle = settings.ignoreAngle ? 360f : settings.sightAngle;
                Handles.DrawSolidArc(transform.position, Vector3.forward, Quaternion.Euler(0f, 0f, -angle / 2) * transform.up, angle, range);

                if (EnemiesInfoPermanent != null) {
                    foreach (KeyValuePair<GameObject, UnitInfo> entry in EnemiesInfoPermanent) {
                        Gizmos.color = new Color(2f, 0f, 0f, 1 - Mathf.Clamp(entry.Value.TimestampAge / 5f, 0f, 0.9f));
                        Gizmos.DrawSphere((Vector3)entry.Value.Position + Vector3.back*0.5f, 0.25f);
                    }
                }

                if (AlliesInfoPermanent != null) {
                    foreach (KeyValuePair<GameObject, UnitInfo> entry in AlliesInfoPermanent) {
                        Gizmos.color = new Color(0f, 0f, 2f, 1 - Mathf.Clamp(entry.Value.TimestampAge / 5f, 0f, 0.9f));
                        Gizmos.DrawSphere((Vector3)entry.Value.Position + Vector3.back*0.5f, 0.25f);
                    }
                }
            }
        }
    }
}
