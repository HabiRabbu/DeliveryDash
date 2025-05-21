using System.Collections.Generic;
using EasyRoads3Dv3;
using UnityEngine;

namespace Harvey.RoadSystem
{
    public enum LaneSide { Left, Right }

    [System.Serializable]
    public class LanePoints
    {
        public List<Vector3> points = new List<Vector3>();
        public LaneSide side;
    }

    /// <summary>
    /// Generates offset waypoint lists for each lane on every EasyRoads3D road.
    /// Attach to an empty GameObject, then use the context menu or call Generate().
    /// </summary>
    [ExecuteInEditMode]
    public class LaneWaypointGenerator : MonoBehaviour
    {
        [Tooltip("How many lanes in each direction (e.g. 1 = one left, one right).")]
        public int lanesPerDirection = 1;

        [Tooltip("If true, draws the generated waypoints in the Scene view.")]
        public bool drawGizmos = true;

        [Tooltip("Radius of the gizmo spheres for each waypoint.")]
        public float gizmoSphereRadius = 0.2f;

        /// <summary>
        /// Holds each lane's list of world-space waypoints plus its side.
        /// </summary>
        public List<LanePoints> generatedLanes = new List<LanePoints>();

        [ContextMenu("Generate Lane Waypoints")]
        public void Generate()
        {
            generatedLanes.Clear();
            // Grab the EasyRoads3D network in the scene
            ERRoadNetwork erNet = new ERRoadNetwork();

            foreach (ERRoad road in erNet.GetRoadObjects())
            {
                // Sample centerline
                Vector3[] centerPoints = road.GetSplinePointsCenter();
                float totalWidth = road.GetWidth();
                float laneWidth = totalWidth / (lanesPerDirection * 2f);

                // sign = +1 for right lanes, -1 for left lanes
                for (int sign = 1; sign >= -1; sign -= 2)
                {
                    LaneSide side = (sign > 0) ? LaneSide.Right : LaneSide.Left;

                    for (int laneIdx = 0; laneIdx < lanesPerDirection; laneIdx++)
                    {
                        float offsetFromCenter = sign * (laneIdx + 0.5f) * laneWidth;
                        var waypoints = new List<Vector3>();

                        // Offset each segment point
                        for (int i = 0; i < centerPoints.Length - 1; i++)
                        {
                            Vector3 A = centerPoints[i];
                            Vector3 B = centerPoints[i + 1];
                            Vector3 forward = (B - A).normalized;
                            Vector3 perp = Vector3.Cross(Vector3.up, forward).normalized;
                            waypoints.Add(A + perp * offsetFromCenter);
                        }

                        // Reverse right-side lanes so that their waypoint order flows opposite the centerline
                        if (side == LaneSide.Right)
                        {
                            waypoints.Reverse();
                        }

                        generatedLanes.Add(new LanePoints
                        {
                            points = waypoints,
                            side = side
                        });
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!drawGizmos || generatedLanes == null) return;

            foreach (var lane in generatedLanes)
            {
                Gizmos.color = (lane.side == LaneSide.Right) ? Color.blue : Color.yellow;
                // Draw lines
                for (int i = 0; i < lane.points.Count - 1; i++)
                {
                    Gizmos.DrawLine(lane.points[i], lane.points[i + 1]);
                }
                // Draw spheres at each waypoint
                foreach (var pt in lane.points)
                {
                    Gizmos.DrawSphere(pt, gizmoSphereRadius);
                }
            }
        }
    }
}