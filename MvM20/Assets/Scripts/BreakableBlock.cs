using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Collections.AllocatorManager;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField] private int breakSize;
    [SerializeField] private GameObject counter;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D blockRb;
    [SerializeField] private BoxCollider2D blockCollider;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private AudioSource audioSource;
    private List<GameObject> counters = new List<GameObject>();
    private readonly float minParticleSpeed = 5f;
    private readonly float maxParticleSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        RenderCounters();
    }

    // Update is called once per frame
    void Update()
    {
        MatchAlphaValue();
    }

    private void RenderCounters()
    {
        float step = 1f / (breakSize + 1);
        float size = .5f / breakSize;
        for (int i = 0; i < breakSize; i++)
        {
            var obj = Instantiate(counter);
            obj.transform.SetParent(transform, false);
            obj.transform.localScale = new Vector3(size, size, size);
            obj.transform.localPosition = new Vector3(-.5f + step + (i * step), .5f - step - (i * step), -1f);
            counters.Add(obj);
        }
    }

    private void MatchAlphaValue()
    {
        for (int i = 0; i < counters.Count; i++)
        {
            var c = counters[i];
            var cSprite = c.GetComponent<SpriteRenderer>();
            Color color = cSprite.color;
            color.a = sprite.color.a;
            cSprite.color = color;
        }

    }

    public bool CanBreak(int size)
    {
        return size >= breakSize;
    }

    public void StartBreaking()
    {
        blockCollider.enabled = false;
        var particleMain = particles.main;
        var startSpeed = particleMain.startSpeed;
        var startSize = particleMain.startSize;
        startSpeed.constantMin = minParticleSpeed * breakSize;
        startSpeed.constantMax = maxParticleSpeed * breakSize;
        startSize.constant = breakSize;
        particleMain.startSpeed = startSpeed;
        particleMain.startSize = startSize;
        particles.Play();
        audioSource.Play();
        animator.SetTrigger("Break");
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
