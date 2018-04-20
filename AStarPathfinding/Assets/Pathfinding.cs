namespace AStar
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Diagnostics;
    using System;

    public class Pathfinding : MonoBehaviour
    {
        PathRequestManager _requestManager;
        public AStarGrid _grid;
        static int s_counter = 0;
 
        void Awake()
        {
            if (_grid == null)
                _grid = GetComponent<AStarGrid>();

            _requestManager = GetComponent<PathRequestManager>();
        }

        public void StartFindPath(Vector3 startPos, Vector3 targetPos)
        {
            StartCoroutine(FindPath(startPos, targetPos));
        }


        IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
        {
            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;

            var sw = new Stopwatch();
            sw.Start();

            Node startNode = _grid.NodeFromWorldPoint(startPos);
            Node targetNode = _grid.NodeFromWorldPoint(targetPos);

            if (startNode._walkable && targetNode._walkable)
            {
                Heap<Node> openSet = new Heap<Node>(_grid.MaxSize);
                HashSet<Node> closedSet = new HashSet<Node>();
                Node currentNode = null;

                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    s_counter++;
                    if (s_counter % 2000 == 0)
                    {
                        sw.Stop();
                        yield return new WaitForEndOfFrame();
                        sw.Start();
                    }

                    currentNode = openSet.RemoveFirst();
                    closedSet.Add(currentNode);

                    if (currentNode == targetNode) //path is complete
                    {
                        sw.Stop();
                        //UnityEngine.Debug.Log("Path computed in " + sw.ElapsedMilliseconds + " ms");
                        pathSuccess = true;

                        break;
                    }

                    foreach (var neighbor in _grid.GetNeighbors(currentNode))
                    {
                        if (!neighbor._walkable || closedSet.Contains(neighbor))
                            continue;

                        int newMovementCostToNeighbor = currentNode._gCost + GestDistanceManahattan(currentNode, neighbor) + neighbor._movementPenalty;
                        if (newMovementCostToNeighbor < neighbor._gCost || !openSet.Contains(neighbor))
                        {
                            neighbor._gCost = newMovementCostToNeighbor;
                            neighbor._hCost = GestDistanceManahattan(neighbor, targetNode);
                            neighbor._parent = currentNode;

                            if (!openSet.Contains(neighbor))
                                openSet.Add(neighbor);
                            else
                                openSet.UpdateItem(neighbor);//heap
                        }
                    }
                }
            }

            if (pathSuccess)
                waypoints = RetracePath(startNode, targetNode);

            _requestManager.FinishedProcessingPath(waypoints, pathSuccess);
        }
         
        Vector3[] RetracePath(Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode._parent;
            }

            Vector3[] waypoints = SimplifyPath(path);

            Array.Reverse(waypoints); 
            return waypoints;
        }

        Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;
            for (int i = 1; i < path.Count; i++)
            {
                var from = path[i - 1];
                var to = path[i];
                Vector2 directionNew = new Vector2(from._gridX - to._gridX, from._gridY - to._gridY);
                if (directionNew != directionOld || true) ///////HACK HACK HACK true
                {
                    waypoints.Add(to._worldPos);
                }
                directionOld = directionNew;
            }

            return waypoints.ToArray();
        }

        int GestDistanceManahattan(Node nodeA, Node nodeB)
        {
            int distX = Mathf.Abs(nodeA._gridX - nodeB._gridX);
            int distY = Mathf.Abs(nodeA._gridY - nodeB._gridY);

            if (distX > distY)
                return 14 * distY + 10 * (distX - distY);

            return 14 * distX + 10 * (distY - distX);

        }

    }
}