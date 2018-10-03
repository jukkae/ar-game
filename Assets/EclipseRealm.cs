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
        for(int x = 0; x < 10; x++)
        {
            for(int z = 0; z < 10; z++)
            {
                Instantiate(coinPrefab, new Vector3(x, 1.0f, z), Quaternion.Euler(0, 0, 90));
            }
        }
    }
}
