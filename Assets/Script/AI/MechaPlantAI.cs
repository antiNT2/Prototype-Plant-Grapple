using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechaPlantAI : MonoBehaviour
{
    public float preferedTargetDistance = 1f;
    public float ignoreTargetDistance = 10f;
    public float delayBetweenAttacks = 0.1f;

    Animator anim;
    [SerializeField]
    GameObject projectilePrefab;
    [SerializeField]
    Transform projectileParent;
    EnemyMotor enemyMotor;
    SpriteRenderer spriteRenderer;

    Transform target;
    float currentTargetDistance;
    AIState currentState;
    enum AIState
    {
        Idle,
        Attacking,
        Fleeing,
        Escaping
    }

    private void OnEnable()
    {
        enemyMotor = GetComponent<EnemyMotor>();
        //enemyMotor.OnCanNoLongerWalkInThatDirection += StartAttack;
        enemyMotor.OnCanNoLongerWalkInThatDirection += () => StartCoroutine(TryToEscape());
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        target = PlayerHealth.instance.transform;
        enemyMotor = GetComponent<EnemyMotor>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        InvokeRepeating("EvaluateDistance", 0f, 0.5f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            SpitAttack();

        if (currentTargetDistance >= ignoreTargetDistance)
        {
            currentState = AIState.Idle;
            enemyMotor.inputAxis = 0;
        }
        if (currentTargetDistance < ignoreTargetDistance && currentTargetDistance > preferedTargetDistance)
        {
            StartAttack();
        }
        else if (currentTargetDistance < preferedTargetDistance) //too close !
        {
            if (currentState == AIState.Attacking)
            {
                currentState = AIState.Idle;
                StopAllCoroutines();
            }
        }

        if (currentTargetDistance < preferedTargetDistance && (currentState != AIState.Attacking && currentState != AIState.Escaping))
        {
            currentState = AIState.Fleeing;
            Flee();
        }

        spriteRenderer.flipX = (target.position.x > transform.position.x) ? false : true;
    }

    void EvaluateDistance()
    {
        currentTargetDistance = Vector2.Distance(this.transform.position, target.position);
    }

    void StartAttack()
    {
        if (currentState != AIState.Attacking)
        {
            currentState = AIState.Attacking;
            enemyMotor.inputAxis = 0;
            StartCoroutine(SpitContinuously());
        }
    }

    IEnumerator SpitContinuously()
    {
        while (currentState == AIState.Attacking)
        {
            SpitAttack();
            yield return new WaitForSeconds(delayBetweenAttacks);
        }
        if (currentState != AIState.Attacking)
            yield break;
    }

    void Flee()
    {
        enemyMotor.inputAxis = (target.position.x > transform.position.x) ? -1 : 1;
    }

    IEnumerator TryToEscape()
    {
        currentState = AIState.Escaping;
        float timer = 0;
        float axis = (spriteRenderer.flipX ? -1 : 1);
        enemyMotor.Jump();

        while (timer < 1f)
        {
            enemyMotor.inputAxis = axis;
            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        currentState = AIState.Idle;
    }

    void SpitAttack()
    {
        anim.Play("Spit");
    }

    #region Animator
    //Used by animator :
    public void SpawnProjectile()
    {
        projectileParent.transform.localPosition = new Vector3(Mathf.Abs(projectileParent.localPosition.x) * (spriteRenderer.flipX ? -1 : 1), projectileParent.localPosition.y);

        GameObject spawnedProjectile = Instantiate(projectilePrefab, projectileParent);
        spawnedProjectile.transform.localPosition = Vector3.zero;
        spawnedProjectile.transform.parent = null;
        spawnedProjectile.transform.rotation = Quaternion.Euler(0, (spriteRenderer.flipX ? 180 : 0), 0);
    }
    #endregion

    private void OnDisable()
    {
        //enemyMotor.OnCanNoLongerWalkInThatDirection -= StartAttack;
        enemyMotor.OnCanNoLongerWalkInThatDirection -= () => StartCoroutine(TryToEscape());
    }
}
