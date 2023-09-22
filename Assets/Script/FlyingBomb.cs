using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingBomb : MonoBehaviour
{
    public PlayerMovement theGuyWhoThrowTheBomb;
    private bool notHitYet = true;
    private bool isHitPlayer = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerMovement hitPlayer = collision.collider.GetComponent<PlayerMovement>();

            if (hitPlayer.isPlayer1 != theGuyWhoThrowTheBomb.isPlayer1 && notHitYet)
            {
                notHitYet = false;

                if (!hitPlayer.haveBarrier)
                {
                    GameManager.instance.DoThePass();
                    isHitPlayer = true;
                }
                else
                {
                    FindObjectOfType<scrpt_AudioManager>().Play("hitBarrier");
                    hitPlayer.haveBarrier = false;
                }

                Destroy(gameObject);
            }
        }
        else if(notHitYet)
        {
            notHitYet = false;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (!isHitPlayer)
        {
            FindObjectOfType<scrpt_AudioManager>().Play("bombHitGround");
            theGuyWhoThrowTheBomb.holdingBomb = true;
        }
        theGuyWhoThrowTheBomb.isThrowingBomb = false;
    }
}
