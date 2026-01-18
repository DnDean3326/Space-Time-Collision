using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InnFunctionality : MonoBehaviour
{
    [SerializeField] private GameObject allySelection;
    [SerializeField] private GameObject allyPrefab;
    [SerializeField] private GameObject allyPreview;
    [SerializeField] private Image allySprite;
    [SerializeField] private Image allyShadowPortrait;
    [SerializeField] private Image allyName;
    
    private PartyManager partyManager;
    
    private List<AllyInfo> allyList;
    
    private const string BASE_SCENE = "BaseScene";
    private const int FULL_ROSTER = 6;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
    }

    private void Start()
    {
        allyList = partyManager.GetAllAllies();
        CreateAllyList();
    }

    private void CreateAllyList()
    {
        int unlockedAllyCount = 0;
        
        foreach (AllyInfo allyInfo in allyList) {
            GameObject newAlly = Instantiate(allyPrefab, allySelection.transform);
            newAlly.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = allyInfo.allySquarePortrait;
            AllySelectButton tempButton = newAlly.GetComponent<AllySelectButton>();
            tempButton.SetMyAlly(allyInfo);
            
            unlockedAllyCount++;
        }

        if (unlockedAllyCount < FULL_ROSTER) {
            for (int i = unlockedAllyCount; i < FULL_ROSTER; i++) {
                GameObject newAlly = Instantiate(allyPrefab, allySelection.transform);
                newAlly.transform.GetChild(1).gameObject.SetActive(false);
                newAlly.GetComponent<Button>().enabled = false;
            }
        }
    }

    public void DisplayAllyInfo(AllyInfo ally)
    {
        if (!allyPreview.activeSelf) {
            allyPreview.SetActive(true);
        }
        allySprite.sprite = ally.allyCombatSprite;
        allyShadowPortrait.sprite = ally.allyShadowPortrait;
        if (ally.allySignature != null) {
            allyName.gameObject.SetActive(true);
            allyName.sprite = ally.allySignature;
        } else {
            allyName.gameObject.SetActive(false);
        }
    }
    
    // OnClick Methods
    
    public void ConfirmParty()
    {
        SceneManager.LoadScene(BASE_SCENE);
    }
}
