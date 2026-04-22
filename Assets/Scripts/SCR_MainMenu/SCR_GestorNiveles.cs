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
            SceneManager.LoadScene(listaDeNiveles[indice].nombreEscenaUnity);
        }
    }

    public void AvanzarDesdeNivel(int indiceActual)
    {
        int siguiente = indiceActual + 1;
        if (siguiente > ObtenerNivelMaximo() && siguiente < listaDeNiveles.Length)
        {
            PlayerPrefs.SetInt("NivelMaximoDesbloqueado", siguiente);
        }

        PlayerPrefs.SetInt("NivelActual", siguiente);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nombreEscenaCinematica);
    }

    public string GetEscenaDeNivelActual()
    {
        int index = PlayerPrefs.GetInt("NivelActual", 0);
        return (index < listaDeNiveles.Length) ? listaDeNiveles[index].nombreEscenaUnity : listaDeNiveles[0].nombreEscenaUnity;
    }
}