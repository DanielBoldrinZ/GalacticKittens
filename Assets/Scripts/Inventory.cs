using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using static InventoryResource;

public class Inventory : MonoBehaviour
{
    private bool active = false;

    [SerializeField]
    SerializableDictionaryBase<GameObject, InventoryResource> resourcesSlots = new SerializableDictionaryBase<GameObject, InventoryResource>() {
        //{ Scenes.SplashScreenScene, "Assets/_Core/Scenes/SplashScreenScene.unity" },
        //{ Scenes.DisplayNameScene, "Assets/_Core/Scenes/DisplayNameScene.unity" },
        //{ Scenes.ChampionMintScene, "Assets/_Core/Scenes/ChampionMintScene.unity" },
        //{ Scenes.ChampionMintedScene, "Assets/_Core/Scenes/ChampionMintedScene.unity" },
        //{ Scenes.MainMenuScene, "Assets/_Core/Scenes/MainMenuScene.unity" },
        //{ Scenes.GameScene, "Assets/_Core/Scenes/GameScene.unity" },
        //{ Scenes.CageEnvironmentScene, "Assets/_Core/Scenes/Environment_Scene.unity" }
    };

    [SerializeField]
    SerializableDictionaryBase<ResourceType, InventoryResource> resourcesPrefabs  = new SerializableDictionaryBase<ResourceType, InventoryResource>()
    {
        //{ Scenes.SplashScreenScene, "Assets/_Core/Scenes/SplashScreenScene.unity" },
        //{ Scenes.DisplayNameScene, "Assets/_Core/Scenes/DisplayNameScene.unity" },
        //{ Scenes.ChampionMintScene, "Assets/_Core/Scenes/ChampionMintScene.unity" },
        //{ Scenes.ChampionMintedScene, "Assets/_Core/Scenes/ChampionMintedScene.unity" },
        //{ Scenes.MainMenuScene, "Assets/_Core/Scenes/MainMenuScene.unity" },
        //{ Scenes.GameScene, "Assets/_Core/Scenes/GameScene.unity" },
        //{ Scenes.CageEnvironmentScene, "Assets/_Core/Scenes/Environment_Scene.unity" }
    };

    public static Inventory Instance;
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

    public void Show()
    {
        if (!active)
        {
            gameObject.SetActive(true);
            active = true;
        }
    }


    public void Hide()
    {
        if (active)
        {
            gameObject.SetActive(false);
            active = false;
        }
    }

    public bool GetItem(ResourceType type)
    {
        foreach (var item in resourcesSlots)
        {
            if (item.Value == null)
            {
                //Slot has space
                resourcesSlots[item.Key] = Instantiate(resourcesPrefabs[type], item.Key.transform); // Instantiate the resource in the inventory slot
                return true;
            }
        }

        //No slot has space
        return false;
    }

    public bool UseItem(ResourceType type)
    {
        foreach (var item in resourcesSlots.Reverse())
        {
            if (item.Value != null && item.Value.resourceType == type)
            {
                // Has item type, use it
                Destroy(item.Value);
                resourcesSlots[item.Key] = null;

                return true;
            }
        }

        return false;
    }
}
