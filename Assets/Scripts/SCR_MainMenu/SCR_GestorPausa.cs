using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_GestionPausa : MonoBehaviour
{
    public static SCR_GestionPausa Instancia;

    [Header("Referencias UI")]
    [SerializeField] private GameObject canvasPausaRaiz; // El Canvas completo
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelOpciones;

    private bool estaPausado = false;

    private void Awake()
    {
        // Configuramos el Singleton para que persista
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject); // Salva al GestorPausa y a su Canvas hijo
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Nos aseguramos de que empiece apagado
        if (canvasPausaRaiz != null) canvasPausaRaiz.SetActive(false);
    }

    void Update()
    {
        // Detectar tecla de pausa (Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Evitamos que el jugador pause estando en el Menú Principal (Escena 0)
            if (SceneManager.GetActiveScene().buildIndex == 0) return;

            if (estaPausado) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        estaPausado = true;
        canvasPausaRaiz.SetActive(true);
        MostrarPanelPrincipal();

        // Magia de Unity: Detiene todas las físicas y Update (si usan deltaTime)
        Time.timeScale = 0f;
    }

    public void Reanudar()
    {
        estaPausado = false;
        canvasPausaRaiz.SetActive(false);

        // Devolvemos el tiempo a la normalidad
        Time.timeScale = 1f;
    }

    // --- NAVEGACIÓN DENTRO DE LA PAUSA ---

    public void MostrarPanelPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelOpciones.SetActive(false);
    }

    public void AbrirOpciones()
    {
        panelPrincipal.SetActive(false);
        panelOpciones.SetActive(true);
    }

    // --- ACCIONES DE SALIDA ---

    public void SalirAlMenuPrincipal()
    {
        // ¡SÚPER IMPORTANTE! Quitar la pausa antes de cambiar de escena
        estaPausado = false;
        Time.timeScale = 1f;
        canvasPausaRaiz.SetActive(false);

        // Asumimos que el Menú Principal es el buildIndex 0
        SceneManager.LoadScene(0);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego desde la pausa...");
        Application.Quit();
    }
}