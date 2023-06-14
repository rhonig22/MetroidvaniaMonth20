using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    [SerializeField] private Transform launchPos;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem particles;
    private readonly float canonForce = 25f;
    private readonly float fireBufferTime = .2f;
    private readonly float centerMargin = 2f;
    private Vector3 playerPos = Vector3.zero;
    private bool hasShot = false;

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
        if (collision.gameObject.tag == "Player" && !hasShot)
        {
            GameObject player = collision.gameObject;
            if (Mathf.Abs((player.transform.position - transform.position).magnitude) < centerMargin)
            {
                PlayerController pc = player.GetComponent<PlayerController>();
                player.transform.position = launchPos.position;
                pc.FireCannon(GetCannonForce());
                hasShot = true;
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
            audioSource.Play();
            particles.transform.localScale = transform.localScale;
            particles.Play();
            playerPos = Vector3.zero;
            hasShot= false;
        }
    }

    private Vector2 GetCannonForce()
    {
        return transform.right * canonForce * DataManager.GrowCount;
    }
}