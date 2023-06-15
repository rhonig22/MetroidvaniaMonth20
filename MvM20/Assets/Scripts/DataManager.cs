using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    [SerializeField] GameObject HUD;
    [SerializeField] GameObject StartScreen;
    [SerializeField] GameObject EndScreen;
    [SerializeField] GameObject Tips;
    [SerializeField] PlayerController playerController;
    [SerializeField] GrowCounterUX growCounterUX;
    [SerializeField] SizeIndicatorUX sizeIndicatorUX;
    [SerializeField] TipUxManager tipManager;
    [SerializeField] Animator creditsAnimator;

    public static DataManager Instance { get; private set; }
    public static int GrowCount { get; private set; } = 0;
    public static int GrowPowerUps { get; private set; } = 0;
    public static int NextThreshold { get; private set; } = 2;
    public static bool GameStarted { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        // Set up singleton
        /*if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }*/

        Instance = this;
        GrowCount = 0;
        GrowPowerUps = 0;
        NextThreshold = 2;
        GameStarted = false;

        // DontDestroyOnLoad(gameObject);
        growCounterUX.SetMaxGrow(NextThreshold);
        growCounterUX.sliderFull.AddListener(() => { Instance.HitThreshhold(); });
        StartScreen.SetActive(true);
    }

    public void IncreaseGrowPowerUps()
    {
        GrowPowerUps += 1;
        growCounterUX.SetGrowCounter(GrowPowerUps);
    }

    public void StartGame()
    {
        GameStarted = true;
        StartScreen.SetActive(false);
        HUD.SetActive(true);
    }

    public void EndGame()
    {
        GameStarted = true;
        EndScreen.SetActive(true);
        HUD.SetActive(false);
        creditsAnimator.SetTrigger("Credits");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowGroundPoundTip()
    {
        Tips.SetActive(true);
        tipManager.ShowGroundPoundTip();
    }

    public void ShowReboundTip()
    {
        Tips.SetActive(true);
        tipManager.ShowReboundTip();
    }

    private void HitThreshhold()
    {
        GrowCount++;
        NextThreshold *= 2;
        GrowPowerUps = 0;
        growCounterUX.SetMaxGrow(NextThreshold);
        sizeIndicatorUX.AddSizeIndicator();
        playerController.IncreaseGrowLogic();
    }
}
