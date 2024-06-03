using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InventoryResource : MonoBehaviour
{
    public enum ResourceType
    {
        Stone,
        Log,
        Berry,
        Fish,
        Iron,
        Coal,
        Gold
    }
    
    [SerializeField]
    public ResourceType resourceType = ResourceType.Stone;
}
