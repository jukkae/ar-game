﻿using System.Collections;
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
    public GameObject coinsParent;

    public GameObject enemyPrefab;
    private GameObject enemy;
    int counter = 0; // TODO for dev purposes only, remove

    private enum ReachabilityMode {CAST, PATH}
    public enum GameMode {TIME, SURVIVAL}
    public GameMode gameMode = GameMode.SURVIVAL;

	void Start () {
		
	}
	
	void Update () {
		if(initialized)
        {
            if(gameMode == GameMode.TIME)
            {
                if (counter == 0) // this code is shit
                {
                    while (true) // TODO timeout at some point
                    {
                        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
                        float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
                        float y = 0.15f;
                        float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
                        Vector3 position = new Vector3(x, y, z);
                        Debug.Log("Trying destination " + position);
                        if (IsReachable(position))
                        {
                            enemy.GetComponent<NavMeshAgent>().destination = position;
                            Debug.Log("New destination: " + position);
                            break;
                        }
                    }
                }
                counter++;
                if (counter >= 600) counter = 0;
            }

            if(gameMode == GameMode.SURVIVAL)
            {
                counter++;
                if(counter % (60 * 10) == 0)
                {
                    SpawnCoin();
                }
                if(counter % (60 * 10) == 0)
                {
                    SkeletonEnemyController skelly = FindObjectOfType<SkeletonEnemyController>(); // TODO multiple skellies

                    while (true) // TODO timeout at some point
                    {
                        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
                        float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
                        float y = 0.15f;
                        float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
                        Vector3 position = new Vector3(x, y, z);
                        Debug.Log("Trying destination " + position);
                        if (IsReachable(position))
                        {
                            skelly.SetTarget(position);
                            Debug.Log("New destination: " + position);
                            break;
                        }
                    }

                }
            }
        }
	}

    public void InitializeRealm (WorldController worldController)
    {
        this.worldController = worldController;

        obstacleMesh = worldController.transform.Find("ObstacleMesh").gameObject;
        if(meshDebugMaterial != null)
            obstacleMesh.GetComponent<Renderer>().material = meshDebugMaterial;

        if(gameMode == GameMode.TIME)
        {
            PlaceCoins();
            PlaceEnemy();
        }
        if(gameMode == GameMode.SURVIVAL)
        {
            FindObjectOfType<UIController>().HideTotalNumberText();
        }


        initialized = true;
        Debug.Log("Eclipse Realm initialized");
    }

    void SpawnEnemy()
    {
        Debug.Log("Spawning enemy");
    }

    void SpawnCoin()
    {
        Debug.Log("Spawning coin");
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        while(true)
        {
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.35f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            Debug.Log("Trying location " + position);
            if (IsReachable(position, ReachabilityMode.PATH))
            {
                GameObject coin = Instantiate(coinPrefab, position, Quaternion.Euler(0, 0, 90));
                coin.transform.parent = coinsParent.transform;
                Debug.Log("OK!");
                return;
            }
        }
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
                    coin.transform.parent = coinsParent.transform;
                }
            }
        }
        FindObjectOfType<UIController>().SetTotalNumberOfEclipseCoins(numberOfCoins);
    }

    void PlaceEnemy()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;

        int timeout = 100;
        while(true) // TODO timeout at some point
        {
            timeout--;
            if (timeout <= 0) break; // TODO

            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.15f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            Debug.Log("Trying location " + position);
            if(IsReachable(position, ReachabilityMode.PATH)) // TODO this needs a better chceck because of colliders etc
            {
                enemy = Instantiate(enemyPrefab, position, Quaternion.Euler(0, 0, 0));
                enemy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                BoxCollider collider = enemy.AddComponent(typeof(BoxCollider)) as BoxCollider;
                collider.size = new Vector3(3.47f, 1f, 2.46f);
                collider.center = new Vector3(0f, 0.44f, 0f);

                Rigidbody rb = enemy.AddComponent(typeof(Rigidbody)) as Rigidbody;

                NavMeshAgent agent = enemy.AddComponent(typeof(NavMeshAgent)) as NavMeshAgent;
                agent.areaMask = 1; // TODO why though

                //agent.destination = new Vector3(x + 10, y, z + 10);

                enemy.transform.parent = transform;
                Debug.Log("OK!");
                return;
            }
        }
    }

    bool IsReachable(Vector3 position, ReachabilityMode mode = ReachabilityMode.PATH) // TODO quick-and-dirty heuristic, doesn't yet actually check if reachable, only if on open floor
    {
        switch (mode)
        {
            case ReachabilityMode.CAST:
                NavMeshHit hit;
                if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas))
                {
                    //Debug.Log("Position valid, hit mask: " + hit.mask + ", position of hit: " + hit.position + ", normal at hit: " + hit.normal);
                    if (hit.position.y < 0.5) // Empirical constant: If higher, then hit on ceiling
                    {
                        return true;
                    }
                }
                return false;
            case ReachabilityMode.PATH:
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(position, new Vector3(0, 0, 0), 1, path);

                bool debug = false;
                if(debug) {
                    string p = "PATH: ";
                    p += path.status;
                    p += ", CORNERS: ";
                    for (int i = 0; i < path.corners.Length; i++)
                    {
                        p += path.corners[i];
                        p += ", ";
                    }
                    Debug.Log(p);
                }
                return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }
}
