namespace AStar
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Target : MonoBehaviour
    {
        public static List<Target> AllTargets = new List<Target>();

        void Awake()
        {
            if (!AllTargets.Contains(this))
                AllTargets.Add(this);

            var kills = new List<Target>();
            foreach (var target in AllTargets)
                if (target == null)
                    kills.Add(target);

            foreach (var kill in kills)
                AllTargets.Remove(kill);

        }
    }
}