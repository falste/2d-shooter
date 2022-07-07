using UnityEngine;

public class Node : IHeapItem<Node>
{

    public bool walkable;
    public Vector2Int gridPosition;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

    public Node() {

    }

    public void SetProperties(Vector2Int gridPosition, int movementPenalty, bool walkable) {
        this.gridPosition = gridPosition;
        this.movementPenalty = movementPenalty;
        this.walkable = walkable;
    }

    public int fCost
    {
        get {
            return gCost + hCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }

    #region IHeapItem Implementation
    public int HeapIndex
    {
        get {
            return heapIndex;
        }
        set {
            heapIndex = value;
        }
    }

    public bool Equals(Node node)
    {
        if (node == null)
            return false;

        return node.gridPosition == gridPosition;
    }
    #endregion
}