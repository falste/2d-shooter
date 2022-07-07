using UnityEngine;

namespace AI {
    [CreateAssetMenu(fileName = "SituationRatingSettings", menuName = "ScriptableObjects/AI/SituationRatingSettings", order = 1)]
    public class SituationRatingSettings : ScriptableObject {
        public bool fixSituationRating;
        [Range(-1f, 1f)] public float fixedSituationRating = 1f;

        public float healthThreshold = 0.3f;
        public float ownHealthWeight = 1f;
        public float diffHealthWeight = 1f;
        public float cooldownWeight = 1f;
        public float cooldownThreshold = 0.5f;
    }
}