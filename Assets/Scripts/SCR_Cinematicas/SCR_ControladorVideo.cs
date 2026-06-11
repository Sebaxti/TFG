using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class SCR_ControladorVideo : MonoBehaviour
{
    private VideoPlayer vPlayer;
    private bool saltando = false;

    void Awake()
    {
        vPlayer = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        // 1. Conseguimos el vídeo que toca desde la base de datos centralizada
        VideoClip clipActual = SCR_GestorNiveles.Instancia.ObtenerDatosNivelActual().cinematicaPrevia;

        if (clipActual != null)
        {
            vPlayer.clip = clipActual;
            vPlayer.Play();
        }
        else
        {
            // Salvavidas por si se carga la escena sin vídeo asignado
            FinalizarVideo();
        }
    }

    void OnEnable() { vPlayer.loopPointReached += AlTerminarVideo; }
    void OnDisable() { vPlayer.loopPointReached -= AlTerminarVideo; }

    void Update()
    {
        // Permitir saltar la cinemática
        if (Input.GetKeyDown(KeyCode.Space) && !saltando)
        {
            saltando = true;
            FinalizarVideo();
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        if (!saltando)
        {
            saltando = true;
            FinalizarVideo();
        }
    }

    void FinalizarVideo()
    {
        // Le devolvemos el control al Gestor de Niveles para que cargue la fase
        SCR_GestorNiveles.Instancia.CargarNivelDespuesDeVideo();
    }
}