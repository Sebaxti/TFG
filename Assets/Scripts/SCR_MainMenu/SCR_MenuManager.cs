using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI; 

public partial class SCR_MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelOpciones;

    [Header("Configuración Sonido")]
    [SerializeField] private Slider sliderVolumen;

    void Start()
    {
        panelPrincipal.SetActive(true);
        panelOpciones.SetActive(false);

        // Carga el volumen guardado (si existe)
        float volGuardado = PlayerPrefs.GetFloat("Volumen", 0.5f);
        sliderVolumen.value = volGuardado;
        AudioListener.volume = volGuardado;
    }

    // --- FUNCIONES DE BOTONES ---

    public void IniciarDemo(string nombreNivel)
    {
        SceneManager.LoadScene(nombreNivel);
    }

    public void AbrirOpciones()
    {
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        panelPrincipal.SetActive(true);
        panelOpciones.SetActive(false);
    }

    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("Volumen", valor); // Guarda el ajuste para la próxima vez
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo");
        Application.Quit(); //cierra el .exe
    }
}