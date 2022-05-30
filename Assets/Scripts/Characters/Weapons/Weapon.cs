using System.Collections;
using UnityEngine;
using Control;

// TODO: Use shootingRecoil and hitRecoil separately
// Set max recoil in recoilDecay.state when equipping weapon?
// Add correlation to recoil? Maybe not...
public class Weapon {
	public enum Firemode { None, Auto, Single, Burst, AutoBurst};

	public WeaponData data;
	[ReadOnly] public Firemode firemode;

	public int ammo;
	Lowpass recoilDecay;
	float recoil;

	System.Action<float> onShotCallback;
	Rigidbody2D rb;
	Unit unit;
	
	int currentShotSpawn;
	int[] shotsSinceLastFixedUpdate;

	int shotsRemainingInBurst;

	public Weapon(WeaponData data, System.Action<float> onShotCallback, Rigidbody2D rigidbody) {
		this.data = data;
		this.onShotCallback = onShotCallback;
		this.rb = rigidbody;
		unit = rigidbody.GetComponent<Unit>();

		// Check settings
		if (data == null)
			Debug.LogError("Could not find weapon data!");

		foreach (Transform t in data.shotSpawns) {
			if (t == null) {
				Debug.LogError("A shotspawn is null!");
			}
		}

		// Check if weapon has any firemodes at all
		if (!data.hasAuto && !data.hasBurst && !data.hasSingle && !data.hasAutoBurst) {
			Debug.LogError("Weapon does not have a firemode!");
			throw new System.Exception();
		}

		ammo = data.magSize;
		firemode = Firemode.None;
		ToggleFiremode();
		shotsSinceLastFixedUpdate = new int[data.shotSpawns.Length];

		recoilDecay = new Lowpass(data.recoilDecayTime);
		recoil = data.maxRecoilAngle * (Mathf.Exp(data.cooldown / data.recoilDecayTime) - 1f) / data.projectilesPerShotSpawn;
	}

	public void FixedUpdate() {
		recoilDecay.Eval(0);

		// Apply recoil
		for (int i = 0; i < shotsSinceLastFixedUpdate.Length; i++) {
			recoilDecay.state += shotsSinceLastFixedUpdate[i] * recoil;

			if (unit.knockable)
				rb.AddForceAtPosition(-GetShotSpawnUp(i) * shotsSinceLastFixedUpdate[i] * data.impulseRecoil, GetShotSpawnPosition(i), ForceMode2D.Impulse);
			shotsSinceLastFixedUpdate[i] = 0;
		}
	}

	#region ShotSpawn functions
	Vector2 GetShotSpawnPosition(int i) {
		Vector2 rotatedShotSpawnPosition = Quaternion.Euler(0f, 0f, rb.rotation) * data.shotSpawns[i].position;

		return rb.position + rotatedShotSpawnPosition;
	}

	Quaternion GetShotSpawnRotation(int i) {
		return Quaternion.Euler(0f, 0f, rb.rotation * data.shotSpawns[i].rotation.eulerAngles.z);
	}

	Vector2 GetShotSpawnUp(int i) {
		return Quaternion.Euler(0f, 0f, rb.rotation) * data.shotSpawns[i].up;
	}
	#endregion

	#region Firemode functions
	bool HasFiremode(Firemode firemode)
	{
		switch (firemode) {
			case Firemode.None: return true;
			case Firemode.Single: return data.hasSingle;
			case Firemode.Burst: return data.hasBurst;
			case Firemode.Auto: return data.hasAuto;
			case Firemode.AutoBurst: return data.hasAutoBurst;
			default: {
					throw new System.NotImplementedException();
				}
		}
	}

	Firemode NextFiremode(Firemode firemode)
	{
		switch (firemode) {
			case Firemode.None: return Firemode.Auto;
			case Firemode.Single: return Firemode.Burst;
			case Firemode.Burst: return Firemode.AutoBurst;
			case Firemode.Auto: return Firemode.Single;
			case Firemode.AutoBurst: return Firemode.Auto;
			default: {
					throw new System.NotImplementedException();
				}
		}
	}

	void SetFiremode(Firemode firemode)
	{
		this.firemode = firemode;
	}

	public void ToggleFiremode()
	{
		do {
			firemode = NextFiremode(firemode);
		} while (!HasFiremode(firemode));
	}
	#endregion

	#region Shooting functions
	public IEnumerator Burst(System.Action callback)
	{
		for (int i = 0; i < data.burstCount; i++) {
			if (ammo > 0) 
				ReleaseShot();

			if (i != data.burstCount - 1) {
				yield return new WaitForSeconds(data.cooldown);
			}
		}

		callback();
	}

	public void ReleaseShot()
	{
		ammo--;

		// Spawn projectile(s)
		if (data.shotSpawnsUsage == WeaponData.ShotSpawnsUsage.Cyclic) {
			SpawnFromShotSpawn(currentShotSpawn);
			currentShotSpawn = (currentShotSpawn + 1) % data.shotSpawns.Length;

		} else if (data.shotSpawnsUsage == WeaponData.ShotSpawnsUsage.Simultaneous) {
			for (int i = 0; i < data.shotSpawns.Length; i++) {
				SpawnFromShotSpawn(i);
			}

		} else {
			throw new System.Exception();
		}
		
		// Set cooldown
		float cooldown;
		if (firemode == Firemode.AutoBurst || firemode == Firemode.Burst) {
			cooldown = data.afterBurstCooldown;
		} else {
			cooldown = data.cooldown;
		}

		onShotCallback(cooldown);
	}

	void SpawnFromShotSpawn(int shotSpawnIndex)
	{
		float randRecoil;
		float randomAngleOffset;
		float shotSpawnAngleOffset;
		float angleOffsetSum;

		for (int i = 0; i < data.projectilesPerShotSpawn; i++) {
			shotsSinceLastFixedUpdate[shotSpawnIndex]++;

			// Calculate angle offset
			randRecoil =			Random.Range(-recoilDecay.state, recoilDecay.state); ;
			randomAngleOffset =		Random.Range(-data.maxOffsetAngle, data.maxOffsetAngle);
			shotSpawnAngleOffset =	(i - (data.projectilesPerShotSpawn-1) / 2f) * data.projectilesOffsetAngle;
			angleOffsetSum = shotSpawnAngleOffset + randRecoil + randomAngleOffset;

			// Calculate projectile velocity
			// Projectile base velocity
			Vector2 velocity = GetShotSpawnUp(shotSpawnIndex) * (data.speed + Random.Range(-data.speedVariation, data.speedVariation));

			// Angle Offset, rotating base velocity
			velocity = Quaternion.Euler(0f, 0f, angleOffsetSum) * velocity;

			// Add player velocity
			velocity += data.inheritVelocityMultiplier * rb.velocity;

			// Add player rotation velocity at that distance
			Vector2 tangent = -Vector2.Perpendicular(rb.position - GetShotSpawnPosition(shotSpawnIndex)).normalized;
			velocity += data.inheritVelocityMultiplier * Mathf.Deg2Rad * rb.angularVelocity * (rb.position - GetShotSpawnPosition(shotSpawnIndex)).magnitude * tangent; 

			Projectile projectile = PrjPool.Instance.GetInstance();

			projectile.SetProperties(
				damage: data.damage,
				impulse: data.impulseHit,
				position: GetShotSpawnPosition(shotSpawnIndex),
				rotation: GetShotSpawnRotation(shotSpawnIndex) * Quaternion.Euler(0f, 0f, angleOffsetSum)
				);
			Rigidbody2D prjRB = projectile.GetComponent<Rigidbody2D>();
			prjRB.velocity = velocity;
			//prjRB.rotation = -Vector2.SignedAngle(Vector2.up, velocity);
		}
	}

	#endregion

	public IEnumerator Reload(System.Action callback) {
		yield return new WaitForSeconds(data.reloadTime);
		ammo = data.magSize;
		callback();
	}

	public void OnDrawGizmos() {

	}

	public void OnDrawGizmosSelected() {
		Color target = new Color(1f, 0f, 0f, 0.5f);
		Color recoil = new Color(0.2f, 0.2f, 0.2f, 1f);
		Color maxRecoil = new Color(0f, 0f, 0f, 0.5f);
		Color recoilOutOfRange = new Color(1f, 1f, 1f, 1f);
		Color shotSpawnColor = Color.yellow*2;

		float shotSpawnAngleOffset;
		float minShotSpawnAngleOffset;
		float sum;
		Vector2 shotSpawnUp;
		Vector2 shotSpawnPosition;

		for (int i = 0; i < data.shotSpawns.Length; i++) {
			Gizmos.color = shotSpawnColor;
			Gizmos.DrawSphere(GetShotSpawnPosition(i), 0.25f);

			shotSpawnPosition = GetShotSpawnPosition(i);
			shotSpawnUp = GetShotSpawnUp(i);
			minShotSpawnAngleOffset = (-(data.projectilesPerShotSpawn - 1) / 2f) * data.projectilesOffsetAngle;

			Gizmos.color = maxRecoil;
			Gizmos.DrawRay(shotSpawnPosition, Quaternion.Euler(0f, 0f, data.maxRecoilAngle + data.maxOffsetAngle - minShotSpawnAngleOffset) * shotSpawnUp * 100);
			Gizmos.DrawRay(shotSpawnPosition, Quaternion.Euler(0f, 0f, -data.maxRecoilAngle - data.maxOffsetAngle + minShotSpawnAngleOffset) * shotSpawnUp * 100);

			sum = recoilDecay.state + data.maxOffsetAngle - minShotSpawnAngleOffset;
			if (sum > 90f) {
				Gizmos.color = recoilOutOfRange;
				sum = 90f;
			} else {
				Gizmos.color = recoil;
			}
			Gizmos.DrawRay(shotSpawnPosition, Quaternion.Euler(0f, 0f, sum) * shotSpawnUp * 100);
			Gizmos.DrawRay(shotSpawnPosition, Quaternion.Euler(0f, 0f, -sum) * shotSpawnUp * 100);

			for (int j = 0; j < data.projectilesPerShotSpawn; j++) {
				shotSpawnAngleOffset = (j - (data.projectilesPerShotSpawn - 1) / 2f) * data.projectilesOffsetAngle;

				Gizmos.color = target;
				Gizmos.DrawRay(shotSpawnPosition, Quaternion.Euler(0f, 0f, shotSpawnAngleOffset) * shotSpawnUp * 100);
			}
		}
	}
}