using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EclipsePlayer : MonoBehaviour {

    public float maxHealth = 100;
    public float health = 100;
    private float healthBarLength = Screen.width / 2;

    private Camera cam;

    float interactRange = 10; // meters
    int enemyLayer = 17;
    LayerMask eclipseLayers;

    void Start () {
        cam = GetComponent<Camera>();
        if (cam == null) Debug.Log("Camera not found!");
        eclipseLayers = 1 << enemyLayer;
	}

    public void TakeDamage(float damage) {
        health -= damage;
        if (health <= 0) Die();
    }
	
	void Update () {
        healthBarLength = (Screen.width / 2) * (health / (float)maxHealth);
        Controls();
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, Screen.height - 30, healthBarLength, 20), health + "/" + maxHealth);
    }

    public void Die()
    {
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "You're dead");
        // TODO
    }

    /**<summary> Input and control management </summary>*/
    private void Controls()
    {
        TrackableHit arcHit;
        RaycastHit hit;
        int layerMask = 1 << 10;
        layerMask = ~layerMask;

        if (EventSystem.current && (EventSystem.current.IsPointerOverGameObject() || EventSystem.current.IsPointerOverGameObject(0)) || StateManager.sceneState == SceneState.Minigame)
            return;

        Vector2 position;

#if UNITY_EDITOR
        if (!Input.GetMouseButtonDown(0))
            return;
        position = Input.mousePosition;
#else
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            return;
        position = touch.position;
#endif

        if (Physics.Raycast(cam.ScreenPointToRay(position), out hit, interactRange, layerMask))
        {
            GameObject go = hit.collider.gameObject;
            if (go.GetComponent<SkeletonEnemyController>() != null)
            {
                SkeletonEnemyController skelly = go.GetComponent<SkeletonEnemyController>();
                skelly.TakeDamage(1);
            }
        }
        else { }
    }

    /**<summary> Interaction on point hit by ray </summary>*/
    private void Interact(Collider col, Vector3 position, bool ar)
    {
        if (col != null) Debug.Log("interact");
        else Debug.Log("null");
        //Item item = null;
        //if (col) item = col.GetComponentInParent<Item>();

        //if (item && !(item is Capturable))
        //{
        //    item.Interact(gameObject);
        //}
        //else if (ObjectEditMode)
        //    InstantiateObject(position, ar);
        //else if (equippedItems.Count > 0 && data.activeItemIndex >= 0)
        //{
        //    if (energy > 0)
        //        energy -= equippedItems[data.activeItemIndex].Use();
        //    else
        //        Debug.Log("Out of energy");
        //}
    }


}
