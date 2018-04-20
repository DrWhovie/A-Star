namespace AStar
{
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    public class PathRequestManager : MonoBehaviour
    {
        Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        PathRequest currentPathRequest;

        static PathRequestManager instance;
        public Pathfinding _pathfinding;
        bool isProcessingPath;

        void Awake()
        {
            instance = this;
        }

        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
        {
            var newRequest = new PathRequest(pathStart, pathEnd, callback);
            instance.pathRequestQueue.Enqueue(newRequest);
            instance.TryProcessNext();

        }
         
        void TryProcessNext()
        {
            if (!isProcessingPath && pathRequestQueue.Count > 0)
            {
                currentPathRequest = pathRequestQueue.Dequeue();
                isProcessingPath = true;
                _pathfinding.StartFindPath(currentPathRequest._pathStart, currentPathRequest._pathEnd);
            }

        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
        {
            currentPathRequest._callback(path, success);
            isProcessingPath = false;
            TryProcessNext();
        }

        struct PathRequest
        {
            public Vector3 _pathStart;
            public Vector3 _pathEnd;
            public Action<Vector3[], bool> _callback;
            public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
            {
                _pathStart = start;
                _pathEnd = end;
                _callback = callback;
            }
        }
    }
}