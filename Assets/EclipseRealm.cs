using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


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
                Vector3 position = new Vector3(x, 1.0f, z);
                if (IsReachable(position))
                {
                    GameObject coin = Instantiate(coinPrefab, position, Quaternion.Euler(0, 0, 90));
                    coin.transform.parent = transform;
                }
            }
        }
    }

    bool IsReachable(Vector3 position) // TODO quick-and-dirty heuristic, doesn't yet actually check if reachable, only if on open floor
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
        {
            //Debug.Log("Position valid, hit mask: " + hit.mask + ", position of hit: " + hit.position + ", normal at hit: " + hit.normal);
            if(hit.position.y < 0.5) // Empirical constant: If higher, then hit on ceiling
            {
                return true;
            }
        }
        return false;
    }
}
