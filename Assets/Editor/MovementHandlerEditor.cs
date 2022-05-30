using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovementHandler))]
public class MovementHandlerEditor : Editor {


	public override void OnInspectorGUI() {
		Utility.EditorUtils.ScriptField(this);

		MovementHandler mov = (MovementHandler)target;

		mov.translationType = (MovementHandler.TranslationType)EditorGUILayout.EnumPopup("Translation Type", mov.translationType);
		mov.lockPosition = EditorGUILayout.Toggle("Lock Position", mov.lockPosition);
		switch (mov.translationType) {
			case MovementHandler.TranslationType.Control:
				mov.trans_speed = EditorGUILayout.FloatField("Speed", mov.trans_speed);
				mov.trans_pole = EditorGUILayout.Vector2Field("Poles", mov.trans_pole);
				break;
			case MovementHandler.TranslationType.Velocity:
				mov.trans_speed = EditorGUILayout.FloatField("Speed", mov.trans_speed);
				break;
			default:
				throw new System.NotImplementedException();
		}

		EditorGUILayout.Space();

		mov.rotationType = (MovementHandler.RotationType)EditorGUILayout.EnumPopup("Rotation Type", mov.rotationType);
		mov.lockRotation = EditorGUILayout.Toggle("Lock Rotation", mov.lockRotation);
		switch (mov.rotationType) {
			case MovementHandler.RotationType.Control:
				mov.rot_pole = EditorGUILayout.Vector2Field("Poles", mov.rot_pole);
				break;
			case MovementHandler.RotationType.Instant:
				break;
			default:
				throw new System.NotImplementedException();
		}

		if (GUI.changed) {
			mov.Init();
		}
	}
}