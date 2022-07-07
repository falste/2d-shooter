using System.Collections.Generic;
using UnityEngine;

public interface IPathfindingGrid
{
    int MaxNodeAmount { get; } // Maximum amount of nodes in the heap. For a constrained, rectangular grid area, this can be grid.size.x * grid.size.y. All grids should be contained!
    Node WorldPosToNode(Vector3 worldPos);
    Vector3 NodeToWorldPos(Node node);
    List<Node> GetNeighborNodes(Node node);
}