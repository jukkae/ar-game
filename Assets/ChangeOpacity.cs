using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ChangeOpacity : MonoBehaviour {
    private Image image;

    void Start()
    {
        image = GetComponent<Image>(); // TODO should [RequireComponent]
    }

	void Update () {
        Color c = image.color;

        if (c.a > 0.0f) c.a -= 0.01f;
        if (c.a < 0.0f) c.a = 0.0f;
        image.color = c;
	}

    public void FlashDamage()
    {
        Color c = image.color;
        c.a = 0.8f;
        image.color = c;
    }
}
