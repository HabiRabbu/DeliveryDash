using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Road Network/Network Data")]
public class RoadNetworkSO : ScriptableObject {
    public List<RoadNode> nodes = new List<RoadNode>();
    public List<RoadLane> lanes = new List<RoadLane>();
    public Dictionary<Vector3,int> centerMarkerToNodeIndex;
    public Dictionary<(int centerLaneIndex, LaneSide), int> centerLaneToSideLaneIndex;
}
