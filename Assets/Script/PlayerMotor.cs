using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotor : MonoBehaviour
{
    public static PlayerMotor instance;

    public float speed = 1f;
    public float acceleration = 1.5f;
    public float jumpForce = 12f;
    public float moveForceGrapple = 1f;
    [SerializeField]
    SpriteRenderer playerRenderer;

    PlayerInput playerInput;
    float inputAxis;
    float knockbackAxis;
    float totalMovement;
    Animator playerAnimator;
    Rigidbody2D playerRigidbody;
    bool jumpButtonPressed;
    float initialGravity;
    public bool isLookingRight { get; private set; } = true;

    //Ground Detection Variables :
    public float groundCheckDistance = 0.5f;
    public float groundCheckHeightOffset;
    public float groundCheckersSeparationDistance = 0.1f;
    public bool isGrounded;
    public bool isFalling;
    public LayerMask groundLayer;

    //Wall Detection Variables :
    public float wallCheckDistance = 0.12f;
    public float wallCheckHeightOffset;
    public bool wallLeft { get; private set; }
    public bool wallRight { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = playerRenderer.GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        initialGravity = playerRigidbody.gravityScale;
    }

    private void Update()
    {
        DetectGround();
        CheckWalls();
        jumpButtonPressed = playerInput.actions.FindAction("Jump").phase == InputActionPhase.Started;

        if (playerInput.actions.FindAction("Move").triggered && playerInput.actions.FindAction("Move").phase == InputActionPhase.Started)
        {
            SetCorrectRenderOrientation(playerInput.actions.FindAction("Move").ReadValue<Vector2>().x < 0f);
            StartCoroutine(AccelerateInput());
        }

        if (playerInput.actions.FindAction("Jump").triggered && isGrounded)
            StartCoroutine(JumpRoutine());

        totalMovement = inputAxis;
        totalMovement += knockbackAxis;

        if (totalMovement != 0)
        {
            if (!ShouldUseForceMovement())
            {
                if (!(isFalling && RopeManager.instance.currentState == RopeManager.RopeState.Retracting && !AxisIsOppositeToVelocity(totalMovement) && Mathf.Abs(playerRigidbody.velocity.x) > 0.5f))
                    // print("Do Nothing (velocity.x is " + playerRigidbody.velocity.x);
                    Move(totalMovement);
            }
        }

        if (ShouldUseForceMovement())
            playerRigidbody.gravityScale = initialGravity;

        SetAnimations();
    }

    private void FixedUpdate()
    {
        if (totalMovement != 0)
        {
            if (ShouldUseForceMovement())
                GrappleMove(totalMovement);
        }
    }

    bool ShouldUseForceMovement()
    {
        if (isFalling && (RopeManager.instance.currentState == RopeManager.RopeState.LockedOn /*|| RopeManager.instance.currentState == RopeManager.RopeState.Retracting*/))
            return true;

        return false;
    }

    public void SetCorrectRenderOrientation(bool lookLeft)
    {
        if (lookLeft)
            playerRenderer.transform.rotation = Quaternion.Euler(0, 180f, 0);
        else
            playerRenderer.transform.rotation = Quaternion.identity;

        isLookingRight = !lookLeft;
    }

    void SetAnimations()
    {
        playerAnimator.SetBool("Walk", totalMovement != 0);
        playerAnimator.SetBool("Fall", isFalling);
        if (isGrounded)
            playerAnimator.SetBool("Descend", false);
    }

    #region Knockback
    public void SetKnockback(float knockbackForce, bool oppositeToPlayerDirection = false)
    {
        knockbackAxis = knockbackForce;

        if (oppositeToPlayerDirection)
        {
            if ((isLookingRight && knockbackForce > 0) || (!isLookingRight && knockbackForce < 0))
                knockbackAxis *= -1f;
        }

        StartCoroutine(ReduceKnockbackRoutine());
    }

    IEnumerator ReduceKnockbackRoutine()
    {
        // float deceleration = 1f;
        // float direction = 1f;
        // if (knockbackAxis < 0)
        //    direction = -1f;
        while (Mathf.Abs(knockbackAxis) > 0)
        {
            knockbackAxis = Mathf.Lerp(knockbackAxis, 0, Time.deltaTime * 6f);
            if (Mathf.Abs(knockbackAxis) < 0.1f)
                knockbackAxis = 0f;

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    #region Jump
    IEnumerator JumpRoutine()
    {
        //Add force on the first frame of the jump
        playerRigidbody.velocity = Vector2.zero;
        playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        playerAnimator.Play("Jump");

        //Wait while the character's y-velocity is positive (the character is going
        //up)
        while (jumpButtonPressed && playerRigidbody.velocity.y > 0)
        {
            yield return null;
        }

        //If the jumpButton is released but the character's y-velocity is still
        //positive...
        if (playerRigidbody.velocity.y > 0 && !ShouldUseForceMovement())
        {
            //...set the character's y-velocity to 0;
            //playerRigidbody.velocity = new Vector2(playerRigidbody.velocity.x, 0);
            StartCoroutine(VerticalVelocityDecreaseRoutine());
        }
        playerRigidbody.gravityScale = initialGravity * 2f;
        playerAnimator.SetBool("Descend", true);

        yield break;

        //jumping = false;
    }

    IEnumerator VerticalVelocityDecreaseRoutine()
    {
        while (playerRigidbody.velocity.y > 0 && !ShouldUseForceMovement())
        {
            playerRigidbody.velocity = Vector2.Lerp(playerRigidbody.velocity, new Vector2(playerRigidbody.velocity.x, 0f), Time.fixedDeltaTime * 6f);
            //print(playerRigidbody.velocity.y);
            yield return new WaitForFixedUpdate();
        }

        yield break;
    }
    #endregion

    IEnumerator AccelerateInput()
    {
        float outputX = 0.3f * Mathf.RoundToInt(playerInput.actions.FindAction("Move").ReadValue<Vector2>().x);

        while (playerInput.actions.FindAction("Move").ReadValue<Vector2>().x != 0)
        {
            //print(playerInput.actions.FindAction("Move").ReadValue<Vector2>().x);

            outputX += Time.deltaTime * Mathf.Exp(acceleration) * Mathf.RoundToInt(playerInput.actions.FindAction("Move").ReadValue<Vector2>().x);
            outputX = Mathf.Clamp(outputX, -1f, 1f);
            inputAxis = outputX;
            yield return new WaitForEndOfFrame();
        }


        inputAxis = 0;
    }
    void Move(float axis)
    {
        playerRigidbody.velocity = new Vector2(0f, playerRigidbody.velocity.y);

        Vector2 movement = Vector2.right * speed * axis * Time.deltaTime;
        if ((axis > 0 && !wallRight) || (axis < 0 && !wallLeft))
            this.transform.Translate(movement);
    }

    bool AxisIsOppositeToVelocity(float axis)
    {
        if (playerRigidbody.velocity.x > 0 && axis < 0 || playerRigidbody.velocity.x < 0 && axis > 0)
            return true;
        else
            return false;
    }

    void GrappleMove(float axis)
    {
        playerRigidbody.AddForce(Vector2.right * axis * moveForceGrapple);
        //Debug.Log("GrappleMOve");
    }

    #region Obstacle Detection
    void DetectGround()
    {
        bool checkGround = false; //true if we're grounded

        Vector2 origin1 = new Vector2(transform.position.x - groundCheckersSeparationDistance, transform.position.y + groundCheckHeightOffset);
        RaycastHit2D groundHit1 = Physics2D.Raycast(origin1, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawLine(origin1, (origin1 + Vector2.down * groundCheckDistance));

        Vector2 origin2 = new Vector2(transform.position.x, transform.position.y + groundCheckHeightOffset);
        RaycastHit2D groundHit2 = Physics2D.Raycast(origin2, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawLine(origin2, (origin2 + Vector2.down * groundCheckDistance));

        Vector2 origin3 = new Vector2(transform.position.x + groundCheckersSeparationDistance, transform.position.y + groundCheckHeightOffset);
        RaycastHit2D groundHit3 = Physics2D.Raycast(origin3, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawLine(origin3, (origin3 + Vector2.down * groundCheckDistance));

        if (groundHit1.collider != null)
        {
            if (groundHit1.collider.isTrigger == false)
                checkGround = true;
            else
                checkGround = false;
        }
        else if (groundHit2.collider != null)
        {
            if (groundHit2.collider.isTrigger == false)
                checkGround = true;
            else
                checkGround = false;
        }
        else if (groundHit3.collider != null)
        {
            if (groundHit3.collider.isTrigger == false)
                checkGround = true;
            else
                checkGround = false;
        }
        else
        {
            checkGround = false;
        }

        //print(playerRigidbody.velocity.y);
        /* if (checkGround == true && playerRigidbody.velocity.y > 3f)
             checkGround = false;*/

        if (checkGround == true)
            playerRigidbody.gravityScale = initialGravity;

        isGrounded = checkGround;

        if (checkGround)
            isFalling = false;
        else
            isFalling = true;

    }

    void CheckWalls()
    {
        Vector2 rayOrigin = transform.position + new Vector3(0, wallCheckHeightOffset);
        RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, wallCheckDistance, groundLayer);
        Debug.DrawLine(rayOrigin, (Vector3)rayOrigin + (Vector3)(Vector2.left * wallCheckDistance), Color.red);
        if (hitLeft.collider != null)
        {
            if (hitLeft.collider.isTrigger == false)
                wallLeft = true;
            else
                wallLeft = false;

            if (hitLeft.distance < wallCheckDistance)
                MoveAwayFromWall(false, hitLeft.distance);
        }
        else
        {
            wallLeft = false;
        }

        RaycastHit2D hitRight = Physics2D.Raycast(rayOrigin, Vector2.right, wallCheckDistance, groundLayer);
        Debug.DrawLine(rayOrigin, (Vector3)rayOrigin + (Vector3)(Vector2.right * wallCheckDistance), Color.red);
        if (hitRight.collider != null)
        {
            if (hitRight.collider.isTrigger == false)
                wallRight = true;
            else
                wallRight = false;


            if (hitRight.distance < wallCheckDistance)
                MoveAwayFromWall(true, hitRight.distance);
        }
        else
        {
            wallRight = false;
        }
    }

    void MoveAwayFromWall(bool right, float wallDistance)
    {
        if (right == false)
        {
            transform.position = new Vector3(transform.position.x + (wallCheckDistance - wallDistance), transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x - (wallCheckDistance - wallDistance), transform.position.y, transform.position.z);
        }
    }
    #endregion
}
