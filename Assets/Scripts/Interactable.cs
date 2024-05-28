using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
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
    List<Rigidbody2D> drops;

    public void ShowCanvas()
    {
        canvas.SetActive(true);
    }

    public void HideCanvas()
    {
        canvas.SetActive(false);
    }

    public void Hit()
    {
        if (drops.Count > 0)
        {
            drops[0].gameObject.SetActive(true);
            drops[0].transform.SetParent(null);
            drops[0].velocity = Vector2.one * Random.Range(1.25f, 3.5f);
            drops.Remove(drops[0]);
            anim.SetTrigger("hit");
        }
    }

    public void FinishHit()
    {
        if (drops.Count <= 0)
        {
            if (shouldDestroy)
            {
                Destroy(gameObject);
            }
        }
    }
}
