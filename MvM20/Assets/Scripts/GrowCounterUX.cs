using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrowCounterUX : MonoBehaviour
{
    [SerializeField] Slider GrowCounter;
    private List<float> newCountValues = new List<float>();
    private List<float> newMaxValues = new List<float>();
    private float countVelocity = 0;
    private readonly float changeTime = .25f;

    private void Update()
    {
        if (newCountValues.Count > 0)
        {
            if (newCountValues[0] > 0)
            {
                GrowCounter.value = Mathf.SmoothDamp(GrowCounter.value, newCountValues[0], ref countVelocity, changeTime);
                if (GrowCounter.value + .01f >= newCountValues[0])
                {
                    newCountValues.RemoveAt(0);
                }
            }
            else
            {
                GrowCounter.value = newCountValues[0];
                countVelocity = 0;
                newCountValues.RemoveAt(0);
            }
        }
        else if (newMaxValues.Count > 0)
        {
            GrowCounter.maxValue = newMaxValues[0];
            newMaxValues.RemoveAt(0);
        }
    }
    public void SetMaxGrow(float max)
    {
        newMaxValues.Add(max);
    }

    public void SetGrowCounter(float growCount)
    {
        newCountValues.Add(growCount);
    }
}
