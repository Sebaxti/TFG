using UnityEngine;
using UnityEngine.Audio;

public class SCR_GestorAudio : MonoBehaviour
{
    public static SCR_GestorAudio Instancia;

    [Header("Referencias")]
    [SerializeField] private AudioMixer mixerPrincipal;

    [Header("Fuentes de Audio (opcionales - se crean automaticamente si estan vacias)")]
    [SerializeField] private AudioSource fuenteSFX;
    [SerializeField] private AudioSource fuenteMusica;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
            AsegurarFuentesAudio();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void AsegurarFuentesAudio()
    {
        if (fuenteSFX == null)
        {
            fuenteSFX = gameObject.AddComponent<AudioSource>();
            fuenteSFX.playOnAwake = false;
        }
        if (fuenteMusica == null)
        {
            fuenteMusica = gameObject.AddComponent<AudioSource>();
            fuenteMusica.playOnAwake = false;
            fuenteMusica.loop = true;
        }
    }

    private void Start()
    {
        SetVolumenMaster(PlayerPrefs.GetFloat("VolumenMaster", 0.75f));
        SetVolumenMusica(PlayerPrefs.GetFloat("VolumenMusica", 0.75f));
        SetVolumenSFX(PlayerPrefs.GetFloat("VolumenSFX", 0.75f));
    }

    public void ReproducirSFX(AudioClip clip)
    {
        if (clip == null || fuenteSFX == null) return;
        fuenteSFX.PlayOneShot(clip);
    }

    public void CambiarMusica(AudioClip clip)
    {
        if (fuenteMusica == null || clip == null) return;
        if (fuenteMusica.clip == clip && fuenteMusica.isPlaying) return;
        fuenteMusica.clip = clip;
        fuenteMusica.loop = true;
        fuenteMusica.Play();
    }

    public void PararMusica()
    {
        if (fuenteMusica != null) fuenteMusica.Stop();
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
