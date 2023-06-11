using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    private PlayerController playerController;
    private CinemachineBasicMultiChannelPerlin noisePerlin;
    private readonly float orthoSize = 5;
    private readonly float minZoom = 0;
    private readonly float maxZoom = 10;
    private readonly float zoomSmoothTime = .25f;
    private readonly float shakeAmplitude = .5f;
    private readonly float shakeFrequency = 2f;
    private readonly float shakeTime = .25f;
    private float shakeTimeElapsed = 0;
    private float zoomVelocity = 0;
    private float currentZoom = 0;
    private bool isShaking = false;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        currentZoom = mainCamera.m_Lens.OrthographicSize;
        playerController.triggerScreenShake.AddListener(() => { ShakeCamera(); });
        noisePerlin = mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (isShaking)
        {
            shakeTimeElapsed+= Time.deltaTime;
            if (shakeTimeElapsed > shakeTime) {
                StopShake();
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ScaleCameraZoom();
    }

    void ShakeCamera()
    {
        noisePerlin.m_AmplitudeGain = shakeAmplitude*DataManager.GrowCount;
        noisePerlin.m_FrequencyGain= shakeFrequency;
        shakeTimeElapsed = 0;
        isShaking = true;
    }

    void StopShake()
    {
        noisePerlin.m_AmplitudeGain = 0;
        noisePerlin.m_FrequencyGain = 0;
        isShaking = false;
    }

    void ScaleCameraZoom()
    {
        currentZoom = orthoSize + DataManager.GrowCount;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        mainCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(mainCamera.m_Lens.OrthographicSize, currentZoom, ref zoomVelocity, zoomSmoothTime);
    }
}
