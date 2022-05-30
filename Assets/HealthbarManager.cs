using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarManager : MonoBehaviour {
	public static HealthbarManager Instance { get; private set; }
	public static GameObject Prefab {
		get {
			if (prefab == null) {
				prefab = (GameObject)Resources.Load("Healthbar", typeof(GameObject));
			}
			return prefab;
		}
	}

	static GameObject prefab;
	public static float offset = 20f;


	void Awake() {
		if (Instance != null)
			throw new System.Exception("Multiple HealthbarManagers detected!");

		Instance = this;
	}
}
