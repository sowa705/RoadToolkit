using System.Collections.Generic;
using System.Linq;

public class Road
{
    public List<int> NodeIDs = new List<int>();

    public override int GetHashCode()
    {
        return NodeIDs.Aggregate(0, (current, nodeID) => current ^ nodeID);
    }
}