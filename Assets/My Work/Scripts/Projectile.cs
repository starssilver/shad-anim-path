using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;

    private GameObject impactEffect;
    private Rigidbody rb;
    private GameObject shooter;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // Démarrer le timer de lifetime à chaque activation
        spawnTime = Time.time;
    }

    void Update()
    {
        // Vérifier le lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    public void Initialize(Vector3 position, Quaternion rotation, Vector3 direction, GameObject weapon, GameObject impact)
    {
        // Position et rotation
        transform.position = position;
        transform.rotation = rotation;

        // Vitesse
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
            rb.angularVelocity = Vector3.zero;
        }

        // Références
        shooter = weapon;
        impactEffect = impact;
    }

    public void SetShooter(GameObject weapon)
    {
        shooter = weapon;
    }

    public void SetImpactEffect(GameObject effect)
    {
        impactEffect = effect;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignorer l'arme qui a tiré
        if (collision.gameObject == shooter)
        {
            return;
        }

        // Ignorer les autres projectiles
        if (collision.gameObject.GetComponent<Projectile>() != null)
        {
            return;
        }

        // Créer l'effet d'impact
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // Retourner au pool au lieu de détruire
        ReturnToPool();
    }

    void ReturnToPool()
    {
        // Réinitialiser la vitesse
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Retourner au pool
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            // Fallback si le pool n'existe pas
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        // Cleanup quand désactivé
        shooter = null;
    }
}