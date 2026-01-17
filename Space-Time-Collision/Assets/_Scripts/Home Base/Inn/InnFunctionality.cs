using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InnFunctionality : MonoBehaviour
{
    [SerializeField] private GameObject allySelection;
    [SerializeField] private GameObject allyPrefab;
    
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
            
            unlockedAllyCount++;
        }

        if (unlockedAllyCount < FULL_ROSTER) {
            for (int i = unlockedAllyCount; i < FULL_ROSTER; i++) {
                GameObject newAlly = Instantiate(allyPrefab, allySelection.transform);
                newAlly.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }
    
    public void ConfirmParty()
    {
        SceneManager.LoadScene(BASE_SCENE);
    }
}
