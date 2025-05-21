using System.Collections.Generic;
using UnityEngine;

public enum LaneSide { Left, Right }

[System.Serializable]
public class RoadLane {
    public int startNodeIndex;
    public int endNodeIndex;
    public LaneSide side;
    public float width;
    public List<Vector3> sampledPoints = new List<Vector3>();
}
