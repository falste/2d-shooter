using System;
using System.Collections.Generic;
using UnityEngine;


// License!
// https://github.com/SebLague/Pathfinding
// https://www.youtube.com/watch?v=mZfyt03LDH4&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=3

[RequireComponent(typeof(PathRequestManager))]
public class Pathfinding : MonoBehaviour
{
    public void FindPath(PathRequest request, Action<PathResult> callback) {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = MapManager.Instance.WorldPosToNode(request.pathStart);
        //Debug.Log("StartPos: " + startNode.gridPosition);
        Node targetNode = MapManager.Instance.WorldPosToNode(request.pathEnd);

        if (startNode != null && targetNode != null) {
            startNode.parent = startNode;

            //Debug.Log("Grid positions: " + startNode.gridPosition + ", " + targetNode.gridPosition);

            if (startNode.walkable && targetNode.walkable) {
                Heap<Node> openSet = new Heap<Node>(MapManager.Instance.MaxNodeAmount);
                HashSet<Node> closedSet = new HashSet<Node>();
                openSet.Add(startNode);

                while (openSet.Count > 0) {
                    Node currentNode = openSet.RemoveFirst();
                    closedSet.Add(currentNode);

                    if (currentNode.gridPosition == targetNode.gridPosition) {
                        targetNode.parent = currentNode.parent;
                        pathSuccess = true;
                        break;
                    }

                    foreach (Node neighbor in MapManager.Instance.GetNeighborNodes(currentNode)) {
                        if (!neighbor.walkable || closedSet.Contains(neighbor)) {
                            continue;
                        }


                        int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;
                        if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                            neighbor.gCost = newMovementCostToNeighbor;
                            neighbor.hCost = GetDistance(neighbor, targetNode);
                            neighbor.parent = currentNode;

                            if (!openSet.Contains(neighbor)) {
                                //Debug.Log(neighbor.gridPosition);
                                openSet.Add(neighbor);
                            } else {
                                openSet.UpdateItem(neighbor);
                            }
                        }
                    }
                }
            }
        }

        if (pathSuccess) {
            waypoints = RetracePath(startNode, targetNode);
            //Debug.Log("Path length: " + waypoints.Length);
        }
        callback(new PathResult(waypoints, pathSuccess, request.callback));
    }

    Vector3[] RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        path.Add(currentNode);

        while (currentNode.gridPosition != startNode.gridPosition) {
            currentNode = currentNode.parent;
            path.Add(currentNode);
        }

        // Reimplement this? Not really needed, is it?
        //Vector3[] waypoints = SimplifyPath(path);
        List<Vector3> wp = new List<Vector3>();
        for (int i = 0; i < path.Count; i++) {
            wp.Add(MapManager.Instance.NodeToWorldPos(path[i]));
        }
        Vector3[] waypoints = wp.ToArray();


        Array.Reverse(waypoints);
        return waypoints;
    }

    int GetDistance(Node nodeA, Node nodeB) {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
