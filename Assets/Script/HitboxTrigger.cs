using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxTrigger : MonoBehaviour
{
    public int damage = 1;
    public float knockbackIntensity = 3f;
    public LayerMask layersToDamage;

    public Action<Collider2D> OnHit;
    public Action<Collider2D> OnBeforeHit;

    public bool disableDamage = false;

    public bool triggerIsTriggered { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (CanDamageThisObject(collision))
        {
            if (OnBeforeHit != null)
                OnBeforeHit(collision);
            DamageLogic(collision);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (CanDamageThisObject(collision))
            triggerIsTriggered = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (CanDamageThisObject(collision))
            triggerIsTriggered = false;
    }

    bool CanDamageThisObject(Collider2D collision)
    {
        if (collision.GetComponent<IDamageable>() != null && layersToDamage == (layersToDamage | (1 << collision.gameObject.layer)))
            return true;
        else
            return false;
    }

    protected void DamageLogic(Collider2D collision)
    {
        if (CanDamageThisObject(collision))
        {
            if (disableDamage == false)
            {
                float angle = transform.rotation.eulerAngles.z;
                Vector2 knockback = new Vector2(Mathf.Sign(Mathf.Cos(angle * Mathf.Deg2Rad)), 0) * knockbackIntensity;

                collision.GetComponent<IDamageable>().Damage(damage, knockback, angle);

                if (OnHit != null)
                    OnHit(collision);

                triggerIsTriggered = true;
            }
        }
    }
}
