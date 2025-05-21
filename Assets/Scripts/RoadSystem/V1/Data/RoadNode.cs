using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadNode {
    public Vector3 position;
    public List<int> outgoingLaneIndices = new List<int>();
    public List<int> incomingLaneIndices = new List<int>();
}
