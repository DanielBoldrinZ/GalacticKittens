using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceScript : MonoBehaviour
{
    public static FurnaceScript Instance;
    [SerializeField] GameObject fire1;
    [SerializeField] GameObject fire2;

    public bool first = true;
    public bool started = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
    }

    private void Animate()
    {
        fire1.SetActive(first);
        fire2.SetActive(!first);
        first = !first;
    }

    public void StartAnimation()
    {
        if (!started)
        {
            StopAnimation();
            started = true;
            InvokeRepeating("Animate", 1, 1);
        }
    }

    public void StopAnimation()
    {
        if (started)
        {
            CancelInvoke("Animate");
            CancelInvoke();
            fire1.SetActive(false);
            fire2.SetActive(false);
            started = false;
        }
    }
}
