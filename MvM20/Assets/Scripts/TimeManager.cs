using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private float slowdownFactor = 0.05f;
    private float slowdownLength = 2f;

    void Update()
    {
        Time.timeScale += (1f / slowdownLength) * Time.unscaledDeltaTime;
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }

    public void DoSlowmotion(float factor, float duration)
    {
        slowdownFactor = factor;
        slowdownLength = duration;
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }
}