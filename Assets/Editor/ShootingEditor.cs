using UnityEngine;
using AI;
using UnityEditor;

[CustomEditor(typeof(Shooting))]
public class ShootingEditor : Editor {
	[SerializeField] bool foldout;

	[SerializeField] Editor settingsEditor;

	public override void OnInspectorGUI() {
		Utility.EditorUtils.ScriptField(this);

		Shooting shoot = (Shooting)target;

		foldout = Utility.EditorUtils.ScriptableObjectFoldout(foldout, ref shoot.settings, ref settingsEditor);
	}
}
