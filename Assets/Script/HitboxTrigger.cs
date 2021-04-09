using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxTrigger : MonoBehaviour
{
    public int damage = 1;
    public float knockbackIntensity = 3f;

    public Action<Collider2D> OnHit;
    public Action<Collider2D> OnBeforeHit;

    //public Predicate<Collider2D> disableDamage;
    public bool disableDamage = false;

    public bool triggerIsTriggered { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<IDamageable>() != null)
        {
            if (OnBeforeHit != null)
                OnBeforeHit(collision);

            if (disableDamage == false /*disableDamage == null || (disableDamage != null && disableDamage(collision) == false)*/)
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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<IDamageable>() != null)
            triggerIsTriggered = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<IDamageable>() != null)
            triggerIsTriggered = false;
    }

}
