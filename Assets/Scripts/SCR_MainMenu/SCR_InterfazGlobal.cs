using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_InterfazGlobal : MonoBehaviour
{
    public static SCR_InterfazGlobal Instancia;

    [Header("Configuración de Visibilidad")]
    [SerializeField] private GameObject contenedorUI;
    [SerializeField] private string nombreEscenaMenu = "Menu";
    [SerializeField] private string nombreEscenaVideo = "SCN_Cinematica";

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
        ComprobarVisibilidad(SceneManager.GetActiveScene());
    }

    private void OnEnable() { SceneManager.sceneLoaded += AlCambiarEscena; }
    private void OnDisable() { SceneManager.sceneLoaded -= AlCambiarEscena; }

    private void AlCambiarEscena(Scene escena, LoadSceneMode modo)
    {
        ComprobarVisibilidad(escena);
    }

    private void ComprobarVisibilidad(Scene escena)
    {
        bool debeMostrarse = (escena.name != nombreEscenaMenu && escena.name != nombreEscenaVideo);

        if (contenedorUI != null)
        {
            contenedorUI.SetActive(debeMostrarse);
        }
    }
}