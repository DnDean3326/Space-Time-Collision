using System;
using UnityEditorInternal;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private GameObject tutorialBox;
    [SerializeField] private GameObject victoryBox;
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

        if (playerPrefs.GetRunStatus() > 2) {
            victoryBox.SetActive(true);
            Time.timeScale = 0;
        } else {
            victoryBox.SetActive(false);
        }
    }
    
    public void CloseTutorialBox()
    {
        playerPrefs.SetTutorialStatus(true);
        Destroy(tutorialBox);
        Time.timeScale = 1;
    }

    public void CloseVictoryBox()
    {
        Destroy(victoryBox);
        Time.timeScale = 1;
    }

    public void SurveyLink()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSf-2VNk2eYvmdz76pQJCJaAQutHb3UHtDsMuBtV8ta4uV1-LA/viewform?usp=header");
    }
}
