public static class Factions {
	public enum Faction { Neutral, Player, Enemy };
	public enum Relationship { Neutral, Friendly, Hostile };

	public static Relationship GetRelationship(Faction a, Faction b) {
		if (a == b)
			return Relationship.Friendly;

		if (a == Faction.Neutral || b == Faction.Neutral)
			return Relationship.Neutral;

		return Relationship.Hostile;
	}
}
