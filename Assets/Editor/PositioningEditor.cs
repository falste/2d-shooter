using UnityEngine;
using UnityEditor;
using AI;

[CustomEditor(typeof(Positioning))]
public class PositioningEditor : Editor {
	[SerializeField] bool debugFoldout;
	[SerializeField] bool foldout;

	[SerializeField] Editor settingsEditor;

	public override void OnInspectorGUI() {
		Utility.EditorUtils.ScriptField(this);

		Positioning pos = (Positioning)target;

		GUI.enabled = false;
		EditorGUILayout.TextField("Noise Offset", pos.noiseOffset.ToString("#0"));
		GUI.enabled = true;

		foldout = Utility.EditorUtils.ScriptableObjectFoldout(foldout, ref pos.settings, ref settingsEditor);
		debugFoldout = Utility.EditorUtils.StaticClassFoldout(debugFoldout, typeof(Positioning.DebugOptions));

		/*
		debugFoldout = EditorGUILayout.Foldout(debugFoldout, "Debug", true);
		if (debugFoldout) {
			EditorGUI.indentLevel++;
			EditorGUILayout.LabelField("These variables are static.", EditorStyles.boldLabel);
			EditorGUILayout.Space();
			Positioning.useCooldownRating = EditorGUILayout.Toggle("Use Cooldown Rating", Positioning.useCooldownRating);
			Positioning.useSituationRating = EditorGUILayout.Toggle("Use Situation Rating", Positioning.useSituationRating);
			EditorGUILayout.Space();
			Positioning.useSameTileBonus = EditorGUILayout.Toggle("Use Same Tile Bonus", Positioning.useSameTileBonus);
			Positioning.useMovementCost = EditorGUILayout.Toggle("Use Movement Cost", Positioning.useMovementCost);
			Positioning.useDistToAlly = EditorGUILayout.Toggle("Use Dist To Ally", Positioning.useDistToAlly);
			Positioning.useDistToEnemy = EditorGUILayout.Toggle("Use Dist To Enemy", Positioning.useDistToEnemy);
			Positioning.useGradient = EditorGUILayout.Toggle("Use Gradient", Positioning.useGradient);
			Positioning.useLOS = EditorGUILayout.Toggle("Use LOS", Positioning.useLOS);
			Positioning.useNoise = EditorGUILayout.Toggle("Use Noise", Positioning.useNoise);
			Positioning.useOccupiedCost = EditorGUILayout.Toggle("Use Occupied Cost", Positioning.useOccupiedCost);
			EditorGUI.indentLevel--;
		}
		*/
	}
}