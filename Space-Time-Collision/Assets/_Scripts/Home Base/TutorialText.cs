using System;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private GameObject tutorialBox;

    private void Start()
    {
        tutorialBox.SetActive(true);
        Time.timeScale = 0;
    }
    
    public void CloseTutorialBox()
    {
        tutorialBox.SetActive(false);
        Time.timeScale = 1;
    }
}
