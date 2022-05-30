using UnityEngine;

public interface IHittable {
	void Hit(float damage, float impulse, Vector2 point, Vector2 direction);
	Factions.Faction Faction { get; }
	float Health { get; }
	float MaxHealth { get; }
}
