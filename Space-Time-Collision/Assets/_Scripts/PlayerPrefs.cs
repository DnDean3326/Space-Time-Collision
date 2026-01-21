using System;
using UnityEngine;

public class PlayerPrefs : MonoBehaviour
{
    private bool didTutorial = false;
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

    public bool GetDidTutorial()
    {
        return didTutorial;
    }

    public int GetRunStatus()
    {
        return runStatus;
    }

    public void SetTutorialStatus(bool status)
    {
        didTutorial = status;
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
