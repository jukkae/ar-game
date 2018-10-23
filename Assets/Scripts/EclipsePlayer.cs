﻿using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EclipsePlayer : MonoBehaviour {
    private int score = 0;

    public float maxHealth = 100;
    public float health = 100;
    private float healthBarLength = Screen.width / 2;

    public float maxEnergy = 100;
    public float energy = 100;
    private float energyBarLength = Screen.width / 2;

    private Camera cam;

    float interactRange = 3.0f; // meters
    int enemyLayer = 17;
    LayerMask eclipseLayers;

    public GameObject damageIndicator;

    public float fastRegenTime = 600.0f;
    public float fastRegenLeft = 0.0f;
    public GameObject fastRegenIndicator;

    public float dualDamageTime = 600.0f;
    public float dualDamageLeft = 0.0f;
    public GameObject dualDamageIndicator;

    public enum FadeDirection { IN, OUT };

    public AudioClip damageSound;
    
    void Start () {
        cam = GetComponent<Camera>();
        if (cam == null) Debug.Log("Camera not found!");
        eclipseLayers = 1 << enemyLayer;
        fastRegenIndicator.SetActive(false);
        dualDamageIndicator.SetActive(false);
    }

    public void TakeDamage(float damage, GameObject attacker) {
        AudioSource.PlayClipAtPoint(damageSound, transform.position);

        Vector3 viewportCoords = cam.WorldToViewportPoint(attacker.transform.position);
        if (viewportCoords.x >= 0 && viewportCoords.x <= 1 && viewportCoords.y >= 0 && viewportCoords.y <= 1 && viewportCoords.z > 0)
        //if(attacker.GetComponentInChildren<Renderer>().isVisible)
        {
            FlashDamage();
        }
        else
        {
            Vector3 targetDir = attacker.transform.position - this.transform.position;
            if (Vector3.Dot(transform.right, targetDir) > 0) FlashDamageRight();
            else FlashDamageLeft();
        }

        health -= damage;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public void FlashDamage()
    {
        damageIndicator.GetComponent<ChangeOpacity>().FlashDamage();
    }

    public void FlashDamageLeft()
    {
        damageIndicator.GetComponent<ChangeOpacity>().FlashDamageLeft();
    }

    public void FlashDamageRight()
    {
        damageIndicator.GetComponent<ChangeOpacity>().FlashDamageRight();
    }


    void Update () {
        healthBarLength = (Screen.width / 2) * (health / (float)maxHealth);
        energyBarLength = (Screen.width / 2) * (energy / (float)maxEnergy);
        if(energy < maxEnergy)
        {
            energy += fastRegenLeft > 0.0f ? 1.4f : 0.35f; // TODO what are good rates?
            if (energy > maxEnergy) energy = maxEnergy;
        }
        if(fastRegenLeft > 0.0f)
        {
            fastRegenLeft -= 1.0f;
            if (fastRegenLeft <= 0.0f) fastRegenLeft = 0.0f;
        }
        if (dualDamageLeft > 0.0f)
        {
            dualDamageLeft -= 1.0f;
            if (dualDamageLeft <= 0.0f) dualDamageLeft = 0.0f;
        }
        Controls();
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, Screen.height - 30, healthBarLength, 20), health + "/" + maxHealth);
        GUI.Box(new Rect(10, Screen.height - 70, energyBarLength, 20), energy + "/" + maxEnergy);
        if (health == 0)
        {
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "You're dead!");
        }
        if(fastRegenLeft > 0.0f)
        {
            fastRegenIndicator.SetActive(true);
        }
        else
        {
            fastRegenIndicator.SetActive(false);
        }
        if (dualDamageLeft > 0.0f)
        {
            dualDamageIndicator.SetActive(true);
        }
        else
        {
            dualDamageIndicator.SetActive(false);
        }
    }

    public void Die()
    {
        Time.timeScale = 0f;
        // TODO
    }

    /**<summary> Input and control management </summary>*/
    private void Controls()
    {
        RaycastHit hit;
        int layerMask = 1 << 10;
        layerMask = ~layerMask;

        if (EventSystem.current &&  (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.IsPointerOverGameObject(0)) || StateManager.sceneState == SceneState.Minigame)
            return;

        Vector2 position;

#if UNITY_EDITOR

        if (!Input.GetMouseButtonDown(0))
            return;
        position = Input.mousePosition;
#else
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            return;
        position = touch.position;
#endif
        if (Physics.Raycast(cam.ScreenPointToRay(position), out hit, interactRange, layerMask))
        {
            GameObject go = hit.collider.gameObject;
            if (go.GetComponent<SkeletonEnemyController>() != null)
            {
                if(energy > 40.0f)
                {
                    SkeletonEnemyController skelly = go.GetComponent<SkeletonEnemyController>();
                    skelly.TakeDamage(dualDamageLeft > 0.0f ? 2 : 1);
                    energy -= 40.0f;
                }
            }
            if (go.GetComponent<EclipsePickable>() != null)
            {
                EclipsePickable pickable = go.GetComponent<EclipsePickable>();
                switch(pickable.pickableType)
                {
                    case EclipsePickable.PickableType.COIN:
                        score++;
                        break;
                    case EclipsePickable.PickableType.CHEST:
                        score += 10; // TODO balance!
                        break;
                    case EclipsePickable.PickableType.REGEN_POTION:
                        fastRegenLeft = fastRegenTime;
                        break;
                    case EclipsePickable.PickableType.DAMAGE_POTION:
                        dualDamageLeft = dualDamageTime;
                        break;
                    case EclipsePickable.PickableType.LONG_RANGE_POTION:
                        throw new System.NotImplementedException("You need to implement this!");
                        break;
                    default:
                        throw new System.NotImplementedException("You need to implement this!");
                }
                pickable.Pickup();
                FindObjectOfType<UIController>().SetCurrentNumberOfEclipseCoins(score);
            }
        }
        else { }
    }

}
