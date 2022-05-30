using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public interface ILoadable
{
	void OnLoad();
	void OnActivate();
	void OnDeactivate();
	void OnUnload();
}

[RequireComponent(typeof(Grid))]
public class ChunkBehaviour : MonoBehaviour{
	public enum State { Unloaded, Inactive, Active };
	[ReadOnly]
	public State state = State.Unloaded;
	public int roomID;
	public NodeGrid nodeGrid;
	[HideInInspector]public Vector2Int chunkPosition;

	protected void SetNodeGrid() {
		nodeGrid = NodeGrid.pool.GetInstance();

		Vector2Int gridPosition;
		int movementPenalty;
		bool walkable;

		for (int x = 0; x < nodeGrid.nodes.GetLength(0); x++) {
			for (int y = 0; y < nodeGrid.nodes.GetLength(1); y++) {
				// Set position
				gridPosition = chunkPosition;
				gridPosition.x *= nodeGrid.nodes.GetLength(0);
				gridPosition.y *= nodeGrid.nodes.GetLength(1);
				gridPosition += new Vector2Int() {
					x = x,
					y = y
				};

				// Set movement penalty
				movementPenalty = 0;

				// Set walkable
				walkable = true;
				for (int i = 0; i < transform.childCount; i++) {
					Tilemap tm = transform.GetChild(i).GetComponent<Tilemap>();
					Tile.ColliderType ct = tm.GetColliderType(new Vector3Int(x, y, 0));
					if (ct == Tile.ColliderType.None || tm.GetComponent<CompositeCollider2D>() == null) {
						continue;
					} else {
						walkable = false;
						break;
					}
				}

				// Apply
				nodeGrid.nodes[x, y].SetProperties(gridPosition, movementPenalty, walkable);
			}
		}
	}

	virtual public void OnLoad() {
		if (state == State.Inactive) {
			Debug.LogError("Trying to load already loaded chunk!");
			return;
		}

		SetNodeGrid();

		foreach (ILoadable l in GetLoadables()) {
			l.OnLoad();
		}
		state = State.Inactive;
	}

	virtual public void OnActivate() {
		if (state == State.Unloaded) {
			Debug.LogError("Trying to activate Chunk before loading it!");
			OnLoad();
		} else if(state == State.Active) {
			Debug.LogError("Trying to activate already activated Chunk!");
			return;
		}

		foreach (ILoadable l in GetLoadables()) {
			l.OnActivate();
		}
		state = State.Active;
	}

	virtual public void OnDeactivate() {
		if (state == State.Inactive) {
			Debug.LogError("Trying to deactivate deactivated chunk!");
			return;
		}

		foreach (ILoadable l in GetLoadables()) {
			l.OnDeactivate();
		}
		state = State.Inactive;
	}

	virtual public void OnUnload() {
		if (state == State.Active) {
			OnDeactivate();
		} else if(state == State.Unloaded) {
			Debug.LogError("Trying to unload already unloaded chunk!");
			return;
		}

		NodeGrid.pool.ReleaseInstance(nodeGrid);

		foreach (ILoadable l in GetLoadables()) {
			l.OnUnload();
		}
		state = State.Unloaded;
	}

	ILoadable[] GetLoadables() {
		return transform.GetComponentsInChildren<ILoadable>(includeInactive: true);
	}
}
