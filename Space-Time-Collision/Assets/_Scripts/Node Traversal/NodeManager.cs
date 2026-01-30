using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NodeManager : MonoBehaviour
{
    private List<Button> nodeButtons;
    private List<NodeInfo> nodeInfos = new List<NodeInfo>();
    private int nodeCount = 0;
    private RunInfo runInfo;
    private EncounterSystem encounterSystem;
    private NodeButtonContainer buttonContainer;
    
    private static GameObject _instance;
    private const string NODE_SCENE = "NodeScene";
    private const string BATTLE_SCENE = "BattleScene";
    private const string EVENT_SCENE = "EventScene";
    private const string SHOP_SCENE = "ShopScene";

    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        runInfo = FindFirstObjectByType<RunInfo>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        runInfo.SetCurrentNode(1); // Default value is 0
        
        GenerateSetNodes();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != NODE_SCENE) { return; }

        buttonContainer = FindFirstObjectByType<NodeButtonContainer>();
        encounterSystem = FindFirstObjectByType<EncounterSystem>();
        
        nodeButtons = buttonContainer.GetNodeButtons();
        UpdateNodeButtons();
    }
    
    private void UpdateNodeButtons()
    {
        int currentNode = runInfo.GetCurrentNode();
        List<int> connectedNodes = nodeInfos[currentNode]._connectedNodes;
        for (int i = 0; i < nodeButtons.Count; i++) {
            var button = nodeButtons[i];
            if (connectedNodes.Contains(i)) {
                button.interactable = true;
            } else {
                button.interactable = false;
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
        AddNode(NodeInfo.NodeType.Shop, false, connectedNodes); // Node 3
        
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

    public void NodeClick(int nodeIndex)
    {
        runInfo.SetCurrentNode(nodeIndex);
        switch (nodeInfos[nodeIndex]._nodeType) {
            case NodeInfo.NodeType.Combat:
                encounterSystem.GenerateStandardEncounter();
                SceneManager.LoadScene(BATTLE_SCENE);
                break;
            case NodeInfo.NodeType.MiniBoss:
                encounterSystem.GenerateMinibossEncounter();
                SceneManager.LoadScene(BATTLE_SCENE);
                break;
            case NodeInfo.NodeType.Boss:
                // TODO Add boss spawn
                SceneManager.LoadScene(BATTLE_SCENE);
                break;
            case NodeInfo.NodeType.Event:
                SceneManager.LoadScene(EVENT_SCENE);
                break;
            case NodeInfo.NodeType.Shop:
                SceneManager.LoadScene(SHOP_SCENE);
                break;
        }
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
