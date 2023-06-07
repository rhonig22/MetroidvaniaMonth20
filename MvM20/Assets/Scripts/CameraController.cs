using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private Animator cameraAnimator;
    private PlayerController playerController;
    private readonly float orthoSize = 5;
    private readonly float minZoom = 0;
    private readonly float maxZoom = 10;
    private readonly float zoomSmoothTime = .25f;
    private float zoomVelocity = 0;
    private float currentZoom = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        currentZoom = mainCamera.m_Lens.OrthographicSize;
        playerController.triggerScreenShake.AddListener(() => { cameraAnimator.SetTrigger("Shake"); });
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ScaleCameraZoom();
    }

    void ScaleCameraZoom()
    {
        currentZoom = orthoSize + playerController.GrowCount;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        mainCamera.m_Lens.OrthographicSize = Mathf.SmoothDamp(mainCamera.m_Lens.OrthographicSize, currentZoom, ref zoomVelocity, zoomSmoothTime);
    }
}
