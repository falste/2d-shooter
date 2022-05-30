using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "VisionSettings", menuName = "ScriptableObjects/AI/VisionSettings", order = 1)]
	public class VisionSettings : ScriptableObject {
		public float updateRate = 0.2f;

		public float sightRange = 10f;
		public float sightAngle = 90f;

		public bool ignoreRange = false;
		public bool ignoreAngle = false;
		public bool ignoreWalls = false;
	}
}