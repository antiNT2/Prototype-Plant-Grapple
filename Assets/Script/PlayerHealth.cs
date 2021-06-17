using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public static PlayerHealth instance;

    public int healthPoints = 6;
    public bool debugInvincible;
    public List<Image> healthPointsDisplay;
    RectTransform healthPointsParentDisplay;
    [SerializeField]
    AudioClip getHitSound;

    bool isInInvincibilityFrames;
    SpriteRenderer playerRenderer;
    Vector2 initialHealthBarDisplayPos;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playerRenderer = GetComponentInChildren<SpriteRenderer>();
        healthPointsParentDisplay = healthPointsDisplay[0].transform.parent.gameObject.GetComponent<RectTransform>();
        initialHealthBarDisplayPos = healthPointsParentDisplay.anchoredPosition;
    }

    /* private void Update()
     {
         if (Input.GetKeyDown(KeyCode.Q))
             GetComponent<IDamageable>().Damage(1, Vector2.zero, 0f);
     }*/

    void IDamageable.Damage(int damageAmount, Vector2 knockback, float damageAngle)
    {
        if (isInInvincibilityFrames || (debugInvincible && Application.isEditor) || healthPoints <= 0)
            return;

        healthPoints -= damageAmount;
        healthPoints = Mathf.Clamp(healthPoints, 0, 99);
        CustomFunctions.HitCameraShake();
        CustomFunctions.HitFreeze(0.2f);
        CustomFunctions.PlaySound(getHitSound, 0.5f, true);
        SetHealthPointsDisplay();

        if ((Vector2)healthPointsParentDisplay.position != initialHealthBarDisplayPos)
            DOTween.Kill(healthPointsParentDisplay);

        healthPointsParentDisplay.localPosition = Vector2.zero;

        StartCoroutine(DamageRoutine());
        if (healthPoints <= 0)
        {
            CustomFunctions.SpawnDeathExplosion(this.transform.position);
            Invoke("Die", 0.25f);
        }

    }

    void IDamageable.Heal(int healAmount)
    {
        if (healthPoints >= 3)
            return;

        healthPoints += healAmount;
        healthPoints = Mathf.Clamp(healthPoints, 0, 3);
        CustomFunctions.HitFreeze(0.2f);
        SetHealthPointsDisplay();

        if ((Vector2)healthPointsParentDisplay.position != initialHealthBarDisplayPos)
            DOTween.Kill(healthPointsParentDisplay);

        healthPointsParentDisplay.localPosition = Vector2.zero;
        healthPointsParentDisplay.DOAnchorPos(initialHealthBarDisplayPos, 0.5f).SetEase(Ease.InOutCubic).SetDelay(.5f);
    }

    void SetHealthPointsDisplay()
    {
        /*foreach (var hP in healthPointsDisplay)
        {
            hP.gameObject.SetActive(true);
        }

        for (int i = healthPoints; i < healthPointsDisplay.Count; i++)
        {
            healthPointsDisplay[i].gameObject.SetActive(false);
        }*/

        for (int i = 0; i < healthPointsDisplay.Count; i++)
        {
            if (i >= healthPoints && healthPointsDisplay[i].gameObject.activeSelf == true)
                healthPointsDisplay[i].gameObject.SetActive(false);
            else if (i < healthPoints && healthPointsDisplay[i].gameObject.activeSelf == false)
                healthPointsDisplay[i].gameObject.SetActive(true);
        }
    }

    IEnumerator DamageRoutine()
    {
        isInInvincibilityFrames = true;
        int numberOfBlinks = 0;
        healthPointsParentDisplay.DOPunchScale(Vector2.one, 0.75f, 10, 0.5f).SetUpdate(UpdateType.Normal, true);

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

        healthPointsParentDisplay.DOAnchorPos(initialHealthBarDisplayPos, 0.5f).SetEase(Ease.InOutCubic);

        isInInvincibilityFrames = false;
    }

    void Die()
    {
        DOTween.Clear(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
