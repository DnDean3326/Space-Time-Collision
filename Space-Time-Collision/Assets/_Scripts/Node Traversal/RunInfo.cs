using System.Collections.Generic;
using UnityEngine;

public class RunInfo : MonoBehaviour
{
    private int encounterCount;
    private int eventCount;
    private int funds;

    private List<NodeInfo> regionNodes;
    private NodeManager nodeManager;
    
    private static GameObject _instance;

    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        DontDestroyOnLoad(gameObject);
        
        nodeManager = FindFirstObjectByType<NodeManager>();
    }

    private void Start()
    {
        if (regionNodes == null) {
            regionNodes = new List<NodeInfo>();
            nodeManager.GetNewNodeList();
        }
    }


    public List<NodeInfo> ProvideRegionNodes()
    {
        return regionNodes;
    }
}
