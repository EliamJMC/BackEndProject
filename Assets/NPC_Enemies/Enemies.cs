using UnityEngine;
using UnityEngine.AI;

public enum State
{
    WALK_RANDOM,
    FOLLOW_PLAYER,
    FOLLOW_WITNESS,
    ATTACK_PLAYER
}

public class Enemies : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private MainCharacterController MCC;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemiesComsSystem enemiesComsSystem;

    [Header("Vision Settings")]
    public float visionRange = 15.0f;
    public float visionAngle = 220.0f; // FOV
    public float proximityRadius = 5.0f;
    [SerializeField] private LayerMask visionMask = ~0; // Configura en el Inspector (ej: excluir "Enemy")

    [Header("Attack Settings")]
    [SerializeField]private float lastAttackTime;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;
    public int attackDamage = 5;
    public float attackWindup = 0.1f; // tiempo para permitir animación
    public float scaleValue;

    [Header("Communication Settings")]
    public float comRadius = 5f;
    public bool detectorIsClose = false;

    [Header("Mouvent Settings")]
    private float lastTimeWalked;
    private float randomWalkCooldown = 2.0f; // segundos entre metas aleatorias
    private Vector3 currentRandomTarget;
    private bool hasRandomTarget;

    [Header("Cached values")]
    private float distanceToPlayer;
    private float angleToPlayer;

    public void OnDestroy()
    { 
        if (enemiesComsSystem.enemiesInScene.Contains(this))
            enemiesComsSystem.enemiesInScene.Remove(this); 
        if (enemiesComsSystem.enemiesDetectedPlayer.Contains(this))
            enemiesComsSystem.enemiesDetectedPlayer.Remove(this);
    }

    public void Start()
    {
        Def_Components();
        Def_Rand_Stats();
        Def_Agent_Sattings();
        
        // Iniciar estado random
        hasRandomTarget = false;
        lastTimeWalked = Time.time;
    }

    public void Update()
    {
        if (player == null) return;

        // Recalcular métricas cada frame
        RecalculateDistances();

        ManageEnemieActions();
        Update_Anim();
    }

    void Def_Components()
    {
        MCC = FindAnyObjectByType<MainCharacterController>();
        characterStats = FindAnyObjectByType<CharacterStats>();
        player = MCC != null ? MCC.transform : null;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemiesComsSystem = FindAnyObjectByType<EnemiesComsSystem>();
    }

    void Def_Rand_Stats()
    {
        scaleValue = Random.Range(0.9f, 1.6f);
        Vector3 scale = new Vector3(scaleValue, scaleValue, scaleValue);
        transform.localScale = scale;
        agent.speed = 2.5f / scaleValue;
        attackDamage = Mathf.RoundToInt(scaleValue * 5);
    }

    void Def_Agent_Sattings()
    {
        // Configurar agente para coherencia
        agent.updateRotation = true;
        agent.stoppingDistance = attackRange;
        agent.autoBraking = true;
    }

    void Update_Anim()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // No marcar IsAttacking por "poder atacar", sino cuando se ataca realmente
        // Se gestiona en Attack()
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Walking"))
            animator.speed = agent.speed / 1.5f;
    }

    // ---------- Perception ----------
    void RecalculateDistances()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 dir = (player.position - transform.position).normalized;
        float dot = Mathf.Clamp(Vector3.Dot(transform.forward, dir), -1f, 1f);
        angleToPlayer = Mathf.Acos(dot) * Mathf.Rad2Deg;
    }

    public bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f * transform.localScale.y;
        Vector3 target = player.position + Vector3.up * 1.5f;
        Vector3 dir = (target - origin).normalized;

        Debug.DrawRay(origin, dir * visionRange, Color.red);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, visionRange, visionMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player;
        }
        return false;
    }

    public bool DetectedPlayer()
    {
        bool inVisionCone = distanceToPlayer < visionRange && angleToPlayer < visionAngle / 2f && HasLineOfSight();
        bool inProximityZone = distanceToPlayer < proximityRadius && HasLineOfSight();
        return (inProximityZone || inVisionCone) && !CanAttack();
    }

    public bool CanAttack()
    {
        // Usar valores recalculados
        bool inRangeAndFacing = (distanceToPlayer <= attackRange && angleToPlayer < 90f);
        bool cooldownReady = (Time.time >= lastAttackTime + attackCooldown);
        return inRangeAndFacing && cooldownReady;
    }

    public bool WitnessedDetectedPlayer()
    {
        Enemies nearestDetector = enemiesComsSystem.FindNearestDetector(this);
        if (!DetectedPlayer() && nearestDetector != null)
        {
            float dist = Vector3.Distance(transform.position, nearestDetector.transform.position);
            detectorIsClose = dist <= comRadius; // ✅ actualiza la variable cada frame
            return detectorIsClose;
        }
        else
        {
            detectorIsClose = false;
            return false;
        }
    }

    public bool CanWalkRandomly(float seconds)
    {
        bool isntDoingElse = !(DetectedPlayer() || CanAttack() || WitnessedDetectedPlayer());
        bool walkFinished = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        bool cooldownOver = (Time.time - lastTimeWalked) >= seconds;
        return walkFinished && cooldownOver && isntDoingElse;
    }

    // ---------- State ----------
    public State enemieState()
    {
        if (CanAttack())
            return State.ATTACK_PLAYER;
        else if (DetectedPlayer())
            return State.FOLLOW_PLAYER;
        else if (WitnessedDetectedPlayer())
            return State.FOLLOW_WITNESS;
        else
            return State.WALK_RANDOM;
    }

    void ManageEnemieActions()
    {
        switch (enemieState())
        {
            case State.WALK_RANDOM:
                WalkToRandPos();
                break;
            case State.FOLLOW_PLAYER:
                FollowPlayer();
                break;
            case State.FOLLOW_WITNESS:
                FollowWitness();
                break;
            case State.ATTACK_PLAYER:
                Attack();
                break;
        }
    }

    // ---------- Actions ----------
    void FollowPlayer()
    {
        // No reasignar destino cada frame si ya está yendo
        if (!agent.hasPath || agent.destination != player.position)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(player.position);
        }
        lastTimeWalked = Time.time; // mantener cooldown coherente
        hasRandomTarget = false;
    }

    void FollowWitness()
    {
        var leader = enemiesComsSystem != null ? enemiesComsSystem.FindNearestDetector(this) : null;
        if (leader == null) return;

        if (!agent.hasPath || agent.destination != leader.transform.position)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(leader.transform.position);
        }
        lastTimeWalked = Time.time;
        hasRandomTarget = false;
    }

    void WalkToRandPos()
    {
        // Solo asigna nuevo destino cuando:
        // - terminó el anterior
        // - pasó el cooldown
        if (CanWalkRandomly(randomWalkCooldown) || !hasRandomTarget)
        {
            agent.isStopped = false;

            // Generar un punto cerca del enemigo, proyectado al NavMesh
            Vector3 randomDirection = Random.insideUnitSphere * 15.0f; // rango menor y controlable
            randomDirection.y = 0f;
            Vector3 candidate = transform.position + randomDirection;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 10.0f, NavMesh.AllAreas))
            {
                currentRandomTarget = navHit.position;
                hasRandomTarget = true;
                agent.ResetPath();
                agent.SetDestination(currentRandomTarget);
                lastTimeWalked = Time.time;
            }
        }
    }

    void Attack()
    {
        // Detener al agente y orientar al jugador
        agent.isStopped = true;
        agent.ResetPath();
        Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(lookTarget);

        // Señal de animación y daño
        animator.SetBool("IsAttacking", true);

        // Pequeño windup para permitir la animación antes de aplicar daño
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time + attackWindup;
            characterStats.EnemyAttackPlayer(attackDamage);
        }

        StartCoroutine(ResetAttackFlagAfter(0.2f));
    }

    System.Collections.IEnumerator ResetAttackFlagAfter(float t)
    {
        yield return new WaitForSeconds(t);
        animator.SetBool("IsAttacking", false);
        agent.isStopped = false; 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}