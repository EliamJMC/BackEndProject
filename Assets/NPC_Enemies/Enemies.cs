using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public class Enemies : MonoBehaviour
{
    [Header("References")]
    [SerializeField]    private Transform player;
    [SerializeField]    private MainCharacterController MCC;
    [SerializeField]    private CharacterStats characterStats;
    [SerializeField]    private NavMeshAgent agent;
    [SerializeField]    private Animator animator;

    [Header("Vision Settings")]
    public float visionRange = 15.0f;
    public float visionAngle = 220.0f;
    public float detectionRadius;

    [Header("Attack Settings")]
    public float attackRange = agent.stoppingDistance;          // Distancia para atacar
    public float attackCooldown = 1f;                         // Tiempo entre ataques
    public float lastAttackTime;
    public int attackDamage;

    // Movement logic settings
    private Vector3 direction;
    private float distance;
    private float dot;
    private float angle;
    private float lastTimeWalked;

    // States
    bool followPlayer;
    bool walkRandomly;
    bool isAtackingPlayer;


    public void Start()
    {
        MCC = FindAnyObjectByType<MainCharacterController>();
        characterStats = FindAnyObjectByType<CharacterStats>();
        player = MCC.transform;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        {
            float scaleValue = Random.Range(0.9f, 1.6f);
            Vector3 scale = new Vector3(scaleValue, scaleValue, scaleValue);
            transform.localScale = scale;

            agent.speed = 3 / scaleValue;
            attackDamage = (int)(scaleValue * 5);
        }

        isAtackingPlayer = false;
        RandomWalk();
    }

    public void Update()
    {
        if (player != null)
        {
            followPlayer = ((distance < visionRange && angle < visionAngle / 2) || (distance < 5.0f));
            walkRandomly = (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);


            direction = (player.position - transform.position).normalized;
            distance = Vector3.Distance(transform.position, player.position);
            dot = Vector3.Dot(transform.forward, direction);
            angle = Mathf.Acos(dot) * Mathf.Rad2Deg;


            if (followPlayer && !isAtackingPlayer)
            {
                agent.SetDestination(player.position);
                lastTimeWalked = 0f;
            }
            else if (walkRandomly)
            {
                if (Time.time - lastTimeWalked >= 5)
                {
                    RandomWalk();
                    lastTimeWalked = Time.time;
                }
            }

            if (distance <= attackRange && angle < 45f) // jugador al frente y cerca
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    Attack();
                }
                else
                {
                    isAtackingPlayer = false;
                    animator.SetBool("IsAttacking", false);
                }
            }

            animator.SetFloat("Speed", agent.velocity.magnitude);
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Walking"))
                animator.speed = agent.speed / 1.5f;
        }
    }

    void RandomWalk() 
    {         
        Vector3 randomDirection = Random.insideUnitSphere * 25.0f;
        randomDirection += transform.position;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, 25.0f, NavMesh.AllAreas);
        agent.SetDestination(navHit.position);
    }

    void Attack()
    {
        // Detiene el movimiento momentáneamente
        agent.ResetPath();
        animator.SetBool("IsAttacking", true);
        isAtackingPlayer = true;
        lastAttackTime = Time.time;
        characterStats.EnemyAttackPlayer(attackDamage);
    }
}
