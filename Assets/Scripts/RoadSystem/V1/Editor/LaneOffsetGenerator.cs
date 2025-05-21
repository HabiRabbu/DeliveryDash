using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LaneOffsetGenerator : EditorWindow {
    RoadNetworkSO networkSO;
    const int lanesPerDirection = 1; // tweak to e.g. 2 for two lanes each way

    [MenuItem("Tools/Roads/Generate Lanes")]
    static void Show() => GetWindow<LaneOffsetGenerator>("Generate Lanes");

    void OnGUI() {
        networkSO = (RoadNetworkSO)EditorGUILayout.ObjectField(
            "Network SO", networkSO, typeof(RoadNetworkSO), false);
        if (GUILayout.Button("Build Left/Right Lanes") && networkSO != null)
            BuildLanes();
    }

    void BuildLanes() {
        var oldCenterLanes = new List<RoadLane>(networkSO.lanes);
        networkSO.lanes.Clear();
        networkSO.centerLaneToSideLaneIndex = new Dictionary<(int, LaneSide), int>();

        // For each center-lane, generate left & right
        for (int i = 0; i < oldCenterLanes.Count; i++) {
            var cl = oldCenterLanes[i];
            Vector3 A = networkSO.nodes[cl.startNodeIndex].position;
            Vector3 B = networkSO.nodes[cl.endNodeIndex].position;
            Vector3 dir = (B - A).normalized;
            Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;

            for (int d = 0; d < 2; d++) {
                LaneSide side = (d == 0 ? LaneSide.Left : LaneSide.Right);
                float offsetMul = (d == 0 ? +1 : -1);
                // compute offset points along the center-spline
                var centerSpline = SampleCenterSpline(A, B);
                var lanePoints = new List<Vector3>(centerSpline.Length);
                float halfWidth = cl.width * 0.5f;
                for (int p = 0; p < centerSpline.Length; p++) {
                    lanePoints.Add(centerSpline[p] + perp * halfWidth * offsetMul);
                }

                var lane = new RoadLane {
                    startNodeIndex = cl.startNodeIndex,
                    endNodeIndex   = cl.endNodeIndex,
                    width          = cl.width / 2f,   // optional: half-width
                    sampledPoints  = lanePoints,
                    side           = side
                };
                networkSO.lanes.Add(lane);
                int newIdx = networkSO.lanes.Count - 1;
                networkSO.centerLaneToSideLaneIndex[(i, side)] = newIdx;
                // hook into nodes
                networkSO.nodes[cl.startNodeIndex].outgoingLaneIndices.Add(newIdx);
                networkSO.nodes[cl.endNodeIndex].incomingLaneIndices.Add(newIdx);
            }
        }

        // Save
        EditorUtility.SetDirty(networkSO);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LaneOffset] Generated {networkSO.lanes.Count} total side-lanes");
    }

    // VERY simple 2-point sample—replace with GetSplinePointsCenter() for curved roads
    Vector3[] SampleCenterSpline(Vector3 A, Vector3 B) {
        return new[]{ A, (A+B)/2f, B };
    }
}
