
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private readonly int baseJumpForce = 4;
    private readonly int jumpMultiplier = 4;
    private readonly int additionalJumpForce = 9;
    private readonly float coyoteTime = .2f;
    private readonly float jumpBufferTime = .2f;
    private readonly float reboundBufferTime = .2f;
    private readonly float scaleChangeTime = .25f;
    private readonly float baseGravity = 2f;
    private readonly float groundPoundGravity = 8f;
    private readonly float poundForceScale = 3;
    private readonly float minParticleSpeed = 2f;
    private readonly float maxParticleSpeed = 6f;
    private readonly float minParticleSize = .1f;
    private readonly float maxParticleSize = .5f;
    private readonly float slowFactor = .05f;
    private readonly float slowDuration = .5f;
    private float speed = 6f;
    private float horizontalInput = 0;
    private bool grounded = false;
    private bool jump = false;
    private bool groundPounding = false;
    private bool rebounding = false;
    private int currentJumpForce;
    private float coyoteTimeCounter = 0;
    private float jumpBufferCounter = 0;
    private float reboundBufferCounter = 0;
    private float landingVelocity = 0;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 smoothScaleChange = Vector3.zero;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private TrailRenderer poundTrail;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private TimeManager timeManager;
    private StickyObject sticky;
    public UnityEvent triggerScreenShake = new UnityEvent();
    public bool isDead { get; private set; } = false;
    public bool HasPound { get; private set; } = false;
    public bool HasRebound { get; private set; } = false;
    private int _jumps = 0;
    private int _scale = 1;
    public int GrowCount { get; private set; } = 0;
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
        currentJumpForce = baseJumpForce;
        sticky = GetComponent<StickyObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        horizontalInput = groundPounding ? 0 : Input.GetAxisRaw("Horizontal");

        // Coyote Time Logic
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump Buffer Logic
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        reboundBufferCounter -= Time.deltaTime;

        // Control jumping
        if (HasRebound && jumpBufferCounter > 0 && reboundBufferCounter > 0)
        {
            jumpBufferCounter = 0;
            reboundBufferCounter= 0;
            StartRebound();
        }
        else if ((jumpBufferCounter > 0f) && (coyoteTimeCounter > 0f || Jumps > 0))
        {
            jump = true;
            jumpBufferCounter = 0;
        }

        if (Input.GetButtonUp("Jump") && playerRB.velocity.y > 0f)
        {
            playerRB.velocity = new Vector2(playerRB.velocity.x, playerRB.velocity.y * .5f);
            coyoteTimeCounter = 0;
        }

        if (rebounding && playerRB.velocity.y <= 0f)
        {
            EndRebound();
        }

        if (HasPound && !grounded && Input.GetButtonDown("Pound"))
        {
            StartGroundPound();
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        UpdateScale();
        Move(horizontalInput * speed * Time.fixedDeltaTime);

        // Control jumping
        if (jump)
        {
            StartJump();
        }

        playerRB.velocity = playerRB.velocity + sticky.AdditionalVelocity;
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
        playerRB.AddForce(Vector3.up * (grounded ? currentJumpForce : additionalJumpForce), ForceMode2D.Impulse);

        jump = false;
    }

    private void Move(float xSpeed)
    {
        Vector3 targetVelocity = new Vector2(xSpeed * 60f, playerRB.velocity.y);
        // And then smoothing it out and applying it to the character
        playerRB.velocity = Vector3.SmoothDamp(playerRB.velocity, targetVelocity, ref currentVelocity, 0);
    }

    private void UpdateScale()
    {
        if (transform.localScale.x != Scale)
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, new Vector3(_scale, _scale, _scale), ref smoothScaleChange, scaleChangeTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool brokeBlock = false;
        if (groundPounding && collision.collider.tag == "Breakable")
        {
            brokeBlock = PerformBreakLogic(collision);
        }

        if (!brokeBlock)
        {
            CheckGrounding(collision);
        }
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
            landingVelocity = collision.relativeVelocity.y;
            EndGroundPount();
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

        bool destroyCollision = false;
        switch(collision.tag)
        {
            case "GrowPowerUp":
                IncreaseGrowLogic();
                destroyCollision = true;
                break;

            case "GroundPound":
                GetGroundPoundLogic();
                destroyCollision = true;
                break;

            case "Rebound":
                GetReboundLogic();
                destroyCollision = true;
                break;
            default:
                break;
        }

        if (destroyCollision)
        {
            Destroy(collision.gameObject);
        }
    }

    private void IncreaseGrowLogic()
    {
        GrowCount++;
        Scale = 1 * (GrowCount + 1);
        currentJumpForce = baseJumpForce + jumpMultiplier * GrowCount;
    }

    private void GetGroundPoundLogic()
    {
        HasPound = true;
    }

    private void GetReboundLogic()
    {
        HasRebound = true;
    }

    private void StartGroundPound()
    {
        groundPounding = true;
        playerRB.gravityScale = groundPoundGravity;
        horizontalInput = 0;
        playerRB.velocity = new Vector2(0, playerRB.velocity.y);
        playerRB.AddForce(Vector3.down * (additionalJumpForce + Scale*poundForceScale), ForceMode2D.Impulse);
        poundTrail.startWidth = Scale;
        poundTrail.emitting = true;
    }

    private void StartRebound()
    {
        rebounding = true;
        playerRB.AddForce(Vector3.up * (currentJumpForce + landingVelocity/4), ForceMode2D.Impulse);
        poundTrail.startWidth = Scale;
        poundTrail.emitting = true;
    }

    private void EndRebound()
    {
        rebounding = false;
        poundTrail.emitting = false;
    }

    private void EndGroundPount()
    {
        if (groundPounding)
        {
            playerRB.gravityScale = baseGravity;
            groundPounding = false;
            triggerScreenShake.Invoke();
            poundTrail.emitting = false;
            var particleMain = particles.main;
            var startSpeed = particleMain.startSpeed;
            var startSize = particleMain.startSize;
            startSpeed.constantMin = minParticleSpeed * Scale;
            startSpeed.constantMax = maxParticleSpeed * Scale;
            startSize.constantMin = minParticleSize * Scale;
            startSize.constantMax = maxParticleSize * Scale;
            particleMain.startSpeed = startSpeed;
            particleMain.startSize = startSize;
            particles.Play();
            if (HasRebound)
            {
                reboundBufferCounter = reboundBufferTime;
                Debug.Log(landingVelocity);
            }
        }
    }

    private bool PerformBreakLogic(Collision2D collision)
    {
        GameObject breakable = collision.collider.gameObject;
        BreakableBlock breakableBlock= breakable.GetComponent<BreakableBlock>();
        if (breakableBlock.CanBreak(GrowCount))
        {
            timeManager.DoSlowmotion(slowFactor, slowDuration);
            triggerScreenShake.Invoke();
            playerRB.velocity = -1 * collision.relativeVelocity;
            breakableBlock.StartBreaking();
            return true;
        }

        return false;
    }
}
