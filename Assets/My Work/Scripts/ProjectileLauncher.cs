using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float fireRate = 0.1f;

    private ParticleSystem muzzleFlash;
    private GameObject impactEffect;
    private XRGrabInteractable grabInteractable;
    private ParticleEffectManager effectManager;

    void Start()
    {
        // Récupérer le manager d'effets
        effectManager = GetComponent<ParticleEffectManager>();

        if (effectManager != null)
        {
            muzzleFlash = effectManager.GetMuzzleFlash();
            impactEffect = effectManager.GetImpactEffect();
        }

        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnActivate);
            grabInteractable.deactivated.AddListener(OnDeactivate);
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
        }
    }

    private void OnActivate(ActivateEventArgs args)
    {
        InvokeRepeating(nameof(Fire), 0f, fireRate);
    }

    private void OnDeactivate(DeactivateEventArgs args)
    {
        CancelInvoke(nameof(Fire));
    }

    void Fire()
    {
        // Vérifier que le pool existe
        if (ProjectilePool.Instance == null)
        {
            Debug.LogError("ProjectilePool. Instance est null !  Assurez-vous qu'il y a un ProjectilePool dans la scène.");
            return;
        }

        // Récupérer un projectile du pool
        GameObject proj = ProjectilePool.Instance.GetProjectile();

        if (proj == null)
        {
            Debug.LogWarning("Impossible de récupérer un projectile du pool");
            return;
        }

        // Initialiser le projectile
        Projectile projectileScript = proj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(
                spawnPoint.position,
                spawnPoint.rotation,
                spawnPoint.forward,
                gameObject,
                impactEffect
            );
        }

        // Jouer le muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
            muzzleFlash.Play();
        }
    }
}