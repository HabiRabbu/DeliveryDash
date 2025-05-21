using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace DeliverDash.RoadSystem.V3
{
    public class RoadGraphBuilder : MonoBehaviour
    {
        public List<SplineContainer> splineContainers;
        public List<GraphNode> nodes = new();

        void Start() => BuildGraphFromKnots();

        void BuildGraphFromKnots()
        {
            nodes.Clear();
            int nodeId = 0;

            foreach (var container in splineContainers)
            {
                var spline = container.Spline;
                var transform = container.transform;
                GraphNode prev = null;

                for (int i = 0; i < spline.Count; i++)
                {
                    var knot = spline[i];
                    Vector3 worldPos = transform.TransformPoint(knot.Position);

                    var node = new GraphNode { id = nodeId++, position = worldPos };
                    nodes.Add(node);

                    if (prev != null)
                        prev.neighbors.Add(node);

                    prev = node;
                }
            }

            Debug.Log($"[Graph] Built {nodes.Count} nodes.");
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            foreach (var node in nodes)
            {
                Gizmos.DrawSphere(node.position, 0.3f);
                foreach (var neighbor in node.neighbors)
                    Gizmos.DrawLine(node.position, neighbor.position);
            }
        }
    }

    [System.Serializable]
    public class GraphNode
    {
        public int id;
        public Vector3 position;
        public List<GraphNode> neighbors = new();
    }
}