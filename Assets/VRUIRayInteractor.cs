using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class VRUIRayInteractor : MonoBehaviour
{
    [Header("XR Ray Interactor Setup")]
    public XRRayInteractor rayInteractor;
    public XRInteractorLineVisual lineVisual;
    public XRInteractorLineVisual lineVisualInvalid;
    
    [Header("UI Interaction")]
    public LayerMask uiLayerMask = 1 << 5; // UI layer
    public float maxRaycastDistance = 10f;
    
    [Header("Visual Feedback")]
    public Material validMaterial;
    public Material invalidMaterial;
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    
    void Start()
    {
        SetupRayInteractor();
    }
    
    void SetupRayInteractor()
    {
        if (rayInteractor == null)
        {
            rayInteractor = GetComponent<XRRayInteractor>();
            if (rayInteractor == null)
            {
                Debug.LogError("XRRayInteractor component not found!");
                return;
            }
        }
        
        // Configure ray interactor for UI interaction
        rayInteractor.raycastMask = uiLayerMask;
        rayInteractor.maxRaycastDistance = maxRaycastDistance;
        rayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
        
        // Setup line visual
        if (lineVisual == null)
        {
            lineVisual = rayInteractor.GetComponent<XRInteractorLineVisual>();
        }
        
        if (lineVisual != null)
        {
            lineVisual.lineWidth = 0.01f;
            lineVisual.validColorGradient = new Gradient();
            lineVisual.validColorGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(validColor, 0.0f), new GradientColorKey(validColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            
            lineVisual.invalidColorGradient = new Gradient();
            lineVisual.invalidColorGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(invalidColor, 0.0f), new GradientColorKey(invalidColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
        }
        
        // Enable UI interaction
        rayInteractor.enableUIInteraction = true;
        
        Debug.Log("VR UI Ray Interactor setup complete!");
    }
    
    void Update()
    {
        // Ensure UI interaction is enabled
        if (rayInteractor != null && !rayInteractor.enableUIInteraction)
        {
            rayInteractor.enableUIInteraction = true;
        }
    }
    
    // Method to manually enable/disable ray interactor
    public void SetRayInteractorEnabled(bool enabled)
    {
        if (rayInteractor != null)
        {
            rayInteractor.enabled = enabled;
        }
    }
    
    // Method to change ray color based on interaction state
    public void SetRayColor(Color color, bool isValid = true)
    {
        if (lineVisual != null)
        {
            if (isValid)
            {
                lineVisual.validColorGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                );
            }
            else
            {
                lineVisual.invalidColorGradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                );
            }
        }
    }
}

