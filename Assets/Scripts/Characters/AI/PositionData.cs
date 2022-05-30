using System.Collections.Generic;

namespace AI {
	public struct PositionData {
		public float situationRating;
		public float cooldown;
		public List<UnitInfo> allies;
		public List<UnitInfo> enemies;
	}
}
