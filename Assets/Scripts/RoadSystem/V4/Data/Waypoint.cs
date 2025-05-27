using UnityEngine;

namespace Harvey.RoadSystem.v4
{
    public class Waypoint
    {
        public Vector3 position;
        public LaneSide side;
        public Waypoint previous;
        public Waypoint next;
        public RoadData parentRoad;

        public Waypoint(Vector3 pos, LaneSide side, RoadData road)
        {
            this.position = pos;
            this.side = side;
            this.parentRoad = road;
        }
    }
}
