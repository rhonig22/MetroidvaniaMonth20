using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    private readonly float canonForce = 25f;
    private readonly float fireBufferTime = .25f;
    private Vector3 playerPos = Vector3.zero;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            pc.EnterCannon();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameObject player = collision.gameObject;
            if (Mathf.Abs((player.transform.position - transform.position).magnitude) < .3f)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                pc.FireCannon(GetCannonForce());
            }
            else
            {
                player.transform.position = Vector3.SmoothDamp(player.transform.position, transform.position, ref playerPos, fireBufferTime);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            pc.LeaveCannon(GetCannonForce());
            playerPos = Vector3.zero;
        }
    }

    private Vector2 GetCannonForce()
    {
        return transform.right * canonForce * DataManager.GrowCount;
    }
}