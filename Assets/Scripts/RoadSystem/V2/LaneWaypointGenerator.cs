using System;
using System.Collections.Generic;
using EasyRoads3Dv3;
using UnityEngine;

namespace Harvey.RoadSystem
{
    public enum LaneSide { Left, Right }

    [Serializable]
    public class LanePoints
    {
        public List<Vector3> points = new List<Vector3>();
        public LaneSide side;
        public ERRoad road;
        public int startMarkerIdx;
        public int endMarkerIdx;
        public float offsetFromCenter;
    }

    [ExecuteInEditMode]
    public class LaneWaypointGenerator : MonoBehaviour
    {
        [Tooltip("How many lanes in each direction (1 = single left + right)")]
        public int lanesPerDirection = 1;
        [Tooltip("Draw lines & spheres in the Scene view")]
        public bool drawGizmos = true;
        [Tooltip("Radius of each gizmo sphere at a waypoint")]
        public float gizmoSphereRadius = 0.2f;

        [Header("Output Data")]
        public List<LanePoints> generatedLanes = new List<LanePoints>();

        [ContextMenu("Generate Lane Waypoints")]
        public void Generate()
        {
            generatedLanes.Clear();

            var net = new ERRoadNetwork();
            foreach (var road in net.GetRoadObjects())
            {
                var centerPts = road.GetSplinePointsCenter();
                var markers = road.GetMarkerPositions();
                int markerCount = markers.Length;
                float totalWidth = road.GetWidth();
                float laneWidth = totalWidth / (lanesPerDirection * 2f);

                // create lanes on left and right
                foreach (int sign in new[] { 1, -1 })
                {
                    var side = sign > 0 ? LaneSide.Right : LaneSide.Left;
                    for (int laneIndex = 0; laneIndex < lanesPerDirection; laneIndex++)
                    {
                        float offset = sign * (laneIndex + 0.5f) * laneWidth;
                        var pts = new List<Vector3>();

                        // sample offset points along the road spline
                        for (int i = 0; i < centerPts.Length; i++)
                        {
                            Vector3 P = centerPts[i];
                            Vector3 forward = (i < centerPts.Length - 1)
                                ? (centerPts[i + 1] - P).normalized
                                : (P - centerPts[i - 1]).normalized;
                            Vector3 perpendicular = Vector3.Cross(Vector3.up, forward).normalized;
                            pts.Add(P + perpendicular * offset);
                        }

                        // ensure lanes flow from start to end
                        if (side == LaneSide.Right)
                            pts.Reverse();

                        int startMarkerIdx = side == LaneSide.Right ? markerCount - 1 : 0;
                        int endMarkerIdx = side == LaneSide.Right ? 0 : markerCount - 1;

                        generatedLanes.Add(new LanePoints
                        {
                            points = pts,
                            side = side,
                            road = road,
                            startMarkerIdx = startMarkerIdx,
                            endMarkerIdx = endMarkerIdx,
                            offsetFromCenter = offset
                        });
                    }
                }
            }

            Debug.Log($"[LaneWaypointGenerator] Generated {generatedLanes.Count} lanes");
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || generatedLanes == null) return;

            foreach (var lane in generatedLanes)
            {
                Gizmos.color = lane.side == LaneSide.Right ? Color.blue : Color.yellow;
                for (int i = 0; i < lane.points.Count - 1; i++)
                    Gizmos.DrawLine(lane.points[i], lane.points[i + 1]);
                foreach (var p in lane.points)
                    Gizmos.DrawSphere(p, gizmoSphereRadius);
            }
        }
    }
}