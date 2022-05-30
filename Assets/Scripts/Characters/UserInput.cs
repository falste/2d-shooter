using UnityEngine;
using Utility;
using UnityEngine.UI;

[RequireComponent(typeof(MovementHandler))]
[RequireComponent(typeof(WeaponHandler))]
public class UserInput : MonoBehaviour {
	public float triggerThreshold = 0.5f;
	public float analogDeadzone = 0.2f;

	public float maxLockAngle = 45f;

	float rt;
	float rtOld;

	MovementHandler mov;
	WeaponHandler weap;
	AI.Vision vis;

	Vector2 velTarget;
	Vector2 rotTargetVector;

	Camera cam;

	Transform crosshair;

	void Start() {
		mov = GetComponent<MovementHandler>();
		weap = GetComponent<WeaponHandler>();
		vis = GetComponent<AI.Vision>();

		rtOld = 0f;

		cam = Camera.main;
		crosshair = GameObject.Find("Crosshair").transform;
		GameObject.Find("CM vcam1").GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = transform;
	}

	void Update() {
		#region WeaponInput
		if (Input.GetKey(KeyCode.Mouse0)) {
			weap.OnTriggerHold();
		}
		if (Input.GetKeyUp(KeyCode.Mouse0)) {
			weap.OnTriggerRelease();
		}
		if (Input.GetKey(KeyCode.Space)) {
			weap.OnTriggerHold();
		}
		if (Input.GetKeyUp(KeyCode.Space)) {
			weap.OnTriggerRelease();
		}
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			weap.Select(0);
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			weap.Select(1);
		}
		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			weap.Select(2);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4)) {
			weap.Select(3);
		}
		if (Input.GetKeyDown(KeyCode.E)) {
			weap.SelectUp();
		}
		if (Input.GetKeyDown(KeyCode.Q)) {
			weap.SelectDown();
		}
		if (Input.GetKeyDown(KeyCode.V)) {
			weap.ToggleFiremode();
		}
		if (Input.GetKeyDown(KeyCode.R)) {
			weap.Reload();
		}

		rt = Input.GetAxis("RT");
		if (rt >= triggerThreshold) {
			weap.OnTriggerHold();
		} else if (rtOld >= triggerThreshold) {
			weap.OnTriggerRelease();
		}
		rtOld = rt;
		if (Input.GetKeyDown(KeyCode.JoystickButton5)) { // RB
			weap.SelectUp();
		}
		if (Input.GetKeyDown(KeyCode.JoystickButton3)) { // Y
			weap.ToggleFiremode();
		}
		if (Input.GetKeyDown(KeyCode.JoystickButton2)) { // X
			weap.Reload();
		}
		#endregion

		#region RotationInput
		rotTargetVector = Vector2.zero;

		
		Vector2 analogInputRight = new Vector2 {
			x = Input.GetAxis("RX"),
			y = -Input.GetAxis("RY")
		};

		if (analogInputRight.sqrMagnitude > analogDeadzone * analogDeadzone) {
			rotTargetVector = Utility.Calc.ClampVector2Magnitude(analogInputRight, 0f, 1f);

			// Aim assist: Lock onto enemy
			bool locking = false;
			Vector2 lockedOnPosition = (Vector2)transform.position + rotTargetVector;
			float minLockingCost = float.MaxValue;

			foreach (AI.UnitInfo enemyInfo in vis.EnemiesInfo) {
				if (enemyInfo.GameObject == null)
					continue;

				Vector2 enemyPos = (Vector2)enemyInfo.GameObject.transform.position;

				float dist = ((Vector2)transform.position - enemyPos).magnitude;
				float angleDiff = Vector3.Angle(rotTargetVector, enemyPos - (Vector2)transform.position) * Mathf.Deg2Rad;

				if (angleDiff > maxLockAngle * Mathf.Deg2Rad) {
					continue;
				}


				float lockingCost = dist * angleDiff;

				if (lockingCost < minLockingCost) {
					locking = true;
					minLockingCost = lockingCost;
					lockedOnPosition = enemyPos;
				}
			}

			if (locking) {
				crosshair.gameObject.SetActive(true);
				crosshair.position = cam.WorldToScreenPoint(lockedOnPosition);
			} else {
				crosshair.gameObject.SetActive(false);
			}

			rotTargetVector = lockedOnPosition - (Vector2)transform.position;


		} else {
			crosshair.gameObject.SetActive(false);

			if (Input.GetKey(KeyCode.DownArrow)) {
				rotTargetVector += Vector2.down;
			}
			if (Input.GetKey(KeyCode.UpArrow)) {
				rotTargetVector += Vector2.up;
			}
			if (Input.GetKey(KeyCode.LeftArrow)) {
				rotTargetVector += Vector2.left;
			}
			if (Input.GetKey(KeyCode.RightArrow)) {
				rotTargetVector += Vector2.right;
			}
		}
		

		/*
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);

		float enter;
		new Plane(Vector3.forward, 0f).Raycast(ray, out enter);

		Vector3 pointInSpace = ray.origin + ray.direction * enter;
		rotTargetVector = pointInSpace - transform.position;
		*/

		if (rotTargetVector != Vector2.zero) {
			mov.SetTargetRotation(rotTargetVector);
		}
		#endregion

		#region TranslationInput
		velTarget = Vector2.zero;

		Vector2 analogInputLeft = new Vector2 {
			x = Input.GetAxis("LX"),
			y = -Input.GetAxis("LY")
		};

		if (analogInputLeft.sqrMagnitude > analogDeadzone * analogDeadzone) {
			velTarget = analogInputLeft;
		} else {
			if (Input.GetKey(KeyCode.S)) {
				velTarget += Vector2.down;
			}
			if (Input.GetKey(KeyCode.W)) {
				velTarget += Vector2.up;
			}
			if (Input.GetKey(KeyCode.A)) {
				velTarget += Vector2.left;
			}
			if (Input.GetKey(KeyCode.D)) {
				velTarget += Vector2.right;
			}
		}

		velTarget = Calc.ClampVector2Magnitude(velTarget, 0f, 1f);
		mov.SetTargetDirection(velTarget);
		#endregion
	}

	void OnDestroy() {
		crosshair.gameObject.SetActive(true);
	}

	public void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;

		Gizmos.DrawRay(transform.position, Quaternion.Euler(0f, 0f, maxLockAngle) * transform.up * 100);
		Gizmos.DrawRay(transform.position, Quaternion.Euler(0f, 0f, -maxLockAngle) * transform.up * 100);
	}
}