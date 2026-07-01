using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SCR_GestorNiveles : MonoBehaviour
{
    public static SCR_GestorNiveles Instancia;

    [System.Serializable]
    public struct DatosNivel
    {
        public string nombreNivelUI;
        public string nombreEscenaUnity;
        public bool esNivelJefe;
        public VideoClip cinematicaPrevia;
    }

    [Header("Base de Datos de Niveles")]
    public DatosNivel[] listaDeNiveles;

    [Header("Escenas del Sistema")]
    public string escenaMenuPrincipal = "MainMenu";
    public string escenaReproductorVideo = "SCN_Cinematica";

    private void Awake()
    {
        if (Instancia == null) { Instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public int ObtenerNivelMaximo()
    {
        return PlayerPrefs.GetInt("NivelMaximoDesbloqueado", 0);
    }

    public void CargarNivelPorIndice(int indice)
    {
        PlayerPrefs.SetInt("NivelActual", indice);
        PlayerPrefs.Save();
        ProcesarCargaNivel(indice);
    }

    public void AvanzarDesdeNivel(int indiceActual)
    {
        int siguiente = indiceActual + 1;

        int maxDesbloqueado = ObtenerNivelMaximo();
        if (siguiente > maxDesbloqueado && siguiente < listaDeNiveles.Length)
        {
            PlayerPrefs.SetInt("NivelMaximoDesbloqueado", siguiente);
        }

        PlayerPrefs.SetInt("NivelActual", siguiente);
        PlayerPrefs.Save();

        if (siguiente < listaDeNiveles.Length)
        {
            ProcesarCargaNivel(siguiente);
        }
        else
        {
            if (SCR_GestorEscena.Instancia != null)
                SCR_GestorEscena.Instancia.CargarEscenaConFade(escenaMenuPrincipal);
        }
    }

    public string GetEscenaDeNivelActual()
    {
        return ObtenerDatosNivelActual().nombreEscenaUnity;
    }

    public DatosNivel ObtenerDatosNivelActual()
    {
        int indice = PlayerPrefs.GetInt("NivelActual", 0);
        if (indice < listaDeNiveles.Length) return listaDeNiveles[indice];
        return new DatosNivel();
    }

    public void NuevaPartida()
    {
        PlayerPrefs.SetInt("NivelActual", 0);
        PlayerPrefs.Save();
        ProcesarCargaNivel(0);
    }

    public void AvanzarSiguienteNivel()
    {
        int nivelActual = PlayerPrefs.GetInt("NivelActual", 0);
        int siguiente = nivelActual + 1;

        int maxDesbloqueado = ObtenerNivelMaximo();
        if (siguiente > maxDesbloqueado && siguiente < listaDeNiveles.Length)
        {
            PlayerPrefs.SetInt("NivelMaximoDesbloqueado", siguiente);
        }

        PlayerPrefs.SetInt("NivelActual", siguiente);
        PlayerPrefs.Save();

        if (siguiente < listaDeNiveles.Length)
        {
            ProcesarCargaNivel(siguiente);
        }
        else
        {
            if (SCR_GestorEscena.Instancia != null)
                SCR_GestorEscena.Instancia.CargarEscenaConFade(escenaMenuPrincipal);
        }
    }

    public void ProcesarCargaNivel(int indice)
    {
        if (indice >= listaDeNiveles.Length) return;

        DatosNivel datos = listaDeNiveles[indice];

        if (datos.cinematicaPrevia != null)
        {
            if (SCR_GestorEscena.Instancia != null)
                SCR_GestorEscena.Instancia.CargarEscenaConFade(escenaReproductorVideo);
        }
        else
        {
            if (SCR_GestorEscena.Instancia != null)
                SCR_GestorEscena.Instancia.CargarEscenaConFade(datos.nombreEscenaUnity);
        }
    }

    public void CargarNivelDespuesDeVideo()
    {
        DatosNivel datos = ObtenerDatosNivelActual();
        if (SCR_GestorEscena.Instancia != null)
            SCR_GestorEscena.Instancia.CargarEscenaConFade(datos.nombreEscenaUnity);
    }
}