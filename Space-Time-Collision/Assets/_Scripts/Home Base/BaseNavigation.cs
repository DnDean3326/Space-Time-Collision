using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BaseNavigation : MonoBehaviour
{
    [SerializeField] private Button voidButton;
    [SerializeField] private Button innButton;
    
    private const string TEST_BATTLE = "BattleScene";
    private const string INN_SCENE = "InnScene";

    private PartyManager partyManager;
    private PlayerPrefs playerPrefs;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        playerPrefs = FindFirstObjectByType<PlayerPrefs>();
    }
    
    private void Start()
    {
        if (playerPrefs.GetRunStatus() > 0) {
            innButton.interactable = false;
        }

        if (partyManager.GetCurrentParty().Count == 0) {
            voidButton.interactable = false;
        }
    }
    
    // OnClick Methods

    public void VoidClick()
    {
        playerPrefs.IncreaseRunStatus();
        SceneManager.LoadScene(TEST_BATTLE);
    }
    
    public void InnClick()
    {
        SceneManager.LoadScene(INN_SCENE);
    }
}
