using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SkeletonEnemyController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetTarget(Vector3 target)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.destination = target;
    }
}
