using UnityEngine;
using UnityEngine.AI;

public class EnnemiSimple : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 15f;
    public float attackRange = 2f;

    [Header("Patrouille")]
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;
    public float idleTimeMin = 2f;
    public float idleTimeMax = 5f;

    [Header("Vie")]
    public int maxHealth = 3;                 // Points de vie
    public GameObject deathEffect;            // Effet de particules à la mort (optionnel)
    private int currentHealth;

    private NavMeshAgent agent;
    private Animator anim;
    private Vector3 startPosition;
    private float timer;
    private float idleTimer;
    private bool isIdle;
    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        startPosition = transform.position;
        timer = wanderTimer;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < detectionRange)
        {
            isIdle = false;
            agent.isStopped = false;
            agent.SetDestination(player.position);

            if (distance <= attackRange)
            {
                anim.SetTrigger("Attack");
                agent.isStopped = true;
            }
        }
        else
        {
            Wander();
        }

        anim.SetFloat("speed", agent.velocity.magnitude);
    }

    // Fonction appelée quand l'ennemi prend des dégâts
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log(gameObject.name + " a pris " + damage + " dégâts. Vie restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // Désactive le NavMeshAgent
        if (agent != null)
            agent.enabled = false;

        // Joue l'effet de mort si assigné
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Désactive le collider pour éviter d'autres collisions
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Détruit l'objet
        Destroy(gameObject);
    }

    // Détection des projectiles
    void OnCollisionEnter(Collision collision)
    {
        // Vérifier si c'est un projectile
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            TakeDamage(1);
            // Le projectile se gère lui-même (retour au pool)
        }
    }

    void Wander()
    {
        timer += Time.deltaTime;

        if (isIdle)
        {
            agent.isStopped = true;
            idleTimer += Time.deltaTime;

            if (idleTimer >= Random.Range(idleTimeMin, idleTimeMax))
            {
                isIdle = false;
                idleTimer = 0f;
            }
        }
        else if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(startPosition, wanderRadius);
            agent.isStopped = false;
            agent.SetDestination(newPos);
            timer = 0;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isIdle)
            {
                isIdle = true;
                idleTimer = 0f;
            }
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, dist, NavMesh.AllAreas);

        return navHit.position;
    }
}