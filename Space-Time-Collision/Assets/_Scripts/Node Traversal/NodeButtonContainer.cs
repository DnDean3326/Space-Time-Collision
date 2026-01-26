using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeButtonContainer : MonoBehaviour
{
    [SerializeField] private List<Button> nodeButtons;

    public List<Button> GetNodeButtons()
    {
        return nodeButtons;
    }
}
