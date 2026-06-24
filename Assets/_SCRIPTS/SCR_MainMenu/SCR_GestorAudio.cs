using UnityEngine;
using UnityEngine.Audio;

public class SCR_GestorAudio : MonoBehaviour
{
    public static SCR_GestorAudio Instancia;

    [Header("Referencias")]
    [SerializeField] private AudioMixer mixerPrincipal;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Al arrancar, cargamos los volúmenes guardados (o 0.75 por defecto)
        SetVolumenMaster(PlayerPrefs.GetFloat("VolumenMaster", 0.75f));
        SetVolumenMusica(PlayerPrefs.GetFloat("VolumenMusica", 0.75f));
        SetVolumenSFX(PlayerPrefs.GetFloat("VolumenSFX", 0.75f));
    }

    // --- FUNCIONES PARA LOS SLIDERS ---

    public void SetVolumenMaster(float valorSlider)
    {
        float valorReal = Mathf.Clamp(valorSlider, 0.0001f, 1f);
        mixerPrincipal.SetFloat("MasterVol", Mathf.Log10(valorReal) * 20f);
        PlayerPrefs.SetFloat("VolumenMaster", valorSlider);
    }

    public void SetVolumenMusica(float valorSlider)
    {
        float valorReal = Mathf.Clamp(valorSlider, 0.0001f, 1f);
        mixerPrincipal.SetFloat("MusicVol", Mathf.Log10(valorReal) * 20f);
        PlayerPrefs.SetFloat("VolumenMusica", valorSlider);
    }

    public void SetVolumenSFX(float valorSlider)
    {
        float valorReal = Mathf.Clamp(valorSlider, 0.0001f, 1f);
        mixerPrincipal.SetFloat("SFXVol", Mathf.Log10(valorReal) * 20f);
        PlayerPrefs.SetFloat("VolumenSFX", valorSlider);
    }
}