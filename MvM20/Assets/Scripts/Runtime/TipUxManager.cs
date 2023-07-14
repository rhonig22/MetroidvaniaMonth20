using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TipUxManager : MonoBehaviour
{
    [SerializeField] GameObject groundPoundTip;
    [SerializeField] GameObject reboundTip;
    private float countdown = 0;
    private readonly float countdownTime = 5f;

    private void Update()
    {
        if (countdown > 0)
        {
            countdown -= Time.deltaTime;
        }
        else if (Input.anyKeyDown)
        {
            Close();
        }
    }

    public void ShowGroundPoundTip()
    {
        groundPoundTip.SetActive(true);
        countdown = countdownTime;
    }

    public void ShowReboundTip()
    {
        reboundTip.SetActive(true);
        countdown = countdownTime;
    }

    public void Close()
    {
        groundPoundTip.SetActive(false);
        reboundTip.SetActive(false);
        gameObject.SetActive(false);
    }
}
