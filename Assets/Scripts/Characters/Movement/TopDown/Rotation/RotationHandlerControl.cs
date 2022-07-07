using UnityEngine;
using Control;
using Utility;

public class RotationHandlerControl : IRotationHandler {

    RotationController2D rotCtrl;
    Rigidbody2D rb;
    float targetRotation;

    public RotationHandlerControl(MovementHandler context, Vector2 poles) {
        rb = context.GetComponent<Rigidbody2D>();

        Complex p = new Complex(poles.x, poles.y, Complex.Notation.MagnPhase);
        rotCtrl = new RotationController2D(p, p, rb);
    }
    
    public void FixedUpdate()
    {
        rb.AddTorque(rotCtrl.Eval(targetRotation, rb.rotation * Utility.Calc.Angles.Deg2Rad));
    }

    #region IRotationHandler Implementation
    public void SetTargetRotation(float rotationRad)
    {
        targetRotation = rotationRad % (2*Mathf.PI);
    }

    public void SetTargetRotation(Vector2 direction) {
        targetRotation = Vector2.SignedAngle(Vector2.up, direction);
        targetRotation *= Calc.Angles.Deg2Rad;
    }
    #endregion
}
