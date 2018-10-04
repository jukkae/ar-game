using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupEclipseItems : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Eclipse Item"))
        {
            other.gameObject.SetActive(false);
        }
    }
}
