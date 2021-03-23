using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignHandWithRope : MonoBehaviour
{
    public static AlignHandWithRope instance;

    [SerializeField]
    Transform startPos;
    [SerializeField]
    Transform endPos;
    [SerializeField]
    Transform restArm;
    [SerializeField]
    Transform handSprite;
    [SerializeField]
    LineRenderer lineRenderer;
    //public bool alignHand;

    Vector2 distanceVectorBetweenPos;
    [SerializeField]
    float angle;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {

        distanceVectorBetweenPos = (endPos.position - startPos.position).normalized;
        angle = Mathf.Atan2(distanceVectorBetweenPos.y, distanceVectorBetweenPos.x);

        RotateHandSprite();
        //lineRenderer.enabled = true;
    }

    void RotateHandSprite()
    {
        handSprite.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
    }

    /* public void SetHandSpritePosition(bool startOfTheLine)
     {
         handSprite.position = lineRenderer.GetPosition(startOfTheLine ? 0 : lineRenderer.positionCount - 1);
     }*/
}
