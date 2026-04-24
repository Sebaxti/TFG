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

    // --- FUNCIÓN PARA CAMBIAR DE NIVEL (Meta, Puerta, Cinematica) ---
    public void CambiarEscena(string nombreNuevaEscena)
    {
        StartCoroutine(RutinaCambioEscena(nombreNuevaEscena));
    }

    private IEnumerator RutinaCambioEscena(string nombre)
    {
        // 1. Bloqueamos clics y vamos a negro
        yield return StartCoroutine(RutinaFade(1f));
        // 2. Cargamos la escena
        SceneManager.LoadScene(nombre);
        // (El Fade In se activa solo al detectar que la escena cargó)
    }

    // --- LÓGICA DE MUERTE (YA EXISTENTE) ---
    public void ProcesarMuerte(SCR_Movimiento jugador)
    {
        StartCoroutine(RutinaMuerte(jugador));
    }

    private IEnumerator RutinaMuerte(SCR_Movimiento jugador)
    {
        yield return StartCoroutine(RutinaFade(1f));
        yield return new WaitForSeconds(0.5f);

        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual == nombreEscenaJefe) SceneManager.LoadScene(escenaActual);
        else
        {
            if (jugador != null)
            {
                jugador.EjecutarTeletransporte();
                yield return StartCoroutine(RutinaFade(0f));
                jugador.DesbloquearMovimiento();
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