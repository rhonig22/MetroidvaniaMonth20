using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GrowCounterUX : MonoBehaviour
{
    [SerializeField] Slider GrowCounter;
    private readonly float changeTime = .1f;
    private float timeCount = 0;
    private float initialValue = 0;
    private List<float> newCountValues = new List<float>();
    public UnityEvent sliderFull = new UnityEvent();

    private void Update()
    {
        if (newCountValues.Count > 0)
        {
            GrowCounter.value = Mathf.Lerp(initialValue, newCountValues[0], timeCount);
            timeCount += Time.deltaTime / changeTime;
            if (newCountValues[0] == GrowCounter.value)
            {
                initialValue = GrowCounter.value;
                timeCount= 0;
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
        initialValue = 0;
        GrowCounter.maxValue = max;
    }

    public void SetGrowCounter(float growCount)
    {
        newCountValues.Add(growCount);
    }
}
