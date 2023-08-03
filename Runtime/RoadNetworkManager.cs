using System.Collections;
using UnityEngine;

public class RoadNetworkManager : MonoBehaviour
{
    public RoadNetwork roadNetwork;
    public RoadMeshGenerator roadMeshGenerator;
    public RoadMeshSettings roadMeshSettings = new RoadMeshSettings();
    public void Reset()
    {
        roadNetwork = new RoadNetwork();
        roadNetwork.AddNode(new RoadNode {NodeID = 1, Position = new Vector3(0, 0, 0)});
        roadNetwork.AddNode(new RoadNode {NodeID = 2, Position = new Vector3(0, 0,20)});
        roadNetwork.AddNode(new RoadNode {NodeID = 3, Position = new Vector3(10, 0,40)});
        roadNetwork.AddNode(new RoadNode {NodeID = 4, Position = new Vector3(20, 5,20)});
        roadNetwork.AddNode(new RoadNode {NodeID = 5, Position = new Vector3(20, 0,0)});
        roadNetwork.AddNode(new RoadNode {NodeID = 6, Position = new Vector3(40, 0,20)});
        roadNetwork.AddNode(new RoadNode {NodeID = 7, Position = new Vector3(40, 0,0)});
        roadNetwork.AddNode(new RoadNode {NodeID = 8, Position = new Vector3(10, 0,60)});

        roadNetwork.AddEdge(new RoadEdge(1, 1, 2, 4));
        roadNetwork.AddEdge(new RoadEdge(2, 2, 3, 4));
        roadNetwork.AddEdge(new RoadEdge(3, 3, 4, 4));
        roadNetwork.AddEdge(new RoadEdge(4, 4, 5, 4));
        roadNetwork.AddEdge(new RoadEdge(5, 4, 2, 4));
        roadNetwork.AddEdge(new RoadEdge(6, 4, 6, 4));
        roadNetwork.AddEdge(new RoadEdge(7, 7, 6, 4));
        roadNetwork.AddEdge(new RoadEdge(8, 7, 5, 4));
        roadNetwork.AddEdge(new RoadEdge(9, 3, 8, 4));
        
        Debug.Log("Number of nodes: " + roadNetwork.Nodes.Count);
        Debug.Log("Number of edges: " + roadNetwork.Edges.Count);
        Debug.Log("Number of roads: " + roadNetwork.GetAllRoads().Count);
        
        roadMeshGenerator = new RoadMeshGenerator(roadNetwork);
        roadMeshGenerator.GenerateMeshes(gameObject);
    }
    
    public void GenerateRoadNetwork()
    {
        roadMeshGenerator = new RoadMeshGenerator(roadNetwork);
        roadMeshGenerator.settings = roadMeshSettings;
        roadMeshGenerator.GenerateMeshes(gameObject);
    }
}