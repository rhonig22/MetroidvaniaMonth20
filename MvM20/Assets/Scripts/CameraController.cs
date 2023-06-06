using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Animator cameraAnimator;
    private PlayerController playerController;
    private readonly float baseZindex = -10;
    private readonly float orthoSize = 5;
    private readonly float yPosThreshold = 5;
    private readonly float axisChangeTime = .15f;
    private readonly float minZoom = 0;
    private readonly float maxZoom = 10;
    private readonly float zoomSmoothTime = .25f;
    private float currentYpos = 0;
    private float yPosChange = 0;
    private float zoomVelocity = 0;
    private float currentZoom = 0;
    private bool isMovingYPos = false;

    // Start is called before the first frame update
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        currentZoom = mainCamera.orthographicSize;
        playerController.triggerScreenShake.AddListener(() => { cameraAnimator.SetTrigger("Shake"); });
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdateCameraPosition();
        CheckYThreshold();
        ScaleCameraZoom();
    }

    void UpdateCameraPosition()
    {
        Vector3 newPos = player.transform.position;
        currentYpos += yPosChange * Time.deltaTime;
        newPos.y = currentYpos;
        newPos.z = baseZindex;
        transform.position = newPos;
    }

    void ScaleCameraZoom()
    {
        currentZoom = orthoSize + playerController.GrowCount;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, currentZoom, ref zoomVelocity, zoomSmoothTime);
    }

    void CheckYThreshold()
    {
        if (isMovingYPos)
            return;

        var playerYpos = player.transform.position.y;
        float threshold = yPosThreshold + playerController.GrowCount;
        if (playerYpos > currentYpos + yPosThreshold)
            yPosChange = threshold;
        else if (playerYpos < currentYpos - yPosThreshold)
            yPosChange = -threshold;

        if (yPosChange != 0)
            StartCoroutine(moveYAxis());
    }

    IEnumerator moveYAxis()
    {
        isMovingYPos = true;
        yield return new WaitForSeconds(axisChangeTime);
        isMovingYPos = false;
        yPosChange = 0;
    }
}
