using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    [CreateAssetMenu(fileName = "ShootingSettings", menuName = "ScriptableObjects/AI/ShootingSettings", order = 1)]
    public class ShootingSettings : ScriptableObject {
        public float targetPredictionFactor = 1f;
        public int interceptBehindWallIterations = 5;
        public float shootingAngleTolerance = 3f;
        public float forgetEnemyTime = 5f;
    }
}