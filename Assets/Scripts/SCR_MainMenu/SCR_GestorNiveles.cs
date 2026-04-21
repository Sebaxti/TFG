using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_GestorNiveles : MonoBehaviour
{
    public static SCR_GestorNiveles Instancia;

    [System.Serializable]
    public struct DatosNivel
    {
        public string nombreNivelUI;
        public string nombreEscenaUnity;
    }

    [Header("Configuración de Niveles")]
    public DatosNivel[] listaDeNiveles;
    [SerializeField] private string nombreEscenaCinematica = "SCN_Cinematica";

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
        }
    }

    // --- PARA EL MENU MANAGER ---
    public int ObtenerNivelMaximo() => PlayerPrefs.GetInt("NivelMaximoDesbloqueado", 0);

    public void NuevaPartida()
    {
        PlayerPrefs.SetInt("NivelActual", 0); // Empezamos desde el primer video (Intro)
        SceneManager.LoadScene(nombreEscenaCinematica);
    }

    public void CargarNivelPorIndice(int indice)
    {
        if (indice >= 0 && indice < listaDeNiveles.Length)
        {
            // Cuando eliges nivel desde el menú, vas directo al juego sin video
            SceneManager.LoadScene(listaDeNiveles[indice].nombreEscenaUnity);
        }
    }

    // --- PARA LA META ---
    public void AvanzarDesdeNivel(int indiceActual)
    {
        // 1. Desbloqueamos el siguiente nivel para el menú
        int siguiente = indiceActual + 1;
        if (siguiente > ObtenerNivelMaximo() && siguiente < listaDeNiveles.Length)
        {
            PlayerPrefs.SetInt("NivelMaximoDesbloqueado", siguiente);
        }

        // 2. Guardamos que el nivel que toca ahora (el video que vamos a ver) es el 'siguiente'
        PlayerPrefs.SetInt("NivelActual", siguiente);
        PlayerPrefs.Save();

        // 3. Vamos a la escena de cinemática
        SceneManager.LoadScene(nombreEscenaCinematica);
    }

    // Función extra para que el video sepa qué escena cargar después
    public string GetEscenaDeNivelActual()
    {
        int index = PlayerPrefs.GetInt("NivelActual", 0);
        return (index < listaDeNiveles.Length) ? listaDeNiveles[index].nombreEscenaUnity : listaDeNiveles[0].nombreEscenaUnity;
    }
}