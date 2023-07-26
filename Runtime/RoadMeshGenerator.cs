using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RoadMeshSettings
{
    public int resolution = 15;
    public float distanceFromIntersection = 3f;
}

public class RoadMeshGenerator
{
    RoadNetwork roadNetwork;
    public RoadMeshSettings settings = new RoadMeshSettings();
    
    class MeshObject
    {
        public Mesh mesh;
        public Vector3 position;
    }

    public RoadMeshGenerator(RoadNetwork roadNetwork)
    {
        this.roadNetwork = roadNetwork;
    }

    public void GenerateMeshes(GameObject root)
    {
        // destroy all children
        for (int i = root.transform.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(root.transform.GetChild(i).gameObject);
        }
            
        var meshes = new List<MeshObject>();
        
        var intersectionNodes = roadNetwork.Nodes.Where(n => roadNetwork.GetAdjacentNodes(n).Count > 2).ToList();
        foreach (var node in intersectionNodes)
        {
            meshes.Add(GenerateIntersectionMesh(node));
        }
        
        var roads = roadNetwork.GetAllRoads();
        foreach (var road in roads)
        {
            meshes.Add(GenerateRoadMesh(road));
        }
        
        foreach (var mesh in meshes)
        {
            var meshObject = new GameObject();
            meshObject.transform.parent = root.transform;
            meshObject.transform.position = mesh.position;
            meshObject.AddComponent<MeshFilter>().mesh = mesh.mesh;
            meshObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            meshObject.AddComponent<MeshCollider>();
        }
    }
    
    MeshObject GenerateIntersectionMesh(RoadNode node)
    {
        var meshObject = new MeshObject();
        
        var adjacentNodes = roadNetwork.GetAdjacentNodes(node);
        var vertices = new List<Vector3>();
        
        foreach (var adjacentNode in adjacentNodes)
        {
            var edge = roadNetwork.GetEdge(node.NodeID, adjacentNode.NodeID);
            var roadWidth = edge.Width;
            var direction = (adjacentNode.Position - node.Position).normalized;
            
            var perpendicular = new Vector3(direction.z, 0, -direction.x);
            var perpendicular2 = new Vector3(-direction.z, 0, direction.x);
            
            var vertex1 = node.Position + perpendicular * roadWidth / 2f + direction * settings.distanceFromIntersection;
            var vertex2 = node.Position + perpendicular2 * roadWidth / 2f + direction * settings.distanceFromIntersection;

            vertices.Add(vertex1);
            vertices.Add(vertex2);
        }
        
        var vertices2D = vertices.Select(v => new Vector2(v.x, v.z)).ToArray();
        

        var center = node.Position;
        var center2D = new Vector2(center.x, center.z);
        
        // sort vertices by angle
        var sortedVertices = vertices2D.OrderBy(v => Mathf.Atan2(v.y - center2D.y, v.x - center2D.x)).ToArray();
        var centeredVertices = sortedVertices.Select(v => v - center2D).ToArray();
        
        var polygon = new Polygon2D(centeredVertices);

        var mesh = polygon.ToMesh();
        
        meshObject.mesh = mesh;
        meshObject.position = node.Position;
        return meshObject;
    }
    public List<SplinePoint> GenerateRoadSpline(Road road, float startOffset, float endOffset)
    {
        var splinePoints = new List<SplinePoint>();
        var nodes = road.NodeIDs.Select(id => roadNetwork.GetNode(id)).ToList();
        var positions = nodes.Select(n => n.Position).ToList();

        var width = 4;
        
        // Calculate start and end positions with offsets
        Vector3 startDirection = (nodes[1].Position - nodes[0].Position).normalized;
        Vector3 endDirection = (nodes[nodes.Count - 2].Position - nodes[nodes.Count - 1].Position).normalized;
        Vector3 startPosition = nodes[0].Position + startDirection * startOffset;
        Vector3 endPosition = nodes[nodes.Count - 1].Position + endDirection * endOffset;
        startPosition.y = nodes[0].Position.y;
        endPosition.y = nodes[nodes.Count - 1].Position.y;
        positions[0] = startPosition;
        positions[positions.Count - 1] = endPosition;
        
        // Add start position as the first SplinePoint
        //splinePoints.Add(new SplinePoint { Position = startPosition, Direction = startDirection, Perpendicular = Vector3.Cross(startDirection, Vector3.up), Width = width });
        
        // Loop through nodes and calculate interpolated points using Catmull-Rom interpolation
        for (int i = 0; i < positions.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ? positions[i - 1] : startPosition;
            Vector3 p1 = positions[i];
            Vector3 p2 = positions[i + 1];
            Vector3 p3 = i < positions.Count - 2 ? positions[i + 2] : endPosition;

            for (int j = 0; j < settings.resolution; j++)
            {
                float t = (float)j / settings.resolution;
                float t2 = t * t;
                float t3 = t2 * t;

                Vector3 position = 0.5f * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
                Vector3 tangent = 0.5f * (-p0 + p2 + 2 * (2 * p0 - 5 * p1 + 4 * p2 - p3) * t + 3 * (-p0 + 3 * p1 - 3 * p2 + p3) * t2);
                Vector3 direction = tangent.normalized;
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

                splinePoints.Add(new SplinePoint { Position = position, Direction = direction, Perpendicular = perpendicular, Width = width });
            }
        }
        
        // Add end position as the last SplinePoint
        splinePoints.Add(new SplinePoint { Position = endPosition, Direction = endDirection, Perpendicular = -Vector3.Cross(endDirection, Vector3.up), Width = width });

        return splinePoints;
    }

    MeshObject GenerateRoadMesh(Road road)
    {
        var meshObject = new MeshObject();
        
        var firstNode = roadNetwork.GetNode(road.NodeIDs.First());
        var lastNode = roadNetwork.GetNode(road.NodeIDs.Last());

        var startOffset = roadNetwork.IsIntersection(firstNode) ? settings.distanceFromIntersection : 0;
        var endOffset = roadNetwork.IsIntersection(lastNode) ? settings.distanceFromIntersection : 0;
        
        var splinePoints = GenerateRoadSpline(road, startOffset, endOffset);
        
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int i = 0; i < splinePoints.Count; i++)
        {
            var point = splinePoints[i];

            var vertex1 = point.Position + point.Perpendicular * point.Width / 2f;
            var vertex2 = point.Position - point.Perpendicular * point.Width / 2f;
            
            vertices.Add(vertex1);
            vertices.Add(vertex2);
            
            if (i > 0)
            {
                var index = i * 2;
                triangles.Add(index - 1);
                triangles.Add(index - 2);
                triangles.Add(index);
                
                triangles.Add(index + 1);
                triangles.Add(index - 1);
                triangles.Add(index);
            }
        }

        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        
        mesh.RecalculateNormals();
        
        meshObject.mesh = mesh;
        meshObject.position = Vector3.zero;
        return meshObject;
    }
}

public class SplinePoint
{
    public Vector3 Position;
    public Vector3 Direction;
    public Vector3 Perpendicular;
    public float Width;
}