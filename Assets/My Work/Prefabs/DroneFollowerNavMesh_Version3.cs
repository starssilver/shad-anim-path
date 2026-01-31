using UnityEngine;
using UnityEngine.AI;

public class DroneFollowerNavMesh : MonoBehaviour
{
    [Header("Cible")]
    public Transform player;

    [Header("Déplacement")]
    public float followDistance = 5f;        // Distance minimale avant de commencer à suivre
    public float maxFollowDistance = 15f;    // Distance max avant de se téléporter
    public float moveSpeed = 3.5f;           // Vitesse de déplacement (plus lent que le joueur)
    public float updatePathInterval = 0.5f;  // Fréquence de mise à jour du chemin

    [Header("Hovering")]
    public float hoverHeight = 1.5f;         // Hauteur du drone
    public bool enableHovering = true;
    public float hoverAmplitude = 0.2f;
    public float hoverSpeed = 2f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    public bool lookAtPlayer = true;         // Regarde vers le joueur ou vers la direction de déplacement

    [Header("Vie")]
    public int maxHealth = 2;
    public GameObject deathEffect;
    private int currentHealth;
    private bool isDead = false;

    private NavMeshAgent agent;
    private Animator anim;
    private float lastPathUpdate;
    private float hoverTime;
    private Vector3 basePosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        hoverTime = Random.Range(0f, 100f);

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.acceleration = 8f;
            agent.angularSpeed = 0f; // Désactiver la rotation automatique du NavMesh
            agent.stoppingDistance = followDistance;
            agent.baseOffset = hoverHeight; // Hauteur par défaut
        }
    }

    void Update()
    {
        if (isDead || player == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Si trop loin, se téléporter près du joueur
        if (distanceToPlayer > maxFollowDistance)
        {
            TeleportNearPlayer();
            return;
        }

        // Mettre à jour le chemin régulièrement
        if (Time.time - lastPathUpdate >= updatePathInterval)
        {
            UpdatePath(distanceToPlayer);
            lastPathUpdate = Time.time;
        }

        // Effet de hovering
        ApplyHovering();

        // Rotation
        HandleRotation();

        // Animation
        if (anim != null)
        {
            anim.SetFloat("speed", agent.velocity.magnitude);
        }
    }

    void UpdatePath(float distanceToPlayer)
    {
        // Si le joueur est trop loin, le suivre
        if (distanceToPlayer > followDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            // Assez proche, arrêter
            agent.isStopped = true;
        }
    }

    void ApplyHovering()
    {
        if (!enableHovering) return;

        hoverTime += Time.deltaTime * hoverSpeed;
        float hoverOffset = Mathf.Sin(hoverTime) * hoverAmplitude;
        
        if (agent != null)
        {
            agent.baseOffset = hoverHeight + hoverOffset;
        }
    }

    void HandleRotation()
    {
        Vector3 lookDirection;

        if (lookAtPlayer)
        {
            // Regarder vers le joueur
            lookDirection = player.position - transform.position;
        }
        else
        {
            // Regarder dans la direction du mouvement
            if (agent.velocity.magnitude > 0.1f)
            {
                lookDirection = agent.velocity;
            }
            else
            {
                return; // Ne pas tourner si immobile
            }
        }

        lookDirection.y = 0; // Garder la rotation horizontale

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void TeleportNearPlayer()
    {
        // Trouver une position valide près du joueur sur le NavMesh
        Vector3 randomOffset = Random.insideUnitSphere * followDistance;
        randomOffset.y = 0;
        Vector3 targetPosition = player.position - randomOffset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, followDistance * 2, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            Debug.Log("Drone téléporté près du joueur");
        }
    }

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

        if (agent != null)
            agent.enabled = false;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            TakeDamage(1);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // Distance de suivi (vert)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, followDistance);

        // Distance max (rouge)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxFollowDistance);

        // Ligne vers le joueur
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, player.position);
    }
}