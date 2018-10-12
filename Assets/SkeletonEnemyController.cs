using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SkeletonEnemyController : MonoBehaviour {
    private NavMeshAgent agent;
    private Animator animator;

    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    private EclipsePlayer player;

    public enum EnemyAiState { WANDER, SEEK_PLAYER, ATTACK }
    public EnemyAiState aiState;

    void Start () {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updatePosition = false;
        agent.updateRotation = true;

        player = FindObjectOfType<EclipsePlayer>();

        aiState = EnemyAiState.WANDER;
    }
	
	void Update () {
        if ((player.transform.position - this.transform.position).magnitude < 1.5f)
        {
            aiState = EnemyAiState.ATTACK;
            SetTarget(this.transform.position);
            animator.SetTrigger("Attack");
        }
        else if((player.transform.position - this.transform.position).magnitude < 10.0f)
        {
            aiState = EnemyAiState.SEEK_PLAYER;
            SetTarget(player.transform.position);
        }
        else
        {
            aiState = EnemyAiState.WANDER;
        }
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

        // Update animation parameters
        //animator.SetBool("move", shouldMove);
        //animator.SetFloat("velx", velocity.x);
        //animator.SetFloat("vely", velocity.y);
        //Debug.Log("Velocity:" + velocity + ", magnitude: " + velocity.magnitude);
        float dampTime = 0.1f;
        animator.SetFloat("Velocity", velocity.magnitude * (4.0f / 3.0f), dampTime, Time.deltaTime);

        //GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;
    }

    public void SetTarget(Vector3 target)
    {
        agent.destination = target;
    }

    void OnAnimatorMove()
    {
        // Update position to agent position
        transform.position = agent.nextPosition;
    }
}
