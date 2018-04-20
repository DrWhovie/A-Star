using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;

	public Transform target;
	public float speed = 20;
	public float turnSpeed = 3;
	public float turnDst = 5;
	public float stoppingDst = 10;

	Path path;

	void Start()
    {
        Go();
	}


    void Go()
    {
        var targets = FindObjectsOfType<Target>();
        Target pick = target.GetComponent<Target>();

        while (pick == null || pick.transform == target && targets.Length > 0) 
            pick = targets[Random.Range(0, targets.Length)];

        target = pick.transform;

        StartCoroutine(UpdatePath());
    }

    public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
		if (pathSuccessful) {
			path = new Path(waypoints, transform.position, turnDst, stoppingDst);

			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator UpdatePath()
    {


		if (Time.timeSinceLevelLoad < .3f) {
			yield return new WaitForSeconds (.3f);
		}
		PathRequestManager.RequestPath (transform.position, target.position, OnPathFound);

		float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 targetPosOld = target.position;

		while (true) {
			yield return new WaitForSeconds (minPathUpdateTime);
			if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
				PathRequestManager.RequestPath (transform.position, target.position, OnPathFound);
				targetPosOld = target.position;
			}
		}
	}

	IEnumerator FollowPath() {

		//bool followingPath = true;
		int pathIndex = 1;

        if (path != null && path.lookPoints.Length > 0)
    		transform.LookAt (path.lookPoints [0]);

		float speedPercent = 1;

		while (path != null && path.lookPoints.Length > 0) {
			Vector2 pos2D = new Vector2 (transform.position.x, transform.position.z);
			while (path.turnBoundaries [pathIndex].HasCrossedLine (pos2D)) {
				if (pathIndex == path.finishLineIndex) {
					path = null;
					break;
				} else {
					pathIndex++;
				}
			}

            if (path != null)
            {

                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.1f)
                    {
                        path = null;
                    }
                }
            }

            if (path != null)
            {  
                Quaternion targetRotation = Quaternion.LookRotation (path.lookPoints [pathIndex] - transform.position);
				transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
				transform.Translate (Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
			}

            
			yield return null;

		}

        Go();
        yield break;
    }

	public void OnDrawGizmos() {
		if (path != null) {
			path.DrawWithGizmos();

            for (int i = 1; i < path.lookPoints.Length; i++)
            {
                Gizmos.DrawLine(path.lookPoints[i-1] + Vector3.up, path.lookPoints[i] + Vector3.up);
            }

        }
	}
}
