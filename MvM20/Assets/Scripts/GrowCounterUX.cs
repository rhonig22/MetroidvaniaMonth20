using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowCounterUX : MonoBehaviour
{
    [SerializeField] Slider GrowCounter;

    public void SetMaxGrow(float max)
    {
        GrowCounter.maxValue = max;
    }

    public void SetGrowCounter(float growCount)
    {
        GrowCounter.value = growCount;
    }
}
