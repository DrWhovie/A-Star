namespace AStar
{
    using System.Collections;
    using UnityEngine;

    public class Unit : MonoBehaviour
    {
        float _speed = 10;
        Vector3[] _path;
        int _targetIndex;
        Color _myColor;
        Target _target;

        Material _mat = null;
        void Start()
        {
            _speed = Random.Range(5, 15);

            var brightness = _speed / 15f;
            transform.localScale = Vector3.one * (2 - brightness);

            _myColor = Random.ColorHSV(0, 1, .5f, 1, brightness, brightness);
            _mat = GetComponent<Renderer>().material;
             Go( true );
        }

        void Go( bool first = false)
        {
            //pick a target

            _target = Target.AllTargets[Random.Range(0, Target.AllTargets.Count)];

            _mat.color = Color.white;

            if (_target != null)
                PathRequestManager.RequestPath(transform.position, _target.transform.position, OnPathFound);
        }

        private void OnPathFound(Vector3[] newPath, bool successful)
        {
            if (successful)
            {
                _mat.color = Color.cyan;
                _path = newPath;
                StopCoroutine("FollowPath");
                StartCoroutine("FollowPath");
            }
            else
            {
                _mat.color = Color.yellow;
            }
        }

        IEnumerator FollowPath()
        {
            //var rb = GetComponent<Rigidbody>();

            _targetIndex = 0;

            _mat.color = Color.magenta;

            if (_path.Length == 0)
            {
                Go();
                yield break;
            }

            _mat.color = _myColor;  

            Vector3 currentWaypoint = _path[0];

            Vector3 mov = Vector3.zero;

            while (true)
            {
                if ((transform.position - currentWaypoint).sqrMagnitude <= Mathf.Epsilon)//close enough
                {
                    _targetIndex++;

                    if (_targetIndex >= _path.Length)
                    {
                        transform.position=(_target.transform.position);
                        _path = null;
                        break;
                    }

                    currentWaypoint = _path[_targetIndex];
                }

                mov = Vector3.MoveTowards(transform.position, currentWaypoint, _speed * Time.deltaTime);
                transform.position=(mov);

                yield return new WaitForEndOfFrame();
            }

            Go();

            yield break;

        }
 
        public void OnDrawGizmos()
        {

            if (_path != null)
            {
                if (_mat != null)
                    Gizmos.color = _mat.color;


                for (int i = _targetIndex; i < _path.Length; i++)
                {
                    Gizmos.DrawCube(_path[i], Vector3.one);

                    if (i == _targetIndex)
                        Gizmos.DrawLine(transform.position, _path[i]);
                    else
                        Gizmos.DrawLine(_path[i - 1], _path[i]);
                }
            }
        }


    }
}