using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Resource : NetworkBehaviour
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

    private void Awake()
    {
        if (canvas != null)
        {
            canvas.transform.SetParent(null);
        }
    }

    public void PickUp()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
