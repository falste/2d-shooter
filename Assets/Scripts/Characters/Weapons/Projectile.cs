using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour, IPoolable {
	public static Pool<Projectile> pool;

	public float lifetime = 5f;

	static WaitForSeconds waitLifetime;
	Coroutine lifetimeCoroutine;
	float damage;
	float impulse;

	Rigidbody2D rb;

	public void OnReset() {
		transform.GetChild(0).GetComponent<TrailRenderer>().Clear();
	}
	public void OnRelease() {
		if (lifetimeCoroutine != null)
			StopCoroutine(lifetimeCoroutine);

		gameObject.SetActive(false);
	}

	public void SetProperties(float damage, float impulse, Vector3 position, Quaternion rotation) {
		this.damage = damage;
		this.impulse = impulse;
		gameObject.SetActive(true);
		transform.position = position;
		transform.rotation = rotation;
		lifetimeCoroutine = StartCoroutine(ReleaseAfterLifetime());
	}

	void Awake() {
		if (waitLifetime == null)
			waitLifetime = new WaitForSeconds(lifetime);

		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate() {
		int terrainMask = 1 << LayerMask.NameToLayer("Terrain");
		int hittableMask = 1 << LayerMask.NameToLayer("Hittable");
		RaycastHit2D terrainHit = Physics2D.Raycast(rb.position, rb.velocity, rb.velocity.magnitude * Time.fixedDeltaTime, terrainMask);
		RaycastHit2D hittableHit = Physics2D.Raycast(rb.position, rb.velocity, rb.velocity.magnitude * Time.fixedDeltaTime, hittableMask);
		if (hittableHit.collider == null && terrainHit.collider == null)
			return;

		if (hittableHit.collider == null) {
			OnHitTerrain();
			return;
		}

		if (terrainHit.collider == null) {
			OnHitHittable(hittableHit.collider.GetComponent<IHittable>(), hittableHit.point, rb.velocity);
			return;
		}

		if (hittableHit.distance > terrainHit.distance) {
			OnHitTerrain();
		} else {
			OnHitHittable(hittableHit.collider.GetComponent<IHittable>(), hittableHit.point, rb.velocity);
		}
	}

	void OnHitTerrain() {
		PrjPool.Instance.ReleaseInstance(this);
	}

	void OnHitHittable(IHittable hittable, Vector2 point, Vector2 direction) {
		if (hittable == null) {
			Debug.LogError("Object on hittable layer does not have a IHittable component!");
		} else {
			hittable.Hit(damage, impulse, point, direction);
		}

		PrjPool.Instance.ReleaseInstance(this);
	}

	IEnumerator ReleaseAfterLifetime() {
		yield return waitLifetime;
		PrjPool.Instance.ReleaseInstance(this);
	}
}
