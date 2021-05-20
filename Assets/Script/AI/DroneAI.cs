using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DroneAI : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Animator anim;
    EnemyHealth health;

    [SerializeField]
    Vector2 destination;
    [SerializeField]
    float leavePlayerAloneDistance = 7f;
    [SerializeField]
    float spotDistance = 2f;
    [SerializeField]
    float flySpeed = 1f;
    [SerializeField]
    float getDestinationDelay = 1f;

    Transform player;
    State currentState;
    enum State
    {
        Patrolling,
        FollowingTarget
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();

        player = PlayerMotor.instance.transform;
        destination = this.transform.position;
        InvokeRepeating("SetDestination", 0.5f, getDestinationDelay);
        StartPatrolling();

        health.OnReceiveDamage += StartFollowingPlayer;
    }

    private void Update()
    {
        if (currentState == State.FollowingTarget)
        {
            spriteRenderer.flipX = (this.transform.position.x < PlayerMotor.instance.transform.position.x) ? false : true;
            transform.position = Vector2.MoveTowards(transform.position, destination, flySpeed * Time.deltaTime);
        }

    }

    void SetDestination()
    {
        // playerIsTooFar = (Vector2.Distance(this.transform.position, PlayerMotor.instance.transform.position) > maxTriggerDistance);
        float distance = Vector2.Distance(this.transform.position, PlayerMotor.instance.transform.position);

        if (currentState == State.FollowingTarget)
        {
            if (distance > leavePlayerAloneDistance)
            {
                StartPatrolling();
                return;
            }
        }
        else if (currentState == State.Patrolling)
        {
            if(distance < spotDistance)
            {
                StartFollowingPlayer();
            }
        }

        //destination = player.transform.position;
        DOTween.To(() => destination, x => destination = x, (Vector2)player.position, 0.5f).SetEase(Ease.OutSine);
    }

    void StartPatrolling()
    {
        currentState = State.Patrolling;
        anim.SetBool("FollowPlayer", false);
        spriteRenderer.flipX = false;
        transform.DOMoveX(destination.x + 3f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).OnStepComplete(() => spriteRenderer.flipX = !spriteRenderer.flipX);
    }

    void StartFollowingPlayer()
    {
        currentState = State.FollowingTarget;
        transform.DOKill();
        anim.SetBool("FollowPlayer", true);
    }

    private void OnDisable()
    {
        health.OnReceiveDamage -= StartFollowingPlayer;
    }
}
