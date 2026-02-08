using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BattleCameraController : MonoBehaviour
{
    private CinemachineCamera mainCamera;
    private CinemachineCamera playerVsEnemyCamera;
    private CinemachineCamera playerToPlayerCamera;
    private CinemachineCamera enemyToEnemyCamera;
    private SpriteRenderer zoomSprite;
    private BattleSystem battleSystem;

    [SerializeField] private GameObject mainBackground;
    [SerializeField] private GameObject blackBackground;
    [SerializeField] private GameObject zoomBackground;
    [SerializeField] private Sprite clearBackgroundSprite;
    [SerializeField] private Sprite blurryBackgroundSprite;

    private float cameraMoveDelay;

    private void Awake()
    {
        mainCamera = transform.GetChild(0).GetComponent<CinemachineCamera>();
        playerVsEnemyCamera = transform.GetChild(1).GetComponent<CinemachineCamera>();
        playerToPlayerCamera = transform.GetChild(2).GetComponent<CinemachineCamera>();
        enemyToEnemyCamera = transform.GetChild(3).GetComponent<CinemachineCamera>();
        battleSystem = FindFirstObjectByType<BattleSystem>();
        zoomSprite = zoomBackground.GetComponent<SpriteRenderer>();

        SetMainCamera();
    }

    private void Start()
    {
        mainBackground.SetActive(true);
        blackBackground.SetActive(false);
        zoomBackground.SetActive(false);
        cameraMoveDelay = battleSystem.GetCameraMoveSpeed();
    }
    
    public void SetMainCamera()
    {
        StartCoroutine(MainCamera());
    }
    
    public void SetPlayerVsEnemyCamera()
    {
        StartCoroutine(PlayerVsEnemyCamera());
    }
    
    public void SetPlayerToPlayerCamera()
    {
        StartCoroutine(PlayerToPlayerCamera());
    }
    
    public void SetEnemyToEnemyCamera()
    {
        StartCoroutine(EnemyToEnemyCamera());
    }
    
    private IEnumerator MainCamera()
    {
        mainCamera.gameObject.SetActive(true);
        playerVsEnemyCamera.gameObject.SetActive(false);
        playerToPlayerCamera.gameObject.SetActive(false);
        enemyToEnemyCamera.gameObject.SetActive(false);
        mainBackground.SetActive(true);
        blackBackground.SetActive(false);
        zoomBackground.SetActive(false);
        yield break;
    }

    private IEnumerator PlayerVsEnemyCamera()
    {
        mainCamera.gameObject.SetActive(false);
        playerVsEnemyCamera.gameObject.SetActive(true);
        playerToPlayerCamera.gameObject.SetActive(false);
        enemyToEnemyCamera.gameObject.SetActive(false);
        blackBackground.SetActive(true);
        zoomSprite.sprite = blurryBackgroundSprite;
        zoomBackground.SetActive(true);
        yield return new WaitForSeconds(cameraMoveDelay);
        zoomSprite.sprite = clearBackgroundSprite;
        mainBackground.SetActive(false);
    }
    
    private IEnumerator PlayerToPlayerCamera()
    {
        mainCamera.gameObject.SetActive(false);
        playerVsEnemyCamera.gameObject.SetActive(true);
        playerToPlayerCamera.gameObject.SetActive(false);
        enemyToEnemyCamera.gameObject.SetActive(false);
        blackBackground.SetActive(true);
        zoomSprite.sprite = blurryBackgroundSprite;
        zoomBackground.SetActive(true);
        yield return new WaitForSeconds(cameraMoveDelay);
        zoomSprite.sprite = clearBackgroundSprite;
        mainBackground.SetActive(false);
    }

    private IEnumerator EnemyToEnemyCamera()
    {
        mainCamera.gameObject.SetActive(false);
        playerVsEnemyCamera.gameObject.SetActive(true);
        playerToPlayerCamera.gameObject.SetActive(false);
        enemyToEnemyCamera.gameObject.SetActive(false);
        blackBackground.SetActive(true);
        zoomSprite.sprite = blurryBackgroundSprite;
        zoomBackground.SetActive(true);
        yield return new WaitForSeconds(cameraMoveDelay);
        zoomSprite.sprite = clearBackgroundSprite;
        mainBackground.SetActive(false);
    }
}
