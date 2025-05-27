using System.Collections.Generic;
using UnityEngine;
using EasyRoads3Dv3;

namespace Harvey.RoadSystem.v4
{
    public class RoadData
    {
        public ERRoad easyRoad;
        public List<Waypoint> leftLane = new();
        public List<Waypoint> rightLane = new();

        public RoadData(ERRoad road)
        {
            this.easyRoad = road;
            GenerateLanes();
        }

        void GenerateLanes()
        {
            Vector3[] leftPoints = easyRoad.GetSplinePointsLeftSide();
            Vector3[] rightPoints = easyRoad.GetSplinePointsRightSide();

            for (int i = 0; i < leftPoints.Length; i++)
            {
                Waypoint wp = new(leftPoints[i], LaneSide.Left, this);
                if (i > 0)
                {
                    wp.previous = leftLane[i - 1];
                    leftLane[i - 1].next = wp;
                }
                leftLane.Add(wp);
            }

            for (int i = 0; i < rightPoints.Length; i++)
            {
                Waypoint wp = new(rightPoints[i], LaneSide.Right, this);
                if (i > 0)
                {
                    wp.previous = rightLane[i - 1];
                    rightLane[i - 1].next = wp;
                }
                rightLane.Add(wp);
            }
        }

        public ERConnection GetStartConnection()
        {
            return easyRoad.GetConnectionAtStart(out _);
        }

        public ERConnection GetEndConnection()
        {
            return easyRoad.GetConnectionAtEnd(out _);
        }
    }
}