using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor {
	[SerializeField] bool shotSpawnFoldout = false;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		SerializedProperty script = serializedObject.FindProperty("m_Script");
		GUI.enabled = false;
		EditorGUILayout.PropertyField(script, true, new GUILayoutOption[0]);
		GUI.enabled = true;
		serializedObject.ApplyModifiedProperties();

		WeaponData wd = (WeaponData)target;

		#region General
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
		wd.damage = EditorGUILayout.FloatField("Damage", wd.damage);
		wd.cooldown = Mathf.Max(EditorGUILayout.FloatField("Cooldown", wd.cooldown), 0f);
		wd.equipTime = Mathf.Max(EditorGUILayout.FloatField("Equip Time", wd.equipTime), 0f);
		wd.reloadTime = Mathf.Max(EditorGUILayout.FloatField("Reload Time", wd.reloadTime), 0f);
		wd.magSize = Mathf.Max(EditorGUILayout.IntField("Mag Size", wd.magSize), -1);
		#endregion

		#region Firemodes
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Firemodes", EditorStyles.boldLabel);
		wd.hasSingle = EditorGUILayout.Toggle("Has Single", wd.hasSingle);
		bool anyBurst = EditorGUILayout.Toggle("Has Burst", wd.hasBurst || wd.hasAutoBurst);
		if (anyBurst) {
			EditorGUI.indentLevel++;
			wd.hasAutoBurst = EditorGUILayout.Toggle("Is Automatic", wd.hasAutoBurst);
			wd.hasBurst = !wd.hasAutoBurst;
			wd.burstCount = Mathf.Max(EditorGUILayout.IntField("Burst Count", wd.burstCount), 2);
			wd.afterBurstCooldown = Mathf.Max(EditorGUILayout.FloatField("After Burst Cooldown", wd.afterBurstCooldown), 0f);
			EditorGUI.indentLevel--;
		} else {
			wd.hasBurst = false;
			wd.hasAutoBurst = false;
		}
		wd.hasAuto = EditorGUILayout.Toggle("Has Auto", wd.hasAuto);
		#endregion

		#region Accuracy
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Accuracy", EditorStyles.boldLabel);
		wd.maxOffsetAngle = Mathf.Max(EditorGUILayout.FloatField("Max Offset Angle", wd.maxOffsetAngle), 0f);
		wd.maxRecoilAngle = Mathf.Max(EditorGUILayout.FloatField("Max Recoil Angle", wd.maxRecoilAngle), 0f);
		if (wd.maxRecoilAngle != 0f) {
			wd.recoilDecayTime = Mathf.Max(EditorGUILayout.DelayedFloatField("Recoil Decay Time", wd.recoilDecayTime), 0);
			if (wd.recoilDecayTime < Time.fixedDeltaTime) {
				EditorGUILayout.HelpBox("Recoil Decay Time has to be bigger than Time.fixedDeltaTime! ("+Time.fixedDeltaTime+")", MessageType.Error);
			}
		} else {
			wd.recoilDecayTime = 1f; // Has to be bigger than Time.fixedDeltaTime!
		}
		#endregion

		#region Projectile
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Projectile", EditorStyles.boldLabel);
		wd.speed = Mathf.Max(EditorGUILayout.FloatField("Speed", wd.speed), 0f);
		wd.speedVariation = Mathf.Max(EditorGUILayout.FloatField("Speed Variation", wd.speedVariation), 0f);
		wd.impulseRecoil = EditorGUILayout.FloatField("Impulse Recoil", wd.impulseRecoil);
		wd.impulseHit = EditorGUILayout.FloatField("Impulse Hit", wd.impulseHit);
		wd.inheritVelocityMultiplier = Mathf.Min(Mathf.Max(EditorGUILayout.FloatField("Inherit Velocity Mult", wd.inheritVelocityMultiplier), 0f), 1f);
		wd.prjPrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", wd.prjPrefab, typeof(GameObject), true);
		#endregion

		#region Shotspawns
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Shotspawns", EditorStyles.boldLabel);
		int shotSpawnNulls = 0;
		for (int i = 0; i < wd.shotSpawns.Length; i++) {
			if (wd.shotSpawns[i] == null)
				shotSpawnNulls++;
		}
		string str = "Shotspawns (" + wd.shotSpawns.Length;
		if (shotSpawnNulls != 0) {
			str += ", " + shotSpawnNulls + " undefined";
		}
		str += ")";

		shotSpawnFoldout = EditorGUILayout.Foldout(shotSpawnFoldout, str, true);
		if (shotSpawnFoldout) {
			EditorGUI.indentLevel++;
			int shotSpawnsSize = Mathf.Max(EditorGUILayout.DelayedIntField("Size", wd.shotSpawns.Length), 1);

			if (shotSpawnsSize != wd.shotSpawns.Length) {
				// Resize
				Transform[] temp = wd.shotSpawns;

				wd.shotSpawns = new Transform[shotSpawnsSize];
				for (int i = 0; i < shotSpawnsSize; i++) {
					wd.shotSpawns[i] = temp[i];
				}
			}

			if (wd.shotSpawns != null) {
				for (int i = 0; i < wd.shotSpawns.Length; i++) {
					wd.shotSpawns[i] = (Transform)EditorGUILayout.ObjectField("Element " + i, wd.shotSpawns[i], typeof(Transform), true);
				}
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}
		if (shotSpawnNulls != 0) {
			EditorGUILayout.HelpBox("Undefined shotspawns detected!", MessageType.Error);
		}
		if (wd.shotSpawns.Length > 1)
			wd.shotSpawnsUsage = (WeaponData.ShotSpawnsUsage)EditorGUILayout.EnumPopup("Shot Spawns Usage", wd.shotSpawnsUsage);
		wd.projectilesPerShotSpawn = Mathf.Max(EditorGUILayout.IntField("Prj Per Shotspawn", wd.projectilesPerShotSpawn), 1);
		if (wd.projectilesPerShotSpawn > 1) {
			EditorGUI.indentLevel++;
			wd.projectilesOffsetAngle = Mathf.Max(EditorGUILayout.FloatField("Prj Offset Angle", wd.projectilesOffsetAngle), 0f);
			EditorGUI.indentLevel--;
		} else {
			wd.projectilesOffsetAngle = 0f;
		}
		#endregion

		// https://gamedev.stackexchange.com/questions/125698/how-to-edit-and-persist-serializable-assets-in-the-editor-window
		if (GUI.changed) {
			// mark the testScriptable object as "dirty" and save it
			EditorUtility.SetDirty(wd);
		}
	}
}
