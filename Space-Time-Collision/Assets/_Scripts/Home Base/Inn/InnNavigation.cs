using UnityEngine;
using UnityEngine.SceneManagement;

public class InnNavigation : MonoBehaviour
{
    private const string BASE_SCENE = "BaseScene";
    
    public void ConfirmParty()
    {
        SceneManager.LoadScene(BASE_SCENE);
    }
}
