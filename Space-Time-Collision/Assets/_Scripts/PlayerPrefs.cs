using System;
using UnityEngine;

public class PlayerPrefs : MonoBehaviour
{
    public bool didTutorial = false;
    
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

    public bool DidTutorial()
    {
        return didTutorial;
    }

    public void SetTutorialStatus(bool status)
    {
        didTutorial = status;
    }
}
