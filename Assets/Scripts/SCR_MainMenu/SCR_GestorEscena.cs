using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SCR_GestorEscena : MonoBehaviour
{
    public static SCR_GestorEscena Instancia;

    [Header("Efecto de Fundido")]
    [SerializeField] private Image imagenFundido;
    [SerializeField] private float velocidadFade = 1.5f;

    private void Awake()
    {
        if (Instancia == null) { Instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        FijarOpacidad(0f);
    }

    private void OnEnable() { SceneManager.sceneLoaded += AlEntrarNuevaEscena; }
    private void OnDisable() { SceneManager.sceneLoaded -= AlEntrarNuevaEscena; }

    // Unity llama a esto automáticamente en cuanto una nueva escena está lista
    private void AlEntrarNuevaEscena(Scene escena, LoadSceneMode modo)
    {
        StopAllCoroutines();
        StartCoroutine(RutinaFade(0f)); // Fade In (aparecer)
    }

    // Función pública universal para viajar a cualquier sitio
    public void CargarEscenaConFade(string nombreEscenaDestino)
    {
        StartCoroutine(RutinaCambio(nombreEscenaDestino));
    }

    // La lógica universal de muerte, sin nombres quemados en el código
    public void ProcesarMuerte(SCR_Movimiento jugador)
    {
        StartCoroutine(RutinaMuerte(jugador));
    }

    private IEnumerator RutinaCambio(string destino)
    {
        yield return StartCoroutine(RutinaFade(1f)); // Fundido a negro
        SceneManager.LoadScene(destino);             // Carga oculta por el negro
    }

    private IEnumerator RutinaMuerte(SCR_Movimiento jugador)
    {
        yield return StartCoroutine(RutinaFade(1f));
        yield return new WaitForSeconds(0.5f);

        // Preguntamos al cerebro (GestorNiveles) qué tipo de nivel es este
        bool esJefe = SCR_GestorNiveles.Instancia.ObtenerDatosNivelActual().esNivelJefe;

        if (esJefe)
        {
            // Reinicio completo del nivel
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (jugador != null)
        {
            // Teletransporte al checkpoint
            jugador.EjecutarTeletransporte();
            yield return StartCoroutine(RutinaFade(0f));
            jugador.DesbloquearMovimiento();
        }
    }

    private void FijarOpacidad(float valor)
    {
        if (imagenFundido != null)
        {
            Color c = imagenFundido.color;
            c.a = valor;
            imagenFundido.color = c;
            imagenFundido.raycastTarget = (valor > 0.1f);
        }
    }

    private IEnumerator RutinaFade(float opacidadObjetivo)
    {
        if (imagenFundido == null) yield break;
        if (opacidadObjetivo > 0.1f) imagenFundido.raycastTarget = true;

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