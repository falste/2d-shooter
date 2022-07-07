using UnityEngine;

public interface ITranslationHandler {
    void SetTargetDirection(Vector2 direction);
    void SetTargetPosition(Vector2 position);
    void FixedUpdate();
}

public interface IRotationHandler {
    void SetTargetRotation(float rotationRad);
    void SetTargetRotation(Vector2 direction);
    void FixedUpdate();
}
