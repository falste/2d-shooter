using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

// TODO: Do a raycast to prevent friendly fire
namespace AI {
	[RequireComponent(typeof(WeaponHandler))]
	[RequireComponent(typeof(Vision))]
	[RequireComponent(typeof(MovementHandler))]
	public class Shooting : MonoBehaviour {

		public ShootingSettings settings;

		WeaponHandler weap;
		MovementHandler mov;
		Vision vis;

		bool isShooting;
		public bool TargetFound { get; private set; }

		// Debug stuff
		Vector2 aimPoint;

		/*
		 * Functions
		 */

		void Awake() {
			weap = GetComponent<WeaponHandler>();
			mov = GetComponent<MovementHandler>();
			vis = GetComponent<Vision>();
		}

		void Update() {
			UnitInfo target = SelectTarget();
			if (target != null)
				ShootInPredictedPath(target);

			TargetFound = target != null;

			if (isShooting) {
				weap.OnTriggerHold();
				isShooting = false;
			} else {
				weap.OnTriggerRelease();
			}
		}
		
		UnitInfo SelectTarget() {
			// Select most dangerous target
			UnitInfo target = null;
			float minDist = float.MaxValue;
			float maxTime = 0f;
			float dist;

			// TODO: Improve target selection
			// Should be a nice function

			// Choose newest and closest EnemyInfo. Newest is more important than closest
			foreach (KeyValuePair<GameObject, UnitInfo> entry in vis.EnemiesInfoPermanent) {
				if (entry.Value.TimestampAge > settings.forgetEnemyTime)
					continue;

				if (entry.Value.Timestamp > maxTime) {
					maxTime = entry.Value.Timestamp;
					minDist = float.MaxValue;
				}

				dist = (entry.Value.Position - (Vector2)transform.position).sqrMagnitude;
				if (dist < minDist) {
					minDist = dist;
					target = entry.Value;
				}
			}

			return target;
		}

		void ShootInPredictedPath(UnitInfo target) {
			// Look at point in front of player, where a projectile would hit him
			if (settings.targetPredictionFactor == 0f) {
				// Simplifying calculations for ai.data.targetPredictionFactor == 0f
				aimPoint = target.Position;
			} else {
				// Complex targetPoint calculations

				// Get the point at which to shoot to hit the enemy
				Vector2 relTargetPosition = target.Position - (Vector2)transform.position;
				Vector2 relTargetVelocity = target.Velocity;

				/* 
				 * Considers inherited velocity due to translation, but not rotation
				 * Does not work correctly due to clipping of the intercept point because
				 * the intercept point should move with the shooter, but does not actually
				 * Seems like there is no obvious fix for this, so lets just not use it
				 */
				//relTargetVelocity -= ai.weap.SelectedWeapon.data.inheritVelocityMultiplier * ai.rb.velocity;

				float interceptTime = Calc.InterceptTime(relTargetPosition, relTargetVelocity, weap.SelectedWeapon.data.speed);
				Vector2 relInterceptPosition = Calc.RelativeInterceptPoint(relTargetPosition, relTargetVelocity, interceptTime);

				/* ### Avoid interceptPoint being behind a wall from enemy perspective ###
				 * 
				 * Raycast from enemy to interceptPoint to see if there is any terrain in the way.
				 * If there is, use the collision point with the terrain as new intercept point.
				 * To do this, raycast and transform to relative space.
				 */
				Vector2 targetToIntercept = relInterceptPosition + (Vector2)transform.position - target.Position;
				int terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
				RaycastHit2D hit = Physics2D.Raycast(target.Position, targetToIntercept, targetToIntercept.magnitude + target.BodyRadius, terrainLayer);
				if (hit.collider != null) {
					// New position touching the wall
					relInterceptPosition = hit.point - (Vector2)transform.position;

					// Get away from the wall by EnemyRadius
					relInterceptPosition -= (relInterceptPosition - relTargetPosition).normalized * target.BodyRadius;
				}

				/* ### Avoid interceptPoint being behind a wall from shooter perspective ###
				 * 
				 * Raycast from shooter to relativeInterceptPosition to see if there is any terrain in the way. If there is, shoot at the next best spot between the intercept point and the player.
				 */
				hit = Physics2D.Raycast((Vector2)transform.position, relInterceptPosition, relInterceptPosition.magnitude, terrainLayer);
				if (hit.collider != null) {
					// Path obstructed, target dynamically restricted position in front of player, that is still reachable
					Vector2 minPoint = relTargetPosition;
					Vector2 maxPoint = relInterceptPosition;
					bool blockedByTerrain = true; // This is there to prevent the ai from shooting, when terrain is blocking the whole path the target is going to take. In that case, we won't shoot.

					for (int i = 0; i < settings.interceptBehindWallIterations; i++) {
						Vector2 midPoint = minPoint + (maxPoint - minPoint) / 2;
						hit = Physics2D.Raycast((Vector2)transform.position, midPoint, midPoint.magnitude, terrainLayer);
						if (hit.collider == null) {
							minPoint = midPoint;
							blockedByTerrain = false;
						} else {
							maxPoint = midPoint;
						}
					}

					if (blockedByTerrain) {
						isShooting = false;
						return;
					}


					aimPoint = (Vector2)transform.position + minPoint;
				} else {
					// Target some point between the target and the intercept point, depending on targetPredictionFactor
					//float freq = 0;
					//float oscillationFactor = (Mathf.Cos(Time.time * 2 * Mathf.PI * freq) + 1f) * 0.5f;
					aimPoint = target.Position + (relInterceptPosition - relTargetPosition) * settings.targetPredictionFactor;// * oscillationFactor;
				}
				//targetPoint = ai.enemyInfo.Position + (relInterceptPosition - relTargetPosition) * ai.data.targetPredictionFactor;
			}

			float interpAngle = Vector2.SignedAngle(Vector2.up, aimPoint - (Vector2)transform.position);
			mov.SetTargetRotation(interpAngle * Calc.Angles.Deg2Rad);

			// Shoot, if direction fits
			// Checks if current rotation is between target rotation and interp rotation, including a tolerance
			float targetAngle = Vector2.SignedAngle(Vector2.up, target.Position - (Vector2)transform.position);

			float angle1 = Mathf.Min(interpAngle, targetAngle);
			float angle2 = Mathf.Max(interpAngle, targetAngle);
			float cwAngle;
			float ccwAngle;

			cwAngle = Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Degrees, angle1 - angle2) < 0f ? angle1 : angle2;
			ccwAngle = Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Degrees, angle1 - angle2) > 0f ? angle1 : angle2;
			cwAngle -= settings.shootingAngleTolerance;
			ccwAngle += settings.shootingAngleTolerance;

			//Debug.Log("CW, CCW " + cwAngle + ", " + ccwAngle);

			if (Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Degrees, transform.rotation.eulerAngles.z - cwAngle) >= 0f &&
				Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Degrees, transform.rotation.eulerAngles.z - ccwAngle) <= 0f) {
				isShooting = true;
			}

			//Debug.Log("Angles (+,-)! : " +
			//		Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Degrees, ai.rb.rotation - cwAngle) + ", " +
			//		Calc.Angles.ClosestPeriodicEquivalent(Calc.Angles.AngleMeasure.Degrees, ai.rb.rotation - ccwAngle));
		}

		void OnDrawGizmosSelected() {
			Gizmos.color = Color.red * 2;
			Gizmos.DrawSphere((Vector3)aimPoint + Vector3.back * 0.5f, 0.2f);
		}
	}
}
