using UnityEngine;
using System.Collections.Generic;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;
    [SerializeField] private Transform poolParent;

    private Queue<GameObject> availableProjectiles = new Queue<GameObject>();
    private List<GameObject> allProjectiles = new List<GameObject>();

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Créer le parent si non assigné
        if (poolParent == null)
        {
            poolParent = new GameObject("ProjectilePool").transform;
            poolParent.SetParent(transform);
        }

        // Pré-remplir le pool
        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewProjectile();
        }

        Debug.Log($"ProjectilePool initialisé avec {initialPoolSize} projectiles");
    }

    GameObject CreateNewProjectile()
    {
        GameObject proj = Instantiate(projectilePrefab, poolParent);
        proj.SetActive(false);
        allProjectiles.Add(proj);
        availableProjectiles.Enqueue(proj);
        return proj;
    }

    public GameObject GetProjectile()
    {
        GameObject projectile;

        // Réutiliser un projectile inactif si disponible
        while (availableProjectiles.Count > 0)
        {
            projectile = availableProjectiles.Dequeue();

            // Vérifier que l'objet existe toujours (pas détruit par erreur)
            if (projectile != null)
            {
                projectile.SetActive(true);
                return projectile;
            }
        }

        // Si le pool est vide, créer un nouveau projectile (si limite non atteinte)
        if (allProjectiles.Count < maxPoolSize)
        {
            projectile = CreateNewProjectile();
            projectile.SetActive(true);
            Debug.LogWarning($"Pool vide : création d'un nouveau projectile ({allProjectiles.Count}/{maxPoolSize})");
            return projectile;
        }

        // Si limite atteinte, réutiliser le plus ancien (pas recommandé mais sécurise)
        Debug.LogError("Pool saturé !  Limite max atteinte.");
        return null;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        if (projectile == null) return;

        projectile.SetActive(false);
        projectile.transform.SetParent(poolParent);

        // Remettre dans la queue seulement s'il n'y est pas déjà
        if (!availableProjectiles.Contains(projectile))
        {
            availableProjectiles.Enqueue(projectile);
        }
    }

    // Infos debug
    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Pool:  {availableProjectiles.Count} disponibles / {allProjectiles.Count} total");
        }
    }
}