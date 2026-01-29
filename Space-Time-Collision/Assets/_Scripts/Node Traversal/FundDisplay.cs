using System;
using TMPro;
using UnityEngine;

public class FundDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fundText;
    private RunInfo runInfo;

    private void Awake()
    {
        runInfo = FindFirstObjectByType<RunInfo>();
    }

    private void Start()
    {
        fundText.text = "$" + runInfo.GetFunds();
    }
}
