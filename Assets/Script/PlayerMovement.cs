using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isPlayer1;
    public CharacterController2D controller;
    public Animator anim;
    public float movementSpeed = 10f, buffedMovementSpeed = 15f;
    public float jumpForce = 1600, buffedJumpForce = 1800, throwForce = 50;
    public GameObject bombSprite, barrierSprite;
    public GameObject trapUI;
    [HideInInspector]public bool holdingBomb;
    [HideInInspector]public bool haveBarrier;
    [HideInInspector]public bool isThrowingBomb;

    public GameObject eatParticle, hitTrapParticle, dieParticle;
    public GameObject trapPrefab, flyingBombPrefab;
    public GameObject audioManagerPrefab;

    private float horizontal = 0f;
    private float currentMovementSpeed;
    private bool jump, isRunning, isStun;
    private bool holdingTrap;
    private scrpt_AudioManager audioManager;

    private void Start()
    {
        currentMovementSpeed = movementSpeed;
        audioManager = FindObjectOfType<scrpt_AudioManager>();
    }

    private void Update()
    {
        //Input
        if (isPlayer1)
        {
            horizontal = Input.GetAxisRaw("Horizontal1") * currentMovementSpeed;
            if (Input.GetButtonDown("Jump1"))
            {
                jump = true;
                anim.SetBool("isJump", true);
            }

            if (Input.GetKeyDown(KeyCode.C) && holdingTrap) PlaceTrap();
        }
        else
        {
            horizontal = Input.GetAxisRaw("Horizontal2") * currentMovementSpeed;
            if (Input.GetButtonDown("Jump2"))
            {
                jump = true;
                anim.SetBool("isJump", true);
            }

            if (Input.GetKeyDown(KeyCode.Slash) && holdingTrap) PlaceTrap();
        }

        // barrier
        barrierSprite.SetActive(haveBarrier);

        // if stunning
        if (isStun || GameManager.instance.gameTime <= 0 || !GameManager.instance.isPlayable)
        {
            horizontal = 0;
            jump = false;
        }

        // Throw bomb
        if(GameManager.instance.gameTime > 0 && !isStun && GameManager.instance.isPlayable) ThrowBomb();

        if (horizontal < 0.01 && horizontal > -0.01) isRunning = false;
        else isRunning = true;

        anim.SetBool("isRunning", isRunning);
        if (!controller.m_Grounded && !isStun) anim.SetBool("isJump", true);
        else anim.SetBool("isJump", false);

        // trap
        if (holdingTrap) trapUI.SetActive(true);
        else trapUI.SetActive(false);

        // Bomb stuff
        bombSprite.SetActive(holdingBomb);
        if (transform.localScale.x == -1) bombSprite.transform.localScale = new Vector3(-10,10,0);
        else bombSprite.transform.localScale = new Vector3(10, 10, 0);

        // stun animation
        if(isStun) anim.SetBool("isStun", true);
    }

    private void FixedUpdate()
    {
        //Move the thing
        if (jump) audioManager.Play("jump");
        controller.Move(horizontal * Time.fixedDeltaTime, false, jump);

        jump = false;
    }

    public void OnLanding()
    {
        anim.SetBool("isJump", false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Hit Player
        if (other.CompareTag("Player") && !isThrowingBomb)
        {
            GameManager.instance.PassBomb();
            //print("hit");
        }

        // Use Cherry
        if (other.CompareTag("Item/FastCherry"))
        {
            audioManager.Play("cherry");
            Destroy(other.gameObject);
            GameObject newEffect = Instantiate(eatParticle, other.transform.position, Quaternion.identity) as GameObject;
            Destroy(newEffect.gameObject, 1);
            StartCoroutine(FastCherryTimer());
        }

        // Pick up Trap
        if (other.CompareTag("Item/PickUpTrap") && !holdingTrap)
        {
            audioManager.Play("pickUpTrap");
            Destroy(other.gameObject);
            holdingTrap = true;
        }

        // Hit Trap
        if (other.CompareTag("Item/PlaceTrap"))
        {
            Destroy(other.gameObject);

            GameObject newEffct = Instantiate(hitTrapParticle, other.transform.position, Quaternion.identity) as GameObject;
            Destroy(newEffct.gameObject, 1);

            if (haveBarrier)
            {
                audioManager.Play("hitBarrier");
                haveBarrier = false;
                isTrapInvincible = true;
                StartCoroutine(TrapInvincible());
                // print("blcok");
                return;
            }
            else if (isTrapInvincible)
            {
                // print("invincibel");
                return;
            }

            audioManager.Play("hitTrap");
            controller.m_Grounded = true;
            controller.Move(0, false, true);
            anim.SetBool("isStun", true);

            if (!isStun) StartCoroutine(StunTimer());
        }

        // Pick up Barrier
        if (other.CompareTag("Item/Barrier") && !haveBarrier)
        {
            audioManager.Play("barrier");
            haveBarrier = true;
            Destroy(other.gameObject);
        }
    }

    private IEnumerator TrapInvincible()
    {
        yield return new WaitForSeconds(.25f);
        isTrapInvincible = false;
    }

    private bool isTrapInvincible = false;
    private bool isContactOtherPlayer = false;
    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
        {
            isContactOtherPlayer = true;
        }
        else isContactOtherPlayer = false;

        // print(isContactOtherPlayer);
    }

    private float cherryTimer = 0;
    private IEnumerator FastCherryTimer()
    {
        if (cherryTimer >= 6)
        {
            currentMovementSpeed = movementSpeed;
            controller.m_JumpForce = jumpForce;
            anim.GetComponent<SpriteRenderer>().color = Color.white;
            cherryTimer = 0;

            yield return new WaitForSeconds(1);
            StopCoroutine(FastCherryTimer());
        }
        else
        {
            currentMovementSpeed = buffedMovementSpeed;
            controller.m_JumpForce = buffedJumpForce;
            anim.GetComponent<SpriteRenderer>().color = new Color(1, .58f, .58f);
            cherryTimer++;

            yield return new WaitForSeconds(1);
            StartCoroutine(FastCherryTimer());
        }

        //print(cherryTimer);
    }

    private float stunTimer = 0;
    private IEnumerator StunTimer()
    {
        if(stunTimer >= 2)
        {
            // reset
            stunTimer = 0;
            isStun = false;
            anim.SetBool("isStun", false);

            yield return new WaitForSeconds(1);
            StopCoroutine(StunTimer());
        }
        else
        {
            // effect
            isStun = true;
            stunTimer++;

            yield return new WaitForSeconds(1);
            StartCoroutine(StunTimer());
        }
    }

    private void PlaceTrap()
    {
        audioManager.Play("settrap");

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 5, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, Vector2.down * 5, Color.red);
            Vector2 spawnPoint = new Vector2(transform.position.x, hit.point.y + .4f);
            holdingTrap = false;

            GameObject newTrap = Instantiate(trapPrefab, spawnPoint, Quaternion.identity) as GameObject;

            StartCoroutine(ActivateTrap(newTrap));
        }
    }

    private IEnumerator ActivateTrap(GameObject trap)
    {
        yield return new WaitForSeconds(1);
        audioManager.Play("activateTrap");
        trap.GetComponent<Collider2D>().enabled = true;
        trap.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void ThrowBomb()
    {
        if (isPlayer1)
        {
            if(Input.GetKeyDown(KeyCode.V) && holdingBomb && !isContactOtherPlayer)
            {
                audioManager.Play("throw");

                holdingBomb = false;
                isThrowingBomb = true;
                GameObject newBomb = Instantiate(flyingBombPrefab, transform.position, Quaternion.identity) as GameObject;
                newBomb.GetComponent<FlyingBomb>().theGuyWhoThrowTheBomb = this;

                Vector2 forceDir = Vector2.right;
                if (transform.localScale.x < 0) forceDir = Vector2.left;
                newBomb.GetComponent<Rigidbody2D>().AddForce(forceDir * throwForce, ForceMode2D.Impulse);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Period) && holdingBomb && !isContactOtherPlayer)
            {
                audioManager.Play("throw");

                holdingBomb = false;
                isThrowingBomb = true;
                GameObject newBomb = Instantiate(flyingBombPrefab, transform.position, Quaternion.identity) as GameObject;
                newBomb.GetComponent<FlyingBomb>().theGuyWhoThrowTheBomb = this;

                Vector2 forceDir = Vector2.right;
                if (transform.localScale.x < 0) forceDir = Vector2.left;
                newBomb.GetComponent<Rigidbody2D>().AddForce(forceDir * throwForce, ForceMode2D.Impulse);
            }
        }
    }

    private IEnumerator SelfExplode()
    {
        yield return new WaitForSeconds(1);

        audioManager.Play("boom");

        foreach (GameObject player in GameManager.instance.players) player.GetComponent<PlayerMovement>().holdingBomb = false;

        GameObject newEffect = Instantiate(dieParticle, transform.position, Quaternion.identity) as GameObject;
        Destroy(newEffect.gameObject, 1);

        anim.GetComponent<SpriteRenderer>().enabled = false;
        trapUI.GetComponent<SpriteRenderer>().enabled = false;
        barrierSprite.GetComponent<SpriteRenderer>().enabled = false;
        holdingBomb = false;
    }

    public void ActivateLoserProtocal()
    {
        StartCoroutine(SelfExplode());
        anim.SetBool("isStun", true);
    }

    public void ActivateToxicPlayerProtocal()
    {
        anim.SetBool("celebration", true);
        audioManager.Play("win");
        StartCoroutine(ResetAnimation());
    }

    private IEnumerator ResetAnimation()
    {
        yield return new WaitForSeconds(.1f);
        anim.SetBool("celebration", false);
    }
}
