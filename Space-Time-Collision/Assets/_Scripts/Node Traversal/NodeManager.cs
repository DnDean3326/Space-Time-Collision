using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NodeManager : MonoBehaviour
{
    private List<NodeInfo> nodeInfos = new List<NodeInfo>();
    private int nodeCount = 0;
    private RunInfo runInfo;
    private Button[] nodeButtons;

    private void Awake()
    {
        runInfo = FindFirstObjectByType<RunInfo>();
    }

    private void Start()
    {
        nodeInfos = runInfo.ProvideRegionNodes();
        for (int i = 0; i < nodeButtons.Length; i++) {
            if (nodeInfos[i]._connectedNodes.All(t => t != i)) {
                nodeButtons[i].interactable = false;
            }
        }
    }

    private void GenerateSetNodes()
    {
        nodeInfos.Clear();
        
        List<int> connectedNodes = new List<int> {1};
        AddNode(NodeInfo.NodeType.Start, true, connectedNodes); // Node 0

        connectedNodes = new List<int> {2, 3};
        AddNode(NodeInfo.NodeType.Combat, false, connectedNodes); // Node 1
        
        connectedNodes = new List<int> {4};
        AddNode(NodeInfo.NodeType.Combat, false, connectedNodes); // Node 2
        AddNode(NodeInfo.NodeType.Event, false, connectedNodes); // Node 3
        
        connectedNodes = new List<int> {5, 6};
        AddNode(NodeInfo.NodeType.Combat, false, connectedNodes); // Node 4
        
        connectedNodes = new List<int> {7, 8};
        AddNode(NodeInfo.NodeType.Event, false, connectedNodes); // Node 5
        AddNode(NodeInfo.NodeType.Combat, false, connectedNodes); // Node 6
        
        connectedNodes = new List<int> {9};
        AddNode(NodeInfo.NodeType.Shop, false, connectedNodes); // Node 7
        AddNode(NodeInfo.NodeType.Event, false, connectedNodes); // Node 8
        
        connectedNodes = new List<int>();
        AddNode(NodeInfo.NodeType.Boss, false, connectedNodes); // Node 9
    }

    private void AddNode(NodeInfo.NodeType type, bool cleared, List<int> connections)
    {
        NodeInfo tempNode = new NodeInfo(nodeCount, type, cleared, connections);
        nodeInfos.Add(tempNode);
        nodeCount++;
    }
    
    public List<NodeInfo> GetNewNodeList()
    {
        GenerateSetNodes();
        return nodeInfos;
    }
}

public struct NodeInfo
{
    public enum NodeType
    {
        Start,
        Combat,
        MiniBoss,
        Boss,
        Event,
        Chest,
        Shop
    }
    
    public int _nodeNumber;
    public NodeType _nodeType;
    public bool _isCleared;
    public List<int> _connectedNodes;

    public NodeInfo(int nodeNumber, NodeType nodeType, bool isCleared, List<int> connectedNodes)
    {
        Debug.Log("Node " + nodeNumber + " created!");
        _nodeNumber = nodeNumber;
        _nodeType = nodeType;
        _isCleared = isCleared;
        _connectedNodes = connectedNodes;
        if (_connectedNodes.Any(t => t == nodeNumber)) {
            _connectedNodes.Remove(nodeNumber);
        }
    }

    public void ChangeClearStatus(bool isCleared)
    {
        _isCleared = isCleared;
    }
}
