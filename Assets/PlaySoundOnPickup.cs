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

    public void Pickup()
    {
        p.transform.parent = null;
        p.Emit(250);
        Destroy(p, 5f);

        AudioSource.PlayClipAtPoint(collectSound, transform.position);
        this.gameObject.SetActive(false);
    }
}
