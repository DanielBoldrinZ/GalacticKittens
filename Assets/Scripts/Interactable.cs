using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    public enum InteractableType
    {
        Boulder,
        Rock,
        Tree,
        Blockage,
        Bush,
        Pond,
        Quest,
        TrainDamage,
        Kitchen
    }

    [SerializeField]
    Animator anim;

    [SerializeField]
    public InteractableType interactableType = InteractableType.Boulder;

    [SerializeField]
    GameObject canvas;

    [SerializeField]
    bool shouldDestroy;

    [SerializeField]
    List<GameObject> drops;

    [SerializeField]
    List<GameObject> toTurnOff;

    [SerializeField]
    Transform spawnLocation;

    bool active = true;

    public void ShowCanvas()
    {
        if (active)
        {
            canvas.SetActive(true);
        }
    }

    public void HideCanvas()
    {
        canvas.SetActive(false);
    }

    public void Hit(bool playerToTheRight)
    {
        if (active && drops.Count > 0)
        {
            if (NetworkManager.Singleton != null)
            {
                HitServerRpc(playerToTheRight);
            }
            else
            {
                GameObject instantiated = Instantiate(drops[0]);
                instantiated.transform.position = spawnLocation.position;

                if (interactableType != InteractableType.Kitchen)
                {
                    Rigidbody2D rb = instantiated.GetComponent<Rigidbody2D>();
                    if (playerToTheRight)
                    {
                        float range = Random.Range(2f, 4f);
                        rb.velocity = Vector2.right * -range + Vector2.up * range;
                    }
                    else
                    {
                        rb.velocity = Vector2.one * Random.Range(2f, 4f);
                    }
                }

                drops.Remove(drops[0]);
                anim.SetTrigger("hit");

                if (toTurnOff.Count > 0)
                {
                    toTurnOff[0].SetActive(false);
                    toTurnOff.Remove(toTurnOff[0]);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitServerRpc(bool playerToTheRight)
    {
        if (active && drops.Count > 0)
        {
            GameObject instantiated = NetworkObjectSpawner.SpawnNewNetworkObject(drops[0]);
            instantiated.transform.position = spawnLocation.position;

            if (interactableType != InteractableType.Kitchen)
            {
                if (IsOwner)
                {
                    Rigidbody2D rb = instantiated.GetComponent<Rigidbody2D>();
                    if (playerToTheRight)
                    {
                        float range = Random.Range(2f, 4f);
                        rb.velocity = Vector2.right * -range + Vector2.up * range;
                    }
                    else
                    {
                        rb.velocity = Vector2.one * Random.Range(2f, 4f);
                    }
                }
            }

            drops.Remove(drops[0]);
            anim.SetTrigger("hit");

            if (toTurnOff.Count > 0)
            {
                toTurnOff[0].SetActive(false);
                toTurnOff.Remove(toTurnOff[0]);
            }
        }
    }

    public void FinishHit()
    {
        if (drops.Count <= 0)
        {
            if (shouldDestroy)
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
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (interactableType != InteractableType.Kitchen)
        {
            if (collision.tag.Equals("train"))
            {
                active = false;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!active)
        {
            if (interactableType != InteractableType.Kitchen)
            {
                if (collision.tag.Equals("train"))
                {
                    active = true;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (interactableType != InteractableType.Kitchen)
        {
            if (collision.tag.Equals("train"))
            {
                active = true;
            }
        }
    }
}
