using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PunchAttack : MonoBehaviour
{
    [SerializeField]
    GameObject fistObject;
    [SerializeField]
    GameObject fistRange;
    [SerializeField]
    Transform punchParent;
    [SerializeField]
    LineRenderer punchArmRope;
    [SerializeField]
    EdgeCollider2D hitbox;
    [SerializeField]
    Collider2D fistHitbox;
    [SerializeField]
    AudioClip punchSound;

    [SerializeField]
    Animator punchAnimator;

    public float windupTime = 0.2f;
    public float impactDuration = 1f;
    public float travelSpeed = 15f;
    public float secondaryPunchPositionLength = 2.0f;
    public bool instantTravel = true;

    Vector2 targetPosition;
    Coroutine fadeOutCoroutine;
    PlayerInput playerInput;
    float originalFistOrientation;
    List<Vector2> additionalStartPositions = new List<Vector2>();

    PunchStatuts currentPunchStatuts;
    public enum PunchStatuts
    {
        None,
        Windup,
        Travel,
        Impact,
        Retracting
    }

    private void Start()
    {
        fistObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        ResetPunch();
        playerInput = GetComponent<PlayerInput>();

        fistHitbox.GetComponent<HitboxTrigger>().OnHit += (Collider2D) => ImpactPunch();
    }

    private void Update()
    {
        SetRopePosition();
        if (currentPunchStatuts == PunchStatuts.None)
            punchParent.transform.rotation = Quaternion.Euler(0, 0, RopeManager.instance.GetGrappleDirectionAngle() * Mathf.Rad2Deg);

        if (currentPunchStatuts == PunchStatuts.None && playerInput.actions.FindAction("Attack").triggered)
        {
            StartPunch();
        }

        if (currentPunchStatuts == PunchStatuts.Retracting)
        {
            fistObject.transform.position = Vector2.Lerp(fistObject.transform.position, punchParent.position, Time.deltaTime * 30f);

            if (Vector2.Distance(fistObject.transform.position, punchParent.position) < 0.2f)
            {
                ResetPunch();
            }
        }

        if (currentPunchStatuts == PunchStatuts.Travel && instantTravel == false)
        {
            fistObject.transform.position = Vector2.Lerp(fistObject.transform.position, targetPosition, Time.deltaTime * travelSpeed);

            if (Vector2.Distance(fistObject.transform.position, targetPosition) < 0.1f)
            {
                ImpactPunch();
            }
        }

        if (currentPunchStatuts == PunchStatuts.Impact)
            CheckIfWantToTravelAgain();
    }

    void StartPunch()
    {
        CustomFunctions.PlaySound(punchSound);

        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);
        fistObject.GetComponent<SpriteRenderer>().color = Color.white;

        currentPunchStatuts = PunchStatuts.Windup;
        targetPosition = fistRange.transform.position;
        punchAnimator.Play("Windup");
        if (instantTravel)
            Invoke("ImpactPunch", windupTime);
        else
            Invoke("TravelPunch", windupTime);

        fistObject.transform.rotation = Quaternion.Euler(0, 0, RopeManager.instance.GetGrappleDirectionAngle() * Mathf.Rad2Deg);
        originalFistOrientation = fistObject.transform.rotation.eulerAngles.z;
    }

    void ImpactPunch()
    {
        if (instantTravel)
            punchAnimator.Play("Impact");
        CustomFunctions.CameraShake();

        /* if (instantTravel)
             SetHitbox();*/

        //ToggleHitbox(true);

        currentPunchStatuts = PunchStatuts.Impact;
        fistObject.transform.parent = null;
        //fistObject.transform.position = targetPosition;
        //fistObject.transform.rotation = Quaternion.Euler(0, 0, RopeManager.instance.GetGrappleDirectionAngle() * Mathf.Rad2Deg);

        Invoke("Retract", impactDuration);
    }

    void TravelPunch()
    {
        /*SetHitbox();*/
        ToggleHitbox(true);
        punchAnimator.Play("Impact");
        currentPunchStatuts = PunchStatuts.Travel;
        fistObject.transform.parent = null;
    }

    void SetHitbox()
    {
        List<Vector2> newPoints = new List<Vector2>() { punchParent.position, fistObject.transform.position };

        hitbox.SetPoints(newPoints);
    }

    void ToggleHitbox(bool enable)
    {
        hitbox.gameObject.SetActive(enable);
        fistHitbox.enabled = enable;
    }

    void Retract()
    {
        CheckIfWantToTravelAgain(true);

        if (currentPunchStatuts != PunchStatuts.Impact)
            return;

        ToggleHitbox(false);
        additionalStartPositions.Clear();
        currentPunchStatuts = PunchStatuts.Retracting;
    }

    void CheckIfWantToTravelAgain(bool retractDelayHasPassed = false)
    {
        float deltaPunchOrientation = Mathf.Abs(fistObject.transform.rotation.eulerAngles.z - originalFistOrientation);

        if (currentPunchStatuts == PunchStatuts.Impact && additionalStartPositions.Count < 1)
        {
            if (playerInput.actions.FindAction("Attack").phase == InputActionPhase.Started)
                fistObject.transform.rotation = Quaternion.Euler(0, 0, RopeManager.instance.GetGrappleDirectionAngle() * Mathf.Rad2Deg);
            

            if (additionalStartPositions.Count < 1 && deltaPunchOrientation > 25f /*&& (playerInput.actions.FindAction("Attack").phase != InputActionPhase.Started || retractDelayHasPassed)*/)
            {
                CancelInvoke("Retract");
                TravelAgain(fistObject.transform.position, fistObject.transform.rotation.eulerAngles.z);
            }
        }
    }

    void TravelAgain(Vector2 additionalStartPos, float angleDirection)
    {
        originalFistOrientation = fistObject.transform.rotation.eulerAngles.z;
        additionalStartPositions.Add(additionalStartPos);
        targetPosition = additionalStartPos + (new Vector2(Mathf.Cos(angleDirection * Mathf.Deg2Rad), Mathf.Sin(angleDirection * Mathf.Deg2Rad))) * (secondaryPunchPositionLength / additionalStartPositions.Count);
        currentPunchStatuts = PunchStatuts.Travel;
    }

    void ResetPunch()
    {
        currentPunchStatuts = PunchStatuts.None;
        fistObject.transform.position = punchParent.position;
        fistObject.transform.parent = punchParent;
        fadeOutCoroutine = StartCoroutine(FadeOutRoutine(fistObject.GetComponent<SpriteRenderer>(), 0.2f));
    }

    void SetRopePosition()
    {
        punchArmRope.positionCount = additionalStartPositions.Count + 2;
        punchArmRope.SetPosition(0, (Vector2)punchParent.position);
        for (int i = 0; i < additionalStartPositions.Count; i++)
        {
            punchArmRope.SetPosition(i + 1, (Vector2)additionalStartPositions[i]);
        }
        punchArmRope.SetPosition(additionalStartPositions.Count + 1, (Vector2)fistObject.transform.position);

        SetHitbox();
    }

    IEnumerator FadeOutRoutine(SpriteRenderer renderer, float duration)
    {
        Color initialColor = renderer.color;
        Color finalColor = renderer.color;
        finalColor.a = 0;
        float timer = 0;

        while (renderer.color.a > 0)
        {
            renderer.color = Color.Lerp(initialColor, finalColor, timer);
            timer += Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }
    }
}
