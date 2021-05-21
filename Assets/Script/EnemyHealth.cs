using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    SpriteRenderer enemyRenderer;

    public int health = 2;
    public Action OnReceiveDamage;

    bool isInInvincibilityFrames;

    private void Start()
    {
        enemyRenderer = GetComponent<SpriteRenderer>();
    }

    void IDamageable.Damage(int damageAmount, Vector2 knockbackDirection, float damageAngle)
    {
        if (isInInvincibilityFrames)
            return;

        CustomFunctions.SpawnAttackExplosion(damageAngle, this.transform.position);

        if (GetComponent<IMotor>() != null)
            GetComponent<IMotor>().SetKnockback(knockbackDirection.x);
        //GetComponent<Rigidbody2D>().AddForce(knockbackDirection, ForceMode2D.Impulse);

        StartCoroutine(DamageRoutine());
        CustomFunctions.PlaySound(CustomFunctions.instance.hitEnemySound, 0.5f, true);
        health--;

        if (health <= 0)
            Die();

        if (OnReceiveDamage != null)
            OnReceiveDamage();
    }

    void Die()
    {
        SpawnRandomAmountOfCoins();
        CustomFunctions.SpawnDeathExplosion(this.transform.position);
        Destroy(this.gameObject);
    }

    void SpawnRandomAmountOfCoins()
    {
        Vector3 coinPos = this.transform.position + Vector3.up * 0.5f;
        int randomAmount = UnityEngine.Random.Range(1, 3);

        for (int i = 0; i < randomAmount; i++)
        {
            CustomFunctions.SpawnCoin(coinPos + i * 0.25f * Vector3.right);
        }
    }

    IEnumerator DamageRoutine()
    {
        CustomFunctions.HitCameraShake();
        CustomFunctions.HitFreeze();

        isInInvincibilityFrames = true;
        enemyRenderer.material.SetFloat("_EnableWhite", 1);
        yield return new WaitForSeconds(0.1f);
        enemyRenderer.material.SetFloat("_EnableWhite", 0);
        isInInvincibilityFrames = false;
    }
}
