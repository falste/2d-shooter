using UnityEngine;

public class EnableOnAwake : MonoBehaviour {
	public Behaviour behaviourToEnable;

	void Awake() {
		behaviourToEnable.enabled = true;
	}
}
