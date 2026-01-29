using System;
using NUnit.Framework.Constraints;
using UnityEngine;

public class PlayerPrefs : MonoBehaviour
{
    [SerializeField] private bool publicDemo = false;
    [SerializeField] private bool didTutorial = false;
    [SerializeField] private bool didVoidTutorial = false;
    private int runStatus = 0;
    
    private static GameObject _instance;
    
    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        DontDestroyOnLoad(gameObject);
    }

    public bool CheckDemoStatus()
    {
        return publicDemo;
    }

    public bool GetDidTutorial()
    {
        return didTutorial;
    }

    public bool GetDidVoidTutorial()
    {
        return didVoidTutorial;
    }

    public int GetRunStatus()
    {
        return runStatus;
    }

    public void SetTutorialStatus(bool status)
    {
        didTutorial = status;
    }

    public void SetVoidTutorialStatus(bool status)
    {
        didVoidTutorial = status;
    }

    public void SetRunStatus(int status)
    {
        runStatus = status;
    }

    public void IncreaseRunStatus()
    {
        runStatus++;
    }
}
