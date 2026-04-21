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
    [SerializeField] private Button[] botonesDeNiveles; // Arrastra botones en orden

    void Start()
    {
        MostrarPanelPrincipal();

        float volGuardado = PlayerPrefs.GetFloat("Volumen", 0.5f);
        sliderVolumen.value = volGuardado;
        AudioListener.volume = volGuardado;
    }

    // --- MÉTODOS DE NAVEGACIÓN DE UI ---

    public void MostrarPanelPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelOpciones.SetActive(false);
        if (panelSeleccionNivel) panelSeleccionNivel.SetActive(false);
    }

    public void BotonNuevaPartida()
    {
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

        // Recorremos todos los botones que pusiste en el array
        for (int i = 0; i < botonesDeNiveles.Length; i++)
        {
            // Si el índice del botón es menor o igual al nivel máximo, se puede clicar
            botonesDeNiveles[i].interactable = (i <= nivelMaximo);

            // Opcional: Puedes cambiarle el color o la opacidad para que se vea "bloqueado"
        }
    }

    // --- ACCIONES DE JUEGO ---

    public void BotonJugarNivel(int indice)
    {
        // Le pide al cerebro (Gestor) que cargue el nivel
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