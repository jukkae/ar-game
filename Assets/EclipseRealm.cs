using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Main controller class for Eclipse Realm game
public class EclipseRealm : MonoBehaviour {
    WorldController worldController;
    bool initialized = false;
    public Material meshDebugMaterial;
    GameObject obstacleMesh;
    public GameObject coinPrefab;

	void Start () {
		
	}
	
	void Update () {
		if(initialized)
        {

        }
	}

    public void InitializeRealm (WorldController worldController)
    {
        this.worldController = worldController;

        obstacleMesh = worldController.transform.Find("ObstacleMesh").gameObject;
        if(meshDebugMaterial != null)
            obstacleMesh.GetComponent<Renderer>().material = meshDebugMaterial;

        PlaceCoins();

        initialized = true;
        Debug.Log("Eclipse Realm initialized");
    }

    void PlaceCoins()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        for(int x = Mathf.CeilToInt(areaBounds.min.x); x < Mathf.FloorToInt(areaBounds.max.x); x++)
        {
            for(int z = Mathf.CeilToInt(areaBounds.min.z); z < Mathf.FloorToInt(areaBounds.max.z); z++)
            {
                Instantiate(coinPrefab, new Vector3(x, 1.0f, z), Quaternion.Euler(0, 0, 90));
            }
        }
    }
}
