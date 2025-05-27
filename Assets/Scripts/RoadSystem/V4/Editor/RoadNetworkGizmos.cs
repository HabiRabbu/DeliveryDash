using UnityEngine;
using System.Collections.Generic;

namespace Harvey.RoadSystem.v4
{
    [ExecuteInEditMode]
    public class RoadNetworkGizmos : MonoBehaviour
    {
        [SerializeField] private RoadNetworkBuilder roadBuilder;
        [SerializeField] private float sphereRadius = 0.2f;
        [SerializeField] private bool drawGizmos = true;

        void OnDrawGizmos()
        {
            if (!drawGizmos || roadBuilder == null || roadBuilder.allRoads == null) return;

            Gizmos.color = Color.red;

            foreach (var road in roadBuilder.allRoads)
            {
                DrawLane(road.leftLane, Color.cyan);
                DrawLane(road.rightLane, Color.magenta);
            }
        }

        private void DrawLane(List<Waypoint> lane, Color color)
        {
            Gizmos.color = color;

            for (int i = 0; i < lane.Count; i++)
            {
                var wp = lane[i];
                Gizmos.DrawSphere(wp.position, sphereRadius);

                if (i > 0)
                {
                    Gizmos.DrawLine(lane[i - 1].position, wp.position);
                }
            }
        }
    }
}
