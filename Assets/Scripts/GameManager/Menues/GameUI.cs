using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
	public static GameUI Instance { get; private set; }

	[SerializeField] Text ammoText;
	[SerializeField] Text firemodeText;
	[SerializeField] Text healthText;

	void Awake() {
		if (Instance != null) {
			Debug.LogError("Multiple GameUI Instances detected!");
		} else {
			Instance = this;
		}
	}

	public void SetAmmo(int ammo) {
		ammoText.text = "Ammo: "+ammo.ToString();
	}

	public void SetHealth(int health) {
		healthText.text = "Health: "+health.ToString();
	}

	public void SetFiremode(Weapon.Firemode firemode) {
		string str;

		switch (firemode) {
			case Weapon.Firemode.Auto:
				str = "Auto";
				break;
			case Weapon.Firemode.Single:
				str = "Single";
				break;
			case Weapon.Firemode.Burst:
				str = "Burst";
				break;
			case Weapon.Firemode.AutoBurst:
				str = "Burst";
				break;
			default:
				throw new System.Exception();
		}

		firemodeText.text = "Firemode: " + str;
	}
}
