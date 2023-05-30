
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private readonly int jumpForce = 11;
    private readonly int additionalJumpForce = 9;
    private readonly float speed = 7.5f;
    private readonly float maxVelocity = 12f;
    private float horizontalInput = 0;
    private bool grounded = false;
    private bool jump = false;
    private Vector3 currentVelocity = Vector3.zero;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Rigidbody2D playerRB;
    public bool isDead { get; private set; } = false;
    private int _jumps = 0;
    private int _scale = 1;
    public int Jumps
    {
        get
        {
            return _jumps;
        }
        private set
        {
            _jumps = value;
        }
    }

    public int Scale
    {
        get
        {
            return _scale;
        }
        private set
        {
            _scale = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Control jumping
        if (Input.GetButtonDown("Jump") && (grounded || Jumps > 0))
        {
            jump = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        Move(horizontalInput * speed * Time.fixedDeltaTime);

        // Cap the player's max velocity
        CapYVelocity();

        // Control jumping
        if (jump)
        {
            StartJump();
        }
    }

    private void CapYVelocity()
    {
        currentVelocity = playerRB.velocity;
        if (currentVelocity.y > maxVelocity)
        {
            currentVelocity.y = maxVelocity;
            playerRB.velocity = currentVelocity;
        }
        else if (currentVelocity.y < -maxVelocity)
        {
            currentVelocity.y = -maxVelocity;
            playerRB.velocity = currentVelocity;
        }
    }

    private void StartJump()
    {
        currentVelocity = playerRB.velocity;
        currentVelocity.y = 0;
        playerRB.velocity = currentVelocity;
        if (!grounded)
        {
            Jumps--;
            // animator.ResetTrigger("jump");
        }

        // animator.SetTrigger("jump");
        playerRB.AddForce(Vector3.up * (grounded ? jumpForce : additionalJumpForce), ForceMode2D.Impulse);

        jump = false;
    }

    private void Move(float xSpeed)
    {
        Vector3 targetVelocity = new Vector2(xSpeed * 60f, playerRB.velocity.y);
        // And then smoothing it out and applying it to the character
        playerRB.velocity = Vector3.SmoothDamp(playerRB.velocity, targetVelocity, ref currentVelocity, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckGrounding(collision);
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        CheckGrounding(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        grounded = false;
    }

    private void CheckGrounding(Collision2D collision)
    {
        bool startGrounded = grounded;
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            grounded |= Vector2.Angle(normal, Vector2.up) < 90;
        }

        if (!startGrounded && grounded && !isDead)
        {
            // animator.ResetTrigger("jump");
            Jumps = 0;
        }
    }

    public void Death()
    {
        if (isDead)
            return;

        isDead = true;
        animator.SetTrigger("Death");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;
    }
}
