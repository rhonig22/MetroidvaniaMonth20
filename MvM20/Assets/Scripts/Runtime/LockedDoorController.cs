using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedDoorController : MonoBehaviour
{
    [SerializeField] ButtonController buttonController;
    [SerializeField] DoorController doorController;

    private void Start()
    {
        buttonController.OpenDoor.AddListener(() => { doorController.OpenDoor(); });
    }

}
