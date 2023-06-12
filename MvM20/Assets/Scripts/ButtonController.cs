using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonController : MonoBehaviour
{
    public UnityEvent OpenDoor = new UnityEvent();

    [SerializeField] private Animator animator;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc.groundPounding || pc.groundPoundLanded || pc.rebounding)
            {
                animator.SetTrigger("Pressed");
            }
        }
        else if (collision.gameObject.tag == "Moving")
        {
            animator.SetTrigger("Pressed");
        }
    }

    private void ButtonPressed()
    {
        OpenDoor.Invoke();
    }
}
