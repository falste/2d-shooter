using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempRotation : MonoBehaviour {

	public float speed;

	void Update () {
		transform.Rotate(speed*Time.deltaTime, speed*Time.deltaTime, speed*Time.deltaTime);
	}
}
