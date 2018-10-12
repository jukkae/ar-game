using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EclipsePlayer : MonoBehaviour {

    public float maxHealth = 100;
    public float health = 100;
    private float healthBarLength = Screen.width / 2;

	void Start () {
		
	}

    public void TakeDamage(float damage) {
        health -= damage;
        if (health <= 0) Die();
    }
	
	void Update () {
        healthBarLength = (Screen.width / 2) * (health / (float)maxHealth);
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, Screen.height - 30, healthBarLength, 20), health + "/" + maxHealth);
    }

    public void Die()
    {
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "You're dead");
        // TODO
    }
}
