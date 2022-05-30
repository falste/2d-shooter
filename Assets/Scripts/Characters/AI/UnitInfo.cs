using UnityEngine;

namespace AI {
	public class UnitInfo {
		public float BodyRadius { get; private set; }
		public float Timestamp { get; private set; }
		public float TimestampAge {
			get {
				return Time.time - Timestamp;
			}
		}
		public Vector2 Position { get; private set; }
		public Vector2 Velocity { get; private set; }
		public GameObject GameObject { get; private set; }
		public float HealthFraction { get; private set; }

		public UnitInfo FromGameObject(GameObject unit) {
			Timestamp = Time.time;
			Position = unit.transform.position;
			Velocity = unit.GetComponent<Rigidbody2D>().velocity;
			GameObject = unit;
			Vector3 bodySize = unit.GetComponent<Collider2D>().bounds.extents;
			BodyRadius = (bodySize.x + bodySize.y) / 2;
			IHittable hittable = unit.GetComponent<IHittable>();
			HealthFraction = hittable.Health / hittable.MaxHealth;

			return this;
		}
	}
}