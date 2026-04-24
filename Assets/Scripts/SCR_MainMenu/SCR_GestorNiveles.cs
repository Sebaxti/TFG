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

    public int ObtenerNivelMaximo() => PlayerPrefs.GetInt("NivelMaximoDesbloqueado", 0);

    public void NuevaPartida()
    {
        PlayerPrefs.SetInt("NivelActual", 0);
        SceneManager.LoadScene(nombreEscenaCinematica);
    }

    public void CargarNivelPorIndice(int indice)
    {
        if (indice >= 0 && indice < listaDeNiveles.Length)
        {
            string nombreEscena = listaDeNiveles[indice].nombreEscenaUnity;

            // CAMBIO: En lugar de SceneManager, usamos nuestro Gestor de Escena para el Fade
            if (SCR_GestorEscena.Instancia != null)
                SCR_GestorEscena.Instancia.CambiarEscena(nombreEscena);
            else
                SceneManager.LoadScene(nombreEscena);
        }
    }

    public void AvanzarDesdeNivel(int indiceActual)
    {
        int siguiente = indiceActual + 1;

        // Guardamos progreso
        if (siguiente > ObtenerNivelMaximo() && siguiente < listaDeNiveles.Length)
        {
            PlayerPrefs.SetInt("NivelMaximoDesbloqueado", siguiente);
        }

        PlayerPrefs.SetInt("NivelActual", siguiente);
        PlayerPrefs.Save();

        // Si hay un nivel siguiente en la lista, lo cargamos con Fade
        if (siguiente < listaDeNiveles.Length)
        {
            CargarNivelPorIndice(siguiente);
        }
        else
        {
            Debug.Log("¡Juego completado! Volviendo al menú...");
            if (SCR_GestorEscena.Instancia != null)
                SCR_GestorEscena.Instancia.CambiarEscena(nombreEscenaCinematica);
        }
    }

    public string GetEscenaDeNivelActual()
    {
        int index = PlayerPrefs.GetInt("NivelActual", 0);
        return (index < listaDeNiveles.Length) ? listaDeNiveles[index].nombreEscenaUnity : listaDeNiveles[0].nombreEscenaUnity;
    }
}