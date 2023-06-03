using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    [SerializeField] private int breakSize;
    [SerializeField] private GameObject counter;

    // Start is called before the first frame update
    void Start()
    {
        RenderCounters();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        }
    }

    public bool CanBreak(int size)
    {
        return size >= breakSize;
    }

    public void StartBreaking()
    {
        Destroy(gameObject);
    }
}
