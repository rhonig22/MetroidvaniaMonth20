using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] GrowCounterUX growCounterUX;

    public static DataManager Instance { get; private set; }
    public static int GrowCount { get; private set; } = 0;
    public static int GrowPowerUps { get; private set; } = 0;
    public static int NextThreshold { get; private set; } = 2;

    // Start is called before the first frame update
    void Start()
    {
        // Set up singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        growCounterUX.SetMaxGrow(NextThreshold);
    }

    public void IncreaseGrowPowerUps()
    {
        GrowPowerUps += 1;
        growCounterUX.SetGrowCounter(GrowPowerUps);
        if (GrowPowerUps == NextThreshold)
            HitThreshhold();
    }

    private void HitThreshhold()
    {
        GrowCount++;
        NextThreshold *= 2;
        GrowPowerUps = 0;
        growCounterUX.SetGrowCounter(GrowPowerUps);
        growCounterUX.SetMaxGrow(NextThreshold);
        playerController.IncreaseGrowLogic();
    }
}
