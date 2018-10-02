using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysDrawFrustum : MonoBehaviour {

    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(transform.position, Camera.main.fieldOfView, Camera.main.nearClipPlane, Camera.main.farClipPlane, Camera.main.aspect);
    }
}
