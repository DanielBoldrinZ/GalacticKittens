using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogStack : MonoBehaviour
{
    public static LogStack Instance;
    [SerializeField] List<GameObject> logs = new List<GameObject>();
    [SerializeField] int maxLogQuantity;
    [SerializeField] int curLogQuantity;
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

    private void OnValidate()
    {
        maxLogQuantity = logs.Count;
        //curLogQuantity = maxLogQuantity;
    }

    public bool HasLogs()
    {
        return curLogQuantity > 0;
    }

    public void Burn()
    {
        curLogQuantity--;
        logs[curLogQuantity].SetActive(false);
    }

    public void Add()
    {
        logs[curLogQuantity].SetActive(true);
        curLogQuantity++;
    }
}
