using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeIndicatorUX : MonoBehaviour
{
    [SerializeField] private Image sizeIndicator;
    private readonly float posBuffer = 25f;
    private float currentPos = 25f;

    public void AddSizeIndicator()
    {
        var pip = Instantiate(sizeIndicator);
        pip.transform.SetParent(transform);
        pip.rectTransform.anchoredPosition = new Vector3(currentPos, 0, 0);
        currentPos += pip.rectTransform.rect.width + posBuffer;
    }
}
