using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple script that connects UI sliders to MarbleGame properties.
/// Attach to your DebugPanelContainer GameObject.
/// </summary>
public class SimpleMarbleControls : MonoBehaviour
{
    [SerializeField] private MarbleGame marbleGame;
    
    [Header("Sliders")]
    [SerializeField] private Slider sizeSlider;
    [SerializeField] private Slider massSlider;
    [SerializeField] private Slider dynamicFrictionSlider;
    [SerializeField] private Slider staticFrictionSlider;
    [SerializeField] private Slider forceMultiplierSlider;
    
    [Header("Value Text Fields (Optional)")]
    [SerializeField] private TMP_Text sizeValueText;
    [SerializeField] private TMP_Text massValueText;
    [SerializeField] private TMP_Text dynamicFrictionValueText;
    [SerializeField] private TMP_Text staticFrictionValueText;
    [SerializeField] private TMP_Text forceMultiplierValueText;
    
    void Start()
    {
        // Find MarbleGame if not assigned
        if (marbleGame == null)
            marbleGame = FindObjectOfType<MarbleGame>();
            
        if (marbleGame == null)
        {
            Debug.LogError("MarbleGame not found!");
            return;
        }
        
        // Initialize slider values
        InitializeSliders();
        
        // Add listeners
        AddSliderListeners();
    }
    
    void InitializeSliders()
    {
        // Set initial values from MarbleGame
        if (sizeSlider != null)
        {
            sizeSlider.value = marbleGame.marbleSize;
            UpdateValueText(sizeValueText, marbleGame.marbleSize, "F2");
        }
        
        if (massSlider != null)
        {
            massSlider.value = marbleGame.marbleMass;
            UpdateValueText(massValueText, marbleGame.marbleMass, "F1");
        }
        
        if (dynamicFrictionSlider != null)
        {
            dynamicFrictionSlider.value = marbleGame.dynamicFriction;
            UpdateValueText(dynamicFrictionValueText, marbleGame.dynamicFriction, "F2");
        }
        
        if (staticFrictionSlider != null)
        {
            staticFrictionSlider.value = marbleGame.staticFriction;
            UpdateValueText(staticFrictionValueText, marbleGame.staticFriction, "F2");
        }
        
        if (forceMultiplierSlider != null)
        {
            forceMultiplierSlider.value = marbleGame.forceMultiplier;
            UpdateValueText(forceMultiplierValueText, marbleGame.forceMultiplier, "F1");
        }
    }
    
    void AddSliderListeners()
    {
        // Size slider
        if (sizeSlider != null)
            sizeSlider.onValueChanged.AddListener(OnSizeChanged);
            
        // Mass slider
        if (massSlider != null)
            massSlider.onValueChanged.AddListener(OnMassChanged);
            
        // Dynamic Friction slider
        if (dynamicFrictionSlider != null)
            dynamicFrictionSlider.onValueChanged.AddListener(OnDynamicFrictionChanged);
            
        // Static Friction slider
        if (staticFrictionSlider != null)
            staticFrictionSlider.onValueChanged.AddListener(OnStaticFrictionChanged);
            
        // Force Multiplier slider
        if (forceMultiplierSlider != null)
            forceMultiplierSlider.onValueChanged.AddListener(OnForceMultiplierChanged);
    }
    
    // Update value text helper
    void UpdateValueText(TMP_Text textField, float value, string format)
    {
        if (textField != null)
            textField.text = value.ToString(format);
    }
    
    // Value change handlers
    void OnSizeChanged(float value)
    {
        marbleGame.marbleSize = value;
        UpdateValueText(sizeValueText, value, "F2");
        marbleGame.UpdateExistingMarbleProperties();
    }
    
    void OnMassChanged(float value)
    {
        marbleGame.marbleMass = value;
        UpdateValueText(massValueText, value, "F1");
        marbleGame.UpdateExistingMarbleProperties();
    }
    
    void OnDynamicFrictionChanged(float value)
    {
        marbleGame.dynamicFriction = value;
        UpdateValueText(dynamicFrictionValueText, value, "F2");
        marbleGame.UpdateExistingMarbleProperties();
    }
    
    void OnStaticFrictionChanged(float value)
    {
        marbleGame.staticFriction = value;
        UpdateValueText(staticFrictionValueText, value, "F2");
        marbleGame.UpdateExistingMarbleProperties();
    }
    
    void OnForceMultiplierChanged(float value)
    {
        marbleGame.forceMultiplier = value;
        UpdateValueText(forceMultiplierValueText, value, "F1");
        marbleGame.UpdateExistingMarbleProperties();
    }
}