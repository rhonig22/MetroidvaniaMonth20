
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private readonly int baseJumpForce = 4;
    private readonly int jumpMultiplier = 4;
    private readonly int additionalJumpForce = 9;
    private readonly int maxSoundQueueCount = 3;
    private readonly float speedIncrease = 2f;
    private readonly float coyoteTime = .2f;
    private readonly float jumpBufferTime = .2f;
    private readonly float landBufferTime = .35f;
    private readonly float reboundBufferTime = .4f;
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
    private readonly float smoothTime = .1f;
    private readonly float cannonSmoothTime = 1f;
    private readonly float baseDrag = 1.2f;
    private readonly float baseMass = 1f;
    private readonly float cannonMass = 20f;
    private readonly float cannonGravity = 20f;
    private readonly float zoomLeft1xVal = -55.8f;
    private readonly float zoomLeft1Orth = 17f;
    private readonly float zoomRight1xVal = 48.7f;
    private readonly float zoomRight1Orth = 19f;
    private readonly float zoomBotxVal = 6.5f;
    private readonly float zoomBotyVal = -50.5f;
    private readonly float zoomBotOrth = 25f;
    private readonly float endingZoomLevelStart = 16f;
    private float speed = 6f;
    private float horizontalInput = 0;
    private bool grounded = false;
    private bool jump = false;
    private bool isFalling = false;
    private bool isInCannon = false;
    private bool isCannoning = false;
    private int currentJumpForce;
    private float coyoteTimeCounter = 0;
    private float jumpBufferCounter = 0;
    private float landBufferCounter = 0;
    private float reboundBufferCounter = 0;
    private float landingVelocity = 0;
    private float triggerAreaSideValue = 0;
    private Vector2 currentVelocity = Vector3.zero;
    private Vector2 additionalVelocity = Vector3.zero;
    private Vector3 smoothScaleChange = Vector3.zero;
    private List<AudioClip> audioQueue = new List<AudioClip>();
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private TrailRenderer poundTrail;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip poundClip;
    [SerializeField] private AudioClip reboundClip;
    [SerializeField] private AudioClip collectClip;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private MusicPlayer musicPlayer;
    [SerializeField] private Light2D lightSource;
    private StickyObject sticky;
    public UnityEvent triggerScreenShake = new UnityEvent();
    public UnityEvent<float, float, float> enterZoomedCamX = new UnityEvent<float, float, float>();
    public UnityEvent<float, float, float> enterZoomedCamY = new UnityEvent<float, float, float>();
    public UnityEvent<bool, float> returnToFollow = new UnityEvent<bool, float>();
    public bool groundPounding { get; private set; } = false;
    public bool groundPoundLanded { get; private set; } = false;
    public bool rebounding { get; private set; } = false;
    public bool isDead { get; private set; } = false;
    public bool HasPound { get; private set; } = false;
    public bool HasRebound { get; private set; } = false;
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
        currentJumpForce = baseJumpForce;
        sticky = GetComponent<StickyObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead || !DataManager.GameStarted)
            return;

        horizontalInput = groundPounding ? 0 : Input.GetAxisRaw("Horizontal");
        groundPoundLanded = false;

        // Coyote Time Logic
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            landBufferCounter -= Time.deltaTime;
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

        if (!isFalling && playerRB.velocity.y < 0f)
        {
            isFalling = true;
        }

        if (rebounding && playerRB.velocity.y <= 0f)
        {
            EndRebound();
        }

        if (isCannoning && additionalVelocity.magnitude < speed && playerRB.velocity.y <= 0f)
        {
            EndCannon();
        }

        if (HasPound && !grounded && Input.GetButtonDown("Pound"))
        {
            StartGroundPound();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || !DataManager.GameStarted)
            return;

        PlaySoundQueue();
        UpdateScale();
        Move(horizontalInput * speed * Time.fixedDeltaTime);

        // Control jumping
        if (jump)
        {
            StartJump();
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
        }

        animator.SetTrigger("Jump");
        PlaySound(jumpClip, 1 - .1f*(Scale-1));
        playerRB.AddForce(Vector3.up * (grounded ? currentJumpForce : additionalJumpForce), ForceMode2D.Impulse);
        jump = false;
    }

    private void AddVelocity(Vector2 newVelocity)
    {
        playerRB.velocity += newVelocity;
        newVelocity.y = 0;
        additionalVelocity = newVelocity;
    }

    private void Move(float xSpeed)
    {
        Vector2 targetVelocity = new Vector2(xSpeed * 60f, playerRB.velocity.y);
        playerRB.velocity = Vector2.SmoothDamp(playerRB.velocity, targetVelocity + additionalVelocity + sticky.AdditionalVelocity, ref currentVelocity, additionalVelocity.magnitude > 0 ? cannonSmoothTime : smoothTime);

        if (isCannoning)
        {
            bool stillCannon = additionalVelocity.magnitude < 1f;
            if (additionalVelocity.magnitude > 0 && !isInCannon)
                additionalVelocity.x -= additionalVelocity.x * baseDrag * Time.deltaTime;

            if (!stillCannon && additionalVelocity.magnitude < 1f)
            {
                EndCannon();
            }
        }
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
        if ((groundPounding || rebounding || additionalVelocity.magnitude > 0f) && collision.collider.tag == "Breakable")
        {
            brokeBlock = PerformBreakLogic(collision);
        }

        if (!brokeBlock)
        {
            if (!isInCannon && isCannoning)
            {
                SlowCannon();
                triggerScreenShake.Invoke();
            }

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
        landBufferCounter = landBufferTime;
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
            if (landBufferCounter < 0)
            {
                animator.SetTrigger("Land");
            }

            Jumps = 0;
            isFalling = false;
            landingVelocity = collision.relativeVelocity.y;
            EndGroundPound();
            EndCannon();
        }
    }

    public void Death()
    {
        if (isDead)
            return;

        isDead = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        bool destroyCollision = false;
        switch(collision.tag)
        {
            case "GrowPowerUp":
                GrabGrowPowerUp();
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
            case "LeftZoom":
                EnterLeftZoomTrigger();
                break;
            case "RightZoom":
                EnterRightZoomTrigger();
                break;
            case "EndGame":
                StartCoroutine(EndGame());
                break;
            case "Ruins":
                musicPlayer.ChangeMusic(musicPlayer.ruins);
                break;
            case "Cave":
                musicPlayer.ChangeMusic(musicPlayer.cave);
                break;
            case "Air":
                musicPlayer.ChangeMusic(musicPlayer.air);
                break;
            case "Finale":
                musicPlayer.ChangeMusic(musicPlayer.finale);
                break;
            default:
                break;
        }

        if (destroyCollision)
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isDead)
            return;

        switch (collision.tag)
        {
            case "LeftZoom":
                ExitLeftZoomTrigger();
                break;
            case "RightZoom":
                ExitRightZoomTrigger();
                break;
            case "BottomZoom":
                ExitBottomZoomTrigger();
                break;
            case "EndFollow":
                EndFollowTrigger();
                break;
            case "EndZoomIn":
                EndFollowTriggerZoomIn();
                break;
            default:
                break;
        }
    }

    public void IncreaseGrowLogic()
    {
        Scale = 1 * (DataManager.GrowCount + 1);
        speed += speedIncrease;
        currentJumpForce = baseJumpForce + jumpMultiplier * DataManager.GrowCount;
        lightSource.pointLightOuterRadius = Scale + 1;
    }

    public void GrabGrowPowerUp()
    {
        DataManager.Instance.IncreaseGrowPowerUps();
        PlaySound(collectClip);
    }

    private void GetGroundPoundLogic()
    {
        HasPound = true;
        DataManager.Instance.ShowGroundPoundTip();
        PlaySound(collectClip);
    }

    private void GetReboundLogic()
    {
        HasRebound = true;
        DataManager.Instance.ShowReboundTip();
        PlaySound(collectClip);
    }

    private void StartGroundPound()
    {
        if (rebounding)
            EndRebound();

        groundPounding = true;
        playerRB.gravityScale = groundPoundGravity;
        horizontalInput = 0;
        playerRB.velocity = new Vector2(0, playerRB.velocity.y * (playerRB.velocity.y > 0 ? -1 : 1));
        playerRB.AddForce(Vector3.down * (additionalJumpForce + Scale*poundForceScale), ForceMode2D.Impulse);
        poundTrail.startWidth = Scale;
        poundTrail.emitting = true;
    }

    private void StartRebound()
    {
        rebounding = true;
        animator.SetTrigger("Jump");
        float newForce = currentJumpForce + landingVelocity / 4;
        Debug.Log(newForce);
        playerRB.AddForce(Vector3.up * newForce, ForceMode2D.Impulse);
        poundTrail.startWidth = Scale;
        poundTrail.emitting = true;
        PlaySound(reboundClip, .5f);
    }

    private void EndRebound()
    {
        rebounding = false;
        poundTrail.emitting = false;
    }

    private void EndGroundPound()
    {
        if (groundPounding)
        {
            playerRB.gravityScale = baseGravity;
            groundPounding = false;
            groundPoundLanded = true;
            triggerScreenShake.Invoke();
            poundTrail.emitting = false;
            particles.transform.SetParent(transform, false);
            particles.transform.localPosition = new Vector3(0, -.5f, 0);
            var particleMain = particles.main;
            var startSpeed = particleMain.startSpeed;
            var startSize = particleMain.startSize;
            startSpeed.constantMin = minParticleSpeed * Scale;
            startSpeed.constantMax = maxParticleSpeed * Scale;
            startSize.constantMin = minParticleSize * Scale;
            startSize.constantMax = maxParticleSize * Scale;
            particleMain.startSpeed = startSpeed;
            particleMain.startSize = startSize;
            particles.transform.SetParent(null, true);
            particles.transform.localScale = new Vector3(Scale/2, Scale/2, Scale/2);
            particles.Play();
            PlaySound(poundClip);
            if (HasRebound)
            {
                reboundBufferCounter = reboundBufferTime;
            }
        }
    }

    public void EnterCannon()
    {
        groundPounding = false;
        playerRB.velocity = Vector2.zero;
        currentVelocity = Vector2.zero;
        isInCannon= true;
        playerRB.gravityScale = 0;
    }

    public void FireCannon(Vector2 newVelocity)
    {
        AddVelocity(newVelocity);
    }

    public void LeaveCannon(Vector2 newVelocity)
    {
        AddVelocity(newVelocity);
        playerRB.gravityScale = cannonGravity;
        playerRB.mass = cannonMass;
        isInCannon = false;
        poundTrail.emitting = true;
        isCannoning = true;
    }

    public void EndCannon()
    {
        if (isCannoning)
        {
            additionalVelocity = Vector2.zero;
            playerRB.gravityScale = baseGravity;
            playerRB.mass = baseMass;
            poundTrail.emitting = false;
            isCannoning = false;
        }
    }

    public void SlowCannon()
    {
        if (isCannoning)
        {
            additionalVelocity *= .5f;
        }
    }

    private bool PerformBreakLogic(Collision2D collision)
    {
        GameObject breakable = collision.collider.gameObject;
        BreakableBlock breakableBlock= breakable.GetComponent<BreakableBlock>();
        if (breakableBlock.CanBreak(DataManager.GrowCount))
        {
            timeManager.DoSlowmotion(slowFactor, slowDuration);
            triggerScreenShake.Invoke();
            playerRB.velocity = -1 * collision.relativeVelocity;
            breakableBlock.StartBreaking();
            return true;
        }

        return false;
    }

    private void PlaySound(AudioClip clip, float pitch = 1f)
    {
        if (DataManager.GameStarted)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = clip;
                audioSource.pitch = pitch;
                audioSource.Play();
            }
            else if (audioQueue.Count < maxSoundQueueCount)
            {
                audioQueue.Add(clip);
            }
        }
    }

    private void PlaySoundQueue()
    {
        if (DataManager.GameStarted)
        {
            if (!audioSource.isPlaying && audioQueue.Count > 0)
            {
                audioSource.clip = audioQueue[0];
                audioSource.pitch = 1f;
                audioSource.Play();
                audioQueue.RemoveAt(0);
            }
        }
    }

    private void EnterLeftZoomTrigger()
    {
        triggerAreaSideValue = transform.position.x;
    }

    private void ExitLeftZoomTrigger()
    {
        if (transform.position.x < triggerAreaSideValue)
            enterZoomedCamY.Invoke(zoomLeft1xVal, transform.position.y, zoomLeft1Orth);
        else
            returnToFollow.Invoke(false, 0);

        triggerAreaSideValue = 0;
    }

    private void EnterRightZoomTrigger()
    {
        triggerAreaSideValue = transform.position.x;
    }

    private void ExitRightZoomTrigger()
    {
        if (transform.position.x < triggerAreaSideValue)
            enterZoomedCamY.Invoke(zoomRight1xVal, transform.position.y, zoomRight1Orth);
        else
            returnToFollow.Invoke(false, 0);

        triggerAreaSideValue = 0;
    }

    private void ExitBottomZoomTrigger()
    {
        enterZoomedCamX.Invoke(zoomBotxVal, zoomBotyVal, zoomBotOrth);
    }

    private void EndFollowTrigger()
    {
        returnToFollow.Invoke(true, endingZoomLevelStart);
    }

    private void EndFollowTriggerZoomIn()
    {
        returnToFollow.Invoke(true, 0);
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(5f);
        DataManager.Instance.EndGame();
    }
}
