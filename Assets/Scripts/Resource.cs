using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static InventoryResource;

public class Resource : NetworkBehaviour
{
    [SerializeField]
    GameObject canvas;


    [SerializeField]
    public ResourceType resourceType = ResourceType.Stone;

    private void Awake()
    {
        if (canvas != null)
        {
            canvas.transform.SetParent(null);
        }
    }

    public bool PickUp()
    {
        if (NetworkManager.Singleton != null)
        {
            if (Inventory.Instance.GetItem(resourceType))
            {
                NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
                return true;
            }
        }
        else
        {
            if (Inventory.Instance.GetItem(resourceType))
            { 
                Destroy(transform.parent.gameObject);
                return true;
            }
        }

        return false;
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
