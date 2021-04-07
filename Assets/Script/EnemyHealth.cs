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

    private void Start()
    {
        enemyRenderer = GetComponent<SpriteRenderer>();
    }

    void IDamageable.Damage(int damageAmount, Vector2 knockbackDirection)
    {
        //SpawnExplosion(knockbackDirection);
        GetComponent<Rigidbody2D>().AddForce(knockbackDirection, ForceMode2D.Impulse);
        StartCoroutine(DamageRoutine());
        CustomFunctions.PlaySound(hitSound, 0.5f, true);
    }

    void SpawnExplosion(Vector2 knockbackDirection)
    {
        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = this.transform.position;
        //explosion.GetComponent<SpriteRenderer>().flipX = knockbackDirection.x < 0;
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
