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

    public GameObject enemyPrefab;

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
        PlaceEnemy();

        initialized = true;
        Debug.Log("Eclipse Realm initialized");
    }

    void PlaceCoins()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int numberOfCoins = 0;
        for(int x = Mathf.CeilToInt(areaBounds.min.x); x < Mathf.FloorToInt(areaBounds.max.x); x++)
        {
            for(int z = Mathf.CeilToInt(areaBounds.min.z); z < Mathf.FloorToInt(areaBounds.max.z); z++)
            {
                Vector3 position = new Vector3(x, 1.0f, z);
                if (IsReachable(position))
                {
                    GameObject coin = Instantiate(coinPrefab, position, Quaternion.Euler(0, 0, 90));
                    numberOfCoins++;
                    coin.transform.parent = transform;
                }
            }
        }
        FindObjectOfType<UIController>().SetTotalNumberOfEclipseCoins(numberOfCoins);
    }

    void PlaceEnemy()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        while(true) // TODO timeout at some point
        {
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.15f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            Debug.Log("Trying location " + position);
            if(IsReachable(position))
            {
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.Euler(0, 0, 0));
                
                BoxCollider collider = enemy.AddComponent(typeof(BoxCollider)) as BoxCollider;
                collider.size = new Vector3(3.47f, 1f, 2.46f);
                collider.center = new Vector3(0f, 0.44f, 0f);

                enemy.transform.parent = transform;
                Debug.Log("OK!");
                return;
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
