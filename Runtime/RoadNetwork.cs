using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RoadNetwork
{
    public List<RoadNode> Nodes;
    public List<RoadEdge> Edges;
    
    public RoadNetwork()
    {
        Nodes = new List<RoadNode>();
        Edges = new List<RoadEdge>();
    }
    
    public RoadNode CreateNode(Vector3 position)
    {
        var node = new RoadNode {NodeID = GenerateID(), Position = position};
        Nodes.Add(node);
        return node;
    }
    
    int GenerateID()
    {
        int id = 0;
        foreach (var node in Nodes)
        {
            if (node.NodeID > id)
            {
                id = node.NodeID;
            }
        }
        
        return id + 1;
    }
    
    public void AddNode(RoadNode node)
    {
        Nodes.Add(node);
    }
    
    public void AddEdge(RoadEdge edge)
    {
        Edges.Add(edge);
    }
    
    public RoadNode GetNode(int nodeID)
    {
        return Nodes.Find(n => n.NodeID == nodeID);
    }
    
    public void RemoveNode(int nodeID)
    {
        var node = Nodes.Find(n => n.NodeID == nodeID);
        Nodes.Remove(node);
        
        var edgesToRemove = Edges.Where(e => e.StartNodeID == nodeID || e.EndNodeID == nodeID).ToList();
        foreach (var edge in edgesToRemove)
        {
            Edges.Remove(edge);
        }
    }
    
    public void SetNodePosition(int nodeID, Vector3 position)
    {
        var node = Nodes.IndexOf(GetNode(nodeID));
        Nodes[node] = new RoadNode {NodeID = nodeID, Position = position};
    }
    
    public RoadEdge GetEdge(int startNodeID, int endNodeID)
    {
        return Edges.Find(e => (e.StartNodeID == startNodeID && e.EndNodeID == endNodeID) || (e.StartNodeID == endNodeID && e.EndNodeID == startNodeID));
    }
 
    /// <summary>
    /// Gets a list of all roads in the network.
    /// </summary>
    public List<Road> GetAllRoads()
    {
        var roads = new List<Road>();
        var visitedEdges = new HashSet<RoadEdge>();

        foreach (var node in Nodes)
        {
            if (IsIntersection(node))
            {
                var adjacentEdges = Edges.Where(e => e.StartNodeID == node.NodeID || e.EndNodeID == node.NodeID).ToList();
                foreach (var edge in adjacentEdges)
                {
                    if (!visitedEdges.Contains(edge))
                    {
                        var road = new Road();
                        road.NodeIDs.Add(node.NodeID);
                        TraverseRoad(edge, node.NodeID, visitedEdges, road);
                        if (road.NodeIDs.Count > 1)
                        {
                            roads.Add(road);
                        }
                    }
                }
            }
        }

        return roads;
    }

    private void TraverseRoad(RoadEdge edge, int comingFromNodeId, HashSet<RoadEdge> visitedEdges, Road road)
    {
        visitedEdges.Add(edge);

        var nextNodeId = edge.StartNodeID == comingFromNodeId ? edge.EndNodeID : edge.StartNodeID;
        road.NodeIDs.Add(nextNodeId);

        if (!IsIntersection(Nodes.Find(n => n.NodeID == nextNodeId)))
        {
            var nextEdge = Edges.Find(e => (e.StartNodeID == nextNodeId || e.EndNodeID == nextNodeId) && !visitedEdges.Contains(e));
            
            if (nextEdge != null)
            {
                TraverseRoad(nextEdge, nextNodeId, visitedEdges, road);
            }
        }
    }

    public bool IsIntersection(RoadNode node)
    {
        return GetAdjacentNodes(node).Count > 2;
    }
    
    public List<RoadNode> GetAdjacentNodes(RoadNode node)
    {
        var adjacentNodes = new List<RoadNode>();
        foreach (var edge in Edges)
        {
            if (edge.StartNodeID == node.NodeID)
                adjacentNodes.Add(Nodes.Find(n => n.NodeID == edge.EndNodeID));
            else if (edge.EndNodeID == node.NodeID)
                adjacentNodes.Add(Nodes.Find(n => n.NodeID == edge.StartNodeID));
        }

        return adjacentNodes;
    }
    
    public void InsertBetween(RoadNode start, RoadNode end, RoadNode newNode)
    {
        var edgesToRemove = new List<RoadEdge>();
        foreach (var edge in Edges)
        {
            if (edge.StartNodeID == start.NodeID && edge.EndNodeID == end.NodeID)
            {
                edgesToRemove.Add(edge);
            }
            else if (edge.StartNodeID == end.NodeID && edge.EndNodeID == start.NodeID)
            {
                edgesToRemove.Add(edge);
            }
        }

        foreach (var edge in edgesToRemove)
        {
            Edges.Remove(edge);
        }
        var width = edgesToRemove[0].Width;
        AddEdge(new RoadEdge(start.NodeID, newNode.NodeID, width));
        AddEdge(new RoadEdge(newNode.NodeID, end.NodeID, width));
    }
}