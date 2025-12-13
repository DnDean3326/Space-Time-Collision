using UnityEngine;
using UnityEngine.SceneManagement;

public class StartTestBattle : MonoBehaviour
{

    private const string TEST_BATTLE = "TestBattleScene";

    public void StartTestBattleButton()
    {
        SceneManager.LoadScene(TEST_BATTLE);
    }
}
