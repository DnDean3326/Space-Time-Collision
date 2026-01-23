using Unity.Cinemachine;
using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    private CinemachineCamera mainCamera;
    private CinemachineCamera playerVsEnemyCamera;
    private CinemachineCamera playerToPlayerCamera;
    private CinemachineCamera enemyToEnemyCamera;

    private void Awake()
    {
        mainCamera = transform.GetChild(0).GetComponent<CinemachineCamera>();
        playerVsEnemyCamera = transform.GetChild(1).GetComponent<CinemachineCamera>();
        playerToPlayerCamera = transform.GetChild(2).GetComponent<CinemachineCamera>();
        enemyToEnemyCamera = transform.GetChild(3).GetComponent<CinemachineCamera>();

        SetMainCamera();
    }

    public void SetMainCamera()
    {
        mainCamera.gameObject.SetActive(true);
        playerVsEnemyCamera.gameObject.SetActive(false);
        playerToPlayerCamera.gameObject.SetActive(false);
        enemyToEnemyCamera.gameObject.SetActive(false);
    }
    
    public void SetPlayerVsEnemyCamera()
    {
        mainCamera.gameObject.SetActive(false);
        playerVsEnemyCamera.gameObject.SetActive(true);
        playerToPlayerCamera.gameObject.SetActive(false);
        enemyToEnemyCamera.gameObject.SetActive(false);
    }
    
    public void SetPlayerToPlayerCamera()
    {
        mainCamera.gameObject.SetActive(false);
        playerVsEnemyCamera.gameObject.SetActive(false);
        playerToPlayerCamera.gameObject.SetActive(true);
        enemyToEnemyCamera.gameObject.SetActive(false);
    }
    
    public void SetEnemyToEnemyCamera()
    {
        mainCamera.gameObject.SetActive(false);
        playerVsEnemyCamera.gameObject.SetActive(false);
        playerToPlayerCamera.gameObject.SetActive(false);
        enemyToEnemyCamera.gameObject.SetActive(true);
    }
}
