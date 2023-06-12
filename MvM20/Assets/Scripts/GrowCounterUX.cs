using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GrowCounterUX : MonoBehaviour
{
    [SerializeField] Slider GrowCounter;
    private List<float> newCountValues = new List<float>();
    private readonly float changeTime = .1f;
    public UnityEvent sliderFull = new UnityEvent();

    private void Update()
    {
        if (newCountValues.Count > 0)
        {
            GrowCounter.value = Mathf.Lerp(GrowCounter.value, newCountValues[0], changeTime);
            if (newCountValues[0] - GrowCounter.value <= .05f)
            {
                GrowCounter.value = newCountValues[0];
                newCountValues.RemoveAt(0);
            }
        }

        if (GrowCounter.value == GrowCounter.maxValue)
        {
            sliderFull.Invoke();
        }
    }
    public void SetMaxGrow(float max)
    {
        GrowCounter.value = 0;
        GrowCounter.maxValue = max;
    }

    public void SetGrowCounter(float growCount)
    {
        newCountValues.Add(growCount);
    }
}
