using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int healthPoints = 6;
    public List<Image> healthPointsDisplay;
    [SerializeField]
    AudioClip getHitSound;

    bool isInInvincibilityFrames;
    SpriteRenderer playerRenderer;

    private void Start()
    {
        playerRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            GetComponent<IDamageable>().Damage(1, Vector2.zero, 0f);
    }

    void IDamageable.Damage(int damageAmount, Vector2 knockback, float damageAngle)
    {
        if (isInInvincibilityFrames)
            return;

        healthPoints -= damageAmount;
        healthPoints = Mathf.Clamp(healthPoints, 0, 99);
        CustomFunctions.HitCameraShake();
        CustomFunctions.HitFreeze();
        CustomFunctions.PlaySound(getHitSound, 0.5f, true);
        StartCoroutine(DamageRoutine());

        if (healthPoints > 0)
            SetHealthPointsDisplay();
        else
            Die();

    }

    void SetHealthPointsDisplay()
    {
        foreach (var hP in healthPointsDisplay)
        {
            hP.gameObject.SetActive(true);
        }

        for (int i = healthPoints; i < healthPointsDisplay.Count; i++)
        {
            healthPointsDisplay[i].gameObject.SetActive(false);
        }
    }

    IEnumerator DamageRoutine()
    {
        isInInvincibilityFrames = true;
        int numberOfBlinks = 0;

        while (numberOfBlinks < 4)
        {
            playerRenderer.enabled = false;
            //playerRenderer.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            playerRenderer.enabled = true;
            //playerRenderer.gameObject.SetActive(true);
            numberOfBlinks++;
            yield return new WaitForSeconds(0.1f);
        }

        isInInvincibilityFrames = false;
    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
