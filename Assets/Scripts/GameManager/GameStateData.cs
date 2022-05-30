using UnityEngine;
using System.IO;

public class GameStateData
{
	/*
	 * INCREMENT GAMEDATAVERSION WHENEVER THIS FILE IS CHANGED!
	 * ADD UPGRADE MECHANISM!
	 */

	public int gameDataVersion;
	public Vector2Int playerPosition;
	public Weapon[] weapons;
	public PlayerData player;

	// New game
	public GameStateData()
	{
		gameDataVersion = 1;
		player = new PlayerData();
		playerPosition = new Vector2Int(2, 2);
		weapons = new Weapon[] {

		};
	}

	static string filePath = "/StreamingAssets/";
	public static GameStateData LoadFromFile(string fileName)
	{
		GameStateData gameData;
		string filePath = Application.dataPath + GameStateData.filePath + fileName;

		if (File.Exists(filePath)) {
			string dataAsJson = File.ReadAllText(filePath);
			gameData = JsonUtility.FromJson<GameStateData>(dataAsJson);
		} else {
			gameData = new GameStateData();
			gameData.SaveToFile(fileName);
		}

		return gameData;
	}

	public void SaveToFile(string fileName)
	{
		string dataAsJson = JsonUtility.ToJson(this);
		string filePath = Application.dataPath + GameStateData.filePath + fileName;

		File.WriteAllText(filePath, dataAsJson);
	}
}