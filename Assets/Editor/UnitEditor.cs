using UnityEditor;

[CustomEditor(typeof(Unit))]
public class UnitEditor : Editor {
	public override void OnInspectorGUI() {
		Utility.EditorUtils.ScriptField(this);

		Unit unit = (Unit)target;

		unit.Faction = (Factions.Faction)EditorGUILayout.EnumPopup("Faction", unit.Faction);
		unit.knockable = EditorGUILayout.Toggle("Knockable", unit.knockable);
		unit.invincible = EditorGUILayout.Toggle("Invincible", unit.invincible);
		unit.Health = EditorGUILayout.FloatField("Health", unit.Health);
		unit.MaxHealth = EditorGUILayout.FloatField("Max Health", unit.MaxHealth);
		EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), unit.Health / unit.MaxHealth, unit.Health + " / " + unit.MaxHealth);
	}
}
