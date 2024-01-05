using System.Collections.Generic;
using PriorityQueues;
using UnityEngine;

/// <summary>
/// Implementation of Amit Patel's A* Pathfinding algorithm studies
/// https://www.redblobgames.com/pathfinding/a-star/introduction.html
/// </summary>
public static class AStar
{
    /// <summary>
    /// Returns the best path as a List of Nodes
    /// </summary>
    public static List<Node> Search(GridGraph graph, Node start, Node goal)
    {
        Dictionary<Node, Node> came_from = new Dictionary<Node, Node>();
        Dictionary<Node, float> cost_so_far = new Dictionary<Node, float>();

        List<Node> path = new List<Node>();

        BinaryPriorityQueue<Node> frontier = new BinaryPriorityQueue<Node>((a, b) => a.Priority.CompareTo(b.Priority));
        start.Priority = 0;
        frontier.Enqueue(start);

        came_from.Add(start, start);
        cost_so_far.Add(start, 0);

        Node current = new Node(0, 0);
        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();
            if (current == goal) break; // Early exit

            for(int i = 0; i < graph.Neighbours(current).Count; i++)
			{
                Node next = graph.Neighbours(current)[i];
                float new_cost = cost_so_far[current] + graph.Cost(next);
                if (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next])
                {
                    cost_so_far[next] = new_cost;
                    came_from[next] = current;
                    float priority = new_cost + Heuristic(next, goal);
                    next.Priority = priority;
                    frontier.Enqueue(next);
                    next.Priority = new_cost;
                }
            }
            /*
            foreach (Node next in graph.Neighbours(current))
            {
                float new_cost = cost_so_far[current] + graph.Cost(next);
                if (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next])
                {
                    cost_so_far[next] = new_cost;
                    came_from[next] = current;
                    float priority = new_cost + Heuristic(next, goal);
                    next.Priority = priority;
                    frontier.Enqueue(next);
                    next.Priority = new_cost;
                }
            }
            */
        }

        while (current != start)
        {
            path.Add(current);
            current = came_from[current];
        }
        path.Reverse();

        return path;
    }

    public static float Heuristic(Node a, Node b)
    {
        return Mathf.Abs(a.Position.x - b.Position.x) + Mathf.Abs(a.Position.y - b.Position.y);
    }
}