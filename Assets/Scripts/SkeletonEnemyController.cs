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

    public enum EnemyAiState { WANDER, SEEK_PLAYER, ATTACK, TAKE_HIT, DEAD }
    public EnemyAiState aiState;
    private bool stopped = false;

    public int health = 5;
    public const int maxHealth = 5;

    public float attackDistance = 2.0f; // meters
    public float seekDistance = 10.0f; // meters, the distance from which the enemy will start actively seeking towards player

    private bool deathAnimation = false; // This is a shit way of fixing shit animation
    private Vector3 positionAtTimeOfDeath;
    private int deathAnimationCountdown = 42; // Like, real shit. BTW, this is yet another empirical constant, not just any ordinary magic number.

    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip attackSound;

    void Start () {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updatePosition = false;
        agent.updateRotation = true;

        player = FindObjectOfType<EclipsePlayer>();

        aiState = EnemyAiState.WANDER;
    }
	
	void Update () {
        if (deathAnimation) // If the enemy is currently dying, ie. the animation is still playing
        {
            if(deathAnimationCountdown > 0) // For the early parts of animation, the skeleton's feet are still on the floor and we just wait
            {
                deathAnimationCountdown--;
            }
            else // But when the feet start to rise off the floor in the animation, we manually force the skeleton model to drop down
            {
                if ((positionAtTimeOfDeath.y - transform.position.y) < 0.614) // empirical constant
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y - 0.025f, transform.position.z);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, positionAtTimeOfDeath.y - 0.614f, transform.position.z);
                    deathAnimation = false; // jesus christ i'm sorry
                }
            }
        }
        if (aiState == EnemyAiState.DEAD) // When the enemy is dead, we disable all movement
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            return;
        }
        if ((player.transform.position - this.transform.position).magnitude < attackDistance) // If we're within attack range
        {
            aiState = EnemyAiState.ATTACK;
            SetTarget(this.transform.position); // Stay in place
            animator.SetTrigger("Attack"); // And attack
        }
        else if((player.transform.position - this.transform.position).magnitude < seekDistance) // If we're close enough
        {
            aiState = EnemyAiState.SEEK_PLAYER;
            SetTarget(player.transform.position); // Seek to player
        }
        else // If we're too far away, just pick a destination at random
        {
            aiState = EnemyAiState.WANDER;
        }

        // Smoothen animation parameters so that blending between animations based on speed gets smoothed out
        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy); // to make skeletons move slower, clamp this to shorter value

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

        // Update animation parameters
        float dampTime = 0.1f;
        animator.SetFloat("Velocity", velocity.magnitude * (4.0f / 3.0f), dampTime, Time.deltaTime);

        //GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;        
    }

    public void TakeDamage(int damage)
    {
        if(aiState != EnemyAiState.DEAD) // Only take damage if not dead
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
            aiState = EnemyAiState.TAKE_HIT;
            DisableMovement();
            GetComponentInChildren<ParticleSystem>().Emit(500);
            health -= damage;
            if (health <= 0) Die();
            else animator.SetTrigger("TakeDamage");
        }
    }

    public void Die()
    {
        AudioSource.PlayClipAtPoint(deathSound, transform.position);
        animator.SetTrigger("Die");
        aiState = EnemyAiState.DEAD;
        deathAnimation = true; // yeh i know i probably don't need both of these anymore
        positionAtTimeOfDeath = transform.position;
        //CapsuleCollider c = this.GetComponent<CapsuleCollider>();
        //Vector3 center = c.center;
        //Vector3 newCenter = new Vector3(center.x, center.y + 0.18f, center.z);
        //c.center = newCenter;
        GetComponent<Collider>().enabled = false;
        DisableMovement();
    }

    public void SetTarget(Vector3 target)
    {
        agent.destination = target;
    }

    public void AttackPlayer()
    {
        AudioSource.PlayClipAtPoint(attackSound, transform.position);
        aiState = EnemyAiState.ATTACK;
        if ((player.transform.position - this.transform.position).magnitude < attackDistance)
        {
            player.TakeDamage(5, this.gameObject);
        }
    }

    public void EnableMovement()
    {
        agent.isStopped = false;
    }

    public void DisableMovement()
    {
        agent.velocity = Vector3.zero;
        agent.isStopped = true;
    }

    void OnAnimatorMove()
    {
        if(aiState != EnemyAiState.DEAD && aiState != EnemyAiState.TAKE_HIT)
            transform.position = agent.nextPosition;
    }

}
