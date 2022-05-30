using UnityEngine;
using UnityEditor;
using AI;

[CustomEditor(typeof(Vision))]
public class VisionEditor : Editor {
	[SerializeField] bool settingsFoldout;
	[SerializeField] bool sitSettingsFoldout;
	[SerializeField] bool sitRatingFoldout;

	[SerializeField] Editor settingsEditor;
	[SerializeField] Editor sitSettingsEditor;

	public override void OnInspectorGUI() {
		Utility.EditorUtils.ScriptField(this);

		Vision vis = (Vision)target;

		sitRatingFoldout = EditorGUILayout.Foldout(sitRatingFoldout, "Situation Rating", true);
		if (sitRatingFoldout) {
			GUI.enabled = false;
			EditorGUI.indentLevel++;
			EditorGUILayout.TextField("Own Health Rating", vis.OwnHealthRating.ToString("#0.00"));
			EditorGUILayout.TextField("Diff Health Rating", vis.DiffHealthRating.ToString("#0.00"));
			EditorGUILayout.TextField("Cooldown Rating", vis.CooldownRating.ToString("#0.00"));
			EditorGUILayout.TextField("Situation Rating", vis.SituationRating.ToString("#0.00"));
			EditorGUI.indentLevel--;
			GUI.enabled = true;
		}

		settingsFoldout = Utility.EditorUtils.ScriptableObjectFoldout(settingsFoldout, ref vis.settings, ref settingsEditor);
		sitSettingsFoldout = Utility.EditorUtils.ScriptableObjectFoldout(sitSettingsFoldout, ref vis.sitSettings, ref sitSettingsEditor);
	}
}