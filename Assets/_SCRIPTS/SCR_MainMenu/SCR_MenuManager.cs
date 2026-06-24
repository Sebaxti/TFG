using UnityEngine;
using UnityEngine.UI;

public class SCR_MenuManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject panelSeleccionNivel; 

    [Header("Configuración Sonido")]
    [SerializeField] private Slider sliderVolumen;

    [Header("Selección de Niveles")]
    [SerializeField] private Button[] botonesDeNiveles; 

    void Start()
    {
        MostrarPanelPrincipal();

        float volGuardado = PlayerPrefs.GetFloat("Volumen", 0.5f);
        sliderVolumen.value = volGuardado;
        AudioListener.volume = volGuardado;
    }

    public void MostrarPanelPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelOpciones.SetActive(false);
        if (panelSeleccionNivel) panelSeleccionNivel.SetActive(false);
    }

    public void BotonNuevaPartida()
    {
        if (SCR_GestorMonedas.Instancia != null)
            SCR_GestorMonedas.Instancia.ResetearMonedas();

        SCR_GestorNiveles.Instancia.NuevaPartida();
    }

    public void AbrirOpciones()
    {
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(true);
    }

    public void AbrirSeleccionNivel()
    {
        panelPrincipal.SetActive(false);
        panelSeleccionNivel.SetActive(true);

        int nivelMaximo = SCR_GestorNiveles.Instancia.ObtenerNivelMaximo();

        for (int i = 0; i < botonesDeNiveles.Length; i++)
        {
            botonesDeNiveles[i].interactable = (i <= nivelMaximo);

        }
    }

    public void BotonJugarNivel(int indice)
    {
        SCR_GestorNiveles.Instancia.CargarNivelPorIndice(indice);
    }

    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("Volumen", valor);
    }

    public void Salir()
    {
        Application.Quit();
    }
}