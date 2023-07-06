using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyCollider : MonoBehaviour
{
    [SerializeField] GameObject platform;
    [SerializeField] Rigidbody2D platformRb;
    private List<GameObject> stuckObjects = new List<GameObject>();

    private void FixedUpdate()
    {
        for (int i = 0; i < stuckObjects.Count; i++)
        {
            GameObject obj = stuckObjects[i];
            StickyObject sticky;
            var isSticky = obj.TryGetComponent<StickyObject>(out sticky);
            if (isSticky)
            {
                sticky.AdditionalVelocity = platformRb.velocity;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        stuckObjects.Add(collision.gameObject);
        collision.gameObject.transform.SetParent(platform.transform);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        stuckObjects.Remove(collision.gameObject);
        collision.gameObject.transform.SetParent(null);
        StickyObject sticky;
        var isSticky = collision.gameObject.TryGetComponent<StickyObject>(out sticky);
        if (isSticky)
        {
            sticky.AdditionalVelocity = Vector2.zero;
        }
    }
}
