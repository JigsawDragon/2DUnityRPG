
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Pathfinder : MonoBehaviour
{

    public Tilemap Treemap;

    public Tilemap Groundmap;


    public Transform Root;
    public bool DrawGizmo = false;
    public float YHeight;
    public float Spacing;
    public bool Flip = false;
    // GridGraph's dimensions
    public int GraphWidth;
    public int GraphHeight;

    // The position of Start and Goal nodes
    public Vector2 StartNodePosition;
    public Vector2 GoalNodePosition;

    // Lists of Walls (blocking) and Forests' (hurdles) positions
    public List<Vector2> Walls;
    public List<Vector2> Forests;

    public List<Node> Path = new List<Node>();

    public Dictionary<Vector3, Vector2> WorldGrid = new Dictionary<Vector3, Vector2>();
    public Dictionary<Vector2, Vector3> Grid = new Dictionary<Vector2, Vector3>();

    private GridGraph _map;

    public void Awake()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        // Initialize a new GridGraph of a given width and height
        _map = new GridGraph(GraphWidth, GraphHeight);

        // Define the List of Vector2 to be considered walls
        _map.Walls = Walls;

        // Define the List of Vector2 to be considered forests
        _map.Forests = Forests;

        int x1 = (int)StartNodePosition.x;
        int y1 = (int)StartNodePosition.y;
        int x2 = (int)GoalNodePosition.x;
        int y2 = (int)GoalNodePosition.y;

        // Find the path from StartNodePosition to GoalNodePosition
        Path = AStar.Search(_map, _map.Grid[x1, y1], _map.Grid[x2, y2]);

        // Draw a Sphere on the Editor window for each Node of the Graph
        for (int y = 0; y < GraphHeight; y++)
        {
            for (int x = 0; x < GraphWidth; x++)
            {
                Vector3 worldPos = new Vector3(Root.position.x + (x * Spacing), Root.position.y + (y * Spacing), YHeight);
                Vector2 gridPos = new Vector2(x, y);
                WorldGrid[worldPos] = gridPos;
                Grid[gridPos] = worldPos;
                //_map.Walls.Add(gridPos);
            }
        }


        foreach (Vector3Int pos in Treemap.cellBounds.allPositionsWithin)
        {
            if (Treemap.HasTile(pos))
            {
                Vector2 gridpos = new Vector2(pos.x + 0.5f, pos.y + 0.5f);
                Walls.Add(WorldToGridPos(gridpos));
            }
        }

    }

    public List<Node> GetPath(Vector2Int start, Vector2Int end)
    {
        List<Node> path = AStar.Search(_map, _map.Grid[start.x, start.y], _map.Grid[end.x, end.y]);
        if (path.Count > 0)
            return path;
        return new List<Node>();
    }

    public Vector2 WorldToGridPos(Vector3 worldPos)
    {
        //if (!Map.WorldGrid.ContainsKey(worldPos))
        //return Vector2.negativeInfinity;
        return WorldGrid[worldPos];
    }

    public Vector3 GridToWorldPos(Vector2 gridPos)
    {
        //if (!Map.WorldGrid.ContainsKey(worldPos))
        //return Vector2.negativeInfinity;
        return Grid[gridPos];
    }

    public void AddIslandPositions(List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            Vector3 leveledPosition = new Vector3(pos.x, YHeight, pos.z);

            if (!WorldGrid.ContainsKey(leveledPosition))
                continue;

            _map.Walls.Remove(WorldGrid[leveledPosition]);
        }
    }

    // When Pathfinder GameObject is selected show the Gizmos
    private void OnDrawGizmosSelected()
    {
        // Initialize a new GridGraph of a given width and height
        _map = new GridGraph(GraphWidth, GraphHeight);

        // Define the List of Vector2 to be considered walls
        _map.Walls = Walls;

        // Define the List of Vector2 to be considered forests
        _map.Forests = Forests;

        int x1 = (int)StartNodePosition.x;
        int y1 = (int)StartNodePosition.y;
        int x2 = (int)GoalNodePosition.x;
        int y2 = (int)GoalNodePosition.y;

        // Find the path from StartNodePosition to GoalNodePosition
        Path = AStar.Search(_map, _map.Grid[x1, y1], _map.Grid[x2, y2]);

        if (DrawGizmo)
        {
            // Draw a Sphere on the Editor window for each Node of the Graph
            for (int y = 0; y < GraphHeight; y++)
            {
                for (int x = 0; x < GraphWidth; x++)
                {
                    Vector3 worldPos = new Vector3(Root.position.x + (x * Spacing), Root.position.y + (y * Spacing), YHeight);
                    Gizmos.DrawSphere(worldPos, 0.2f);
                }
            }
        }

        // The Start node is BLUE
        Gizmos.color = Color.blue;
        Vector3 startWorldPos = new Vector3(Root.position.x + (StartNodePosition.x * Spacing), Root.position.y + (StartNodePosition.y * Spacing), YHeight);
        Gizmos.DrawSphere(startWorldPos, 0.2f);

        // The walls are BLACK
        Gizmos.color = Color.black;
        foreach (Vector2 wall in Walls)
        {
            Vector3 worldPos = new Vector3(Root.position.x + (wall.x * Spacing), Root.position.y + (wall.y * Spacing), YHeight);
            Gizmos.DrawSphere(worldPos, 0.2f);
        }

        // The forests are GREEN
        Gizmos.color = Color.green;
        foreach (Vector2 forest in Forests)
        {
            Vector3 worldPos = new Vector3(Root.position.x + (forest.x * Spacing), Root.position.y + (forest.y + Spacing), YHeight);
            Gizmos.DrawSphere(worldPos, 0.2f);
        }

        foreach (Node n in Path)
        {
            // The Goal node is RED
            if (n.Position == GoalNodePosition)
            {
                Gizmos.color = Color.red;
            }
            // Every other node in the path is YELLOW
            else
            {
                Gizmos.color = Color.yellow;
            }
            Vector3 worldPos = new Vector3(Root.position.x + (n.Position.x * Spacing), Root.position.y + (n.Position.y * Spacing), YHeight);
            Gizmos.DrawSphere(worldPos, 0.2f);
        }
    }
}
