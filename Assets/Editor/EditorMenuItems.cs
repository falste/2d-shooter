using UnityEngine;
using UnityEditor;
using AI;

// https://forum.unity.com/threads/solved-adding-context-menus-to-game-object.410353/
public class EditorMenuItems {

	[MenuItem("GameObject/_Custom_/AI", false, 0)]
	public static void CreateAI() {
		GameObject go = new GameObject("AI");

		
		go.AddComponent<Rigidbody2D>();
		go.AddComponent<BoxCollider2D>();
		go.AddComponent<Unit>();
		go.AddComponent<WeaponHandler>();
		go.AddComponent<MovementHandler>();

		go.AddComponent<Vision>();
		go.AddComponent<Positioning>();
		go.AddComponent<Shooting>();		
	}
}