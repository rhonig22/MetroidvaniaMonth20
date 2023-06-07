using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    [SerializeField] Rigidbody2D blockRb;
    [SerializeField] GameObject blockSprite;
    private float speed = 3;
    private Vector2 currentVelocity = Vector2.zero;
    private bool flipped = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        Move(speed * Time.fixedDeltaTime);
    }

    private void Move(float xSpeed)
    {
        Vector3 targetVelocity = new Vector2(xSpeed * 60f, blockRb.velocity.y);
        // And then smoothing it out and applying it to the character
        blockRb.velocity = Vector2.SmoothDamp(blockRb.velocity, targetVelocity, ref currentVelocity, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (flipped)
            return;

        bool flip = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            flip |= Vector2.Angle(normal, Vector2.right) == 0 || Vector2.Angle(normal, Vector2.right) == 180;
        }

        if (flip)
        {
            var currentScale = blockSprite.transform.localScale;
            currentScale.x *= -1;
            blockSprite.transform.localScale = currentScale;
            speed *= -1;
            flipped = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        flipped = false;
    }
}
