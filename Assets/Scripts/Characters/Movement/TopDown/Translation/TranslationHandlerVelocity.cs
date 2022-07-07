using UnityEngine;

// TODO: Fix velocity mode
public class TranslationHandlerVelocity : ITranslationHandler {
    enum TranslationMode { Position, Velocity };

    float speed = 5;

    Rigidbody2D rb;
    Vector2 targetDirection;
    Vector2 targetPosition;
    TranslationMode translationMode = TranslationMode.Velocity;

    public TranslationHandlerVelocity(MovementHandler context, float speed)
    {
        rb = context.GetComponent<Rigidbody2D>();
        this.speed = speed;
    }

    public void FixedUpdate()
    {
        if (translationMode == TranslationMode.Velocity) {
            rb.velocity = targetDirection * speed;
        } else {
            if ((targetPosition - rb.position).sqrMagnitude < Mathf.Pow(speed * Time.fixedDeltaTime, 2)) {
                rb.velocity = (targetPosition - rb.position) * speed;
            } else {
                rb.velocity = (targetPosition - rb.position).normalized * speed;
            }
        }
    }

    public void SetTargetDirection(Vector2 direction)
    {
        targetDirection = direction;
        translationMode = TranslationMode.Velocity;
    }

    public void SetTargetPosition(Vector2 position)
    {
        targetPosition = position;
        translationMode = TranslationMode.Position;
    }
}
