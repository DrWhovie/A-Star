namespace AStar
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Node : IHeapItem<Node> 
    {
        public bool _walkable;
        public Vector3 _worldPos;

        public int _gCost;
        public int _hCost;

        public int _gridX;
        public int _gridY;

        public int _movementPenalty;

        public Node _parent;
        public List<Node> _neighbors;

        int _heapIdx;
         
        public Node(bool walkable, Vector3 worldPos, int gridX, int gridY, int penalty)
        {
            _walkable = walkable;
            _worldPos = worldPos; 
            _gridX = gridX;
            _gridY = gridY;
            _movementPenalty = penalty; 
        }
         
        public int FCost
        {
            get
            {
                return _gCost + _hCost;
                //return  Mathf.RoundToInt(fCost * _weight);
            }
        }

        public int HeapIndex 
        {
            get
            {
                return _heapIdx;  
            }

            set
            {
                _heapIdx = value;  
            }
        }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = FCost.CompareTo(nodeToCompare.FCost);
            if (compare == 0)
                compare = _hCost.CompareTo(nodeToCompare._hCost);

            return -compare; //lower, not higher!!!
         }
    }
}