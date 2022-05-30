using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserSight : MonoBehaviour {
	const float zdist = -0.01f;

	LineRenderer lr;

	void Awake() {
		lr = GetComponent<LineRenderer>();
	}

	void Update () {
		Vector3 position;
		Vector3 hitpoint;

		RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, transform.up);

		position = transform.position + Vector3.forward * zdist;
		hitpoint = (Vector3)hitInfo.point + Vector3.forward * zdist;
		lr.SetPositions(new Vector3[] { position, hitpoint });
	}
}
