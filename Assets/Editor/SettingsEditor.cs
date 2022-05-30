using UnityEngine;
using UnityEditor;
using AI;

[CustomEditor(typeof(Settings))]
public class SettingsEditor : Editor {
	public override void OnInspectorGUI() {
		Utility.EditorUtils.ScriptField(this);

		Settings data = (Settings)target;

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
		data.checkForEnemiesTime = EditorGUILayout.FloatField("Check For Enemies Time", data.checkForEnemiesTime);
		data.dodgeProjectilesTime = EditorGUILayout.FloatField("Dodge Projectiles Time", data.dodgeProjectilesTime);
		data.thresholdVelocity = EditorGUILayout.FloatField("Theshold Velocity", data.thresholdVelocity);
		data.sightAngle = EditorGUILayout.FloatField("Sight Angle", data.sightAngle);
		data.sightRange = EditorGUILayout.FloatField("Sight Range", data.sightRange);
		EditorGUILayout.Space();

		// Idle Stuff
		data.timeToIdle = EditorGUILayout.FloatField("Time To Chase", data.timeToIdle);
		data.minIdleWalkDelay = EditorGUILayout.FloatField("Min Idle Walk Delay", data.minIdleWalkDelay);
		data.maxIdleWalkDelay = EditorGUILayout.FloatField("Max Idle Walk Delay", data.maxIdleWalkDelay);
		data.maxIdleWalkDist = EditorGUILayout.FloatField("Max Idle Walk Dist", data.maxIdleWalkDist);
		EditorGUILayout.Space();
		// Encounter Stuff
		data.shootingAngleTolerance = EditorGUILayout.FloatField("Shooting Angle Tolerance", data.shootingAngleTolerance);
		data.targetPredictionFactor = EditorGUILayout.FloatField("Target Prediction Factor", data.targetPredictionFactor);
		data.interceptBehindWallIterations = EditorGUILayout.IntField("Intercept Behind Walls Iterations", data.interceptBehindWallIterations);
		EditorGUILayout.Space();
		// Chase Stuff
		EditorGUILayout.Space();
		// Investigate Stuff
		data.minInvestigateTime = EditorGUILayout.FloatField("Min Investigate Time", data.minInvestigateTime);
		data.maxInvestigateTime = EditorGUILayout.FloatField("Max Investigate Time", data.maxInvestigateTime);

		// https://gamedev.stackexchange.com/questions/125698/how-to-edit-and-persist-serializable-assets-in-the-editor-window
		if (GUI.changed) {
			// mark the testScriptable object as "dirty" and save it
			EditorUtility.SetDirty(data);
		}
	}
}