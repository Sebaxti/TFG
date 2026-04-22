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
        canvasPausaRaiz.SetActive(true);
        MostrarPanelPrincipal();
    }

    public void Reanudar()
    {
        estaPausado = false;
        Time.timeScale = 1f;
        canvasPausaRaiz.SetActive(false);
    }

    public void MostrarPanelPrincipal()
    {
        panelPrincipalPausa.SetActive(true);
        panelOpcionesPausa.SetActive(false);
    }

    public void AbrirOpciones()
    {
        panelPrincipalPausa.SetActive(false);
        panelOpcionesPausa.SetActive(true);
    }

    public void SalirAlMenuPrincipal()
    {
        Reanudar();
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void SalirAlEscritorio()
    {
        Debug.Log("Cerrando aplicación...");
        Application.Quit();
    }
}