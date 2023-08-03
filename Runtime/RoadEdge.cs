using System;

/// <summary>
/// Note that the order of the nodes in the edge does not matter, the graph is undirected.
/// </summary>
[Serializable]
public class RoadEdge
{
    public int EdgeID;
    public int StartNodeID;
    public int EndNodeID;
    public float Width;
    
    public RoadEdge(int edgeID, int startNodeID, int endNodeID, float width)
    {
        EdgeID = edgeID;
        StartNodeID = startNodeID;
        EndNodeID = endNodeID;
        Width = width;
    }
}