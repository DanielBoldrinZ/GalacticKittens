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
        Hunter,
        All
    }
    public LayerMask trainLayer;
    public LayerMask resourceLayer;

    [SerializeField]
    CharacterType characterType = CharacterType.Manager;

    [Header("ShipSprites")]
    [SerializeField]
    SpriteRenderer m_shipRenderer;

    [SerializeField]
    SpriteMask mask;

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

    private bool onRock = false;
    private bool onBoulder = false;
    private bool onStone = false;
    private bool onCoal = false;
    private bool onIron = false;
    private bool onTree = false;
    private bool onBlockage = false;
    private bool onLog = false;
    private bool onPond = false;
    private bool onFish = false;
    private bool onQuest = false;
    private bool onBush = false;
    private bool onBerry = false;
    private bool onTrain = false;
    private bool onTrainDamage = false;
    private bool onControls = false;
    private bool onFires = false;
    private bool isGrounded = false;
    private bool isBusy = false;
    private bool isLookingRight = true;
    private bool isHoldingJump = false;
    private Vector3 vecSpeed;
    private Interactable currInteractable;
    private Resource currResource;

    //private NetworkVariable<bool> networkIsLookingRight = new NetworkVariable<bool>(true);
    //private NetworkVariable<bool> networkIsGrounded = new NetworkVariable<bool>(true);
    //private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>(Vector3.zero);
    //private NetworkVariable<Vector2> networkVelocity = new NetworkVariable<Vector2>(Vector2.zero);

    private Vector3 trainLastPosition;

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
            //if (onTrain)
            //{
            //    if (trainLastPosition != null)
            //    {
            //        transform.Translate(TrainEngine.Instance.gameObject.transform.position - trainLastPosition);
            //    }
            //    trainLastPosition = TrainEngine.Instance.gameObject.transform.position;
            //}
            return;
        }

        HandleKeyboardInput();
        UpdateAnimations(rb.velocity);

        switch (characterType)
        {
            case CharacterType.All:
                ResolveAll();
                break;
            case CharacterType.Manager:
                ResolveAll();
                //ResolveManager();
                break;
            case CharacterType.Lumberjack:
                ResolveAll();
                //ResolveLumberjack();
                break;
            case CharacterType.Miner:
                ResolveAll();
                //ResolveMiner();
                break;
            case CharacterType.Hunter:
                ResolveAll();
                //ResolveHunter();
                break;
        }
    }

    void LateUpdate()
    {
        if (mask.sprite != m_shipRenderer.sprite)
        {
            mask.sprite = m_shipRenderer.sprite;
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
        if (!IsOwner && !overwriteOwner)
        {
            return;
        }


        if (other.tag.Equals("controls"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager || characterType == CharacterType.All)
            {
                ShowTrainControls();
            }
        }
        else if (other.tag.Equals("fires"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager || characterType == CharacterType.All)
            {
                ShowFireControls();
            }
        }
        else if (((1 << other.gameObject.layer) & resourceLayer) != 0)
        {
            Resource resource = other.GetComponentInChildren<Resource>();
            if (resource != null)
            {
                if (currResource != null)
                {
                    currResource.HideCanvas();
                }
                if (currInteractable != null)
                {
                    currInteractable.HideCanvas();
                    currInteractable = null;
                    ResetInteracts();
                }
                currResource = resource;

                switch (resource.resourceType)
                {
                    case Resource.ResourceType.Stone:
                        if (!onStone)
                        {
                            onStone = true;
                            resource.ShowCanvas();
                        }
                        break;
                    case Resource.ResourceType.Log:
                        if (!onLog)
                        {
                            onLog = true;
                            resource.ShowCanvas();
                        }
                        break;
                    case Resource.ResourceType.Berry:
                        if (!onBerry)
                        {
                            onBerry = true;
                            resource.ShowCanvas();
                        }
                        break;
                    case Resource.ResourceType.Fish:
                        if (!onFish)
                        {
                            onFish = true;
                            resource.ShowCanvas();
                        }
                        break;
                    case Resource.ResourceType.Iron:
                        if (!onIron)
                        {
                            onIron = true;
                            resource.ShowCanvas();
                        }
                        break;
                    case Resource.ResourceType.Coal:
                        if (!onCoal)
                        {
                            onCoal = true;
                            resource.ShowCanvas();
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        else if (other.tag.Equals("interact"))
        {
            Interactable interact = other.GetComponent<Interactable>();
            if (interact != null)
            {
                if (currInteractable != null)
                {
                    currInteractable.HideCanvas();
                }
                currInteractable = interact;
                if (currResource != null)
                {
                    currResource.HideCanvas();
                    currResource = null;
                    ResetResources();
                }

                switch (interact.interactableType)
                {
                    case Interactable.InteractableType.Boulder:
                        if (characterType == CharacterType.Miner || characterType == CharacterType.All)
                        {
                            if (!onBoulder)
                            {
                                onBoulder = true;
                                interact.ShowCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Rock:
                        if (characterType == CharacterType.Miner || characterType == CharacterType.All)
                        {
                            if (!onRock)
                            {
                                onRock = true;
                                interact.ShowCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Tree:
                        if (characterType == CharacterType.Lumberjack || characterType == CharacterType.All)
                        {
                            if (!onTree)
                            {
                                onTree = true;
                                interact.ShowCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Blockage:
                        if (characterType == CharacterType.Lumberjack || characterType == CharacterType.All)
                        {
                            if (!onBlockage)
                            {
                                onBlockage = true;
                                interact.ShowCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Bush:
                        onBush = true;
                        interact.ShowCanvas();
                        break;
                    case Interactable.InteractableType.Pond:
                        if (characterType == CharacterType.Hunter || characterType == CharacterType.All)
                        {
                            if (!onPond)
                            {
                                onPond = true;
                                interact.ShowCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Quest:
                        if (characterType == CharacterType.Manager || characterType == CharacterType.All)
                        {
                            if (!onQuest)
                            {
                                onQuest = true;
                                interact.ShowCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.TrainDamage:
                        if (characterType == CharacterType.Miner || characterType == CharacterType.Lumberjack || characterType == CharacterType.All)
                        {
                            if (!onTrainDamage)
                            {
                                onTrainDamage = true;
                                interact.ShowCanvas();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void ResetInteracts()
    {
        onTree = false;
        onRock = false;
        onPond = false;
        onBush = false;
        onQuest = false;
        onTrainDamage = false;
        onFires = false;
        onControls = false;
        onBlockage = false;
        onBoulder = false;
    }

    private void ResetResources()
    {
        onLog = false;
        onBerry = false;
        onFish = false;
        onStone = false;
        onCoal = false;
        onIron = false;
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
        if (!IsOwner && !overwriteOwner)
        {
            return;
        }

        if (other.tag.Equals("controls"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager || characterType == CharacterType.All)
            {
                HideTrainControls();
            }
        }
        else if (other.tag.Equals("fires"))
        {
            if (characterType == CharacterType.Lumberjack || characterType == CharacterType.Manager || characterType == CharacterType.All)
            {
                HideFireControls();
            }
        }
        else if (((1 << other.gameObject.layer) & resourceLayer) != 0)
        {
            Resource resource = other.GetComponentInChildren<Resource>();
            if (resource != null)
            {
                switch (resource.resourceType)
                {
                    case Resource.ResourceType.Stone:
                        if (onStone)
                        {
                            onStone = false;
                            resource.HideCanvas();
                        }
                        break;
                    case Resource.ResourceType.Log:
                        if (onLog)
                        {
                            onLog = false;
                            resource.HideCanvas();
                        }
                        break;
                    case Resource.ResourceType.Berry:
                        if (onBerry)
                        {
                            onBerry = false;
                            resource.HideCanvas();
                        }
                        break;
                    case Resource.ResourceType.Fish:
                        if (onFish)
                        {
                            onFish = false;
                            resource.HideCanvas();
                        }
                        break;
                    case Resource.ResourceType.Iron:
                        if (onIron)
                        {
                            onIron = false;
                            resource.HideCanvas();
                        }
                        break;
                    case Resource.ResourceType.Coal:
                        if (onCoal)
                        {
                            onCoal = false;
                            resource.HideCanvas();
                        }
                        break;
                    default:
                        break;
                }

                if (currResource == resource)
                {
                    currResource = null;
                }
            }
        }
        else if (other.tag.Equals("interact"))
        {
            Interactable interact = other.GetComponent<Interactable>();
            if (interact != null)
            {
                if (currInteractable == interact)
                {
                    currInteractable = null;
                }

                switch (interact.interactableType)
                {
                    case Interactable.InteractableType.Boulder:
                        if (characterType == CharacterType.Miner || characterType == CharacterType.All)
                        {
                            if (onBoulder)
                            {
                                onBoulder = false;
                                interact.HideCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Rock:
                        if (characterType == CharacterType.Miner || characterType == CharacterType.All)
                        {
                            if (onRock)
                            {
                                onRock = false;
                                interact.HideCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Tree:
                        if (characterType == CharacterType.Lumberjack || characterType == CharacterType.All)
                        {
                            if (onTree)
                            {
                                onTree = false;
                                interact.HideCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Blockage:
                        if (characterType == CharacterType.Lumberjack || characterType == CharacterType.All)
                        {
                            if (onBlockage)
                            {
                                onBlockage = false;
                                interact.HideCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Bush:
                        onBush = false;
                        interact.HideCanvas();
                        break;
                    case Interactable.InteractableType.Pond:
                        if (characterType == CharacterType.Hunter || characterType == CharacterType.All)
                        {
                            if (onPond)
                            {
                                onPond = false;
                                interact.HideCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.Quest:
                        if (characterType == CharacterType.Manager || characterType == CharacterType.All)
                        {
                            if (onQuest)
                            {
                                onQuest = false;
                                interact.HideCanvas();
                            }
                        }
                        break;
                    case Interactable.InteractableType.TrainDamage:
                        if (characterType == CharacterType.Miner || characterType == CharacterType.Lumberjack || characterType == CharacterType.All)
                        {
                            if (onTrainDamage)
                            {
                                onTrainDamage = false;
                                interact.HideCanvas();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void OnCollisionEnter2D_FROMFEET(Collision2D collision)
    {
        if (!IsOwner && !overwriteOwner)
        {
            return;
        }

        // Check if the collision object is on the "Train" layer
        if (((1 << collision.collider.gameObject.layer) & trainLayer) != 0)
        {
            if (!onTrain)
            {
                //onTrain = true;
                //// Parent player to train
                //if (IsOwner)
                if (NetworkManager.Singleton != null)
                {
                    SetOnTrainServerRpc(true);
                }
                else
                {
                    onTrain = true;
                }
            }

            if (!isGrounded)
            {
                isGrounded = true;
                if (rb.velocity.y < -1)
                {
                    anim.SetTrigger("contact");
                }
                SetGroundedServerRpc(isGrounded);
            }
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            if (onTrain)
            {
                //onTrain = false;
                //// Unparent player from train
                //if (IsOwner)
                if (NetworkManager.Singleton != null)
                {
                    SetOnTrainServerRpc(false);
                }
                else
                {
                    onTrain = false;
                }
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
    public void OnCollisionStay2D_FROMFEET(Collision2D collision)
    {
        if (!IsOwner && !overwriteOwner)
        {
            return;
        }

        // Check if the collision object is on the "Train" layer
        if (((1 << collision.collider.gameObject.layer) & trainLayer) != 0)
        {
            if (!onTrain)
            {
                //onTrain = true;
                //// Parent player to train
                //if (IsOwner)
                if (NetworkManager.Singleton != null)
                {
                    SetOnTrainServerRpc(true);
                }
                else
                {
                    onTrain = true;
                }
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
                //onTrain = false;
                //// Unparent player from train
                //if (IsOwner)
                if (NetworkManager.Singleton != null)
                {
                    SetOnTrainServerRpc(false);
                }
                else
                {
                    onTrain = false;
                }
            }

            if (!isGrounded)
            {
                isGrounded = true;
                if (rb.velocity.y < -1)
                {
                    anim.SetTrigger("contact");
                }
                if (NetworkManager.Singleton != null)
                {
                    SetGroundedServerRpc(isGrounded);
                }
                else
                {
                    isGrounded = true;
                }
            }
        }
    }
    public void OnCollisionExit2D_FROMFEET(Collision2D collision)
    {
        if (!IsOwner && !overwriteOwner)
        {
            return;
        }

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
    private void SetOnTrainServerRpc(bool _onTrain)
    {
        onTrain = _onTrain;

        //SetOnTrainClientRpc(_onTrain);
    }

    //[ClientRpc]
    //private void SetOnTrainClientRpc(bool _onTrain)
    //{
    //    if (IsOwner || overwriteOwner)
    //    {
    //        return;
    //    }
    //
    //    onTrain = _onTrain;
    //}

    [ServerRpc(RequireOwnership = true)]
    private void SetGroundedServerRpc(bool grounded)
    {
        SetGroundedClientRpc(grounded);
    }

    [ClientRpc]
    private void SetGroundedClientRpc(bool grounded)
    {
        if (IsOwner || overwriteOwner)
        {
            return;
        }

        isGrounded = grounded;
        anim.SetBool("grounded", grounded);
    }

  

    private void PickupResource()
    {
        currResource.PickUp();
        currResource = null;
    }

    private void InteractableHit()
    {
        currInteractable.Hit(transform.position.x > currInteractable.transform.position.x);
    }

    private void ResolveAll()
    {
        //////////////////////////////////////////// Train Controls
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (onControls)
            {
                TrainControls.Instance.Decrease();
                DecreaseTrainControlsServerRpc();
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (onControls)
            {
                TrainControls.Instance.Increase();
                IncreaseTrainControlsServerRpc();
            }
            else
            if (onFires)
            {
                FireControls.Instance.Increase();
                IncreaseFireControlsServerRpc();
            }//////////////////////////////////////////// Interactables
            else
            if (onRock)
            {
                InteractableHit();
            }
            else if (onTree)
            {
                InteractableHit();
            }
            else if (onBush)
            {
                InteractableHit();
            }
            else if (onPond)
            {
                InteractableHit();
            }//////////////////////////////////////////// Resources
            else if (onFish)
            {
                PickupResource();
            }
            else if (onLog)
            {
                PickupResource();
            }
            else if (onStone)
            {
                PickupResource();
            }
            else if (onIron)
            {
                PickupResource();
            }
            else if (onBerry)
            {
                PickupResource();
            }
        }
    }

    //[ServerRpc(RequireOwnership = true)]
    //private void InteractHitServerRpc()
    //{
    //    InteractHitClientRpc();
    //}
    //[ClientRpc]
    //private void InteractHitClientRpc()
    //{
    //    if (IsOwner || overwriteOwner)
    //    {
    //        return;
    //    }
    //    currInteractable.Hit();
    //}

    [ServerRpc(RequireOwnership = true)]
    private void IncreaseFireControlsServerRpc()
    {
        IncreaseFireControlsClientRpc();
    }
    [ClientRpc]
    private void IncreaseFireControlsClientRpc()
    {
        if (IsOwner || overwriteOwner)
        {
            return;
        }
        FireControls.Instance.Increase();
    }

    [ServerRpc(RequireOwnership = true)]
    private void IncreaseTrainControlsServerRpc()
    {
        IncreaseTrainControlsClientRpc();
    }
    [ClientRpc]
    private void IncreaseTrainControlsClientRpc()
    {
        if (IsOwner || overwriteOwner)
        {
            return;
        }
        TrainControls.Instance.Increase();
    }

    [ServerRpc(RequireOwnership = true)]
    private void DecreaseTrainControlsServerRpc()
    {
        DecreaseTrainControlsClientRpc();
    }
    [ClientRpc]
    private void DecreaseTrainControlsClientRpc()
    {
        if (IsOwner || overwriteOwner)
        {
            return;
        }
        TrainControls.Instance.Decrease();
    }
}