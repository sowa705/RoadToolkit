using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadNetworkManager))]
public class RoadNetworkEditor : Editor
{
    Stack<int> selectedNodes = new Stack<int>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var roadGenerator = (RoadNetworkManager)target;
        if (GUILayout.Button("Generate"))
        {
            roadGenerator.GenerateRoadNetwork();
        }
    }

    public void OnSceneGUI()
    {
        // Draw nodes
        var roadGenerator = (RoadNetworkManager)target;
        var roadNetwork = roadGenerator.roadNetwork;
        
        foreach (var node in roadNetwork.Nodes)
        {
            var position = node.Position;
            Handles.Label(position+Vector3.up*3f, node.NodeID.ToString());
            var size = HandleUtility.GetHandleSize(position) * 0.35f;
            if (selectedNodes.Contains(node.NodeID))
            {
                if (selectedNodes.Peek()==node.NodeID)
                {
                    Handles.color = Color.red;
                }
                else
                {
                    Handles.color = Color.yellow;
                }
            }
            else
            {
                Handles.color = Color.white;
            }
            if (Handles.Button(position, Quaternion.identity, size, size, Handles.SphereHandleCap))
            {
                // if shift is pressed, add to selection
                if (Event.current.shift)
                {
                    if (selectedNodes.Contains(node.NodeID))
                    {
                        selectedNodes.Pop();
                    }
                    else
                    {
                        selectedNodes.Push(node.NodeID);
                    }
                }
                else
                {
                    selectedNodes.Clear();
                    selectedNodes.Push(node.NodeID);
                }
            }
        }
        
        // Draw edges
        foreach (var edge in roadNetwork.Edges)
        {
            var node1 = roadNetwork.GetNode(edge.StartNodeID);
            var node2 = roadNetwork.GetNode(edge.EndNodeID);
            var position1 = node1.Position;
            var position2 = node2.Position;
            Handles.color = Color.cyan;
            var middle = (position1 + position2) / 2;
            var size = HandleUtility.GetHandleSize(middle) * 0.25f;

            //edge is clicked on
            if (Handles.Button(middle, Quaternion.identity, size, size, Handles.CylinderHandleCap))
            {
                // if ctrl/cmd is pressed, split edge
                
                if (Event.current.control || Event.current.command)
                {
                    roadNetwork.InsertBetween(roadNetwork.GetNode(edge.StartNodeID), roadNetwork.GetNode(edge.EndNodeID), roadNetwork.CreateNode(middle));

                    roadGenerator.GenerateRoadNetwork();
                    return;
                }
            }
        }
                    
        // draw movement handle for the selection
        var positionOffset = Vector3.zero;
        if (selectedNodes.Count > 0)
        {
            var node = roadNetwork.GetNode(selectedNodes.Peek());
            positionOffset = Handles.PositionHandle(node.Position, Quaternion.identity) - node.Position;
            
            // move all selected nodes
            foreach (var nodeID in selectedNodes)
            {
                var nodeToMove = roadNetwork.GetNode(nodeID);
                roadNetwork.SetNodePosition(nodeID, nodeToMove.Position + positionOffset);
            }
            
            if(positionOffset != Vector3.zero)
            {
                roadGenerator.GenerateRoadNetwork();
            }
        }
        
        // Create an edge between the two selected nodes
        if (selectedNodes.Count == 2 && Event.current.isKey && Event.current.keyCode == KeyCode.E)
        {
            var node1 = roadNetwork.GetNode(selectedNodes.Pop());
            var node2 = roadNetwork.GetNode(selectedNodes.Pop());
            roadNetwork.AddEdge(new RoadEdge(node1.NodeID, node2.NodeID, 4f));
            roadGenerator.GenerateRoadNetwork();
        }
        
        // Delete the selected nodes
        if (Event.current.isKey && (Event.current.keyCode == KeyCode.Delete || (Event.current.keyCode == KeyCode.Backspace && Event.current.command))&& selectedNodes.Count > 0)
        {
            // remove the event so we don't delete the object
            Event.current.Use();
            foreach (var nodeID in selectedNodes)
            {
                roadNetwork.RemoveNode(nodeID);
            }
            selectedNodes.Clear();
            roadGenerator.GenerateRoadNetwork();
        }
        
        // create a new connected node
        if (Event.current.isKey && Event.current.keyCode == KeyCode.N && selectedNodes.Count == 1)
        {
            Event.current.Use();
            Event.current.Use();
            var node = roadNetwork.GetNode(selectedNodes.Peek());
            
            // raycast to the ground
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                var newNode = roadNetwork.CreateNode(hit.point+Vector3.up*0.5f);
                roadNetwork.AddEdge(new RoadEdge(node.NodeID, newNode.NodeID, 4f));
                roadGenerator.GenerateRoadNetwork();
                
                // select the new node
                selectedNodes.Clear();
                selectedNodes.Push(newNode.NodeID);
                Debug.Log("Created new node");
            }
        }
    }
}
