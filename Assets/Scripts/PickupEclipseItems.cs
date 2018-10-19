using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupEclipseItems : MonoBehaviour {
    int score = 0;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Eclipse Item"))
        {
            other.gameObject.SetActive(false);
            score++;
            FindObjectOfType<UIController>().SetCurrentNumberOfEclipseCoins(score);
        }
    }
}
