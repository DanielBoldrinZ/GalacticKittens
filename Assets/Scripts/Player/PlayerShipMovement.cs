using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerShipMovement : NetworkBehaviour
{
    public enum CharacterType
    {
        Manager,
        Lumberjack,
        Miner,
        Hunter
    }
    public LayerMask trainLayer;

    [SerializeField]
    CharacterType characterType = CharacterType.Manager;

    [Header("ShipSprites")]
    [SerializeField]
    SpriteRenderer m_shipRenderer;

    [SerializeField]
    Animator anim;

    [SerializeField]
    Rigidbody2D rb;

    [SerializeField]
    Sprite m_normalSprite;

    [SerializeField]
    Sprite m_upSprite;

    [SerializeField]
    Sprite m_downSprite;

    [SerializeField]
    GameObject camera;

    [SerializeField]
    bool overwriteOwner;

    [SerializeField]
    private float m_jump_speed;

    [SerializeField]
    private float m_run_speed;

    //private float m_hold_speed = 2.65f;

    private float xSize;

    private bool onTrain = false;
    private bool onControls = false;
    private bool onFires = false;
    private bool isGrounded = false;
    private bool isBusy = false;
    private bool isLookingRight = true;
    private bool isHoldingJump = false;
    private Vector3 vecSpeed;

    //private NetworkVariable<bool> networkIsLookingRight = new NetworkVariable<bool>(true);
    //private NetworkVariable<bool> networkIsGrounded = new NetworkVariable<bool>(true);
    //private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero);
    private NetworkVariable<Vector2> networkVelocity = new NetworkVariable<Vector2>(Vector2.zero);

    void Start()
    {
        xSize = transform.localScale.x;

        if (!IsOwner && !overwriteOwner)
        {
            camera.SetActive(false);
        }

        camera.transform.SetParent(null);
    }

    void Update()
    {
        if (!IsOwner && !overwriteOwner)
        {
            return;
        }

        HandleKeyboardInput();
        UpdateAnimations(rb.velocity);

        switch (characterType)
        {
            case CharacterType.Manager:
                ResolveMachinist();
                break;
            case CharacterType.Lumberjack:
                ResolveMachinist();
                break;
            case CharacterType.Miner:
                break;
            case CharacterType.Hunter:
                break;
        }
    }

    private void FixedUpdate()
    {
        if (onTrain)
        {
            transform.Translate(TrainEngine.Instance.currentTrainPosition - TrainEngine.Instance.lastTrainPosition);
        }
    }

    private void UpdateAnimations(Vector2 velocity)
    {
        anim.SetFloat("xspeed", Mathf.Abs(velocity.x));
        anim.SetFloat("yspeed", velocity.y);
        anim.SetBool("grounded", isGrounded);
    }

    [ServerRpc(RequireOwnership = true)]
    private void UpdateAnimationsServerRpc(Vector2 velocity)
    {
        UpdateAnimationsClientRpc(velocity);
    }

    [ClientRpc]
    private void UpdateAnimationsClientRpc(Vector2 velocity)
    {
        if (IsOwner || overwriteOwner)
        {
            return;
        }

        anim.SetFloat("xspeed", Mathf.Abs(velocity.x));
        anim.SetFloat("yspeed", velocity.y);
        anim.SetBool("grounded", isGrounded);
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            TryJump();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            GoDown();
        }

        if (Input.GetKey(KeyCode.W))
        {
            isHoldingJump = true;
        }
        else
        {
            isHoldingJump = false;
        }

        if (isHoldingJump && rb.velocity.y > 0.1f)
        {
            ApplyAdditionalJumpForce();
        }

        if (Input.GetKey(KeyCode.D))
        {
            rb.velocity = new Vector2(m_run_speed, rb.velocity.y);
            if(IsOwner)
                UpdateAnimationsServerRpc(rb.velocity);
            if (!isLookingRight)
            {
                Flip();
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rb.velocity = new Vector2(-m_run_speed, rb.velocity.y);
            if (IsOwner)
                UpdateAnimationsServerRpc(rb.velocity);
            if (isLookingRight)
            {
                Flip();
            }
        }
        else if (rb.velocity.x != 0 || isHoldingJump)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            if (IsOwner)
                UpdateAnimationsServerRpc(rb.velocity);
        }
    }

    private void ApplyAdditionalJumpForce()
    {
        
        //rb.AddForce(Vector2.up * m_hold_speed, ForceMode2D.Force);
    }

    private void TryJump()
    {
        if (isGrounded && !isBusy)
        {
            isGrounded = false;
            anim.SetTrigger("jump");
            rb.velocity = new Vector2(rb.velocity.x, m_jump_speed);
            if (IsOwner)
                TryJumpServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void TryJumpServerRpc()
    {
        //networkIsGrounded.Value = false;
        //networkPosition.Value = transform.position;
        //networkVelocity.Value = rb.velocity;
        TryJumpClientRpc();
    }

    [ClientRpc]
    private void TryJumpClientRpc()
    {
        if (IsOwner || overwriteOwner)
        {
            return;
        }

        anim.SetTrigger("jump");
    }

    private void GoDown()
    {
        if (!isGrounded && rb.velocity.y > -m_jump_speed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -m_jump_speed);

            if (IsServer)
            {
                //networkPosition.Value = transform.position;
                //networkVelocity.Value = rb.velocity;
            }
            else
            {
                //SubmitPositionServerRpc(transform.position);
                //SubmitVelocityServerRpc(rb.velocity);
            }
        }
    }

    private void Flip()
    {
        isLookingRight = !isLookingRight;
        transform.localScale = new Vector3(isLookingRight ? xSize : -xSize, transform.localScale.y, transform.localScale.z);
        if (IsOwner)
            FlipServerRpc();
    }

    [ServerRpc(RequireOwnership = true)]
    private void FlipServerRpc()
    {
        FlipClientRpc();
    }

    [ClientRpc]
    private void FlipClientRpc()
    {
        if (IsOwner || overwriteOwner)
        {
            return;
        }

        isLookingRight = !isLookingRight;
        transform.localScale = new Vector3(isLookingRight ? xSize : -xSize, transform.localScale.y, transform.localScale.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Equals("controls"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager)
            {
                ShowTrainControls();
            }
        }
        else if (other.tag.Equals("fires"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager)
            {
                ShowFireControls();
            }
        }
    }

    private void ShowTrainControls()
    {
        TrainControls.Instance.Show();
        onControls = true;
    }

    private void HideTrainControls()
    {
        TrainControls.Instance.Hide();
        onControls = false;
    }

    private void ShowFireControls()
    {
        FireControls.Instance.Show();
        onFires = true;
    }

    private void HideFireControls()
    {
        FireControls.Instance.Hide();
        onFires = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag.Equals("controls"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager)
            {
                HideTrainControls();
            }
        }
        else if (other.tag.Equals("fires"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager)
            {
                HideFireControls();
            }
        }
    }

    public void OnCollisionEnter2D_FROMFEET(Collision2D collision)
    {
        
        // Check if the collision object is on the "Train" layer
        if (((1 << collision.collider.gameObject.layer) & trainLayer) != 0)
        {
            if (!onTrain)
            {
                onTrain = true;
                // Parent player to train
                if (IsOwner)
                    SetOnTrainServerRpc(onTrain);
            }

            if (!isGrounded)
            {
                isGrounded = true;
                if (rb.velocity.y < -1)
                {
                    anim.SetTrigger("contact");
                }
                if (IsOwner)
                    SetGroundedServerRpc(isGrounded);
            }
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            if (onTrain)
            {
                onTrain = false;
                // Unparent player from train
                if (IsOwner)
                    SetOnTrainServerRpc(onTrain);
            }

            if (!isGrounded)
            {
                isGrounded = true;
                if (rb.velocity.y < -1)
                {
                    anim.SetTrigger("contact");
                }
                if (IsOwner)
                    SetGroundedServerRpc(isGrounded);
            }
        }
    }

    public void OnCollisionExit2D_FROMFEET(Collision2D collision)
    {
        GameObject collisionObject = collision.gameObject;
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (isGrounded)
            {
                isGrounded = false;
                if (IsOwner)
                    SetGroundedServerRpc(isGrounded);
            }
        }
        else if (((1 << collision.collider.gameObject.layer) & trainLayer) != 0)
        {
            if (isGrounded)
            {
                isGrounded = false;
                if (IsOwner)
                    SetGroundedServerRpc(isGrounded);
            }
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void SetOnTrainServerRpc(bool onTrain)
    {
        SetOnTrainClientRpc(onTrain);
    }

    [ClientRpc]
    private void SetOnTrainClientRpc(bool onTrain)
    {
        isGrounded = onTrain;
        // Parent player to train
    }

    [ServerRpc(RequireOwnership = true)]
    private void SetGroundedServerRpc(bool grounded)
    {
        SetGroundedClientRpc(grounded);
    }

    [ClientRpc]
    private void SetGroundedClientRpc(bool grounded)
    {
        isGrounded = grounded;
        anim.SetBool("grounded", grounded);
    }

    public void OnCollisionStay2D_FROMFEET(Collision2D collision)
    {
        // This method can be implemented if needed for specific collision logic
    }

    private void ResolveMachinist()
    {
        if (onControls)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TrainControls.Instance.Increase();
            }
            else
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TrainControls.Instance.Decrease();
            }
        }
        else
        if (onFires)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                FireControls.Instance.Increase();
            }
        }
    }

    private void ResolveLumberjack()
    {
        if (onControls)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TrainControls.Instance.Increase();
            }
            else
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TrainControls.Instance.Decrease();
            }
        }
        else
        if (onFires)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                FireControls.Instance.Increase();
            }
        }
    }

    private void ResolveMiner()
    {
        
    }

    private void ResolveHunter()
    {
        
    }
}