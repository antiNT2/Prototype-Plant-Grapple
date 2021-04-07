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
    GameObject hitbox;
    [SerializeField]
    AudioClip punchSound;

    [SerializeField]
    Animator punchAnimator;

    public float windupTime = 0.2f;
    public float impactDuration = 1f;
    public float travelSpeed = 15f;
    public bool instantTravel = true;

    Vector2 targetPosition;
    Coroutine fadeOutCoroutine;
    PlayerInput playerInput;

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
    }

    void ImpactPunch()
    {
        if (instantTravel)
            punchAnimator.Play("Impact");
        CustomFunctions.CameraShake();

        if (instantTravel)
            SetHitbox();

        hitbox.SetActive(true);

        //hitbox.transform.localScale = new Vector3(1, 1, 1);
        currentPunchStatuts = PunchStatuts.Impact;
        fistObject.transform.parent = null;
        fistObject.transform.position = targetPosition;
        Invoke("Retract", impactDuration);

        fistObject.transform.rotation = Quaternion.Euler(0, 0, RopeManager.instance.GetGrappleDirectionAngle() * Mathf.Rad2Deg);
    }

    void TravelPunch()
    {
        SetHitbox();
        punchAnimator.Play("Impact");
        currentPunchStatuts = PunchStatuts.Travel;
        fistObject.transform.parent = null;

        //Debug.Break();
    }

    void SetHitbox()
    {
        hitbox.transform.rotation = Quaternion.Euler(0, 0, RopeManager.instance.GetGrappleDirectionAngle() * Mathf.Rad2Deg);
        hitbox.transform.position = fistObject.transform.position;
    }

    void Retract()
    {
        hitbox.SetActive(false);
        currentPunchStatuts = PunchStatuts.Retracting;
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
        punchArmRope.SetPosition(0, punchParent.position);
        punchArmRope.SetPosition(1, fistObject.transform.position);
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
