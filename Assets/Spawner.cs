using UnityEngine;

public class Spawner : MonoBehaviour {
	public GameObject prefab;
	public float amount;
	
	void Update() {
		while (transform.childCount < amount) {
			Instantiate(prefab, transform);
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, (Mathf.Cos(Time.time)*0.5f+0.5f)*0.5f + 0.5f);
	}
}
