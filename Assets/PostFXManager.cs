using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PostFXManager : MonoBehaviour
{
    public static PostFXManager Instance { get; private set; }

    [Header("Volume Reference")]
    [SerializeField] private Volume postProcessVolume;

    private FilmGrain filmGrain;
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private Camera currentCamera;

    private const float MaxLight = 100f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out filmGrain);
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out colorAdjustments);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start clean (full light = 100)
        UpdateEffects(MaxLight);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignCamera();
    }

    private void AssignCamera()
    {
        currentCamera = Camera.main;
        if (currentCamera != null)
        {
            var camData = currentCamera.GetUniversalAdditionalCameraData();
            if (camData != null)
                camData.renderPostProcessing = true;
        }
    }

    /// <summary>
    /// Update screen FX based on current Light (0–100).
    /// </summary>
    public void UpdateEffects(float currentLight)
    {
        float t = 1f - Mathf.Clamp01(currentLight / MaxLight);

        if (filmGrain != null)
            filmGrain.intensity.Override(Mathf.Lerp(0f, 1f, t));

        if (vignette != null)
            vignette.intensity.Override(Mathf.Lerp(0f, 0.35f, t));

        if (colorAdjustments != null)
            colorAdjustments.saturation.Override(Mathf.Lerp(0f, -50f, t));
    }
}
