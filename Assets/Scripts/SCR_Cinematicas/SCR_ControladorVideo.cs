using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SCR_ControladorVideo : MonoBehaviour
{
    private VideoPlayer vPlayer;

    [Header("Base de Datos de Videos")]
    [SerializeField] private VideoClip[] clipsCinematicas;

    void Awake()
    {
        vPlayer = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        int indiceNivelActual = PlayerPrefs.GetInt("NivelActual", 0);
        Debug.Log("CONTROLADOR VIDEO: Iniciando video para el índice de nivel: " + indiceNivelActual);

        if (indiceNivelActual < clipsCinematicas.Length)
        {
            vPlayer.clip = clipsCinematicas[indiceNivelActual];
            vPlayer.Play();
            Debug.Log("CONTROLADOR VIDEO: Reproduciendo clip: " + vPlayer.clip.name);
        }
        else
        {
            Debug.LogWarning("CONTROLADOR VIDEO: No hay video para el índice " + indiceNivelActual + ". Saltando al nivel.");
            CargarSiguienteEscena();
        }
    }

    void OnEnable() { vPlayer.loopPointReached += AlTerminarVideo; }
    void OnDisable() { vPlayer.loopPointReached -= AlTerminarVideo; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("CONTROLADOR VIDEO: Salto de video manual detectado.");
            CargarSiguienteEscena();
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        Debug.Log("CONTROLADOR VIDEO: El video ha terminado de forma natural.");
        CargarSiguienteEscena();
    }

    void CargarSiguienteEscena()
    {
        if (SCR_GestorNiveles.Instancia == null)
        {
            Debug.LogError("ERROR CRÍTICO: ¡No se encuentra el SCR_GestorNiveles en la escena! ¿Has empezado el juego desde la escena del MENU?");
            return;
        }

        string nombreEscena = SCR_GestorNiveles.Instancia.GetEscenaDeNivelActual();
        Debug.Log("CONTROLADOR VIDEO: Intentando cargar la escena: " + nombreEscena);

        if (!string.IsNullOrEmpty(nombreEscena))
        {
            SceneManager.LoadScene(nombreEscena);
        }
        else
        {
            Debug.LogError("ERROR: El nombre de la escena devuelto por el Gestor está vacío.");
        }
    }
}