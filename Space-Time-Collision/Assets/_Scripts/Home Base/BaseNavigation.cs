using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BaseNavigation : MonoBehaviour
{
    [SerializeField] private GameObject voidObject;
    [SerializeField] private GameObject innObject;
    
    private const string RUN_SCENE = "NodeScene";
    private const string TEST_BATTLE = "BattleScene";
    private const string INN_SCENE = "InnScene";

    private Button voidButton;
    private Button innButton;
    private InnAnimationScript innAnimationScript;
    private PartyManager partyManager;
    private PlayerPrefs playerPrefs;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        playerPrefs = FindFirstObjectByType<PlayerPrefs>();
        voidButton = voidObject.GetComponent<Button>();
        innButton = innObject.GetComponent<Button>();
        innAnimationScript = innObject.GetComponentInChildren<InnAnimationScript>();
    }
    
    private void Start()
    {
        if (playerPrefs.GetRunStatus() > 0) {
            innButton.interactable = false;
            innAnimationScript.SetInactive(true);
        }

        if (partyManager.GetCurrentParty().Count == 0) {
            voidButton.interactable = false;
        } else  {
            voidButton.interactable = true;
        }
    }
    
    // OnClick Methods

    public void VoidClick()
    {
        playerPrefs.IncreaseRunStatus();
        SceneManager.LoadScene(RUN_SCENE);
    }
    
    public void InnClick()
    {
        SceneManager.LoadScene(INN_SCENE);
    }
}
