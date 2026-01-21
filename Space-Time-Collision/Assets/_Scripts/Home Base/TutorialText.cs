using System;
using UnityEditorInternal;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private GameObject tutorialBox;
    private PlayerPrefs playerPrefs;

    private void Awake()
    {
        playerPrefs = FindFirstObjectByType<PlayerPrefs>();
    }

    private void Start()
    {
        if (!playerPrefs.GetDidTutorial()) {
            tutorialBox.SetActive(true);
            Time.timeScale = 0;
        } else {
            tutorialBox.SetActive(false);
        }
    }
    
    public void CloseTutorialBox()
    {
        playerPrefs.SetTutorialStatus(true);
        Destroy(tutorialBox);
        Time.timeScale = 1;
    }
}
