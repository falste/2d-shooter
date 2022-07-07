using UnityEngine;
using Utility;

public class RotationHandlerDirect : IRotationHandler {
    Rigidbody2D rb;
    float targetRotation;

    public RotationHandlerDirect(MovementHandler context) {
        rb = context.GetComponent<Rigidbody2D>();
        targetRotation = rb.rotation * Mathf.Deg2Rad;
    }

    public void FixedUpdate() {
        rb.MoveRotation(targetRotation * Mathf.Rad2Deg);
    }

    public void SetTargetRotation(float rotationRad) {
        targetRotation = rotationRad % (2 * Mathf.PI);
    }

    public void SetTargetRotation(Vector2 direction) {
        targetRotation = Vector2.SignedAngle(Vector2.up, direction);
        targetRotation *= Calc.Angles.Deg2Rad;
    }
}
