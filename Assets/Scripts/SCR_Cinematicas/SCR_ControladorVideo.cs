using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement; 

public class SCR_ControladorVideo : MonoBehaviour
{
    private VideoPlayer vPlayer;
    [SerializeField] private string nombreEscenaJuego = "Nivel1";

    void Awake()
    {
        vPlayer = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        // Suscribimos una función al evento "loopPointReached" (cuando el video llega al final)
        vPlayer.loopPointReached += AlTerminarVideo;
    }

    void OnDisable()
    {
        //desuscribirse al desactivar el objeto
        vPlayer.loopPointReached -= AlTerminarVideo;
    }

    void Update()
    {
        // Si el jugador pulsa Espacio o Escape, saltamos el video
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
        {
            CargarSiguienteEscena();
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        CargarSiguienteEscena();
    }

    void CargarSiguienteEscena()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }
}