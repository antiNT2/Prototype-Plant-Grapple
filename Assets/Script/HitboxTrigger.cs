using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxTrigger : MonoBehaviour
{
    public int damage = 1;
    public float knockbackIntensity = 3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        float angle = transform.rotation.eulerAngles.z;

        Vector2 knockback = new Vector2(Mathf.Sign(Mathf.Cos(angle * Mathf.Deg2Rad)), 0) * knockbackIntensity;
        print(knockback);

        if (collision.GetComponent<IDamageable>() != null)
            collision.GetComponent<IDamageable>().Damage(damage, knockback);
    }
}
