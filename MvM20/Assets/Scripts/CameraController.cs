using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Camera mainCamera;
    private PlayerController playerController;
    private readonly float baseZindex = -10;
    private readonly float orthoSize = 5;
    private readonly float yPosThreshold = 5;
    private readonly float axisChangeTime = 1f;
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
        float zoom = orthoSize + (playerController.Scale - 1);
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, zoom, ref zoomVelocity, zoomSmoothTime);
    }

    void CheckYThreshold()
    {
        if (isMovingYPos)
            return;

        var playerYpos = player.transform.position.y;
        if (playerYpos > currentYpos + yPosThreshold)
            yPosChange = yPosThreshold;
        else if (playerYpos < currentYpos - yPosThreshold)
            yPosChange = -yPosThreshold;

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
