using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;
    
    private bool pauseActive;
    private bool inAbilities;
    private bool inTargeting;

    private void Start()
    {
        pauseActive = false;
        
        pauseUI.SetActive(false);
        Time.timeScale = 1;
    }
    
    private void OnPause(InputValue value)
    {
        if (value.isPressed) {
            pauseActive = !pauseActive;
        }

        if (pauseActive) {
            pauseUI.SetActive(true);
            Time.timeScale = 0;
        } else {
            pauseUI.SetActive(false);
            Time.timeScale = 1;
        }
        
        Tooltip.HideTooltip_Static();
    }

    public void Resume()
    {
        if (pauseActive) {
            pauseActive = false;
            
            pauseUI.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
