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
        if (SCR_GestorNiveles.Instancia == null) { FinalizarVideo(); return; }

        VideoClip clipActual = SCR_GestorNiveles.Instancia.ObtenerDatosNivelActual().cinematicaPrevia;

        if (clipActual != null)
        {
            vPlayer.clip = clipActual;
            vPlayer.Play();
        }
        else
        {
            FinalizarVideo();
        }
    }

    void OnEnable() { vPlayer.loopPointReached += AlTerminarVideo; }
    void OnDisable() { vPlayer.loopPointReached -= AlTerminarVideo; }

    void Update()
    {
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
        if (SCR_GestorNiveles.Instancia != null)
            SCR_GestorNiveles.Instancia.CargarNivelDespuesDeVideo();
    }
}