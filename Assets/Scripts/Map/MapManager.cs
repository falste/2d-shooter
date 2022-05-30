using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utility;

public class MapManager : MonoBehaviour, IPathfindingGrid
{
	public static MapManager Instance;

	public Vector2Int chunkSize = new Vector2Int(16, 9); // Should be a constant size!

	public float refreshDelay = 0.1f;
	public int loadingRadius = 3;
	public int activationRadius = 3;
	public int deactivationRadius = 4;
	public int unloadingRadius = 4;

	Dictionary<Vector2Int, ChunkBehaviour> loadedChunks;
	Dictionary<Vector2Int, int> roomIds; // Automatically filled over time by loading new chunks

	#region IPathfindingGrid Implementation
	public int MaxNodeAmount
	{
		get {
			return chunkSize.x * chunkSize.y * unloadingRadius * unloadingRadius / 2;
		}
	}

	/// <summary>
	/// Gets the node at the given position if its chunk is currently loaded. Returns null otherwise.
	/// </summary>
	public Node WorldPosToNode(Vector3 worldPos) {
		Vector2Int tilePos = WorldToTilePos(worldPos);
		return NodeFromTilePos(tilePos);
	}

	/// <summary>
	/// Gets the node at the given position if its chunk is currently loaded. Returns null otherwise.
	/// </summary>
	public Node NodeFromTilePos(Vector2Int tilePos) {
		ChunkBehaviour chunkBehaviour = GetChunkFromTilePos(tilePos);
		if (chunkBehaviour == null) {
			return null;
		}

		Vector2Int relTilePos = TileToRelTilePos(tilePos);
		return chunkBehaviour.nodeGrid.nodes[relTilePos.x, relTilePos.y];
	}

	/// <summary>
	/// Returns a list of all loaded Nodes next to the input node.
	/// </summary>
	public List<Node> GetNeighborNodes(Node node) {
		List<Node> nodes = new List<Node>();

		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if (i == 0 && j == 0) // Skip center node
					continue;

				if (i != 0 && j != 0) // Skip diagonals
					continue;

				Node targetNode = NodeFromTilePos(new Vector2Int {
					x = node.gridPosition.x + i,
					y = node.gridPosition.y + j
				});

				if (targetNode == null) {
					continue;
				}


				nodes.Add(targetNode);
			}
		}
		return nodes;
	}

	public Vector3 NodeToWorldPos(Node node) {
		return new Vector3 {
			x = node.gridPosition.x + 0.5f,
			y = node.gridPosition.y + 0.5f,
			z = 0f
		};
	}
	#endregion

	public Vector3 TileToWorldPos(Vector2Int tilePos) {
		return new Vector3 {
			x = tilePos.x + 0.5f,
			y = tilePos.y + 0.5f,
			z = 0.0f
		};
	}

	void Awake() {
		if (Instance != null)
			Debug.LogError("Multiple MapManagers detected!");
		Instance = this;
		loadedChunks = new Dictionary<Vector2Int, ChunkBehaviour>();
		roomIds = new Dictionary<Vector2Int, int>();
		StartCoroutine(DynamicLoading(refreshDelay, loadingRadius, unloadingRadius));
	}

	/// <summary>
	/// Gets the chunk at the given position if it is currently loaded. Returns null otherwise.
	/// </summary>
	ChunkBehaviour GetChunkAt(Vector3 worldPos) {
		return GetChunkAt(WorldToChunkPos(worldPos));
	}

	/// <summary>
	/// Gets the chunk at the given position if it is currently loaded. Returns null otherwise.
	/// </summary>
	ChunkBehaviour GetChunkAt(Vector2Int chunkPos) {
		ChunkBehaviour chunk;
		loadedChunks.TryGetValue(chunkPos, out chunk);
		return chunk;

		/*
		if (loadedChunks.TryGetValue(chunkPos, out chunk) == true) {
			return chunk;
		} else {
			return null;
		}
		*/
	}

	/// <summary>
	/// Gets the chunk at the given position if it is currently loaded. Returns null otherwise.
	/// </summary>
	ChunkBehaviour GetChunkFromTilePos(Vector2Int tilePos) {
		return GetChunkAt(new Vector2Int {
			x = Calc.FlooredDivision(tilePos.x, chunkSize.x),
			y = Calc.FlooredDivision(tilePos.y, chunkSize.y)
		});
	}

	Vector2Int WorldToChunkPos(Vector3 worldPos) {
		return new Vector2Int {
			x = Calc.FlooredDivision((int)worldPos.x, chunkSize.x),
			y = Calc.FlooredDivision((int)worldPos.y, chunkSize.y)
		};
	}

	public Vector2Int WorldToTilePos(Vector3 worldPos) {
		return new Vector2Int {
			x = Mathf.FloorToInt(worldPos.x),
			y = Mathf.FloorToInt(worldPos.y)
		};
	}

	/// <summary>
	/// Maps global tilePos to chunk-relative tilePos
	/// </summary>
	Vector2Int TileToRelTilePos(Vector2Int tilePos)
	{
		return new Vector2Int {
			x = tilePos.x % chunkSize.x,
			y = tilePos.y % chunkSize.y
		};
	}

	int GetRoomId(Vector2Int chunkCoords) {
		int id;
		if(!roomIds.TryGetValue(chunkCoords, out id)) {
			Debug.LogError("RoomID for chunk " + chunkCoords + " not found! Object might be out of bounds!");
			return -1;
		}
		if (id < 0) {
			Debug.LogError("Room with negative ID found. Negative IDs are reserved for errors!");
		}
		return id;
	}

	bool IsSameRoom(Vector2Int a, Vector2Int b) {
		return GetRoomId(a) == GetRoomId(b);
	}

	ChunkBehaviour GetChunkBehaviour(Vector2Int pos) {
		ChunkBehaviour chunk;
		if (!loadedChunks.TryGetValue(pos, out chunk)) {
			Debug.LogError("Trying to get ChunkBehaviour from unloaded chunk!");
		}

		return chunk;
	}

	public Vector2Int GetChunkCoordsFromWorldSpace(Transform player) {
		return new Vector2Int {
			x = Mathf.FloorToInt(player.position.x / chunkSize.x),
			y = Mathf.FloorToInt(player.position.y / chunkSize.y)
		};
	}

	#region Chunk Lifecycle Management
	/// <summary>
	/// Loads and instantiates a chunk at a certain position in chunk space.
	/// </summary>
	void LoadChunkAt(Vector2Int pos) {
		if (loadedChunks.ContainsKey(pos)) {
			Debug.LogWarning("Trying to load already loaded chunk at " + pos + "!");
			return;
		}

		string path = "Chunks/Chunk "+pos;
		GameObject prefab = Resources.Load<GameObject>(path); // TODO: Use Resources.LoadAsync here!
		if (prefab == null) {
			//Debug.LogWarning("Chunk at \""+path+"\" not found!");
			return;
		}

		Vector3 worldPos = new Vector3(pos.x * chunkSize.x, pos.y * chunkSize.y, 0f);
		GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity, transform);
		ChunkBehaviour chunkBehaviour = instance.GetComponent<ChunkBehaviour>();
		if (chunkBehaviour == null) {
			Debug.LogError("Chunk at \"" + path + "\" is missing a ChunkBehaviour component!");
		}
		loadedChunks.Add(pos, instance.GetComponent<ChunkBehaviour>());
		if (!roomIds.ContainsKey(pos)) {
			roomIds.Add(pos, chunkBehaviour.roomID);
		}

		//Debug.Log("Chunk at " + pos + " loaded.");
		chunkBehaviour.chunkPosition = pos;
		chunkBehaviour.OnLoad();
	}

	/// <summary>
	/// Activates a chunk at a certain position in chunk space
	/// </summary>
	void ActivateChunkAt(Vector2Int pos) {
		ChunkBehaviour chunkBehaviour = GetChunkBehaviour(pos);
		if (chunkBehaviour.state == ChunkBehaviour.State.Inactive) {
			chunkBehaviour.OnActivate();
			//Debug.Log("Chunk at " + pos + " activated.");
		} else if (chunkBehaviour.state == ChunkBehaviour.State.Unloaded) {
			Debug.LogError("Chunk state is set to unloaded, but trying to activate it!");
		}
	}

	/// <summary>
	/// Deactivates a chunk at a certain position in chunk space
	/// </summary>
	void DeactivateChunkAt(Vector2Int pos) {
		ChunkBehaviour chunkBehaviour = GetChunkBehaviour(pos);
		if (chunkBehaviour.state == ChunkBehaviour.State.Active) {
			chunkBehaviour.OnDeactivate();
			Debug.Log("Chunk at " + pos + " deactivated.");
		} else if (chunkBehaviour.state == ChunkBehaviour.State.Unloaded) {
			Debug.LogError("Chunk state is set to unloaded, but trying to deactivate it!");
		}
	}

	/// <summary>
	/// Unloads and destroys a chunk at a certain position in chunk space.
	/// </summary>
	void UnloadChunkAt(Vector2Int pos) {
		ChunkBehaviour chunk;
		loadedChunks.TryGetValue(pos, out chunk);

		if (chunk == null) {
			Debug.LogWarning("Trying to unload chunk " + pos + " that is not loaded!");
			return;
		}

		chunk.OnUnload();
		Destroy(chunk.gameObject); // Can we reuse the gameObject instead?
		loadedChunks.Remove(pos);
		Debug.Log("Chunk at " + pos + " unloaded!");
	}

	/// <summary>
	/// Loads all chunks in loadingRadius around pos if they are not loaded already.
	/// </summary>
	void LoadChunksAround(Vector2Int curChunk, int loadingRadius) {
		Vector2Int c = new Vector2Int();

		for (int x = curChunk.x - loadingRadius; x <= curChunk.x + loadingRadius; x++) {
			c.x = x;
			for (int y = curChunk.y - loadingRadius; y <= curChunk.y + loadingRadius; y++) {
				c.y = y;

				if (Utility.Calc.V2IntOneNorm(c - curChunk) > loadingRadius) {
					continue;
				}

				// Ends up checking twice if dictionary contains that key. Good for readability and security, slightly bad for performance.
				if (!loadedChunks.ContainsKey(c)) {
					LoadChunkAt(c);
				}
			}
		}
	}

	/// <summary>
	/// Activates all chunks in activationRadius if they are not activated already and are in the same room.
	/// </summary>
	void ActivateChunksAround(Vector2Int curChunk, int activationRadius) {
		Vector2Int c = new Vector2Int();

		for (int x = curChunk.x - activationRadius; x <= curChunk.x + activationRadius; x++) {
			c.x = x;
			for (int y = curChunk.y - activationRadius; y <= curChunk.y + activationRadius; y++) {
				c.y = y;

				if (Utility.Calc.V2IntOneNorm(c - curChunk) > activationRadius) {
					continue;
				}

				if (!loadedChunks.ContainsKey(c)) {
					//Chunk could not exist due to map boundaries
					//Debug.LogError("Trying to activate unloaded chunk!");
					continue;
				}

				if (IsSameRoom(curChunk, c)){ // Only activate chunks in the same room!
					ActivateChunkAt(c);
				}
			}
		}
	}

	/// <summary>
	/// Deactivates all chunks in deactivationRadius, that are not in current deactivationRadius anymore or are in a different room now.
	/// </summary>
	void DeactivateChunksAround(Vector2Int curChunk, Vector2Int oldChunk, int deactivationRadius) {
		Vector2Int c = new Vector2Int();
		List<Vector2Int> chunkCoordsInDeactivationRange = new List<Vector2Int>();

		// Get all chunks in cur deactivationRadius
		for (int x = curChunk.x - deactivationRadius; x <= curChunk.x + deactivationRadius; x++) {
			c.x = x;
			for (int y = curChunk.y - deactivationRadius; y <= curChunk.y + deactivationRadius; y++) {
				c.y = y;

				if (Utility.Calc.V2IntOneNorm(c - curChunk) > deactivationRadius) {
					continue;
				}

				chunkCoordsInDeactivationRange.Add(c);
			}
		}

		// Get all chunks in old deactivationRadius and unload those, who are not in cur deactivationRadius
		for (int x = oldChunk.x - deactivationRadius; x <= oldChunk.x + deactivationRadius; x++) {
			c.x = x;
			for (int y = oldChunk.y - deactivationRadius; y <= oldChunk.y + deactivationRadius; y++) {
				c.y = y;

				if (Utility.Calc.V2IntOneNorm(c - oldChunk) > deactivationRadius) { // Only check diamond shape
					continue;
				}

				if (!loadedChunks.ContainsKey(c)) { // Skip if room does not exist
					continue;
				}

				if (!IsSameRoom(curChunk, c)) { // Different rooms
					DeactivateChunkAt(c);
					continue;
				}

				if (!chunkCoordsInDeactivationRange.Contains(c) && loadedChunks.ContainsKey(c)) { // Too far away now
					DeactivateChunkAt(c);
					continue;
				}
			}
		}
	}

	/// <summary>
	/// Unloads all chunks from the old unloadingRadius, that are not in the current unloadingRadius anymore, provided they are currently loaded
	/// </summary>
	void UnloadChunksAround(Vector2Int curChunk, Vector2Int oldChunk, int unloadingRadius) {
		Vector2Int c = new Vector2Int();
		List<Vector2Int> chunkCoordsInUnloadingRange = new List<Vector2Int>();

		// Get all chunks in cur unloadingRadius
		for (int x = curChunk.x - unloadingRadius; x <= curChunk.x + unloadingRadius; x++) {
			c.x = x;
			for (int y = curChunk.y - unloadingRadius; y <= curChunk.y + unloadingRadius; y++) {
				c.y = y;

				if (Utility.Calc.V2IntOneNorm(c - curChunk) > unloadingRadius) {
					continue;
				}

				chunkCoordsInUnloadingRange.Add(c);
			}
		}

		// Get all chunks in old unloadingRadius and unload those, who are not in cur unloadingRadius
		for (int x = oldChunk.x - unloadingRadius; x <= oldChunk.x + unloadingRadius; x++) {
			c.x = x;
			for (int y = oldChunk.y - unloadingRadius; y <= oldChunk.y + unloadingRadius; y++) {
				c.y = y;

				if (Utility.Calc.V2IntOneNorm(c - oldChunk) > unloadingRadius) {
					continue;
				}

				if (!chunkCoordsInUnloadingRange.Contains(c) && loadedChunks.ContainsKey(c)) {
					UnloadChunkAt(c);
				}
			}
		}
	}
	#endregion

	IEnumerator DynamicLoading(float resfreshDelay, int loadingRadius, int unloadingRadius) {
		Transform player;
		GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
		if (playerObject == null) {
			Debug.LogWarning("Can't dynamically (un)load map, since no player was found!");
			yield break;
		} else {
			player = playerObject.transform;
		}

		if (deactivationRadius > unloadingRadius) {
			Debug.LogWarning("DeactivationRadius is bigger than unloadingRadius! Setting deactivationRadius to unloadingRadius!");
			deactivationRadius = unloadingRadius;
		}
		if (loadingRadius > unloadingRadius) {
			Debug.LogWarning("UnloadingRange is smaller than loadingRadius! Setting unloadingRange to unloadingRadius.");
			unloadingRadius = loadingRadius;
		}
		if (activationRadius > loadingRadius) {
			Debug.LogWarning("ActivationRadius is bigger than loadingRadius! Setting activationRadius to loadingRadius!");
			activationRadius = loadingRadius;
		}

		Vector2Int curChunk = GetChunkCoordsFromWorldSpace(player);
		Vector2Int oldChunk = curChunk;
		LoadChunksAround(curChunk, loadingRadius);
		ActivateChunksAround(curChunk, activationRadius);

		while (true) {
			if (player == null) {
				Debug.LogError("Can't dynamically load world anymore, since player was destroyed!");
				yield break;
			}

			curChunk = GetChunkCoordsFromWorldSpace(player);
			if (curChunk != oldChunk) {
				DeactivateChunksAround(curChunk, oldChunk, deactivationRadius);
				UnloadChunksAround(curChunk, oldChunk, unloadingRadius);
				LoadChunksAround(curChunk, loadingRadius);
				ActivateChunksAround(curChunk, activationRadius);
			}
			oldChunk = curChunk;
			yield return new WaitForSeconds(refreshDelay);
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.black;
		Vector3 p1 = Vector3.zero;
		Vector3 p2 = new Vector3(chunkSize.x, 0, 0);
		Vector3 p3 = new Vector3(chunkSize.x, chunkSize.y, 0);
		Vector3 p4 = new Vector3(0, chunkSize.y, 0);
		Utility.General.DrawRectGizmo(p1, p2, p3, p4);
	}
}