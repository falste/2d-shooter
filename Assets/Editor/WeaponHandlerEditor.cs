using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WeaponHandler))]
public class WeaponHandlerEditor : Editor
{
	[SerializeField] bool weaponsFoldout = true;
	[SerializeField] bool[] weaponsArrayFoldout = new bool[0];

	[SerializeField] Editor[] weaponEditor = new Editor[0];

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		SerializedProperty script = serializedObject.FindProperty("m_Script");
		GUI.enabled = false;
		EditorGUILayout.PropertyField(script, true, new GUILayoutOption[0]);
		serializedObject.ApplyModifiedProperties();

		WeaponHandler wh = (WeaponHandler)target;

		Weapon weapon = wh.SelectedWeapon;
		string selectedWeaponString;
		if (wh.SelectedWeapon != null) {
			selectedWeaponString = wh.SelectedWeapon.data.name;
		} else {
			selectedWeaponString = "None";
		}
		EditorGUILayout.TextField("Selected Weapon", selectedWeaponString);


		int ammoDisp = 0;
		int maxAmmoDisp = 0;
		Weapon.Firemode firemodeDisp = Weapon.Firemode.None;
		if (wh.SelectedWeapon != null) {
			ammoDisp = wh.SelectedWeapon.ammo;
			maxAmmoDisp = wh.SelectedWeapon.data.magSize;
			firemodeDisp = wh.SelectedWeapon.firemode;
		}

		EditorGUILayout.EnumPopup("Firemode", firemodeDisp);
		EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), ammoDisp / (float)maxAmmoDisp, "Ammo: " + ammoDisp + " / " + maxAmmoDisp);
		float curCooldown = Mathf.Max(0f, wh.Cooldown);
		float cdPercent = wh.SelectedWeapon != null ? curCooldown / wh.SelectedWeapon.data.cooldown : 0f;
		EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), cdPercent, "Cooldown: "+ curCooldown.ToString("#0.00") + " s");

		EditorGUILayout.Space();
		GUI.enabled = true;
		wh.connectToGameUI = EditorGUILayout.Toggle("Connect to Game UI", wh.connectToGameUI);

		EditorGUILayout.Space();
		int weaponDataNulls = 0;
		for (int i = 0; i < wh.weaponData.Length; i++) {
			if (wh.weaponData[i] == null)
				weaponDataNulls++;
		}
	
		string weaponsString = "Weapons (" + wh.weaponData.Length;

		if (weaponDataNulls != 0) {
			weaponsString += ", " + weaponDataNulls + " undefined";
		}
		weaponsString += ")";

		weaponsFoldout = EditorGUILayout.Foldout(weaponsFoldout, weaponsString, true);
		if (weaponsFoldout) {
			EditorGUI.indentLevel++;
			int weaponsSize = Mathf.Max(EditorGUILayout.DelayedIntField("Size", wh.weaponData.Length), 0);

			if (weaponsSize != wh.weaponData.Length) {
				// Resize
				WeaponData[] temp = wh.weaponData;

				wh.weaponData = new WeaponData[weaponsSize];
				for (int i = 0; i < Mathf.Min(wh.weaponData.Length,temp.Length); i++) {
					wh.weaponData[i] = temp[i];
				}
			}

			if (weaponsSize != weaponsArrayFoldout.Length) {
				// Resize
				bool[] temp = weaponsArrayFoldout;

				weaponsArrayFoldout = new bool[weaponsSize];
				for (int i = 0; i < Mathf.Min(weaponsArrayFoldout.Length, temp.Length); i++) {
					weaponsArrayFoldout[i] = temp[i];
				}
			}

			if (weaponsSize != weaponEditor.Length) {
				// Resize
				Editor[] temp = weaponEditor;

				weaponEditor = new Editor[weaponsSize];
				for (int i = 0; i < Mathf.Min(weaponEditor.Length, temp.Length); i++) {
					weaponEditor[i] = temp[i];
				}
			}

			if (wh.weaponData != null) {
				for (int i = 0; i < wh.weaponData.Length; i++) {
					//wh.weaponData[i] = (WeaponData)EditorGUILayout.ObjectField("Element " + i, wh.weaponData[i], typeof(WeaponData), true);
					weaponsArrayFoldout[i] = Utility.EditorUtils.ScriptableObjectFoldout(weaponsArrayFoldout[i], ref wh.weaponData[i], ref weaponEditor[i], "Element " + i);
				}
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}
	}
}