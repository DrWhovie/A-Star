namespace AStar
{
    using System.Collections.Generic;
    using UnityEngine;

    public class AStarGrid : MonoBehaviour
    {
        public bool _displayGridGizmos;
        public Vector2 _gridWorldSize;
        public float _nodeRadius;
        public TerrainType[] walkableRegions;
        LayerMask _walkableMask;
        Dictionary<int, int> walkableRegionsDict = new Dictionary<int, int>();

        public LayerMask _unwalkabelMask;
        
        Node[,] _grid;
        float _nodeDiameter;
        int _gridSizeX;
        int _gridSizeY;

        void Awake()
        {
            _nodeDiameter = _nodeRadius * 2;
            _gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiameter);
            _gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiameter);

            foreach (TerrainType region in walkableRegions)
            {
                walkableRegionsDict.Add((int)Mathf.Log(region._terrainMask.value, 2), region._penalty);
                _walkableMask += region._terrainMask;
            }
        }

        void Start()
        {  
            CreateGrid();
        }

        public int MaxSize
        {
            get { return _gridSizeX * _gridSizeY;  }
        }

        void CreateGrid()
        {
            _grid = new Node[_gridSizeX, _gridSizeY];
            Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.forward * _gridWorldSize.y / 2;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + _nodeRadius) + Vector3.forward * (y * _nodeDiameter + _nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkabelMask));

                    int movementPenalty = 0;

                    if (walkable)
                    {
                        Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100, _walkableMask))
                        {
                            walkableRegionsDict.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        }

                    }

                    _grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
                }
            }

            BlurPenaltyMap(3);
            BlurPenaltyMap(3);
            BlurPenaltyMap(3);
        }


        void BlurPenaltyMap(int blurSize)
        {
            int kernelSize = blurSize + 2 + 1;
            int kernelExtents = (kernelSize -1) / 2;

            int[,] penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
            int[,] penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

            for (int y = 0; y < _gridSizeY; y++)//row
            {
                for (int x = -kernelExtents; x <= kernelExtents; x++)
                {
                    int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                    penaltiesHorizontalPass[0, y] += _grid[sampleX, y]._movementPenalty;
                }

                for (int x= 0; x < _gridSizeX; x++)
                {
                    int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, _gridSizeX);
                    int addIndex = Mathf.Clamp(x + kernelExtents, 0, _gridSizeX - 1);

                    penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - _grid[removeIndex, y]._movementPenalty + _grid[addIndex, y]._movementPenalty;
                }
            }

            for (int x = 0; x < _gridSizeX; x++)//column
            {
                for (int y = -kernelExtents; y <= kernelExtents; y++)
                {
                    int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                    penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                }

                for (int y = 0; y < _gridSizeY; y++)
                {
                    int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY);
                    int addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY - 1);

                    penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x,y-1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];

                    int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));

                    _grid[x, y]._movementPenalty = blurredPenalty;
                }
            }

        }

        public List<Node> GetNeighbors(Node node)
        {
            if (node._neighbors == null)
            {
                node._neighbors = new List<Node>();
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        int checkX = node._gridX + x;
                        int checkY = node._gridY + y;
                        if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                            node._neighbors.Add(_grid[checkX, checkY]);
                    }
                }
            }

            return node._neighbors;
        }


        public Node NodeFromWorldPoint(Vector3 worldPoint)
        {
            float percentX = (worldPoint.x + _gridWorldSize.x / 2) / _gridWorldSize.x;
            float percentY = (worldPoint.z + _gridWorldSize.y / 2) / _gridWorldSize.y;
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
            int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

            return _grid[x, y];
        }


        void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, 1, _gridWorldSize.y));
            if (_grid != null && _displayGridGizmos)
            {                 
                foreach (var n in _grid)
                {
                    var color = (n._walkable) ? Color.white: Color.red;

                    var K = n._movementPenalty / 100f;
                     
                    color= Color.Lerp(color, Color.cyan, K);

                    Gizmos.color = color;

                    Gizmos.DrawWireCube(n._worldPos, Vector3.one * (_nodeDiameter - .3f));
                }
            }
             
        }

        [System.Serializable]
        public class TerrainType
        {
            public LayerMask _terrainMask;
            public int _penalty;
        }

    }
}