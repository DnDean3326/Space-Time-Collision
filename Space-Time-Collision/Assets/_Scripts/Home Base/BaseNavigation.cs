using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseNavigation : MonoBehaviour
{
    private const string TEST_BATTLE = "BattleScene";
    private const string INN_SCENE = "InnScene";
    
    // OnClick Methods

    public void VoidButton()
    {
        SceneManager.LoadScene(TEST_BATTLE);
    }
    
    public void InnButton()
    {
        SceneManager.LoadScene(INN_SCENE);
    }
}
