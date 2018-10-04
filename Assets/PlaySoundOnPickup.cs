using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnPickup : MonoBehaviour {
    public AudioClip collectSound;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("MainCamera")) // TODO clear up tag use
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}
