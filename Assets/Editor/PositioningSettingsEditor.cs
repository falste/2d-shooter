using UnityEngine;
using UnityEditor;
using AI;

[CustomEditor(typeof(PositioningSettings))]
public class PositioningSettingsEditor : Editor {
	public override void OnInspectorGUI() {
		Utility.EditorUtils.ScriptField(this);

		PositioningSettings data = (PositioningSettings)target;

		EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
		data.gridSize = EditorGUILayout.IntField("Grid Size", data.gridSize);
		data.followPathUpdateTime = EditorGUILayout.FloatField("Follow Path Update Time", data.followPathUpdateTime);
		data.positioningUpdateTime = EditorGUILayout.FloatField("Positioning Update Time", data.positioningUpdateTime);
		data.collisionRecalcTime = EditorGUILayout.FloatField("Collision Recalc Time", data.collisionRecalcTime);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Space", EditorStyles.boldLabel);
		data.sameTileBonus = EditorGUILayout.FloatField("Same Tile Bonus", data.sameTileBonus);
		data.occupiedCost = EditorGUILayout.FloatField("Occupied Cost", data.occupiedCost);
		data.movementCost = EditorGUILayout.FloatField("Movement Cost", data.movementCost);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Distance To Ally", EditorStyles.boldLabel);
		data.idealDistanceToAlly = EditorGUILayout.FloatField("Distance", data.idealDistanceToAlly);
		data.distToAllyBand = EditorGUILayout.FloatField("Band", data.distToAllyBand);
		data.distToAllyPower = EditorGUILayout.IntField("Power", data.distToAllyPower);
		data.distToAllyMultiplier = EditorGUILayout.FloatField("Multiplier", data.distToAllyMultiplier);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Distance To Enemy", EditorStyles.boldLabel);
		data.idealDistanceToEnemy = EditorGUILayout.FloatField("Distance", data.idealDistanceToEnemy);
		data.distToEnemyBand = EditorGUILayout.FloatField("Band", data.distToEnemyBand);
		data.distToEnemyPower = EditorGUILayout.IntField("Power", data.distToEnemyPower);
		data.distToEnemyMultiplier = EditorGUILayout.FloatField("Multiplier", data.distToEnemyMultiplier);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Line Of Sight", EditorStyles.boldLabel);
		data.LOSSitRatThreshold = EditorGUILayout.FloatField("LOS Sit Rat Threshold", data.LOSSitRatThreshold);
		data.LOSMultiplier = EditorGUILayout.FloatField("LOS Multiplier", data.LOSMultiplier);
		data.gradientSitRatThreshold = EditorGUILayout.FloatField("Gradient Sit Rat Threshold", data.gradientSitRatThreshold);
		data.gradientMultiplier = EditorGUILayout.FloatField("Gradient Multiplier", data.gradientMultiplier);
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Noise", EditorStyles.boldLabel);
		data.noiseScaling = EditorGUILayout.FloatField("Noise Scaling", data.noiseScaling);
		data.noiseSpeed = EditorGUILayout.FloatField("Noise Speed", data.noiseSpeed);
		data.noiseMultiplier = EditorGUILayout.FloatField("Noise Multiplier", data.noiseMultiplier);

		// https://gamedev.stackexchange.com/questions/125698/how-to-edit-and-persist-serializable-assets-in-the-editor-window
		if (GUI.changed) {
			// mark the testScriptable object as "dirty" and save it
			EditorUtility.SetDirty(data);
		}
	}
}
 