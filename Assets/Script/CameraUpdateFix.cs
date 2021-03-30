using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraUpdateFix : MonoBehaviour
{
    PlayerMotor playerMotor;
    CinemachineBrain cinemachineBrain;

    private void Start()
    {
        playerMotor = PlayerMotor.instance;
        cinemachineBrain = FindObjectOfType<CinemachineBrain>();
    }

    private void Update()
    {
        cinemachineBrain.m_UpdateMethod = (playerMotor.totalMovement != 0) ? CinemachineBrain.UpdateMethod.SmartUpdate : CinemachineBrain.UpdateMethod.FixedUpdate;
    }
}
