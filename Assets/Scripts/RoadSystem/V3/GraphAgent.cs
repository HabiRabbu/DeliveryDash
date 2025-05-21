using System.Collections.Generic;
using UnityEngine;

namespace DeliverDash.RoadSystem.V3
{
    public class GraphAgent : MonoBehaviour
    {
        public RoadGraphBuilder graphBuilder;
        public float speed = 5f;
        public float threshold = 1f;

        List<GraphNode> path = new();
        int currentIndex = 0;

        bool setupComplete = false;

        void Start()
        {

        }

        void Update()
        {
            if (!setupComplete)
            {
                GenerateRandomPath();
                setupComplete = true;
            }    

            if (path.Count == 0) return;

            var target = path[currentIndex].position;
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, target) < threshold)
            {
                currentIndex++;
                if (currentIndex >= path.Count)
                    GenerateRandomPath();
            }
        }

        void GenerateRandomPath()
        {
            path.Clear();

            Debug.Log("Path cleared.");
            var nodes = graphBuilder.nodes;
            Debug.Log("Amount of nodes: " + nodes.Count);

            if (nodes.Count < 2) return;

            GraphNode start = nodes[Random.Range(0, nodes.Count)];
            GraphNode current = start;

            for (int i = 0; i < 10; i++)
            {
                path.Add(current);
                if (current.neighbors.Count == 0) break;
                current = current.neighbors[Random.Range(0, current.neighbors.Count)];
            }

            Debug.Log("Path: " + path);

            transform.position = path[0].position;
            currentIndex = 0;
        }
    }
}