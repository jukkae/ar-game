﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// Main controller class for Eclipse Realm game
public class EclipseRealm : MonoBehaviour {
    WorldController worldController;
    bool initialized = false;
    public bool changeMaterialToDebug;
    public Material meshDebugMaterial;
    GameObject obstacleMesh;
    public GameObject coinPrefab;
    public GameObject chestPrefab;
    public GameObject coinsParent;

    public GameObject enemyPrefab;

    int counter = 0; // TODO for dev purposes only, remove

    private enum ReachabilityMode {CAST, PATH}
    public enum GameMode {TIME, SURVIVAL}
    public GameMode gameMode = GameMode.SURVIVAL;
    
    public int timeBetweenCoinSpawns;

	void Start () {
		
	}
	
	void Update () {
		if(initialized)
        {
            if(gameMode == GameMode.TIME)
            {

            }

            if(gameMode == GameMode.SURVIVAL)
            {
                counter++;
                if(counter % (60 * timeBetweenCoinSpawns) == 0)
                {
                    SpawnCoin();
                }
                if (IsChestSpawnTime(counter))
                {
                    Debug.Log("chest spawn");
                    SpawnChest();
                }
                if (IsEnemySpawnTime(counter))
                {
                    SpawnEnemy();
                }
            }
        }
	}

    // Shit. Working shit, but still shit. Sorry.
    private int SpawnCounter = 30 * 60;
    private int EnemiesSpawned = 0;
    bool IsEnemySpawnTime(int frame)
    {
        SpawnCounter--;
        if(SpawnCounter == 0)
        {
            EnemiesSpawned++;
            SpawnCounter = (30 - 2 * EnemiesSpawned) * 60;
            if (SpawnCounter < 5 * 60) SpawnCounter = 5 * 60;
            return true;
        }
        return false;
    }

    bool IsChestSpawnTime(int frame)
    {
        float probability = (1f / 60f) / 10f; // once in 10 seconds on average
        return Random.Range(0.0f, 1.0f) < probability;
    }

    public void InitializeRealm (WorldController worldController)
    {
        this.worldController = worldController;

        obstacleMesh = worldController.transform.Find("ObstacleMesh").gameObject;
        if (changeMaterialToDebug)
        {
            if (meshDebugMaterial != null)
                obstacleMesh.GetComponent<Renderer>().material = meshDebugMaterial;
        }

        if(gameMode == GameMode.TIME)
        {
            PlaceCoins();
        }
        if(gameMode == GameMode.SURVIVAL)
        {
            FindObjectOfType<UIController>().HideTotalNumberText();
        }

        MovePlayerToRandomPosition();

        initialized = true;
        Debug.Log("Eclipse Realm initialized");
    }

    void SpawnEnemy()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        while (true)
        {
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.35f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH))
            {
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.Euler(0, 0, 90));
                enemy.transform.parent = coinsParent.transform;
                return;
            }
        }
    }

    void SpawnCoin()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int timeout = 100;
        while(true)
        {
            timeout--;
            if (timeout <= 0) return;
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 1.0f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH))
            {
                GameObject coin = Instantiate(coinPrefab, position, Quaternion.Euler(0, 0, 90));
                coin.transform.parent = coinsParent.transform;
                return;
            }
        }
    }

    void SpawnChest()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        while (true)
        {
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.5f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH))
            {
                GameObject chest = Instantiate(chestPrefab, position, Quaternion.Euler(-90, 0, 0));
                chest.transform.parent = coinsParent.transform;
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

    void MovePlayerToRandomPosition()
    {
        Debug.Log("Moving player");
        EclipsePlayer player = FindObjectOfType<EclipsePlayer>();

        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        while (true)
        {
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = areaBounds.min.y + 0.5f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.CAST))
            {
                Debug.Log("Position found: " + position);
                player.transform.position = position;
                return;
            }
            else Debug.Log("Position not good: " + position);
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
                EclipsePlayer player = FindObjectOfType<EclipsePlayer>();
                Vector3 playerPosition = player.transform.position;
                NavMesh.CalculatePath(position, playerPosition, 1, path);

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