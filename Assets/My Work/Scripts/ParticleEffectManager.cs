using UnityEngine;

public class ParticleEffectManager : MonoBehaviour
{
    [Header("Muzzle Flash Effects")]
    [SerializeField] private ParticleSystem muzzleFlashPCVR;
    [SerializeField] private ParticleSystem muzzleFlashQuest;

    [Header("Impact Effects")]
    [SerializeField] private GameObject impactEffectPCVR;
    [SerializeField] private GameObject impactEffectQuest;

    private bool isQuest = false;

    void Awake()
    {
        // Détecter la plateforme
#if UNITY_ANDROID
            isQuest = true;
#else
        isQuest = false;
#endif

        // Activer/désactiver les muzzle flash selon la plateforme
        if (isQuest)
        {
            if (muzzleFlashPCVR != null) muzzleFlashPCVR.gameObject.SetActive(false);
            if (muzzleFlashQuest != null) muzzleFlashQuest.gameObject.SetActive(true);

            Debug.Log("Platform: Meta Quest - Effets optimisés");
        }
        else
        {
            if (muzzleFlashPCVR != null) muzzleFlashPCVR.gameObject.SetActive(true);
            if (muzzleFlashQuest != null) muzzleFlashQuest.gameObject.SetActive(false);

            Debug.Log("Platform: PCVR - Effets haute qualité");
        }
    }

    public ParticleSystem GetMuzzleFlash()
    {
        return isQuest ? muzzleFlashQuest : muzzleFlashPCVR;
    }

    public GameObject GetImpactEffect()
    {
        return isQuest ? impactEffectQuest : impactEffectPCVR;
    }
}