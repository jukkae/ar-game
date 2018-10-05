using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnPickup : MonoBehaviour {
    public AudioClip collectSound;
    private ParticleSystem p;

    private void Start()
    {
        p = GetComponentInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("MainCamera")) // TODO clear up tag use
        {
            p.transform.parent = null;
            p.Emit(250);
            Destroy(p, 5f);

            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}
