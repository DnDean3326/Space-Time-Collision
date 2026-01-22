using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PartyPositionFunction : MonoBehaviour
{
    [SerializeField] private List<GameObject> partyDisplays;
    [SerializeField] private List<Transform> gridTransforms;
    [SerializeField] private List<Button> gridButtons;
    [SerializeField] private Button confirmationButton;
    
    private const string BASE_SCENE = "BaseScene";
    private const string INN_SCENE = "InnScene";
    
    private PartyManager partyManager;
    private List<Image> partyPortraits = new List<Image>();
    private List<Image> partyBorders = new List<Image>();
    private List<GameObject> confirmationIcon = new List<GameObject>();
    private List<PartyMember> activeParty;
    private List<bool> isOccupied = new List<bool>();
    
    private PartyMember selectedMember = null;
    
    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();

        foreach (var partyDisplay in partyDisplays) {
            partyPortraits.Add(partyDisplay.transform.GetChild(0).GetComponent<Image>());
            partyBorders.Add(partyDisplay.transform.GetChild(1).GetComponent<Image>());
            confirmationIcon.Add(partyDisplay.transform.GetChild(2).gameObject);
        }

        foreach (var button in gridButtons) {
            isOccupied.Add(false);
        }
    }

    private void Start()
    {
        activeParty = partyManager.GetCurrentParty();

        for (int i = 0; i < activeParty.Count; i++) {
            if (activeParty[i].xPos == 0 || activeParty[i].yPos == 0) {
                confirmationIcon[i].SetActive(false);
            }

            partyPortraits[i].sprite = activeParty[i].memberSquarePortrait;
        }
        if (activeParty.Count < 4) {
            for (int i = activeParty.Count; i < partyDisplays.Count; i++) {
                partyDisplays[i].SetActive(false);
            }
        }

        ButtonStatus();
    }

    private void ButtonStatus()
    {
        if (selectedMember == null) {
            for (int i = 0; i < gridButtons.Count; i++) {
                var button = gridButtons[i];
                if (isOccupied[i]) {
                    button.interactable = true;
                    continue;
                }
                button.interactable = false;
            }
        } else {
            for (int i = 0; i < gridButtons.Count; i++) {
                var button = gridButtons[i];
                if (isOccupied[i]) {
                    button.interactable = false;
                    continue;
                }
                button.interactable = true;
            }
        }

        if (activeParty.Any(t => t.xPos == 0 || t.yPos == 0)) {
            confirmationButton.interactable = false;
        } else {
            confirmationButton.interactable = true;
        }

        for (int i = 0; i < activeParty.Count; i++) {
            if (activeParty[i].xPos == 0 || activeParty[i].yPos == 0) {
                confirmationIcon[i].SetActive(false);
            } else {
                confirmationIcon[i].SetActive(true);
            }
        }
    }
    
    // OnClick Methods

    public void ToPartySelect()
    {
        SceneManager.LoadScene(INN_SCENE);
    }
    
    public void PartyButton(int index)
    {
        if (selectedMember != activeParty[index] && (activeParty[index].xPos == 0 && activeParty[index].yPos == 0)) {
            selectedMember = activeParty[index];
            partyBorders[index].color = Color.gray3;
        } else if (activeParty[index].xPos != 0 && activeParty[index].yPos != 0) {
            int gridIndex = 16;
            if (activeParty[index].xPos == 1 && activeParty[index].yPos == 1) {
                gridIndex = 0;
            } else if (activeParty[index].xPos == 2 && activeParty[index].yPos == 1) {
                gridIndex = 1;
            } else if (activeParty[index].xPos == 3 && activeParty[index].yPos == 1) {
                gridIndex = 2;
            } else if (activeParty[index].xPos == 4 && activeParty[index].yPos == 1) {
                gridIndex = 3;
            } else if (activeParty[index].xPos == 1 && activeParty[index].yPos == 2) {
                gridIndex = 4;
            } else if (activeParty[index].xPos == 2 && activeParty[index].yPos == 2) {
                gridIndex = 5;
            } else if (activeParty[index].xPos == 3 && activeParty[index].yPos == 2) {
                gridIndex = 6;
            } else if (activeParty[index].xPos == 4 && activeParty[index].yPos == 2) {
                gridIndex = 7;
            } else if (activeParty[index].xPos == 1 && activeParty[index].yPos == 3) {
                gridIndex = 8;
            } else if (activeParty[index].xPos == 2 && activeParty[index].yPos == 3) {
                gridIndex = 9;
            } else if (activeParty[index].xPos == 3 && activeParty[index].yPos == 3) {
                gridIndex = 10;
            } else if (activeParty[index].xPos == 4 && activeParty[index].yPos == 3) {
                gridIndex = 11;
            } else if (activeParty[index].xPos == 1 && activeParty[index].yPos == 4) {
                gridIndex = 12;
            } else if (activeParty[index].xPos == 2 && activeParty[index].yPos == 4) {
                gridIndex = 13;
            } else if (activeParty[index].xPos == 3 && activeParty[index].yPos == 4) {
                gridIndex = 14;
            } else if (activeParty[index].xPos == 4 && activeParty[index].yPos == 4) {
                gridIndex = 15;
            }
            if (gridIndex == 16) {
                return;
            }
            foreach (Transform child in gridTransforms[gridIndex])
            {
                Destroy(child.gameObject);
            }
            activeParty[index].xPos = 0;
            activeParty[index].yPos = 0;
            isOccupied[gridIndex] = false;
        } else if (selectedMember == activeParty[index]) {
            selectedMember = null;
            partyBorders[index].color = Color.white;
        }

        ButtonStatus();
    }

    public void GridButton(int index)
    {
        if (selectedMember != null) {
            selectedMember.xPos = (index % 4) + 1;
            selectedMember.yPos = (Mathf.FloorToInt((float)index / 4) + 1);
            
            if (selectedMember != null) {
                foreach (Transform child in gridTransforms[index])
                {
                    Destroy(child.gameObject);
                }
            }
            if (selectedMember != null) {
                GameObject tempVisualObject = Instantiate(selectedMember.allyBattleVisualPrefab, gridTransforms[index]);
                BattleVisuals tempBattleVisuals = tempVisualObject.GetComponent<BattleVisuals>();
                tempBattleVisuals.SetMyOrder(4 - Mathf.FloorToInt((float)index / 4) + 1);
                tempBattleVisuals.DisableUIBar();
                isOccupied[index] = true;
            }

            partyBorders[activeParty.IndexOf(selectedMember)].color = Color.white;
            selectedMember = null;
        } else if (isOccupied[index]) {
            foreach (Transform child in gridTransforms[index])
            {
                Destroy(child.gameObject);
            }
            int memberIndex = activeParty.FindIndex(t => t.xPos == (index % 4) + 1 && t.yPos == (Mathf.FloorToInt((float)index / 4) + 1));
            activeParty[memberIndex].xPos = 0;
            activeParty[memberIndex].yPos = 0;
            isOccupied[index] = false;
        }
        
        ButtonStatus();
    }
    
    public void ConfirmPositions()
    {
        if (activeParty.Any(t => t.xPos == 0 || t.yPos == 0)) {
            print("One or more party members does not have their position set!");
            return;
        }
        SceneManager.LoadScene(BASE_SCENE);
    }
    
    // OnPointerEnter Methods

    public void GridPreview(int index)
    {
        if (selectedMember != null && !isOccupied[index]) {
            GameObject tempVisualObject = Instantiate(selectedMember.allyBattleVisualPrefab, gridTransforms[index]);
            BattleVisuals tempBattleVisuals = tempVisualObject.GetComponent<BattleVisuals>();
            tempBattleVisuals.SetMyOrder(4 - Mathf.FloorToInt((float)index / 4) + 1);
            tempBattleVisuals.DisableUIBar();
            tempBattleVisuals.SetSharedRowAnimation(true);
        }
    }
    
    // OnPointerExit Methods

    public void HideGridPreview(int index)
    {
        if (selectedMember != null) {
            
            foreach (Transform child in gridTransforms[index])
            {
                if (!isOccupied[index]) {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
