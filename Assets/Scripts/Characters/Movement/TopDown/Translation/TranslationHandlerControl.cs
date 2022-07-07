using UnityEngine;
using Control;
using Utility;

public class TranslationHandlerControl : ITranslationHandler {

    float speed = 5f;
    
    VelocityController2D velCtrl;
    Rigidbody2D rb;

    Vector2 targetDirection;
    Vector2 targetPosition;

    enum TranslationMode { Position, Velocity };
    TranslationMode translationMode;

    public TranslationHandlerControl(MovementHandler context, float speed, Vector2 poles) {
        rb = context.GetComponent<Rigidbody2D>();
        SetTargetPosition(rb.position);

        Complex p = new Complex(poles.x, poles.y, Complex.Notation.MagnPhase);
        velCtrl = new VelocityController2D(p, p, rb);
        this.speed = speed;
    }
    
    public void FixedUpdate() {
        // TODO: Implement more robust position control! Stat. Genauigkeit nicht erreicht!

        if (translationMode == TranslationMode.Position) {

        } else {
            targetPosition += targetDirection * Time.fixedDeltaTime * speed;
        }
        rb.AddForce(velCtrl.Eval(targetPosition, rb.position));
    }

    public void SetTargetDirection(Vector2 direction) {
        targetDirection = direction * speed;
        targetPosition = rb.position;
        translationMode = TranslationMode.Velocity;
    }

    public void SetTargetPosition(Vector2 position) {
        targetPosition = position;
        translationMode = TranslationMode.Position;
    }
}