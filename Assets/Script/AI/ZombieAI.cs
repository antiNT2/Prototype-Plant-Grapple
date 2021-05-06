using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMotor))]
public class ZombieAI : MonoBehaviour
{
    EnemyMotor motor;
    bool goingRight;

    private void Start()
    {
        motor = GetComponent<EnemyMotor>();
        motor.OnCanNoLongerWalkInThatDirection += () => goingRight = !goingRight;
    }

    private void Update()
    {
        motor.inputAxis = goingRight ? 1 : -1;
    }
}
