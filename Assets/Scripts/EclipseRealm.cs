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

    public GameObject regenPotionPrefab;
    public GameObject damagePotionPrefab;
    public GameObject longRangePotionPrefab;

    int counter = 0; // TODO for dev purposes only, remove
    int numberOfCoinsSpawned = 0;

    private enum ReachabilityMode {CAST, PATH}
    
    public int timeBetweenCoinSpawns;

	void Start () { // We don't want to initialize Eclipse Realm in Start(), rather when the game is selected.
		
	}
	
	void Update () {
		if(initialized)
        {
            if(Time.timeScale != 0.0f) // Only spawn items if the game is not paused
            {
                counter++;
                if (counter % (60 * timeBetweenCoinSpawns) == 0)
                {
                    SpawnCoin();
                }
                if (IsChestSpawnTime(counter))
                {
                    SpawnChest();
                }
                if (IsEnemySpawnTime(counter))
                {
                    SpawnEnemy();
                }
                if (IsRegenPotionSpawnTime(counter))
                {
                    SpawnRegenPotion();
                }
                if (IsDamagePotionSpawnTime(counter))
                {
                    SpawnDamagePotion();
                }
                if (IsLongRangePotionSpawnTime(counter))
                {
                    SpawnLongRangePotion();
                }
            }    
        }
	}

    // Shit. Working shit, but still shit.
    private int SpawnCounter = 10 * 60; // Spawn first enemy after 10 seconds
    private int EnemiesSpawned = 0;
    bool IsEnemySpawnTime(int frame)
    {
        SpawnCounter--;
        if(SpawnCounter == 0)
        {
            EnemiesSpawned++;
            SpawnCounter = (26 - 2 * EnemiesSpawned) * 60; // After the first enemy, spawn the next one after 24 seconds, then make time shorter by 2 seconds each time
            if (SpawnCounter < 5 * 60) SpawnCounter = 5 * 60;
            return true;
        }
        return false;
    }

    bool IsChestSpawnTime(int frame)
    {
        float probability = (1f / 60f) / 20f; // once in 20 seconds on average
        return Random.Range(0.0f, 1.0f) < probability;
    }

    bool IsRegenPotionSpawnTime(int frame)
    {
        if (GameObject.FindGameObjectsWithTag("Regen potion").Length > 1) return false; // Only have a maximum of 2 potions of each type at a time
        float probability = (1f / 60f) / 20f; // once in 20 seconds on average
        return Random.Range(0.0f, 1.0f) < probability;
    }

    bool IsDamagePotionSpawnTime(int frame)
    {
        if (GameObject.FindGameObjectsWithTag("Damage potion").Length > 1) return false;
        float probability = (1f / 60f) / 20f; // once in 20 seconds on average
        return Random.Range(0.0f, 1.0f) < probability;
    }

    bool IsLongRangePotionSpawnTime(int frame)
    {
        if (GameObject.FindGameObjectsWithTag("Long range potion").Length > 1) return false;
        float probability = (1f / 60f) / 20f; // once in 20 seconds on average
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

#if UNITY_EDITOR
        MovePlayerToRandomPosition();
#endif
        GameObject.FindObjectOfType<EclipsePlayer>().showStatusBars = true;

        initialized = true;
        Debug.Log("Eclipse Realm initialized");
        GameObject.FindObjectOfType<UIController>().Help();
    }

    public void Pause()
    {
        Time.timeScale = 0.0f;
        //GameObject.FindObjectOfType<EclipsePlayer>().showStatusBars = false;
    }

    public void Unpause()
    {
        Time.timeScale = 1.0f;
        //GameObject.FindObjectOfType<EclipsePlayer>().showStatusBars = true;
    }

    void SpawnEnemy()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int timeout = 100; // Timeout is necessary especially if the player position ends up outside of play area, within walls, etc.
        while (true)
        {
            timeout--;
            float x, y, z;
            // Spawning first enemies near the player turned out to be a rather punishing experience
            //if(EnemiesSpawned <= 2) // spawn first enemies close
            //{
            //    float radius = 15.0f;
            //    Vector2 point = radius * Random.insideUnitCircle;
            //    EclipsePlayer player = FindObjectOfType<EclipsePlayer>();
            //    Vector2 offset = new Vector2(player.transform.position.x, player.transform.position.z);
            //    Vector2 location = point + offset;
            //    x = location.x;
            //    z = location.y; // coordinate systems don't match intuition
            //}
            //else
            //{
                x = Random.Range(areaBounds.min.x, areaBounds.max.x);
                z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            //}

            y = areaBounds.min.y + 0.5f;

            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH)) // PATH is preferred, as it actually checks _reachability_
            {
                GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.Euler(0, 0, 90));
                enemy.transform.parent = coinsParent.transform;
                return;
            }
            if(timeout <= 0)
            {
                if (IsReachable(position, ReachabilityMode.CAST)) // CAST checks for *a* valid position on a NavMesh, but not necessarily one that's connected to player position
                {
                    GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.Euler(0, 0, 90));
                    enemy.transform.parent = coinsParent.transform;
                    return;
                }
            }
            if (timeout <= -100) return;
        }
    }

    void SpawnCoin()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int timeout = 100;
        while(true)
        {
            timeout--;
            float x, y, z;
            if(numberOfCoinsSpawned < 5)
            {
                float radius = 5.0f;
                Vector2 point = radius * Random.insideUnitCircle;
                EclipsePlayer player = FindObjectOfType<EclipsePlayer>();
                Vector2 offset = new Vector2(player.transform.position.x, player.transform.position.z);
                Vector2 location = point + offset;

                x = location.x;
                y = 1.0f;
                z = location.y; // Coordinate systems don't match intuition
            }
            else
            {
                x = Random.Range(areaBounds.min.x, areaBounds.max.x);
                y = 1.0f;
                z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            }
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH)) // See SpawnEnemy for discussion on ReachabilityModes
            {
                GameObject coin = Instantiate(coinPrefab, position, Quaternion.Euler(0, 0, 90));
                coin.transform.parent = coinsParent.transform;
                numberOfCoinsSpawned++;
                return;
            }
            if(timeout <= 0)
            {
                if (IsReachable(position, ReachabilityMode.CAST))
                {
                    GameObject coin = Instantiate(coinPrefab, position, Quaternion.Euler(0, 0, 90));
                    coin.transform.parent = coinsParent.transform;
                    numberOfCoinsSpawned++;
                    return;
                }
            }
            if (timeout <= -100) return;
        }
    }

    void SpawnChest()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int timeout = 100;
        while (true)
        {
            timeout--;
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.5f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH)) // See SpawnEnemy for discussion on ReachabilityModes
            {
                GameObject chest = Instantiate(chestPrefab, position, Quaternion.Euler(-90, 0, 0));
                chest.transform.parent = coinsParent.transform;
                return;
            }
            if(timeout <= 0)
            {
                if (IsReachable(position, ReachabilityMode.CAST))
                {
                    GameObject chest = Instantiate(chestPrefab, position, Quaternion.Euler(-90, 0, 0));
                    chest.transform.parent = coinsParent.transform;
                    return;
                }
            }
            if (timeout <= -100) return;
        }
    }

    void SpawnRegenPotion()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int timeout = 100;
        while (true)
        {
            timeout--;
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.5f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH)) // See SpawnEnemy for discussion on ReachabilityModes
            {
                GameObject regenPotion = Instantiate(regenPotionPrefab, position, Quaternion.Euler(-45, 0, 0));
                regenPotion.transform.parent = coinsParent.transform;
                return;
            }
            if (timeout <= 0)
            {
                if (IsReachable(position, ReachabilityMode.CAST))
                {
                    GameObject regenPotion = Instantiate(regenPotionPrefab, position, Quaternion.Euler(-45, 0, 0));
                    regenPotion.transform.parent = coinsParent.transform;
                    return;
                }
            }
            if (timeout <= -100) return;
        }
    }

    void SpawnDamagePotion()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int timeout = 100;
        while (true)
        {
            timeout--;
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.5f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH)) // See SpawnEnemy for discussion on ReachabilityModes
            {
                GameObject damagePotion = Instantiate(damagePotionPrefab, position, Quaternion.Euler(-45, 0, 0));
                damagePotion.transform.parent = coinsParent.transform;
                return;
            }
            if (timeout <= 0)
            {
                if (IsReachable(position, ReachabilityMode.CAST))
                {
                    GameObject damagePotion = Instantiate(damagePotionPrefab, position, Quaternion.Euler(-45, 0, 0));
                    damagePotion.transform.parent = coinsParent.transform;
                    return;
                }
            }
            if (timeout <= -100) return;
        }
    }

    void SpawnLongRangePotion()
    {
        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        int timeout = 100;
        while (true)
        {
            timeout--;
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = 0.5f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.PATH)) // See SpawnEnemy for discussion on ReachabilityModes
            {
                GameObject longRangePotion = Instantiate(longRangePotionPrefab, position, Quaternion.Euler(-45, 0, 0));
                longRangePotion.transform.parent = coinsParent.transform;
                return;
            }
            if (timeout <= 0)
            {
                if (IsReachable(position, ReachabilityMode.CAST))
                {
                    GameObject longRangePotion = Instantiate(longRangePotionPrefab, position, Quaternion.Euler(-45, 0, 0));
                    longRangePotion.transform.parent = coinsParent.transform;
                    return;
                }
            }
            if (timeout <= -100) return;
        }
    }

    void PlaceCoins() // This function spawns coins on a regular, 1x1 m grid.
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
    }

    void MovePlayerToRandomPosition()
    {
        EclipsePlayer player = FindObjectOfType<EclipsePlayer>();

        Bounds areaBounds = obstacleMesh.GetComponent<Renderer>().bounds;
        while (true) // This function should only be called in debug, so no timeout
        {
            float x = Random.Range(areaBounds.min.x, areaBounds.max.x);
            float y = areaBounds.min.y + 0.5f;
            float z = Random.Range(areaBounds.min.z, areaBounds.max.z);
            Vector3 position = new Vector3(x, y, z);
            if (IsReachable(position, ReachabilityMode.CAST))
            {
                player.transform.position = position;
                return;
            }
            //else Debug.Log("Position not good: " + position);
        }
    }

    bool IsReachable(Vector3 position, ReachabilityMode mode = ReachabilityMode.PATH)
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
