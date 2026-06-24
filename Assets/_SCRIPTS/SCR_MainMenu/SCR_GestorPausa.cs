using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_GestorPausa : MonoBehaviour
{
    public static SCR_GestorPausa Instancia;

    [Header("Referencias UI")]
    [SerializeField] private GameObject canvasPausaRaiz;
    [SerializeField] private GameObject panelPrincipalPausa;
    [SerializeField] private GameObject panelOpcionesPausa;

    [Header("Nombres de Escena (Bloqueo de Pausa)")]
    [SerializeField] private string nombreEscenaMenu = "SCN_Menu";
    [SerializeField] private string nombreEscenaVideo = "SCN_Cinematica";

    private bool estaPausado = false;

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

        if (canvasPausaRaiz != null) canvasPausaRaiz.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            string escenaActual = SceneManager.GetActiveScene().name;

            // Evitar pausar en el menú principal o en cinemáticas
            if (escenaActual == nombreEscenaMenu || escenaActual == nombreEscenaVideo)
            {
                return;
            }

            if (estaPausado) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        estaPausado = true;
        Time.timeScale = 0f;
        if (canvasPausaRaiz != null) canvasPausaRaiz.SetActive(true);
        MostrarPanelPrincipal();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Reanudar()
    {
        estaPausado = false;
        Time.timeScale = 1f;
        if (canvasPausaRaiz != null) canvasPausaRaiz.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    // ==========================================
    // FUNCIONES PARA LOS BOTONES DEL CANVAS
    // ==========================================
    public void MostrarPanelPrincipal()
    {
        if (panelPrincipalPausa != null) panelPrincipalPausa.SetActive(true);
        if (panelOpcionesPausa != null) panelOpcionesPausa.SetActive(false);
    }

    public void AbrirOpciones()
    {
        if (panelPrincipalPausa != null) panelPrincipalPausa.SetActive(false);
        if (panelOpcionesPausa != null) panelOpcionesPausa.SetActive(true);
    }

    public void CerrarOpciones()
    {
        MostrarPanelPrincipal();
    }

    public void VolverMenuPrincipal()
    {
        Time.timeScale = 1f;
        estaPausado = false;

        if (canvasPausaRaiz != null) canvasPausaRaiz.SetActive(false);

        if (SCR_GestorEscena.Instancia != null)
        {
            SCR_GestorEscena.Instancia.CargarEscenaConFade(nombreEscenaMenu);
        }
        else
        {
            SceneManager.LoadScene(nombreEscenaMenu);
        }
    }

    public void SalirDelJuego()
    {
               Time.timeScale = 1f;

        Debug.Log("¡Cerrando el juego!");

        Application.Quit();
    }
}