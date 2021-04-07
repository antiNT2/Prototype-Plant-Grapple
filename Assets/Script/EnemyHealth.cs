using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    SpriteRenderer enemyRenderer;
    [SerializeField]
    GameObject explosionPrefab;
    [SerializeField]
    AudioClip hitSound;

    public int health = 2;

    private void Start()
    {
        enemyRenderer = GetComponent<SpriteRenderer>();
    }

    void IDamageable.Damage(int damageAmount, Vector2 knockbackDirection, float damageAngle)
    {
        SpawnExplosion(damageAngle);
        GetComponent<Rigidbody2D>().AddForce(knockbackDirection, ForceMode2D.Impulse);
        StartCoroutine(DamageRoutine());
        CustomFunctions.PlaySound(hitSound, 0.5f, true);
        health--;

        if (health <= 0)
            Destroy(this.gameObject);   
    }

    void SpawnExplosion(float angle)
    {
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = this.transform.position;
        explosion.transform.rotation = Quaternion.Euler(0, 0, angle);
       // explosion.GetComponent<SpriteRenderer>().flipX = knockbackDirection.x < 0;
        Destroy(explosion, 0.4f);
    }

    IEnumerator DamageRoutine()
    {
        CustomFunctions.HitCameraShake();
        CustomFunctions.HitFreeze();

        enemyRenderer.material.SetFloat("_EnableWhite", 1);
        yield return new WaitForSeconds(0.1f);
        enemyRenderer.material.SetFloat("_EnableWhite", 0);
    }
}
