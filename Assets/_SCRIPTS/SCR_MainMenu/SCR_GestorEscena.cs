using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SCR_GestorEscena : MonoBehaviour
{
    public static SCR_GestorEscena Instancia;

    [Header("Configuración de Escenas")]
    public string nombreEscenaMenu = "MainMenu";
    public string nombreEscenaJefe = "Nivel_3_Jefe";

    [Header("Efecto de Fundido (Fade)")]
    [SerializeField] private Image imagenFundido;
    [SerializeField] private float velocidadFade = 1.5f;

    private void Awake()
    {
        if (Instancia == null) { Instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    private void OnEnable() { SceneManager.sceneLoaded += AlCargarEscena; }
    private void OnDisable() { SceneManager.sceneLoaded -= AlCargarEscena; }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        StopAllCoroutines();
        if (escena.name == nombreEscenaMenu) FijarOpacidad(0f);
        else StartCoroutine(RutinaFade(0f));
    }

    public void CambiarEscena(string nombreNuevaEscena)
    {
        StartCoroutine(RutinaCambioEscena(nombreNuevaEscena));
    }

    public void CargarEscenaConFade(string nombreNuevaEscena)
    {
        CambiarEscena(nombreNuevaEscena);
    }

    private IEnumerator RutinaCambioEscena(string nombre)
    {
        yield return StartCoroutine(RutinaFade(1f));
        SceneManager.LoadScene(nombre);
    }

    // AHORA SOLICITA EL SCRIPT DE RESPAWN ESPECIALIZADO
    public void ProcesarMuerte(SCR_RespawnJugador respawnJugador)
    {
        StartCoroutine(RutinaMuerte(respawnJugador));
    }

    private IEnumerator RutinaMuerte(SCR_RespawnJugador respawnJugador)
    {
        yield return StartCoroutine(RutinaFade(1f));
        yield return new WaitForSeconds(0.5f);

        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual == nombreEscenaJefe)
        {
            SceneManager.LoadScene(escenaActual);
        }
        else
        {
            if (respawnJugador != null)
            {
                respawnJugador.EjecutarTeletransporte();
                yield return StartCoroutine(RutinaFade(0f));
                respawnJugador.FinalizarRespawn();
            }
        }
    }

    private void FijarOpacidad(float valor)
    {
        if (imagenFundido != null)
        {
            Color c = imagenFundido.color; c.a = valor; imagenFundido.color = c;
            imagenFundido.raycastTarget = (valor > 0.1f);
        }
    }

    private IEnumerator RutinaFade(float opacidadObjetivo)
    {
        if (imagenFundido == null) yield break;
        imagenFundido.raycastTarget = (opacidadObjetivo > 0.1f);
        Color color = imagenFundido.color;
        while (Mathf.Abs(color.a - opacidadObjetivo) > 0.01f)
        {
            color.a = Mathf.MoveTowards(color.a, opacidadObjetivo, velocidadFade * Time.deltaTime);
            imagenFundido.color = color;
            yield return null;
        }
        FijarOpacidad(opacidadObjetivo);
    }
}