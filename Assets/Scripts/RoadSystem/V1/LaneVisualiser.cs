using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LaneVisualiser : MonoBehaviour {
    public RoadNetworkSO networkSO;
    public float gizmoSphereSize = 0.2f;

    void OnDrawGizmos() {
        if (networkSO == null) return;
        foreach (var lane in networkSO.lanes) {
            // choose color per side
            Gizmos.color = (lane.side == LaneSide.Left)
                ? Color.yellow
                : Color.blue;
            // draw segments
            for (int i = 0; i < lane.sampledPoints.Count - 1; i++) {
                Gizmos.DrawLine(lane.sampledPoints[i], lane.sampledPoints[i+1]);
            }
            // draw nodes
            Gizmos.DrawSphere(
                lane.sampledPoints[0], gizmoSphereSize);
            Gizmos.DrawSphere(
                lane.sampledPoints[^1], gizmoSphereSize);
        }
    }
}
