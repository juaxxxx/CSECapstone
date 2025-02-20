using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainFloorGenerator : MonoBehaviour
{
    public SquareGrid squareGrid;
    public MeshFilter floors;

    List<Vector3> vertices;
    List<int> triangles;


    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    HashSet<int> checkedVertices = new HashSet<int>();
    public GameObject floorObjects;

    public void GenerateMesh(int[,] map, float squareSize, float zeroPointx, float zeroPointy, float height)
    {
        if (floorObjects.GetComponent<MeshCollider>() != null)
        {
            Destroy(floorObjects.GetComponent<MeshCollider>());
        }
        triangleDictionary.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(map, squareSize, zeroPointx, zeroPointy, height);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }

        Mesh floormesh = new Mesh();

        floormesh.vertices = vertices.ToArray();
        floormesh.triangles = triangles.ToArray();
        floormesh.RecalculateNormals();
        floors.mesh = floormesh;
        floorObjects.AddComponent<MeshCollider>();
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            // 4 points :
            case 0:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;

            // 3 points:
            case 1:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 2:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 4:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 8:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 9:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 12:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;

            // 1 points:
            case 7:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 13:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 14:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;

            // 0 point:
            case 15:
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);

    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }


        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize, float zeroPointx, float zeroPointy, float height)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(x * squareSize - squareSize / 2 + zeroPointx, -height, y * squareSize - squareSize / 2 + zeroPointy);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }

        }
    }

    public class Square
    {

        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }

    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {

        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }

    }
}