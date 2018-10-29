using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EclipsePickable : MonoBehaviour {
    public AudioClip collectSound;
    private ParticleSystem p;
    public ParticleSystem halo;

    public enum PickableType { COIN, CHEST, REGEN_POTION, DAMAGE_POTION, LONG_RANGE_POTION }
    public PickableType pickableType;

    private void Start()
    {
        p = GetComponentInChildren<ParticleSystem>();
    }

    public void Pickup()
    {
        if(p != null)
        {
            p.transform.parent = null;
            p.Emit(250);
            Destroy(p, 5f);
        }
        if (halo != null)
        {
            halo.transform.parent = null;
            var e = halo.emission;
            e.enabled = false;
            Destroy(halo, 5f);
        }

        AudioSource.PlayClipAtPoint(collectSound, transform.position);
        this.gameObject.SetActive(false);
    }
}
