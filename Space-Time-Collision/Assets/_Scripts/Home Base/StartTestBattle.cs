using UnityEngine;
using UnityEngine.SceneManagement;

public class StartTestBattle : MonoBehaviour
{

    private const string TEST_BATTLE = "BattleScene";

    public void StartTestBattleButton()
    {
        SceneManager.LoadScene(TEST_BATTLE);
    }
}
