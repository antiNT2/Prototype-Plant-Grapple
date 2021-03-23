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
    PlayerInput playerInput;
    DistanceJoint2D endPointJoint;

    [SerializeField]
    Transform startPos;
    [SerializeField]
    Transform endPos;
    [SerializeField]
    Transform restingPos;

    float initialGrappleAngle;

    [SerializeField]
    public RopeState currentState;
    [SerializeField]
    public float wallCheckDistance;
    [SerializeField]
    LayerMask grappleLayer;
    Rigidbody2D playerRigidbody;
    public float initialPropulsionImpulse;
    float lastVelocity = 0f;

    public float maxRopeLength;
    public float propulsionSpeed;

    public float actualAngleTest;
    public enum RopeState
    {
        Rest,
        Retracting,
        Traveling,
        LockedOn
    }

    Vector2 initialHandLocalPosition;
    public float initialRopeSegLength;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        initialRopeSegLength = bridgeScript.ropeSegLen;
        playerInput = FindObjectOfType<PlayerInput>();
        playerRigidbody = playerInput.GetComponent<Rigidbody2D>();
        initialHandLocalPosition = AlignHandWithRope.instance.transform.GetChild(0).localPosition;
        endPointJoint = endPos.GetComponent<DistanceJoint2D>();
    }

    private void Update()
    {
        if (currentState == RopeState.Rest)
        {
            ApplyRestPosEffects();
        }
        else if (currentState == RopeState.Retracting)
        {
            //bridgeScript.enabled = true;
            //grappleScript.enabled = false;
            armRenderer.enabled = true;
            if (Vector2.Distance(AlignHandWithRope.instance.transform.position, restingPos.position) > 0.1f)
            {
                AlignHandWithRope.instance.transform.position = Vector3.Lerp(AlignHandWithRope.instance.transform.position, restingPos.position, (Time.deltaTime * 10f) / (AlignHandWithRope.instance.transform.position - restingPos.position).magnitude);
               // bridgeScript.ropeSegLen = Mathf.Lerp(bridgeScript.ropeSegLen, 0.01f, Time.deltaTime * 2f);
            }
            else
                currentState = RopeState.Rest;

            if (!AlignHandWithRope.instance.enabled)
                AlignHandWithRope.instance.enabled = true;

            //bridgeScript.StartPoint = restingPos;
            // AlignHandWithRope.instance.SetHandSpritePosition(true);
        }
        else if (currentState == RopeState.Traveling)
        {
            armRenderer.enabled = true;
            grappleScript.enabled = true;
            AlignHandWithRope.instance.transform.GetChild(0).position = armRenderer.GetPosition(armRenderer.positionCount - 1);

            if (AlignHandWithRope.instance.transform.GetChild(0).localPosition == Vector3.zero)
            {
                currentState = RopeState.LockedOn;
                //PropulseToEndLocation();
                playerRigidbody.AddForce((endPos.transform.position - playerRigidbody.transform.position).normalized * initialPropulsionImpulse, ForceMode2D.Impulse);
                AlignHandWithRope.instance.transform.GetChild(0).localPosition = initialHandLocalPosition;
            }
        }


        if (currentState == RopeState.Rest || currentState == RopeState.Retracting)
        {
            /* if (playerInput.actions.FindAction("Grapple").triggered)
                 GrappleToTarget(GetGrappleDirection());*/


            if (playerInput.actions.FindAction("Grapple").triggered)
            {
                ApplyRestPosEffects();
                GrappleToTarget(GetGrappleDirection());
            }
        }
        if (currentState == RopeState.LockedOn)
        {

            if (playerInput.actions.FindAction("Grapple").triggered)
                currentState = RopeState.Retracting;

            if (HasCollidedWhileFlying())
            {
                currentState = RopeState.Retracting;
            }
        }


        actualAngleTest = GetGrappleAngle();
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

    /* private void FixedUpdate()
     {
         //LIMIT VELOCITY
         if (currentState == RopeState.LockedOn)
         {
             if (playerRigidbody.velocity.magnitude > maxVelocity)
             {
                 playerRigidbody.drag = 3f;
             }
             else
             {
                 playerRigidbody.drag = 0f;
             }
         }
         else
                 playerRigidbody.drag = 0f;

     }*/

    private void LateUpdate()
    {
        lastVelocity = playerRigidbody.velocity.magnitude;
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

        if (AlignHandWithRope.instance.enabled)
            AlignHandWithRope.instance.enabled = false;
    }

    Vector2 GetGrappleDirection()
    {
        /*Vector2 output = playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        if (output == Vector2.zero)
        {
            if (PlayerMotor.instance.isLookingRight)
                output = Vector2.right;
            else
                output = Vector2.left;
        }

        return output;*/

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
                currentState = RopeState.Retracting;
                Debug.Log("RETRACTING WITH LOW ANGLE");
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
                currentState = RopeState.Retracting;
                Debug.Log("RETRACTING WITH high ANGLE, current angle was " + GetGrappleAngle());
            }
        }

        /* if (distanceBetweenPlayerAndEndPos.magnitude > maxRopeLength)
             ApplyPropulsion();
         else
             currentState = RopeState.Retracting;*/
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
        Vector2 touchedWall = DetectWallToGrapple(direction);

        if (touchedWall != Vector2.zero)
        {
            endPos.position = touchedWall;
            PlayerMotor.instance.SetCorrectRenderOrientation((touchedWall - (Vector2)playerRigidbody.transform.position).x > 0 ? false : true);
            currentState = RopeState.Traveling;
        }

        initialGrappleAngle = GetGrappleAngle();
        endPointJoint.distance = Vector2.Distance(playerRigidbody.transform.position, touchedWall);
    }

    float GetGrappleAngle()
    {
        Vector2 travelVector = ((Vector2)endPos.position - (Vector2)playerRigidbody.transform.position).normalized;
        return (Mathf.Atan2(travelVector.x, travelVector.y) * Mathf.Rad2Deg);
    }

    /*void GrappleToTarget()
    {
        Vector2 touchedWall = DetectWallToGrapple();
        //print(direction);


        if (touchedWall != Vector2.zero)
        {
            endPos.position = touchedWall;
            currentState = RopeState.Traveling;
        }
    }*/

    Vector2 DetectWallToGrapple(Vector2 direction)
    {
        Vector2 rayOrigin = playerInput.transform.position;
        RaycastHit2D[] hit = Physics2D.RaycastAll(rayOrigin, direction, wallCheckDistance, grappleLayer);
        Debug.DrawLine(rayOrigin, (Vector3)rayOrigin + (Vector3)(direction * wallCheckDistance), Color.red);

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].collider != null)
            {
                if (hit[i].transform.gameObject.layer == 9)
                {
                    if (hit[i].collider.isTrigger == false)
                        return hit[i].point;
                }
                else
                    return hit[i].transform.position;
            }
        }

        Debug.Log("Nothing found to grapple on");
        return Vector2.zero;
    }

}
