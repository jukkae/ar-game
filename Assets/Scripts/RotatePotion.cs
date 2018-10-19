using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePotion : MonoBehaviour {
    public float speed = 40.0f;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * speed, Space.World);
    }
}
