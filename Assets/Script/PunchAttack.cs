using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PunchAttack : MonoBehaviour
{
    public static PunchAttack instance;

    [SerializeField]
    GameObject fistObject;
    [SerializeField]
    GameObject fistRange;
    [SerializeField]
    Transform punchParent;
    [SerializeField]
    LineRenderer punchArmRope;
    /*[SerializeField]
    EdgeCollider2D hitbox;*/
    [SerializeField]
    Collider2D fistHitbox;
    [SerializeField]
    AudioClip punchSound;
    [SerializeField]
    SpriteRenderer enemyFocusDisplay;
    [SerializeField]
    ClosestEnemyDetector closestEnemyDetector;

    [SerializeField]
    Animator punchAnimator;

    public float windupTime = 0.2f;
    public float impactDuration = 0.2f;
    public float travelSpeed = 15f;
    public float secondaryPunchPositionLength = 2.0f;

    Vector2 targetPosition;
    Coroutine fadeOutCoroutine;
    PlayerInput playerInput;
    float originalFistOrientation;
    bool hasInflictedDamageWithThisPunch;
    List<Vector2> additionalStartPositions = new List<Vector2>();

    [SerializeField]
    public GameObject focusedEnemy;
    public bool isFocusingEnemy { get; private set; }

    PunchStatuts currentPunchStatuts;
    public enum PunchStatuts
    {
        None,
        Windup,
        Travel,
        Impact,
        Retracting
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        fistObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        ResetPunch();
        playerInput = GetComponent<PlayerInput>();

        fistHitbox.GetComponent<HitboxTrigger>().OnHit += (Collider2D) => ImpactPunch();
        fistHitbox.GetComponent<HitboxTrigger>().OnHit += (Collider2D) => hasInflictedDamageWithThisPunch = true;
    }

    private void Update()
    {
        SetRopePosition();
        SetEnemyFocusDisplay();

        #region PunchLogic
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

        if (currentPunchStatuts == PunchStatuts.Travel)
        {
            fistObject.transform.position = Vector2.Lerp(fistObject.transform.position, targetPosition, Time.deltaTime * travelSpeed);

            if (Vector2.Distance(fistObject.transform.position, targetPosition) < 0.1f)
            {
                ImpactPunch();
            }
        }

        if (currentPunchStatuts == PunchStatuts.Impact)
            CheckIfWantToTravelAgain();
        #endregion

        if (playerInput.actions.FindAction("FocusEnemy").triggered)
            StartFocusEnemy();
        else if (playerInput.actions.FindAction("FocusEnemy").phase != InputActionPhase.Started && isFocusingEnemy)
            StopFocusEnemy();
        if (isFocusingEnemy && focusedEnemy == null)
            StopFocusEnemy();
    }

    #region Punch Mechanic
    void StartPunch()
    {
        CustomFunctions.PlaySound(punchSound);

        if (fadeOutCoroutine != null)
            CustomFunctions.instance.StopCoroutine(fadeOutCoroutine);
        fistObject.GetComponent<SpriteRenderer>().color = Color.white;

        currentPunchStatuts = PunchStatuts.Windup;
        targetPosition = fistRange.transform.position;
        punchAnimator.Play("Windup");
        Invoke("TravelPunch", windupTime);

        fistObject.transform.rotation = Quaternion.Euler(0, 0, RopeManager.instance.GetGrappleDirectionAngle() * Mathf.Rad2Deg);
        originalFistOrientation = fistObject.transform.rotation.eulerAngles.z;
    }

    public void ImpactPunch()
    {
        if (currentPunchStatuts != PunchStatuts.Travel)
            return;

        currentPunchStatuts = PunchStatuts.Impact;
        punchAnimator.Play("Impact");
        //iTween.ShakeScale(fistObject, Vector3.right * 0.3f, 0.2f);
        iTween.PunchPosition(fistObject, Vector3.right * 0.5f, 0.3f);
        CustomFunctions.CameraShake();
        fistObject.transform.parent = null;
        ToggleHitbox(false);
        Invoke("Retract", impactDuration);
    }

    void TravelPunch()
    {
        ToggleHitbox(true);
        punchAnimator.Play("Impact");
        currentPunchStatuts = PunchStatuts.Travel;
        fistObject.transform.parent = null;
    }

    void ToggleHitbox(bool enable)
    {
        fistHitbox.enabled = enable;
    }

    void Retract()
    {
        CheckIfWantToTravelAgain();

        if (currentPunchStatuts != PunchStatuts.Impact)
            return;

        ToggleHitbox(false);
        additionalStartPositions.Clear();
        currentPunchStatuts = PunchStatuts.Retracting;
    }

    void CheckIfWantToTravelAgain()
    {
        float deltaPunchOrientation = Mathf.Abs(fistObject.transform.rotation.eulerAngles.z - originalFistOrientation);

        if (currentPunchStatuts == PunchStatuts.Impact && additionalStartPositions.Count < 1 && hasInflictedDamageWithThisPunch == false)
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
        ToggleHitbox(true);
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
        hasInflictedDamageWithThisPunch = false;
        fadeOutCoroutine = CustomFunctions.FadeOut(fistObject.GetComponent<SpriteRenderer>(), 0.2f);
    }

    void SetRopePosition()
    {
        punchArmRope.positionCount = additionalStartPositions.Count + 2;

        punchArmRope.SetPosition(0, (Vector2)fistObject.transform.position);
        for (int i = 0; i < additionalStartPositions.Count; i++)
        {
            punchArmRope.SetPosition(i + 1, (Vector2)additionalStartPositions[i]);
        }
        punchArmRope.SetPosition(additionalStartPositions.Count + 1, (Vector2)punchParent.position);
    }
    #endregion

    void SetEnemyFocusDisplay()
    {
        if (focusedEnemy != null)
            enemyFocusDisplay.transform.position = focusedEnemy.transform.position;
        else
            enemyFocusDisplay.color = new Color(1, 1, 1, 0);
    }

    void StartFocusEnemy()
    {
        if (isFocusingEnemy == false)
        {
            if (closestEnemyDetector.GetClosestEnemy() != null)
            {
                focusedEnemy = closestEnemyDetector.GetClosestEnemy();

                Rect enemySpriteDimensions = focusedEnemy.GetComponentInChildren<SpriteRenderer>().sprite.rect;
                Vector2 detectorSize = new Vector2((enemySpriteDimensions.width * 2) / 32, (enemySpriteDimensions.height * 2) / 32);
                detectorSize *= focusedEnemy.transform.localScale;

                //enemyFocusDisplay.size = detectorSize;
                StopCoroutine("EnemyFocusSizeInterpolation");
                StartCoroutine(EnemyFocusSizeInterpolation(detectorSize, 0.1f));
                enemyFocusDisplay.color = Color.white;
                isFocusingEnemy = true;
            }
        }
    }

    IEnumerator EnemyFocusSizeInterpolation(Vector2 targetSize, float duration)
    {
        Vector2 initialSize = targetSize * 2f;
        Vector2 finalSize = targetSize;
        float timer = 0;

        enemyFocusDisplay.size = initialSize;

        while (enemyFocusDisplay.size.magnitude > finalSize.magnitude)
        {
            enemyFocusDisplay.size = Vector2.Lerp(initialSize, finalSize, timer);
            timer += Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }
    }

    void StopFocusEnemy()
    {
        //if(focusedEnemy != null)
        //{
        isFocusingEnemy = false;
        focusedEnemy = null;
        enemyFocusDisplay.color = new Color(1, 1, 1, 0);
        //CustomFunctions.FadeOut(enemyFocusDisplay, 0.1f);
        //}
    }
}
