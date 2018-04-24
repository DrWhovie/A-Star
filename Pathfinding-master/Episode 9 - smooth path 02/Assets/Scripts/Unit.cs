using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;

	public Target target;
	public float speed = 20;
	public float turnSpeed = 3;
	public float turnDst = 5;
	public float stoppingDst = 10;

	Path path;

    Material mat;

	void Start()
    {
        var rens = GetComponentsInChildren<MeshRenderer>();
        if (mat == null)
        {
            mat = new Material(rens[0].sharedMaterial);
            mat.color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
        }

        foreach (var ren in rens)
            if (ren.sharedMaterial.name.Contains("Chassis"))
                ren.sharedMaterial = mat;

        speed = Random.Range(10f, 20f);

        var targets = FindObjectsOfType<Target>();
        target = targets[Random.Range(0, targets.Length)];

        for (int i = 0; i < 20; i++)
        {
            var prev = target;

            while (prev == target)
                target = targets[Random.Range(0, targets.Length)];

            randoTargets.Add(target);
        }

        Go();
	}

    

    List<Target> randoTargets = new List<Target>();

    void Go()
    {

        target = randoTargets[Mathf.RoundToInt(Time.time) % randoTargets.Count];
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

        Vector3 targetPos = target.transform.position;

		if (Time.timeSinceLevelLoad < .3f) {
			yield return new WaitForSeconds (.3f);
		}
		PathRequestManager.RequestPath (transform.position, targetPos, OnPathFound);

		float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 targetPosOld = targetPos;

		while (true) {
			yield return new WaitForSeconds (minPathUpdateTime);
			if ((targetPos - targetPosOld).sqrMagnitude > sqrMoveThreshold) {
				PathRequestManager.RequestPath (transform.position, targetPos, OnPathFound);
				targetPosOld = targetPos;
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
			while (pathIndex < path.turnBoundaries.Length && path.turnBoundaries [pathIndex].HasCrossedLine (pos2D)) {
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

            if (path != null && pathIndex < path.lookPoints.Length )
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
