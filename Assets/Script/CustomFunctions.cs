﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class CustomFunctions : MonoBehaviour
{
    public static CustomFunctions instance;
    public CinemachineImpulseSource hitCameraShakeSource;
    public CinemachineImpulseSource grappleCameraShakeSource;
    public GameObject attackExplosionPrefab;
    public GameObject deathExplosionPrefab;
    static GameObject soundHolder;

    public List<CollectibleTile> allCollectibleTiles;

    [SerializeField]
    public AudioClip hitEnemySound;
    [SerializeField]
    AudioClip explosionSound;
    public GameObject droppedCoinPrefab;
    [SerializeField]
    Transform playerCameraFollowPoint;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame))
        {
            DOTween.Clear(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.Q))
            HitCameraShake();
    }
    public static void CameraShake()
    {
        //instance.grappleCameraShakeSource.GenerateImpulse();
        instance.playerCameraFollowPoint.DOKill();
        instance.playerCameraFollowPoint.localPosition = Vector3.zero;
        instance.playerCameraFollowPoint.DOShakePosition(0.25f, 1, 100).SetUpdate(true);
    }

    public static void CameraShake(Vector2 velocity)
    {
        FindObjectOfType<CinemachineImpulseSource>().GenerateImpulse(velocity);
    }

    public static void HitCameraShake()
    {
        instance.hitCameraShakeSource.GenerateImpulse();
    }

    public static void HitFreeze(float duration = 0.1f)
    {
        instance.StartCoroutine(instance.HitPause(duration));
        VibrateController(0.2f, 0.3f);
    }

    public static void SpawnAttackExplosion(float angle, Vector2 position)
    {
        GameObject explosion = Instantiate(instance.attackExplosionPrefab);
        explosion.transform.position = position;
        explosion.transform.rotation = Quaternion.Euler(0, 0, angle);
        Destroy(explosion, 0.4f);
    }

    public static void SpawnDeathExplosion(Vector2 position)
    {
        PlaySound(instance.explosionSound);
        GameObject explosion = Instantiate(instance.deathExplosionPrefab);
        explosion.transform.position = position;
        Destroy(explosion, 0.4f);
    }

    public static void SpawnParticleEffects(Vector3 position, GameObject particlePrefab)
    {
        if (particlePrefab == null)
            return;

        GameObject spawnedEffect = Instantiate(particlePrefab);
        spawnedEffect.transform.position = position;
        Destroy(spawnedEffect, 0.4f);
    }

    public static void VibrateController(float lowFrequency, float highFrequency)
    {
        if (Gamepad.current != null)
        {
            instance.StopCoroutine("VibrationRoutine");
            instance.StartCoroutine(instance.VibrationRoutine(lowFrequency, highFrequency));
        }
    }

    public static Coroutine FadeOut(SpriteRenderer renderer, float duration)
    {
        return instance.StartCoroutine(instance.FadeOutRoutine(renderer, duration));
    }

    public static void SpawnCoin(Vector3 position)
    {
        GameObject spawnedObject = Instantiate(instance.droppedCoinPrefab);
        spawnedObject.transform.position = position;
        OnTrigger coinTrigger = spawnedObject.GetComponent<OnTrigger>();
        coinTrigger.triggerEnter.AddListener(() => instance.allCollectibleTiles[0].DoCollectEffect(spawnedObject.transform.position - Vector3.one * 0.25f));
        coinTrigger.triggerEnter.AddListener(() => Destroy(spawnedObject));
    }

    IEnumerator VibrationRoutine(float lowFrequency, float highFrequency)
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
        yield return new WaitForSecondsRealtime(0.15f);
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0f, 0f);
    }

    IEnumerator FadeOutRoutine(SpriteRenderer renderer, float duration)
    {
        Color initialColor = renderer.color;
        Color finalColor = renderer.color;
        finalColor.a = 0;
        float timer = 0;

        while (timer < 1)
        {
            renderer.color = Color.Lerp(initialColor, finalColor, timer);
            timer += Time.deltaTime / duration;
            yield return new WaitForEndOfFrame();
        }
        renderer.color = finalColor;
    }

    /*  public static void HitBlink(SpriteRenderer characterBlinking, IDamageable characterDamaged)
      {
          instance.StartCoroutine(instance.HitBlinkRoutine(characterBlinking, characterDamaged));
      }*/

    IEnumerator HitPause(float duration)
    {
        if (Time.timeScale != 0)
        {
            Time.timeScale = 0.1f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
    }

    /* IEnumerator HitBlinkRoutine(SpriteRenderer characterBlinking, IDamageable characterDamaged)
     {
         characterDamaged.invincible = true;

         float timer = .8f;
         while (timer > 0)
         {
             if (characterBlinking != null)
             {
                 if (characterBlinking != null)
                     characterBlinking.enabled = false;
                 yield return new WaitForSeconds(0.1f);
                 if (characterBlinking != null)
                     characterBlinking.enabled = true;
                 yield return new WaitForSeconds(0.1f);
             }
             timer -= 0.2f;
         }

         if (characterBlinking != null)
             characterBlinking.enabled = true;


         characterDamaged.invincible = false;
     }*/

    public static void PlaySound(AudioClip soundToPlay, float volume = 0.5f, bool unscaledTime = false)
    {
        if ((Time.timeScale == 0 && unscaledTime == false))
            return;

        if (soundHolder == null)
        {
            soundHolder = GameObject.Instantiate(new GameObject());
            soundHolder.name = "Sound Holder";
        }

        AudioSource audio;
        audio = soundHolder.AddComponent<AudioSource>();
        audio.volume = volume;
        audio.clip = soundToPlay;
        audio.Play();
        GameObject.Destroy(audio, soundToPlay.length + 0.2f);
    }
    public static bool IsInLayerMask(int layer, LayerMask layermask)
    {
        return layermask == (layermask | (1 << layer));
    }

    public static void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, System.Action<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback = new EventTrigger.TriggerEvent();
        entry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(callback));
        trigger.triggers.Add(entry);
    }

    public static string FormatTime(float time)
    {
        int intTime = (int)time;
        //int minutes = intTime / 60;
        int seconds = intTime;
        float fraction = time * 100;
        fraction = (fraction % 100);
        string timeText = String.Format("{0:00}:{1:00}", seconds, fraction);
        return timeText;
    }
}
