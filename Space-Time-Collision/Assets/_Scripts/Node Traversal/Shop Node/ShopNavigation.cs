using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopNavigation : MonoBehaviour
{
    private const string NODE_SCENE = "NodeScene";
    
    public void BackToNodes()
    {
        SceneManager.LoadScene(NODE_SCENE);
    }
}
