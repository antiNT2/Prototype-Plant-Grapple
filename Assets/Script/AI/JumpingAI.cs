using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingAI : MonoBehaviour
{
    EnemyMotor enemyMotor;
    Animator anim;
    Rigidbody2D enemyRigidbody;
    public float groundCheckDistance = 0.5f;
    public float delayBetweenJumps = 0.1f;
    public float maxHoleDepth = 3f;
    State currentState;
    enum State
    {
        Idle,
        PrepareToJump,
        Midair,
        Land
    }

    Transform target;
    float desiredInputAxis;
    bool hasBeenMidairLongEnough;
    bool stayIdle;
    bool isGrounded;

    float leftHoleDepth;
    float rightHoleDepth;

    private void Start()
    {
        enemyMotor = GetComponent<EnemyMotor>();
        anim = GetComponent<Animator>();
        target = PlayerHealth.instance.transform;
        enemyRigidbody = GetComponent<Rigidbody2D>();

        InvokeRepeating("CheckIfGrounded", 0, 0.1f);
    }

    private void Update()
    {
        CheckHoles();
        if (currentState == State.Midair)
        {
            //If we're just above the player
            if (Mathf.Abs(this.transform.position.x - target.position.x) <= 0.1f && this.transform.position.y >= target.position.y)
            {
                desiredInputAxis = 0f;
                enemyRigidbody.AddForce(Vector2.down * 30f, ForceMode2D.Force);
            }

            if (CanMove())
                enemyMotor.inputAxis = desiredInputAxis;
            else
                enemyMotor.inputAxis = 0f;
        }
        else
            enemyMotor.inputAxis = 0f;

        if (isGrounded)
        {
            if (currentState == State.Midair && hasBeenMidairLongEnough)
            {
                currentState = State.Land;
                hasBeenMidairLongEnough = false;
                anim.Play("Land");
            }
        }

        if (currentState == State.Idle && Vector2.Distance(target.position, this.transform.position) < 10f && stayIdle == false)
            JumpMove();

        //anim.SetBool("IsGrounded", isGrounded);
    }

    void JumpMove()
    {
        if (isGrounded == false && Vector2.Distance(target.position, this.transform.position) < 10f)
            return;

        currentState = State.PrepareToJump;
        desiredInputAxis = target.position.x > this.transform.position.x ? 1 : -1;
        anim.Play("PrepareJump");
    }

    bool CanMove()
    {      
        if (desiredInputAxis == 1f && rightHoleDepth >= maxHoleDepth)
            return false;
        if (desiredInputAxis == -1f && leftHoleDepth >= maxHoleDepth)
            return false;

        return true;
    }

    IEnumerator CountMidairTime()
    {
        hasBeenMidairLongEnough = false;
        yield return new WaitForSeconds(.5f);
        hasBeenMidairLongEnough = true;
    }

    IEnumerator RestABit()
    {
        stayIdle = true;
        yield return new WaitForSeconds(delayBetweenJumps);
        stayIdle = false;
    }

    void CheckIfGrounded()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        RaycastHit2D groundHit2 = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, enemyMotor.ground);

        if (groundHit2.collider != null)
        {
            if (groundHit2.collider.isTrigger == false && enemyMotor.GroundAngleIsValid(groundHit2.normal))
            {
                isGrounded = true;
            }
            else
                isGrounded = false;
        }
        else
            isGrounded = false;

        if (isGrounded == false)
        {
            if (enemyMotor.onSlopeLeft || enemyMotor.onSlopeRight || leftHoleDepth < 0.05f || rightHoleDepth < 0.05f)
                isGrounded = true;
        }
        //isGrounded = Mathf.Abs(enemyRigidbody.velocity.y) < 0.1f;
    }

    void CheckHoles()
    {
        /*Vector2 origin = new Vector2(transform.position.x - enemyMotor.radiusLeftRightDetector, transform.position.y - enemyMotor.holeCheckYOffset);
        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down);
        Debug.DrawRay(origin, Vector2.down);

        if (groundHit.collider != null)
        {
            if (groundHit.collider.isTrigger == false && enemyMotor.GroundAngleIsValid(groundHit.normal))
            {
                print(groundHit.distance);
            }
        }*/

        RaycastHit2D[] holeLeftHit = Physics2D.RaycastAll(new Vector2(transform.position.x - enemyMotor.radiusLeftRightDetector, transform.position.y - enemyMotor.holeCheckYOffset), Vector2.down, 10f, enemyMotor.ground);
        Debug.DrawLine(new Vector2(transform.position.x - enemyMotor.radiusLeftRightDetector, transform.position.y - enemyMotor.holeCheckYOffset), new Vector2(transform.position.x - enemyMotor.radiusLeftRightDetector, transform.position.y - enemyMotor.holeCheckYOffset - 10f), Color.red);

        for (int i = 0; i < holeLeftHit.Length; i++)
        {
            if (holeLeftHit[i] && holeLeftHit[i].transform.gameObject != this.transform.gameObject)
            {
                leftHoleDepth = holeLeftHit[i].distance;
                break;
            }
        }

        RaycastHit2D[] holeRightHit = Physics2D.RaycastAll(new Vector2(transform.position.x + enemyMotor.radiusLeftRightDetector, transform.position.y - enemyMotor.holeCheckYOffset), Vector2.down, 10f, enemyMotor.ground);
        Debug.DrawLine(new Vector2(transform.position.x + enemyMotor.radiusLeftRightDetector, transform.position.y - enemyMotor.holeCheckYOffset), new Vector2(transform.position.x + enemyMotor.radiusLeftRightDetector, transform.position.y - enemyMotor.holeCheckYOffset - 10f), Color.red);

        for (int i = 0; i < holeRightHit.Length; i++)
        {
            if (holeRightHit[i] && holeRightHit[i].transform.gameObject != this.transform.gameObject)
            {
                rightHoleDepth = holeRightHit[i].distance;
                break;
            }
        }
    }

    #region Animator Methods
    public void ApplyJumpVelocity()
    {
        currentState = State.Midair;
        enemyMotor.Jump();
        anim.Play("Midair");
        isGrounded = false;
        StopCoroutine(CountMidairTime());
        StartCoroutine(CountMidairTime());
    }

    public void LandJump()
    {
        currentState = State.Idle;
        anim.Play("Idle");
        StartCoroutine(RestABit());
    }
    #endregion
}
