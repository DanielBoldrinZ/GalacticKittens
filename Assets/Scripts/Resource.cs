using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public enum ResourceType
    {
        Stone,
        Log,
        Berry,
        Fish,
        Iron,
        Coal
    }
    
    [SerializeField]
    public ResourceType resourceType = ResourceType.Stone;

    [SerializeField]
    GameObject canvas;

    public void PickUp()
    {
        Destroy(canvas);
        Destroy(gameObject.transform.parent.gameObject);
    }

    public void ShowCanvas()
    {
        canvas.SetActive(true);
    }

    public void HideCanvas()
    {
        canvas.SetActive(false);
    }
}
