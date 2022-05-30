using UnityEngine;

[RequireComponent(typeof(Unit))]
[RequireComponent(typeof(Rigidbody2D))]
public class WeaponHandler : MonoBehaviour {
	public bool connectToGameUI = false;
	public WeaponData[] weaponData;
	public Weapon SelectedWeapon {
		get {
			if (weapons == null) {
				return null;
			}
			if (selectedWeaponIndex >= weapons.Length) {
				return null;
			}

			return weapons[selectedWeaponIndex];
		}
	}
	public float Cooldown {
		get {
			return nextShotTime - Time.time;
		}
	}

	Weapon[] weapons;
	int selectedWeaponIndex;

	Coroutine burstRoutine;
	Coroutine reloadRoutine;
	bool bursting;
	bool reloading;

	float nextShotTime;
	float nextShotTimeAdjust; // Keeps track of the small losses of time due to Update not running in sync to weapon cooldown.
	bool triggerReleasedSinceLastShot;
	
	void Awake () {
		selectedWeaponIndex = 0;

		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		weapons = new Weapon[weaponData.Length];
		for (int i = 0; i < weaponData.Length; i++) {
			weapons[i] = new Weapon(weaponData[i], OnShot, rb);
		}
	}

	void Start() {
		if (connectToGameUI) {
			GameUI.Instance.SetAmmo(SelectedWeapon.ammo);
			GameUI.Instance.SetFiremode(SelectedWeapon.firemode);
		}
	}

	void Reset() {
		weaponData = new WeaponData[0];
	}

	void FixedUpdate() {
		for (int i = 0; i < weapons.Length; i++) {
			weapons[i].FixedUpdate();
		}
	}

	public void OnTriggerHold()
	{
		if (SelectedWeapon != null) {
			TryShoot();
		}
	}

	void TryShoot() {
		if (reloading)
			return;
		if (bursting)
			return;
		if (SelectedWeapon.ammo == 0) {
			Reload();
			return;
		}
		if (nextShotTime > Time.time)
			return;


		if (!triggerReleasedSinceLastShot)
			nextShotTimeAdjust = Time.time - nextShotTime;

		if (SelectedWeapon.firemode == Weapon.Firemode.AutoBurst) {
			burstRoutine = StartCoroutine(SelectedWeapon.Burst(OnBurstFinished));
			bursting = true;
		} else if (SelectedWeapon.firemode == Weapon.Firemode.Burst && triggerReleasedSinceLastShot) {
			burstRoutine = StartCoroutine(SelectedWeapon.Burst(OnBurstFinished));
			bursting = true;
		} else if (SelectedWeapon.firemode == Weapon.Firemode.Single && triggerReleasedSinceLastShot) {
			SelectedWeapon.ReleaseShot();
		} else if (SelectedWeapon.firemode == Weapon.Firemode.Auto) {
			SelectedWeapon.ReleaseShot();
		}
	}

	public void OnTriggerRelease()
	{
		triggerReleasedSinceLastShot = true;
		nextShotTimeAdjust = 0f;
	}

	public void Reload() {
		if (bursting)
			return;

		if (SelectedWeapon.ammo != SelectedWeapon.data.magSize) {
			reloadRoutine = StartCoroutine(SelectedWeapon.Reload(OnReloadFinished));
			reloading = true;
		}
	}

	void OnReloadFinished() {
		reloading = false;

		nextShotTime = Time.time;
		nextShotTimeAdjust = 0f;

		if (connectToGameUI)
			GameUI.Instance.SetAmmo(SelectedWeapon.ammo);
	}

	void OnBurstFinished() {
		bursting = false;
	}

	void OnShot(float newCooldown) {
		nextShotTime = Time.time + newCooldown - nextShotTimeAdjust / 2;
		triggerReleasedSinceLastShot = false;
		if (connectToGameUI)
			GameUI.Instance.SetAmmo(SelectedWeapon.ammo);
	}
	
	public void Select(int selection) {
		if (selection == selectedWeaponIndex)
			return;

		if (selection >= weapons.Length)
			return;

		selectedWeaponIndex = selection;

		if (burstRoutine != null)
			StopCoroutine(burstRoutine);
		if (reloadRoutine != null)
			StopCoroutine(reloadRoutine);
		bursting = false;
		reloading = false;

		nextShotTime = Time.time + SelectedWeapon.data.equipTime;
		triggerReleasedSinceLastShot = true;

		if (connectToGameUI) {
			GameUI.Instance.SetAmmo(SelectedWeapon.ammo);
			GameUI.Instance.SetFiremode(SelectedWeapon.firemode);
		}
	}

	public void SelectUp()
	{
		Select((selectedWeaponIndex + 1) % weapons.Length);
	}

	public void SelectDown()
	{
		Select((selectedWeaponIndex - 1) % weapons.Length);
	}

	public void ToggleFiremode()
	{
		if (SelectedWeapon != null) {
			SelectedWeapon.ToggleFiremode();

			if (connectToGameUI)
				GameUI.Instance.SetFiremode(SelectedWeapon.firemode);
		}
	}

	void OnDrawGizmos() {
		if (SelectedWeapon != null)
			SelectedWeapon.OnDrawGizmos();
	}

	void OnDrawGizmosSelected() {
		if (SelectedWeapon != null)
			SelectedWeapon.OnDrawGizmosSelected();
	}
}
