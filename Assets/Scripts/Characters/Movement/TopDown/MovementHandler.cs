using UnityEngine;

// TODO: Implement Stellbegrenzung für Control Systems
[RequireComponent(typeof(Rigidbody2D))]
public class MovementHandler : MonoBehaviour {
	public enum TranslationType { Velocity, Control };
	public enum RotationType { Instant, Control };

	public ITranslationHandler trans;
	public IRotationHandler rot;

	public TranslationType translationType = TranslationType.Velocity;
	public RotationType rotationType = RotationType.Control;
	public float trans_speed = 5f;
	public Vector2 trans_pole = new Vector2(0.8f, 0.1f);
	public Vector2 rot_pole = new Vector2(0.7f, 0.1f);

	public bool lockPosition;
	public bool lockRotation;

	Rigidbody2D rb;

	void Awake() {
		Init();
		rb = GetComponent<Rigidbody2D>();
	}

	public void Init() {
		switch (translationType) {
			case TranslationType.Control:
				trans = new TranslationHandlerControl(this, trans_speed, trans_pole);
				break;
			case TranslationType.Velocity:
				trans = new TranslationHandlerVelocity(this, trans_speed);
				break;
			default:
				throw new System.NotImplementedException();
		}

		switch (rotationType) {
			case RotationType.Control:
				rot = new RotationHandlerControl(this, rot_pole);
				break;
			case RotationType.Instant:
				rot = new RotationHandlerDirect(this);
				break;
			default:
				throw new System.NotImplementedException();
		}
	}

	public void SetTargetDirection(Vector2 direction) {
		if (lockPosition) {
			trans.SetTargetPosition(transform.position);
		} else {
			trans.SetTargetDirection(direction);
		}
	}
	public void SetTargetPosition(Vector2 position) {
		if (lockPosition) {
			trans.SetTargetPosition(transform.position);
		} else {
			trans.SetTargetPosition(position);
		}
	}

	public void SetTargetRotation(float rotationRad) {
		if (lockRotation) {
			rot.SetTargetRotation(GetComponent<Rigidbody2D>().rotation * Mathf.Deg2Rad);
		} else {
			rot.SetTargetRotation(rotationRad);
		}
	}
	public void SetTargetRotation(Vector2 direction) {
		if (lockRotation) {
			rot.SetTargetRotation(GetComponent<Rigidbody2D>().rotation * Mathf.Deg2Rad);
		} else {
			rot.SetTargetRotation(direction);
		}
	}

	// Set target rotation to current velocity direction
	public void SetTargetRotation() {
		if (rb.velocity.sqrMagnitude > 0.01f)
			SetTargetRotation(rb.velocity);
	}

	void FixedUpdate() {
		trans.FixedUpdate();
		rot.FixedUpdate();
	}
}
