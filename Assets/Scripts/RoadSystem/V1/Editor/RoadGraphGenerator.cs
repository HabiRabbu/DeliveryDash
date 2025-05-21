using UnityEditor;
using UnityEngine;
using EasyRoads3Dv3;
using System.Collections.Generic;

public class RoadGraphGenerator : EditorWindow {
    RoadNetworkSO networkSO;
    ERRoadNetwork erNet;

    [MenuItem("Tools/Roads/Generate Center Graph")]
    static void ShowWindow() => GetWindow<RoadGraphGenerator>("Center Graph");

    void OnGUI() {
        networkSO = (RoadNetworkSO)EditorGUILayout.ObjectField(
            "Network SO", networkSO, typeof(RoadNetworkSO), false);
        if (GUILayout.Button("Build Center Graph") && networkSO != null) {
            BuildCenterGraph();
        }
    }

    void BuildCenterGraph() {
        // 1) Clear previous data
        networkSO.nodes.Clear();
        networkSO.lanes.Clear();
        networkSO.centerMarkerToNodeIndex = new Dictionary<Vector3,int>();
        erNet = new ERRoadNetwork();

        // 2) First pass: register EVERY marker as a node
        foreach (var road in erNet.GetRoadObjects()) {
            var markers = road.GetMarkerPositions();
            for (int m = 0; m < markers.Length; m++) {
                if (!networkSO.centerMarkerToNodeIndex.ContainsKey(markers[m])) {
                    int newIdx = networkSO.nodes.Count;
                    networkSO.nodes.Add(new RoadNode { position = markers[m] });
                    networkSO.centerMarkerToNodeIndex[markers[m]] = newIdx;
                }
            }
        }

        // 3) Second pass: create a lane for each marker→next-marker
        foreach (var road in erNet.GetRoadObjects()) {
            var markers = road.GetMarkerPositions();
            for (int i = 0; i < markers.Length - 1; i++) {
                int nodeA = networkSO.centerMarkerToNodeIndex[markers[i]];
                int nodeB = networkSO.centerMarkerToNodeIndex[markers[i+1]];

                var lane = new RoadLane {
                    startNodeIndex = nodeA,
                    endNodeIndex   = nodeB,
                    width          = road.GetWidth(),
                    sampledPoints  = new List<Vector3> { markers[i], markers[i+1] },
                    side           = LaneSide.Left // placeholder for now
                };

                networkSO.nodes[nodeA].outgoingLaneIndices.Add(networkSO.lanes.Count);
                networkSO.nodes[nodeB].incomingLaneIndices.Add(networkSO.lanes.Count);
                networkSO.lanes.Add(lane);
            }
        }

        // 4) Save
        EditorUtility.SetDirty(networkSO);
        AssetDatabase.SaveAssets();
        Debug.Log($"[CenterGraph] {networkSO.nodes.Count} nodes, {networkSO.lanes.Count} lanes built.");
    }
}
