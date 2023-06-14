using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera zoomOutCamera;
    [SerializeField] private Animator cameraAnimator;
    private PlayerController playerController;
    private CinemachineBasicMultiChannelPerlin followNoisePerlin;
    private CinemachineBasicMultiChannelPerlin zoomNoisePerlin;
    private readonly float orthoSize = 5;
    private readonly float shrinkSize = 5;
    private readonly float minZoom = 0;
    private readonly float maxZoom = 10;
    private readonly float zoomSmoothTime = .25f;
    private readonly float shakeAmplitude = 2f;
    private readonly float shakeFrequency = 2f;
    private readonly float shakeTime = .25f;
    private readonly float xBounds = 5.5f;
    private readonly float yBounds = 6f;
    private float shakeTimeElapsed = 0;
    private float zoomVelocity = 0;
    private float yVelocity = 0;
    private float xVelocity = 0;
    private float currentZoom = 0;
    private bool isShaking = false;
    private bool isFollowCam = true;
    private bool isPanX = false;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        currentZoom = mainCamera.m_Lens.OrthographicSize;
        playerController.triggerScreenShake.AddListener(() => { ShakeCamera(); });
        playerController.enterZoomedCamX.AddListener((xVal, yVal, orth) => { StartZoomedCam(xVal, yVal, orth, true); });
        playerController.enterZoomedCamY.AddListener((xVal, yVal, orth) => { StartZoomedCam(xVal, yVal, orth, false); });
        playerController.returnToFollow.AddListener(() => { StartFollowCam(); });
        followNoisePerlin = mainCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        zoomNoisePerlin = zoomOutCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        if (isShaking)
        {
            shakeTimeElapsed += Time.deltaTime;
            if (shakeTimeElapsed > shakeTime)
            {
                StopShake();
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ScaleCameraZoom();
        PanZoomedCam();
    }

    private void ShakeCamera()
    {
        var noisePerlin = isFollowCam ? followNoisePerlin : zoomNoisePerlin;
        noisePerlin.m_AmplitudeGain = shakeAmplitude * DataManager.GrowCount;
        noisePerlin.m_FrequencyGain = shakeFrequency;
        shakeTimeElapsed = 0;
        isShaking = true;
    }

    private void StopShake()
    {
        var noisePerlin = isFollowCam ? followNoisePerlin : zoomNoisePerlin;
        noisePerlin.m_AmplitudeGain = 0;
        noisePerlin.m_FrequencyGain = 0;
        isShaking = false;
    }

    private void ScaleCameraZoom()
    {
        currentZoom = orthoSize + (DataManager.GrowCount < shrinkSize ? DataManager.GrowCount : 0);
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        mainCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(mainCamera.m_Lens.OrthographicSize, currentZoom, ref zoomVelocity, zoomSmoothTime);
    }

    private void PanZoomedCam()
    {
        if (!isFollowCam)
        {
            var playerPos = player.transform.position;
            var cameraPos = zoomOutCamera.transform.position;
            if (isPanX)
            {
                if (Mathf.Abs(cameraPos.x - playerPos.x) >= xBounds)
                {
                    zoomOutCamera.transform.position = new Vector3(Mathf.SmoothDamp(cameraPos.x, playerPos.x, ref xVelocity, zoomSmoothTime), cameraPos.y, cameraPos.z);
                }
            }
            else
            {
                if (Mathf.Abs(cameraPos.y - playerPos.y) >= yBounds)
                {
                    zoomOutCamera.transform.position = new Vector3(cameraPos.x, Mathf.SmoothDamp(cameraPos.y, playerPos.y, ref yVelocity, zoomSmoothTime), cameraPos.z);
                }
            }
        }
    }

    private void StartFollowCam()
    {
        if (!isFollowCam)
        {
            cameraAnimator.SetTrigger("Follow");
            isFollowCam = true;
        }
    }

    private void StartZoomedCam(float xVal, float yVal, float orth, bool panX)
    {
        zoomOutCamera.transform.position = new Vector3(xVal, yVal, zoomOutCamera.transform.position.z);
        zoomOutCamera.m_Lens.OrthographicSize = orth;
        cameraAnimator.SetTrigger("LeftZoom");
        isFollowCam = false;
        isPanX = panX;
    }
}