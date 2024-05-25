using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetScript : MonoBehaviour
{

    [SerializeField] PlayerShipMovement player;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        player.OnCollisionEnter2D_FROMFEET(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        player.OnCollisionStay2D_FROMFEET(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        player.OnCollisionExit2D_FROMFEET(collision);
    }

}
