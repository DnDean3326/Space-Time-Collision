using UnityEngine;
using UnityEngine.SceneManagement;

public class TempTransport : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene("BaseScene");
    }
}
