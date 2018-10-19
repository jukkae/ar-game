using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ChangeOpacity : MonoBehaviour {
    private Image image;
    private Image arrowL;
    private Image arrowR;

    void Start()
    {
        image = GetComponent<Image>(); // TODO should [RequireComponent]
        arrowL = transform.Find("Arrow Left").GetComponent<Image>();
        arrowR = transform.Find("Arrow Right").GetComponent<Image>();
    }

	void Update () { // sorry
        Color c = image.color;
        if (c.a > 0.0f) c.a -= 0.01f;
        if (c.a < 0.0f) c.a = 0.0f;
        image.color = c;

        Color cl = arrowL.color;
        if (cl.a > 0.0f) cl.a -= 0.01f;
        if (cl.a < 0.0f) cl.a = 0.0f;
        arrowL.color = cl;

        Color cr = arrowR.color;
        if (cr.a > 0.0f) cr.a -= 0.01f;
        if (cr.a < 0.0f) cr.a = 0.0f;
        arrowR.color = cr;
    }

    public void FlashDamage()
    {
        Color c = image.color;
        c.a = 0.8f;
        image.color = c;
    }

    public void FlashDamageRight()
    {
        FlashDamage();
        Color cr = arrowR.color;
        cr.a = 0.8f;
        arrowR.color = cr;
    }

    public void FlashDamageLeft()
    {
        FlashDamage();
        Color cl = arrowL.color;
        cl.a = 0.8f;
        arrowL.color = cl;
    }
}
