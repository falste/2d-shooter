using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    [CreateAssetMenu(fileName = "PositioningSettings", menuName = "ScriptableObjects/AI/PositioningSettings", order = 1)]
    public class PositioningSettings : ScriptableObject {
        public int gridSize = 11;

        public float followPathUpdateTime = 0.1f;
        public float positioningUpdateTime = 0.2f;
        public float collisionRecalcTime = 0.2f;

        public float idealDistanceToEnemy = 4f;
        public float idealDistanceToAlly = 4f;
        public float distToAllyMultiplier = 15f;
        public float distToEnemyMultiplier = 15f;
        public float distToAllyBand = 4f;
        public float distToEnemyBand = 4f;
        public int distToEnemyPower = 1;
        public int distToAllyPower = 1;

        public float movementCost = 1f;
        public float sameTileBonus = 0f;
        public float occupiedCost = 10f;
        
        public float LOSMultiplier = 10f;
        public float LOSSitRatThreshold = -0.3f;
        public float gradientMultiplier = 10f;
        public float gradientSitRatThreshold = -0.5f;
        public float noiseMultiplier = 10f;
        public float noiseScaling = 0.3f;
        public float noiseSpeed = 0.3f;
    }
}