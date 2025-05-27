using System.Collections.Generic;
using UnityEngine;
using EasyRoads3Dv3;

namespace Harvey.RoadSystem.v4
{
    public enum LaneSide { Left, Right }

    public class RoadNetworkBuilder : MonoBehaviour
    {
        [SerializeField] private bool drawGizmos = true;

        [Header("Generated Data")]
        public List<RoadData> allRoads = new();

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying)
            {
                BuildRoadGraph();
            }
        }
#endif

        void Start()
        {
            BuildRoadGraph();
        }

        public void BuildRoadGraph()
        {
            allRoads.Clear();

            ERRoadNetwork erNetwork = new ERRoadNetwork();
            ERRoad[] roads = erNetwork.GetRoadObjects();

            foreach (var road in roads)
            {
                RoadData rd = new(road);
                allRoads.Add(rd);
            }

            Debug.Log($"Built road graph with {allRoads.Count} roads.");
        }
    }
}
