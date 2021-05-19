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
        initialHealthBarDisplayPos = healthPointsParentDisplay.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            GetComponent<IDamageable>().Damage(1, Vector2.zero, 0f);
    }

    void IDamageable.Damage(int damageAmount, Vector2 knockback, float damageAngle)
    {
        if (isInInvincibilityFrames || (debugInvincible && Application.isEditor) || healthPoints <= 0)
            return;

        healthPoints -= damageAmount;
        healthPoints = Mathf.Clamp(healthPoints, 0, 99);
        CustomFunctions.HitCameraShake();
        CustomFunctions.HitFreeze();
        CustomFunctions.PlaySound(getHitSound, 0.5f, true);
        SetHealthPointsDisplay();

        if ((Vector2)healthPointsParentDisplay.position != initialHealthBarDisplayPos)
            iTween.Stop(healthPointsParentDisplay.gameObject);

        healthPointsParentDisplay.localPosition = Vector2.zero;

        StartCoroutine(DamageRoutine());
        if (healthPoints <= 0)
        {
            CustomFunctions.SpawnDeathExplosion(this.transform.position);
            Invoke("Die", 0.25f);
        }

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
        iTween.PunchScale(healthPointsParentDisplay.gameObject, Vector2.one * 1.5f, 0.75f);

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
        iTween.MoveTo(healthPointsParentDisplay.gameObject, initialHealthBarDisplayPos, 1f);

        isInInvincibilityFrames = false;
    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
