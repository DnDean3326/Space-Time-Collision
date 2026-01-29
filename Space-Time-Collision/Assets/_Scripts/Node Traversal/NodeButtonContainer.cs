using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeButtonContainer : MonoBehaviour
{
    [SerializeField] private List<Button> nodeButtons;
    private NodeManager nodeManager;

    private void Awake()
    {
        nodeManager = FindFirstObjectByType<NodeManager>();
    }
    
    public List<Button> GetNodeButtons()
    {
        return nodeButtons;
    }

    public void NodeButtonPress(int nodeIndex)
    {
        nodeManager.NodeClick(nodeIndex);
    }
}
