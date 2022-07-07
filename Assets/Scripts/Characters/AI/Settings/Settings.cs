using UnityEngine;

namespace AI {
    [CreateAssetMenu(fileName = "Settings", menuName = "ScriptableObjects/AI/Settings", order = 2)]
    public class Settings : ScriptableObject {
        public float timeToIdle = 5f;
        public float minIdleWalkDelay = 0f;
        public float maxIdleWalkDelay = 5f;
        public float maxIdleWalkDist = 10f;

        public float checkForEnemiesTime = 0.2f;
        public float dodgeProjectilesTime = 0.1f;

        public float minInvestigateTime = 3f;
        public float maxInvestigateTime = 5f;
        
        public float thresholdVelocity = 0.1f;
        public float sightAngle = 45f;
        public float sightRange = 10f;
        public float shootingAngleTolerance = 5f;
        public float targetPredictionFactor = 1f;
        public int interceptBehindWallIterations = 5;
    }
}