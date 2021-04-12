using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyMotor : MonoBehaviour, IMotor
{
    Rigidbody2D enemyRigidbody;
    SpriteRenderer spriteRenderer;
    Animator anim;
    //EnnemyHealth enemyHealth;
    public bool showGizmos;
    public bool invertFlipX = false;
    public float speed = 1.1f;
    public bool isFalling;
    public LayerMask ground;
    public float groundCheckDistance = 0.45f;
    public float wallCheckDistance = 0.45f;
    public float radiusLeftRightDetector = 0.1f;
    public float heightCheck = 0.2f;
    public float holeCheckYOffset = 0.3f;
    public float jumpForce = 9f;
    public float oneBlockSize = 2f;
    public float maxSlopeAngle = 25f;
    public bool preventsWalkingInHole;
    public bool preventFallingOneBlock;
    public bool autoJump = true;
    float airSpeed;
    public bool isGrounded { get; private set; }
    public float inputAxis;
    bool hasStartedJumping;
    float knockbackAxis;
    float totalMovement;
    public bool isWalking;
    bool startDamageWall; //when enemy is stuck inside a wall

    bool onSlopeLeft;
    bool onSlopeRight;
    Vector2 groundNormal;

    public DetectInfo rightWallDetection { get; private set; }
    public DetectInfo leftWallDetection { get; private set; }
    public DetectInfo rightHoleDetection { get; private set; }
    public DetectInfo leftHoleDetection { get; private set; }

    public enum DetectInfo
    {
        None,
        OneBlock, //hole or wall is 1 block tall
        TwoBlocks,
        ThreeBlocks
    }

    private void Start()
    {
        enemyRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        DetectSlope();
        DetectGround();

        DetectRightObstacle();
        DetectLeftObstacle();
        /*DetectLeftHole();
        DetectRightHole();*/

        if (onSlopeLeft || onSlopeRight)
        {
            enemyRigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            enemyRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            //enemyRigidbody.gravityScale = 2f;
        }

        anim.SetBool("Walk", inputAxis != 0);

/*#if UNITY_EDITOR
        if (Keyboard.current.rightArrowKey.isPressed)
            inputAxis = 1;
        else if (Keyboard.current.leftArrowKey.isPressed)
            inputAxis = -1;
        else
            inputAxis = 0;

        if (Keyboard.current.upArrowKey.isPressed)
            Jump();

        if (Input.GetKeyDown(KeyCode.T))
            SetKnockback(5f, true);
#endif*/

        //print("Ground: " + isGrounded + "/ SlopeRight: " + onSlopeRight + "/ SlopeLeft: " + onSlopeLeft);

        totalMovement = inputAxis;
        totalMovement += knockbackAxis;

        if (totalMovement != 0)
            Move(totalMovement);
    }

    private void LateUpdate()
    {
        #region Stuck In Wall
        if (CheckStuckWall())
        {
            //print("ENEMY IS STUCK IN A WALL");
            if (!startDamageWall)
            {
                //StartCoroutine(DoWallDamage());
                startDamageWall = true;
            }
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.2f);
        }
        else
        {
            //StopCoroutine(DoWallDamage());
            if (startDamageWall == true)
            {
                startDamageWall = false;
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
        }
        #endregion
    }

    #region Movement
    /// <summary>
    /// Move this enemy in the direction specified in the bool
    /// </summary>
    /// <param name="right">True if we want to move right</param>
    public void Move(float axis)
    {
        bool right = inputAxis > 0;

        if (CanMove(right) == false)
            return;

        if (right)
        {
            DetectRightHole();
        }
        else
        {
            DetectLeftHole();
        }

        enemyRigidbody.velocity = new Vector2(0, enemyRigidbody.velocity.y);

        bool obstacleIsPresent = ObstacleIsPresent(axis > 0);

        /*float direction = 1f;
        if (right == false)
            direction = -1f;*/

        Vector2 vectorDirection = Vector2.right;

        if (isGrounded && groundNormal != null)
            vectorDirection = new Vector2(groundNormal.y, -groundNormal.x);

        vectorDirection *= axis;

        if (obstacleIsPresent == false)
        {
            this.transform.Translate(vectorDirection * speed * Time.deltaTime);
            //anim.SetBool("Walk", true);
        }
        else if (obstacleIsPresent == true)
        {
            if (((right == true && rightWallDetection == DetectInfo.OneBlock) || (right == false && leftWallDetection == DetectInfo.OneBlock)) && isGrounded && autoJump)
            {
                if (hasStartedJumping == false)
                    Jump();
                //this.transform.Translate(new Vector2(direction, 0) * speed * Time.deltaTime);
                this.transform.Translate(vectorDirection * speed * Time.deltaTime);
                //anim.SetBool("Walk", true);
            }
        }

        if (inputAxis != 0)
        {
            if (!right)
                spriteRenderer.flipX = invertFlipX;
            else
                spriteRenderer.flipX = !invertFlipX;
        }
    }

    /// <summary>
    /// True if there is an obstacle or hole in the direction
    /// </summary>
    /// <param name="right">True if direction is right, false if its left</param>
    /// <returns></returns>
    bool ObstacleIsPresent(bool right)
    {
        if (right)
        {
            if (rightWallDetection != DetectInfo.None)
                return true;

            if (preventsWalkingInHole)
                if ((rightHoleDetection == DetectInfo.TwoBlocks && onSlopeRight == false) || (rightHoleDetection == DetectInfo.ThreeBlocks)) //we tolerate going down 2 blocks if were on a slope (bugfix)
                {
                    if (isGrounded && Mathf.Abs(knockbackAxis) < 0.1f)
                        return true;
                }

            if (preventFallingOneBlock)
                if (rightHoleDetection == DetectInfo.OneBlock && onSlopeRight == false)
                    return true;
        }
        else
        {
            if (leftWallDetection != DetectInfo.None)
                return true;

            if (preventsWalkingInHole)
                if ((leftHoleDetection == DetectInfo.TwoBlocks && onSlopeLeft == false) || leftHoleDetection == DetectInfo.ThreeBlocks)
                {
                    if (isGrounded && Mathf.Abs(knockbackAxis) < 0.1f)
                        return true;
                }

            if (preventFallingOneBlock)
                if (leftHoleDetection == DetectInfo.OneBlock && onSlopeLeft == false)
                    return true;
        }

        return false;
    }

    public void Jump()
    {
        if (isGrounded)
        {
            //anim.SetBool("Jump", true);
            StartCoroutine(StartedJump());
            enemyRigidbody.gravityScale = 2f;
            enemyRigidbody.velocity = new Vector2(0f, 0f);

            enemyRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator StartedJump()
    {
        hasStartedJumping = true;
        isFalling = false;
        yield return new WaitForSeconds(0.2f);
        /*anim.SetBool("Jump", false);

        enemyRigidbody.gravityScale = 2f;
        enemyRigidbody.velocity = new Vector2(0f, 0f);

        enemyRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);*/

        yield return new WaitForSeconds(0.15f);
        isFalling = true;
        hasStartedJumping = false;
    }

    bool CanMove(bool right)
    {
        /*if (enemyHealth.health <= 0)
            return false;*/

        return true;
    }
    #endregion

    #region Knockback

    void IMotor.SetKnockback(float _knockbackAxis)
    {
        knockbackAxis = _knockbackAxis;

        StopCoroutine("ReduceKnockbackRoutine");
        StartCoroutine(ReduceKnockbackRoutine());
    }

    public void SetKnockback(float knockbackForce, bool oppositeToEnemyDirection = false)
    {
        float axis = knockbackForce;

        if (oppositeToEnemyDirection)
        {
            if ((spriteRenderer.flipX != invertFlipX && knockbackForce > 0) || (spriteRenderer.flipX == invertFlipX && knockbackForce < 0))
                axis *= -1f;
        }

       GetComponent<IMotor>().SetKnockback(axis);
    }

    IEnumerator ReduceKnockbackRoutine()
    {
        while (Mathf.Abs(knockbackAxis) > 0)
        {
            knockbackAxis = Mathf.Lerp(knockbackAxis, 0, Time.deltaTime * 6f);
            if (Mathf.Abs(knockbackAxis) < 0.1f)
                knockbackAxis = 0f;

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    #region Obstacle Detection
    void DetectSlope()
    {
        if (isGrounded)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance + 0.05f, ground);

            if (hit.collider != null && Mathf.Abs(hit.normal.x) > 0.1f && isGrounded)
            {
                if (hit.normal.x < 0)
                {
                    onSlopeRight = false;
                    onSlopeLeft = true;
                }
                else if (hit.normal.x > 0)
                {
                    onSlopeLeft = false;
                    onSlopeRight = true;
                }
            }
            else
            {
                onSlopeLeft = false;
                onSlopeRight = false;
            }
        }
        else
        {
            onSlopeLeft = false;
            onSlopeRight = false;
        }
    }

    void DetectGround()
    {
        bool checkGround = false; //true if we're grounded

        Vector2 origin1 = new Vector2(transform.position.x - 0.06f, transform.position.y);
        RaycastHit2D groundHit1 = Physics2D.Raycast(origin1, Vector2.down, groundCheckDistance, ground);

        Vector2 origin2 = new Vector2(transform.position.x, transform.position.y);
        RaycastHit2D groundHit2 = Physics2D.Raycast(origin2, Vector2.down, groundCheckDistance, ground);

        Vector2 origin3 = new Vector2(transform.position.x + 0.06f, transform.position.y);
        RaycastHit2D groundHit3 = Physics2D.Raycast(origin3, Vector2.down, groundCheckDistance, ground);

        if (groundHit1.collider != null)
        {
            if (groundHit1.collider.isTrigger == false && GroundAngleIsValid(groundHit1.normal))
            {
                groundNormal = groundHit1.normal;
                checkGround = true;
            }
            else
                checkGround = false;
        }
        else if (groundHit2.collider != null)
        {
            if (groundHit2.collider.isTrigger == false && GroundAngleIsValid(groundHit2.normal))
            {
                groundNormal = groundHit2.normal;
                checkGround = true;
            }
            else
                checkGround = false;
        }
        else if (groundHit3.collider != null)
        {
            if (groundHit3.collider.isTrigger == false && GroundAngleIsValid(groundHit3.normal))
            {
                groundNormal = groundHit3.normal;
                checkGround = true;
            }
            else
                checkGround = false;
        }
        else
        {
            checkGround = false;
        }

        /*if (checkGround == false && Mathf.Abs(enemyRigidbody.velocity.y) <= 0.05f)
            checkGround = true;*/

        isGrounded = checkGround;

        if (checkGround)
            isFalling = false;
        else
            isFalling = true;

    }

    void DetectRightObstacle()
    {
        bool twoBlocks = false;
        bool oneBlock = false;

        RaycastHit2D[] wallRightHit = Physics2D.RaycastAll(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y + heightCheck), Vector2.right, wallCheckDistance, ground);
        if (showGizmos)
            Debug.DrawRay(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y + heightCheck), Vector2.right, Color.blue);

        for (int i = 0; i < wallRightHit.Length; i++)
        {
            if (wallRightHit[i] && wallRightHit[i].transform.gameObject != this.transform.gameObject && Mathf.Abs(wallRightHit[i].normal.x) > 0.9f)
            {
                //rightWallDetection = DetectInfo.TwoBlocks;
                twoBlocks = true;
                //return;
            }
        }

        RaycastHit2D[] wallRightHitUnder = Physics2D.RaycastAll(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y + heightCheck - oneBlockSize), Vector2.right, wallCheckDistance, ground);
        if (showGizmos)
            Debug.DrawRay(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y + heightCheck - oneBlockSize), Vector2.right, Color.blue);

        for (int i = 0; i < wallRightHitUnder.Length; i++)
        {
            if (wallRightHitUnder[i] && wallRightHitUnder[i].transform.gameObject != this.transform.gameObject && Mathf.Abs(wallRightHitUnder[i].normal.x) > 0.9f)
            {
                //rightWallDetection = DetectInfo.OneBlock;
                oneBlock = true;
                //return;
            }
        }

        if (twoBlocks && oneBlock)
            rightWallDetection = DetectInfo.TwoBlocks;
        else if (twoBlocks == false && oneBlock)
            rightWallDetection = DetectInfo.OneBlock;
        else
            rightWallDetection = DetectInfo.None;
    }

    void DetectLeftObstacle()
    {
        bool twoBlocks = false;
        bool oneBlock = false;

        RaycastHit2D[] wallLeftHit = Physics2D.RaycastAll(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y + heightCheck), Vector2.left, wallCheckDistance, ground);
        if (showGizmos)
            Debug.DrawRay(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y + heightCheck), Vector2.left, Color.blue);
        for (int i = 0; i < wallLeftHit.Length; i++)
        {
            if (wallLeftHit[i] && wallLeftHit[i].transform.gameObject != this.transform.gameObject && Mathf.Abs(wallLeftHit[i].normal.x) > 0.9f)
            {
                //leftWallDetection = DetectInfo.TwoBlocks;
                twoBlocks = true;
                //return;
            }
        }

        RaycastHit2D[] wallLeftHitUnder = Physics2D.RaycastAll(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y + heightCheck - oneBlockSize), Vector2.left, wallCheckDistance, ground);
        if (showGizmos)
            Debug.DrawRay(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y + heightCheck - oneBlockSize), Vector2.left, Color.blue);
        for (int i = 0; i < wallLeftHitUnder.Length; i++)
        {
            if (wallLeftHitUnder[i] && wallLeftHitUnder[i].transform.gameObject != this.transform.gameObject && Mathf.Abs(wallLeftHitUnder[i].normal.x) > 0.9f)
            {
                //leftWallDetection = DetectInfo.OneBlock;
                oneBlock = true;
                //return;
            }
        }

        if (twoBlocks && oneBlock)
            leftWallDetection = DetectInfo.TwoBlocks;
        else if (twoBlocks == false && oneBlock)
            leftWallDetection = DetectInfo.OneBlock;
        else
            leftWallDetection = DetectInfo.None;
    }

    void DetectRightHole()
    {
        RaycastHit2D[] holeRightHit = Physics2D.RaycastAll(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset), Vector2.down, 0.1f, ground);
        if (showGizmos)
            Debug.DrawLine(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset), new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset - 0.1f), Color.red);

        for (int i = 0; i < holeRightHit.Length; i++)
        {
            if (holeRightHit[i] && holeRightHit[i].transform.gameObject != this.transform.gameObject)
            {
                rightHoleDetection = DetectInfo.None;
                return;
            }
        }

        RaycastHit2D[] holeRightHitUnder = Physics2D.RaycastAll(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset), Vector2.down, 0.1f + oneBlockSize, ground);
        if (showGizmos)
            Debug.DrawLine(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset), new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset - 0.1f - oneBlockSize), Color.green);

        for (int i = 0; i < holeRightHitUnder.Length; i++)
        {
            if (holeRightHitUnder[i] && holeRightHitUnder[i].transform.gameObject != this.transform.gameObject)
            {
                rightHoleDetection = DetectInfo.OneBlock;
                return;
            }
        }

        RaycastHit2D[] holeRightHitUnder2 = Physics2D.RaycastAll(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset), Vector2.down, 0.1f + (oneBlockSize * 2), ground);
        if (showGizmos)
            Debug.DrawLine(new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset), new Vector2(transform.position.x + radiusLeftRightDetector, transform.position.y - holeCheckYOffset - 0.1f - (oneBlockSize * 2)), Color.yellow);

        for (int i = 0; i < holeRightHitUnder2.Length; i++)
        {
            if (holeRightHitUnder2[i] && holeRightHitUnder2[i].transform.gameObject != this.transform.gameObject)
            {
                if (Mathf.Abs(holeRightHitUnder2[i].normal.x) < 0.1f) //if its not a slope
                    rightHoleDetection = DetectInfo.TwoBlocks;
                else
                    rightHoleDetection = DetectInfo.OneBlock;
                return;
            }
        }

        rightHoleDetection = DetectInfo.ThreeBlocks;
    }

    void DetectLeftHole()
    {
        RaycastHit2D[] holeLeftHit = Physics2D.RaycastAll(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset), Vector2.down, 0.1f, ground);
        if (showGizmos)
            Debug.DrawLine(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset), new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset - 0.1f), Color.red);

        for (int i = 0; i < holeLeftHit.Length; i++)
        {
            if (holeLeftHit[i] && holeLeftHit[i].transform.gameObject != this.transform.gameObject)
            {
                leftHoleDetection = DetectInfo.None;
                return;
            }
        }

        RaycastHit2D[] holeLeftHitUnder = Physics2D.RaycastAll(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset), Vector2.down, 0.1f + oneBlockSize, ground);
        if (showGizmos)
            Debug.DrawLine(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset), new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset - 0.1f - oneBlockSize), Color.green);

        for (int i = 0; i < holeLeftHitUnder.Length; i++)
        {
            if (holeLeftHitUnder[i] && holeLeftHitUnder[i].transform.gameObject != this.transform.gameObject)
            {
                leftHoleDetection = DetectInfo.OneBlock;
                return;
            }
        }

        RaycastHit2D[] holeLeftHitUnder2 = Physics2D.RaycastAll(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset), Vector2.down, 0.1f + (oneBlockSize * 2), ground);
        if (showGizmos)
            Debug.DrawLine(new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset), new Vector2(transform.position.x - radiusLeftRightDetector, transform.position.y - holeCheckYOffset - 0.1f - (oneBlockSize * 2)), Color.yellow);

        for (int i = 0; i < holeLeftHitUnder2.Length; i++)
        {
            if (holeLeftHitUnder2[i] && holeLeftHitUnder2[i].transform.gameObject != this.transform.gameObject)
            {
                if (Mathf.Abs(holeLeftHitUnder2[i].normal.x) < 0.1f) //if its not a slope
                    leftHoleDetection = DetectInfo.TwoBlocks;
                else
                    leftHoleDetection = DetectInfo.OneBlock;
                return;
            }
        }

        leftHoleDetection = DetectInfo.ThreeBlocks;
    }

    bool CheckStuckWall()
    {
        //this will return true if the player is stuck in a wall

        if (leftWallDetection != DetectInfo.None && rightWallDetection != DetectInfo.None && isGrounded)
        {
            RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, 0.01f, ground);
            if (hitUp.collider != null)
            {
                if (hitUp.collider.isTrigger == false && hitUp.collider.GetComponent<PlatformEffector2D>() == null)
                    return true;
            }

        }

        return false;
    }

    bool GroundAngleIsValid(Vector2 collisionNormal)
    {
        float groundAngle = Mathf.Atan2(Mathf.Abs(collisionNormal.y), Mathf.Abs(collisionNormal.x)) * Mathf.Rad2Deg;

        if (groundAngle < maxSlopeAngle)
            return false;

        return true;
    }
    #endregion

    /*IEnumerator DoWallDamage()
    {
        while (enemyHealth.health > 0 && CheckStuckWall())
        {
            enemyHealth.Damage(enemyHealth.maxHealth * 0.1f, null, 0f);
            yield return new WaitForSeconds(1f);
        }
    }*/
}
