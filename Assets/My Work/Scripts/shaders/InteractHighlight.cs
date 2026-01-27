using UnityEngine;

public class InteractHighlight : MonoBehaviour
{
    [Header("Settings")]
    public Renderer targetRenderer;

    private MaterialPropertyBlock _propBlock;
    private static readonly int HoverProp = Shader.PropertyToID("_Hover");
    private static readonly int GrabProp = Shader.PropertyToID("_Grab");

    // Valeurs cibles pour les transitions
    private float _hoverValue = 0f;
    private float _grabValue = 0f;

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
    }

    public void SetHover(bool active)
    {
        _hoverValue = active ? 1f : 0f;
        UpdateProperties();
    }

    public void SetGrab(bool active)
    {
        _grabValue = active ? 1f : 0f;
        UpdateProperties();
    }

    private void UpdateProperties()
    {
        targetRenderer.GetPropertyBlock(_propBlock);

        _propBlock.SetFloat(HoverProp, _hoverValue);
        _propBlock.SetFloat(GrabProp, _grabValue);

        targetRenderer.SetPropertyBlock(_propBlock);
    }
}