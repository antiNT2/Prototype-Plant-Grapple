using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxContinuousTrigger : HitboxTrigger
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("not hitting");
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        DamageLogic(collision);
    }
}
