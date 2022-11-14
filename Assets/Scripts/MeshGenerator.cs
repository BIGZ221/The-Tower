using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    public bool drawSquareGrid = false;
    public bool is2D = true;
    public int tileAmount = 10;
    public float wallHeight = 5f;
    public GameObject map;
    public GameObject walls;
    public GameObject colliders;

    public SquareGrid squareGrid;
    private List<Vector3> vertices;
    private List<int> triangles;
    private Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    private List<List<int>> outlines = new List<List<int>>();
    private HashSet<int> checkedVertices = new HashSet<int>();

    public void GenerateMesh(int[,] activeMap, float squareSize) {
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();

        squareGrid = new SquareGrid(activeMap, squareSize);
        triangles = new List<int>();
        vertices = new List<Vector3>();

        int width = squareGrid.squares.GetLength(0);
        int height = squareGrid.squares.GetLength(1);
        for (int x = 0; x < width; ++x) {
            for (int y = 0; y < height; ++y) {
                TriangulateSquare(squareGrid.squares[x,y]);
            }
        }
        Mesh mesh = new Mesh();
        map.GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

		Vector2[] uvs = new Vector2[vertices.Count];
		for (int i =0; i < vertices.Count; i ++) {
			float percentX = Mathf.InverseLerp(-activeMap.GetLength(0)/2*squareSize,activeMap.GetLength(0)/2*squareSize,vertices[i].x) * tileAmount;
			float percentY = Mathf.InverseLerp(-activeMap.GetLength(0)/2*squareSize,activeMap.GetLength(0)/2*squareSize,vertices[i].z) * tileAmount;
			uvs[i] = new Vector2(percentX,percentY);
		}
		mesh.uv = uvs;
        if (is2D) {
			Generate2DColliders();
		} else {
			CreateWallMesh ();
		}
    }

    public void CreateWallMesh() {
        CalculateMeshOutlines();
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();

        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // Left vertex
                wallVertices.Add(vertices[outline[i + 1]]); // Right vertex
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // Bottom Left vertex
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * wallHeight); // Bottom Right vertex
                
                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }
        Mesh wallMesh = new Mesh();
        walls.GetComponent<MeshFilter>().mesh = wallMesh;
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        wallMesh.RecalculateNormals();
    }

    void Generate2DColliders() {
		EdgeCollider2D[] currentColliders = colliders.GetComponents<EdgeCollider2D> ();
		for (int i = 0; i < currentColliders.Length; i++) {
			Destroy(currentColliders[i]);
		}

		CalculateMeshOutlines ();

		foreach (List<int> outline in outlines) {
			EdgeCollider2D edgeCollider = colliders.AddComponent<EdgeCollider2D>();
			Vector2[] edgePoints = new Vector2[outline.Count];

			for (int i =0; i < outline.Count; i ++) {
				edgePoints[i] = new Vector2(vertices[outline[i]].x,vertices[outline[i]].z);
			}
			edgeCollider.points = edgePoints;
		}

	}

    void TriangulateSquare(Square square) {
        switch (square.configuration)
        {
            // Off
            case 0:
                break;
            // 1 Point
            case 1:
                MeshFromPoints(square.leftCenter, square.bottomCenter, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.bottomCenter, square.rightCenter);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.rightCenter, square.topCenter);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.topCenter, square.leftCenter);
                break;
            // 2 Points
            case 3:
                MeshFromPoints(square.rightCenter, square.bottomRight, square.bottomLeft, square.leftCenter);
                break;
            case 6:
                MeshFromPoints(square.topCenter, square.topRight, square.bottomRight, square.bottomCenter);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.topCenter, square.bottomCenter, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.rightCenter, square.leftCenter);
                break;
            case 5:
                MeshFromPoints(square.topCenter, square.topRight, square.rightCenter, square.bottomCenter, square.bottomLeft, square.leftCenter);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.topCenter, square.rightCenter, square.bottomRight, square.bottomCenter, square.leftCenter);
                break;
            // 3 Points
            case 7:
                MeshFromPoints(square.topCenter, square.topRight, square.bottomRight, square.bottomLeft, square.leftCenter);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.topCenter, square.rightCenter, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.rightCenter, square.bottomCenter, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomCenter, square.leftCenter);
                break;
            // 4 Points
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.index);
                checkedVertices.Add(square.topRight.index);
                checkedVertices.Add(square.bottomRight.index);
                checkedVertices.Add(square.bottomLeft.index);
                break;
        }
    }

    void MeshFromPoints(params Node[] points) {
        AssignVertices(points);
        if (points.Length >= 3) CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4) CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5) CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6) CreateTriangle(points[0], points[4], points[5]);
    }

    void AssignVertices(Node[] points) {
        for (int i = 0; i < points.Length; i++) {
            if (points[i].index == -1) {
                points[i].index = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add(a.index);
        triangles.Add(b.index);
        triangles.Add(c.index);
        Triangle triangle = new Triangle(a.index, b.index, c.index);
        AddTriangleToDict(triangle.vertexIndexA, triangle);
        AddTriangleToDict(triangle.vertexIndexB, triangle);
        AddTriangleToDict(triangle.vertexIndexC, triangle);
    }

    void CalculateMeshOutlines() {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if (!checkedVertices.Contains(vertexIndex)) {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1) {
                    checkedVertices.Add(vertexIndex);
                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
        SimplifyMeshOutlines();
    }

    void SimplifyMeshOutlines() {
		for (int outlineIndex = 0; outlineIndex < outlines.Count; outlineIndex ++) {
			List<int> simplifiedOutline = new List<int>();
			Vector3 dirOld = Vector3.zero;
			for (int i = 0; i < outlines[outlineIndex].Count; i ++) {
				Vector3 p1 = vertices[outlines[outlineIndex][i]];
				Vector3 p2 = vertices[outlines[outlineIndex][(i+1)%outlines[outlineIndex].Count]];
				Vector3 dir = p1-p2;
				if (dir != dirOld) {
					dirOld = dir;
					simplifiedOutline.Add(outlines[outlineIndex][i]);
				}
			}
			outlines[outlineIndex] = simplifiedOutline;
		}
	}

    void FollowOutline(int index, int outlineIndex) {
        outlines[outlineIndex].Add(index);
        checkedVertices.Add(index);
        int nextIndex = GetConnectedOutlineVertex(index);
        if (nextIndex != -1) {
            FollowOutline(nextIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int index) {
        List<Triangle> connectedTriangles = triangleDictionary[index];
        for (int i = 0; i < connectedTriangles.Count; i++) {
            Triangle triangle = connectedTriangles[i];
            for (int j = 0; j < 3; j++) {
                int indexB = triangle[j];
                if (indexB != index && !checkedVertices.Contains(indexB)) {
                    if (isOutlineEdge(index, indexB)) {
                        return indexB;
                    }
                }
            }
        }
        return -1;
    }

    bool isOutlineEdge(int a, int b) {
        List<Triangle> aTriangles = triangleDictionary[a];
        int sharedTriangles = 0;
        for (int i = 0; i < aTriangles.Count; i++) {
            if (aTriangles[i].Contains(b)) {
                sharedTriangles += 1;
                if (sharedTriangles > 1) break;
            }
        }
        return sharedTriangles == 1;
    }

    void AddTriangleToDict(int index, Triangle triangle) {
        if (triangleDictionary.ContainsKey(index)) {
            triangleDictionary[index].Add(triangle);
        } else {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(index, triangleList);
        }
    }

    public struct Triangle {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        public Triangle(int a, int b, int c) {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
        }

        public int this[int i] {
            get {
                if (i == 0) return vertexIndexA;
                if (i == 1) return vertexIndexB;
                return vertexIndexC;
            }
        }

        public bool Contains(int index) {
            return index == vertexIndexA || index == vertexIndexB || index == vertexIndexC;
        }
    }

    public class SquareGrid {
        public Square[,] squares;

        public SquareGrid(int[,] activeMap, float squareSize) {
            int x = activeMap.GetLength(0);
            int y = activeMap.GetLength(1);
            float mapWidth = x * squareSize;
            float mapHeight = y * squareSize;
            ControlNode[,] controlNodes = new ControlNode[x,y];
            for (int i = 0; i < x; i++) {
                for (int j = 0; j < y; j++) {
                    Vector3 position = new Vector3(-mapWidth / 2f + i * squareSize + squareSize / 2, 0, -mapHeight / 2f + j * squareSize + squareSize / 2);
                    controlNodes[i,j] = new ControlNode(position, activeMap[i,j] == 1, squareSize);
                }
            }
            squares = new Square[x - 1, y -1];
            for (int i = 0; i < x - 1; i++) {
                for (int j = 0; j < y - 1; j++) {
                    squares[i, j] = new Square(controlNodes[i, j+1], controlNodes[i+1, j+1], controlNodes[i+1, j], controlNodes[i,j]);
                }
            }
        }

    }

    public class Square {
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node topCenter, rightCenter, bottomCenter, leftCenter;
        public int configuration = 0;

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft) {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;
            topCenter = topLeft.right;
            rightCenter = bottomRight.above;
            bottomCenter = bottomLeft.right;
            leftCenter = bottomLeft.above;

            if (topLeft.active) configuration += 8;
            if (topRight.active) configuration += 4;
            if (bottomRight.active) configuration += 2;
            if (bottomLeft.active) configuration += 1;

        }
    }
    
    public class Node {
        public Vector3 position;
        public int index = -1;

        public Node(Vector3 position) {
            this.position = position;
        }
    }

    public class ControlNode : Node {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 position, bool active, float size) : base(position) {
            this.active = active;
            above = new Node(position + Vector3.forward * size / 2f);
            right = new Node(position + Vector3.right * size / 2f);
        }
    }

}
