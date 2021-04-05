using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RopeManager : MonoBehaviour
{
    public static RopeManager instance;

    [SerializeField]
    RopeBridge bridgeScript;
    [SerializeField]
    GrappleRope grappleScript;
    [SerializeField]
    LineRenderer armRenderer;
    [SerializeField]
    GameObject grappleIndicator;
    [SerializeField]
    SpriteRenderer grappleDirectionIndicator;
    PlayerInput playerInput;
    DistanceJoint2D endPointJoint;

    [SerializeField]
    DistanceJoint2D pullJoint;

    [SerializeField]
    Transform startPos;
    [SerializeField]
    Transform endPos;
    [SerializeField]
    Transform restingPos;

    [SerializeField]
    Transform armEndObject;

    float initialGrappleAngle;

    [SerializeField]
    public RopeState currentState;
    [SerializeField]
    public float wallCheckDistance;
    [SerializeField]
    LayerMask grappleLayer;
    [SerializeField]
    LayerMask pullLayer;
    Rigidbody2D playerRigidbody;
    public float initialPropulsionImpulse;
    public float delayBetweenGrabs = 0.1f;
    public float pullDistanceJoint = 1f;


    float lastVelocity = 0f;
    float lastGrabTime;
    bool shouldStayAttachedToPoint;
    Vector2 lastJoystickPos;
    Transform lastGrapplePoint;
    public Transform pullingObject { get; private set; }
    bool invertYAxis = false;

    public float maxRopeLength;
    public float propulsionSpeed;
    public float grappleDirectionIndicatorRadius = 1f;

    public float actualAngleTest;
    public enum RopeState
    {
        Rest,
        Retracting,
        Traveling,
        LockedOn,
        Failed,
        Pulling
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        playerRigidbody = playerInput.GetComponent<Rigidbody2D>();
        endPointJoint = endPos.GetComponent<DistanceJoint2D>();

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            invertYAxis = true;
    }

    private void Update()
    {
        if (currentState == RopeState.Rest)
        {
            ApplyRestPosEffects();
        }
        else if (currentState == RopeState.Retracting /*|| currentState == RopeState.Pulling*/)
        {
            shouldStayAttachedToPoint = false;
            armRenderer.enabled = true;
            if (Vector2.Distance(endPos.position, restingPos.position) > 0.1f)
            {
                endPos.position = Vector3.MoveTowards(endPos.position, restingPos.position, Time.deltaTime * 20f);
                armEndObject.position = endPos.position;
            }
            else
                currentState = RopeState.Rest;

            if (!AlignHandWithRope.instance.enabled)
                AlignHandWithRope.instance.enabled = true;

        }
        else if (currentState == RopeState.Traveling || currentState == RopeState.Failed)
        {
            armRenderer.enabled = true;
            grappleScript.enabled = true;
            armEndObject.transform.parent = null;
            armEndObject.position = armRenderer.GetPosition(armRenderer.positionCount - 1);

            if (Vector2.Distance(armEndObject.position, endPos.position) < 0.1f)
            {
                OnGrappleLock();
            }
        }
        else if (currentState == RopeState.Pulling)
        {
            armEndObject.position = endPos.position;
            if (pullJoint.distance > pullDistanceJoint + 0.2f)
                pullJoint.distance = Mathf.Lerp(pullJoint.distance, pullDistanceJoint, Time.deltaTime * 3f);

            if (playerInput.actions.FindAction("Grapple").phase != InputActionPhase.Started)
            {
                pullingObject = null;
                currentState = RopeState.Retracting;
            }
        }

        if (currentState == RopeState.Rest || currentState == RopeState.Retracting || currentState == RopeState.Pulling)
        {
            if (playerInput.actions.FindAction("Grapple").triggered && Time.time > lastGrabTime + delayBetweenGrabs)
            {
                ApplyRestPosEffects();
                GrappleToTarget(GetGrappleDirection());
            }
        }
        if (currentState == RopeState.LockedOn)
        {
            if (!shouldStayAttachedToPoint)
            {
                if (playerInput.actions.FindAction("Grapple").triggered)
                    currentState = RopeState.Retracting;

                if (HasCollidedWhileFlying())
                {
                    currentState = RopeState.Retracting;
                }
            }
            else
            {
                if (playerInput.actions.FindAction("Grapple").phase != InputActionPhase.Started)
                    currentState = RopeState.Retracting;
                //print(playerInput.actions.FindAction("Grapple").phase);
            }
        }

        ShowGrapplePoint();

        ShowGrappleDirectionIndicator();

        actualAngleTest = GetGrappleAngle();

        if (Input.GetKeyDown(KeyCode.P))
            invertYAxis = !invertYAxis;

    }

    private void FixedUpdate()
    {
        if (currentState == RopeState.LockedOn)
        {
            grappleScript.enabled = true;
            AlignHandWithRope.instance.enabled = true;
            PropulseToEndLocation();
        }
    }

    private void LateUpdate()
    {
        lastVelocity = playerRigidbody.velocity.magnitude;
    }

    void ShowGrapplePoint()
    {
        (Vector2, Transform) detectedGrapplePoint = DetectWallToGrapple(GetGrappleDirection());

        if (detectedGrapplePoint.Item2 != null)
        {
            grappleIndicator.gameObject.SetActive(true);
            grappleIndicator.transform.position = detectedGrapplePoint.Item1;
        }
        else
        {
            grappleIndicator.gameObject.SetActive(false);
        }
    }

    void ShowGrappleDirectionIndicator()
    {
        float angle = Mathf.Atan2(GetGrappleDirection().y, GetGrappleDirection().x);
        grappleDirectionIndicator.transform.localPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * grappleDirectionIndicatorRadius;
    }

    void OnGrappleLock()
    {
        if (currentState == RopeState.Traveling)
        {
            //print("Arrived, " + armEndObject.position + " / " + endPos.position);
            print("Last Grapple point " + lastGrapplePoint);

            if (playerInput.actions.FindAction("Grapple").phase == InputActionPhase.Started && pullingObject != null) //if we are holding button
            {
                if (pullLayer == (pullLayer | (1 << pullingObject.gameObject.layer))) //if object is pullable
                {
                    lastGrapplePoint = null;
                    endPointJoint.enabled = false;
                    pullJoint.connectedBody = pullingObject.gameObject.GetComponent<Rigidbody2D>();

                    pullJoint.distance = Vector2.Distance(pullJoint.transform.position, pullingObject.position);
                    if (pullJoint.distance < pullDistanceJoint)
                        pullJoint.distance = pullDistanceJoint;

                    pullJoint.enabled = true;
                    currentState = RopeState.Pulling;
                }
                else
                {
                    shouldStayAttachedToPoint = true;
                    LockOn();
                }
            }
            else
            {
                LockOn();
            }

            CustomFunctions.CameraShake();
            CustomFunctions.HitFreeze(0.05f);
        }
        else if (currentState == RopeState.Failed)
            currentState = RopeState.Retracting;
    }

    bool HasCollidedWhileFlying()
    {
        /*if (Mathf.Abs(playerRigidbody.velocity.x) > 0f)
        {
            if (PlayerMotor.instance.wallLeft && !PlayerMotor.instance.isLookingRight)
            {
                Debug.Log("COLLIDED WITH LEFT WHILE FLYING");
                return true;
            }
            if (PlayerMotor.instance.wallRight && PlayerMotor.instance.isLookingRight)
            {
                Debug.Log("COLLIDED WITH LEFT WHILE FLYING");
                return true;
            }
        }*/
        if (lastVelocity > 0.1f && Mathf.Abs(playerRigidbody.velocity.magnitude) <= 0.1f)
            return true;

        return false;
    }

    void ApplyRestPosEffects()
    {
        bridgeScript.enabled = false;
        grappleScript.enabled = false;
        armRenderer.enabled = false;
        AlignHandWithRope.instance.transform.position = restingPos.position;
        pullJoint.enabled = false;

        if (armEndObject.parent != restingPos)
        {
            armEndObject.parent = restingPos;
            armEndObject.transform.position = restingPos.position;
            armEndObject.localRotation = Quaternion.identity;
        }

        if (AlignHandWithRope.instance.enabled)
            AlignHandWithRope.instance.enabled = false;
    }

    void LockOn()
    {
        currentState = RopeState.LockedOn;
        armEndObject.parent = endPos.transform;
        playerRigidbody.AddForce((endPos.transform.position - playerRigidbody.transform.position).normalized * initialPropulsionImpulse, ForceMode2D.Impulse);
    }

    Vector2 GetGrappleDirection()
    {
        //playerInput.actions.FindAction("Move").ReadValue<Vector2>()
        if (playerInput.currentControlScheme == "Gamepad")
        {
            Vector2 gamePadStickValue = playerInput.actions.FindAction("Move").ReadValue<Vector2>().normalized;
            if (invertYAxis)
                gamePadStickValue.y = -gamePadStickValue.y;

            if (playerInput.actions.FindAction("Move").phase == InputActionPhase.Started)
                lastJoystickPos = gamePadStickValue;
            return lastJoystickPos;
        }
        else
            return (Camera.main.ScreenToWorldPoint(Input.mousePosition) - playerRigidbody.transform.position).normalized;
    }

    void PropulseToEndLocation()
    {
        Vector2 distanceBetweenPlayerAndEndPos = ((Vector2)endPos.position - (Vector2)playerRigidbody.transform.position);

        Vector2 forceToAdd = (distanceBetweenPlayerAndEndPos.normalized) * propulsionSpeed;

        if (Mathf.Abs(initialGrappleAngle) < 25f)
        {
            if (distanceBetweenPlayerAndEndPos.magnitude > maxRopeLength)
                ApplyPropulsion();
            else
            {
                //currentState = RopeState.Retracting;
                FinishPropulsion();
                // Debug.Log("RETRACTING WITH LOW ANGLE");
            }
        }
        else
        {
            if ((initialGrappleAngle > 0 && GetGrappleAngle() > -(initialGrappleAngle * 0.75f)) || (initialGrappleAngle < 0 && GetGrappleAngle() < -(initialGrappleAngle * 0.75f)))
            {
                if (distanceBetweenPlayerAndEndPos.magnitude > maxRopeLength)
                    ApplyPropulsion();
            }
            else
            {
                FinishPropulsion();
                //Debug.Log("RETRACTING WITH high ANGLE, current angle was " + GetGrappleAngle());
            }
        }

        if (playerRigidbody.velocity.magnitude <= 0.1f)
            FinishPropulsion();

        if (Mathf.Abs(PlayerMotor.instance.totalMovement) >= 1f && Mathf.Sign(forceToAdd.x) != Mathf.Sign(PlayerMotor.instance.totalMovement) && Mathf.Abs(GetGrappleAngle()) >= 90f)
            FinishPropulsion();

        /* if (distanceBetweenPlayerAndEndPos.magnitude > maxRopeLength)
             ApplyPropulsion();
         else
             currentState = RopeState.Retracting;*/
    }

    void FinishPropulsion()
    {
        if (!shouldStayAttachedToPoint)
        {
            currentState = RopeState.Retracting;

            if (playerRigidbody.velocity.magnitude > PlayerMotor.instance.velocityThatIsConsideredStrong)
                PlayerMotor.instance.playerAnimator.Play("RollJump");
        }
    }

    void ApplyPropulsion()
    {
        Vector2 distanceBetweenPlayerAndEndPos = ((Vector2)endPos.position - (Vector2)playerRigidbody.transform.position);

        Vector2 forceToAdd = (distanceBetweenPlayerAndEndPos.normalized) * propulsionSpeed;


        playerRigidbody.AddForce(forceToAdd);
        endPointJoint.distance = Mathf.Min(distanceBetweenPlayerAndEndPos.magnitude, endPointJoint.distance);
    }

    void GrappleToTarget(Vector2 direction)
    {
        (Vector2, Transform) touchedWall = DetectWallToGrapple(direction);

        if (touchedWall.Item2 != null)
        {
            StartCoroutine(SetLastGrapplePoint(touchedWall.Item2));
            endPos.position = touchedWall.Item1;
            endPos.transform.parent = touchedWall.Item2;
            PlayerMotor.instance.SetCorrectRenderOrientation((touchedWall.Item1 - (Vector2)playerRigidbody.transform.position).x > 0 ? false : true);
            endPointJoint.enabled = true;
            currentState = RopeState.Traveling;
        }
        else
        {
            endPos.position = (Vector2)playerRigidbody.transform.position + direction.normalized * 4f;
            endPointJoint.enabled = false;
            currentState = RopeState.Failed;
        }

        initialGrappleAngle = GetGrappleAngle();
        endPointJoint.distance = Vector2.Distance(playerRigidbody.transform.position, touchedWall.Item1);
        lastGrabTime = Time.time;
    }

    float GetGrappleAngle()
    {
        Vector2 travelVector = ((Vector2)endPos.position - (Vector2)playerRigidbody.transform.position).normalized;
        return (Mathf.Atan2(travelVector.x, travelVector.y) * Mathf.Rad2Deg);
    }

    (Vector2, Transform) DetectWallToGrapple(Vector2 direction)
    {
        Vector2 rayOrigin = playerInput.transform.position;
        RaycastHit2D[] hit = Physics2D.RaycastAll(rayOrigin, direction.normalized, wallCheckDistance, grappleLayer);
        Debug.DrawLine(rayOrigin, (Vector3)rayOrigin + (Vector3)(direction.normalized * wallCheckDistance), Color.red);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider != null)
            {
                if (hit[i].transform.gameObject.layer == 9)
                {
                    if (hit[i].collider.isTrigger == false)
                        return (hit[i].point, hit[i].transform);
                }
                else
                {
                    if (lastGrapplePoint == null || hit[i].transform != lastGrapplePoint)
                    {
                        return (hit[i].transform.position, hit[i].transform);
                    }
                }
            }
        }

        //Debug.Log("Nothing found to grapple on");
        return (Vector2.zero, null);
    }

    IEnumerator SetLastGrapplePoint(Transform grapplePoint)
    {
        lastGrapplePoint = grapplePoint;
        if (lastGrapplePoint != null)
            pullingObject = lastGrapplePoint;
        yield return new WaitForSeconds(1f);
        lastGrapplePoint = null;
    }

}
