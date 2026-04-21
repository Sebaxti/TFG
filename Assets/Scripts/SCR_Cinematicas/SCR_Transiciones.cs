using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SCR_Transiciones : MonoBehaviour
{
    // Instancia estática para que cualquier script pueda llamarlo: SCR_Transiciones.Instancia
    public static SCR_Transiciones Instancia;

    [Header("Configuración de UI")]
    [Tooltip("Arrastra aquí la Imagen negra que cubre toda la pantalla")]
    public Image imagenNegra;

    [Tooltip("Velocidad a la que ocurre el fundido (más alto = más rápido)")]
    public float velocidadFundido = 1.5f;

    private void Awake()
    {
        // --- PATRÓN SINGLETON INMORTAL ---
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject); // No se destruye al cambiar de escena
        }
        else
        {
            Destroy(gameObject); // Si ya existe uno, destruye la copia
            return;
        }

        // Al empezar el juego, nos aseguramos de que la imagen sea invisible
        if (imagenNegra != null)
        {
            imagenNegra.color = new Color(0, 0, 0, 0);
            imagenNegra.raycastTarget = false;
        }
    }

    // Suscribirse al evento de carga de escena para hacer el Fade In automático
    private void OnEnable() { SceneManager.sceneLoaded += AlCargarEscena; }
    private void OnDisable() { SceneManager.sceneLoaded -= AlCargarEscena; }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // Cada vez que entramos a un nivel nuevo (o reiniciamos), quitamos el negro
        QuitarFundido();
    }

    // ==========================================
    // MÉTODOS DE CONTROL (Llamados por otros scripts)
    // ==========================================

    /// <summary>
    /// Pone la pantalla en negro y luego reinicia o carga una escena nueva.
    /// </summary>
    public void ReiniciarEscenaConEspera(float espera)
    {
        StartCoroutine(RutinaFadeOutYLoad(SceneManager.GetActiveScene().name, espera));
    }

    public void CargarEscenaConEspera(string nombreEscena, float espera)
    {
        StartCoroutine(RutinaFadeOutYLoad(nombreEscena, espera));
    }

    /// <summary>
    /// SOLO pone la pantalla en negro (usado para el respawn local del Nivel 2).
    /// </summary>
    public IEnumerator SoloFundidoNegro(float duracion)
    {
        imagenNegra.raycastTarget = true;
        float t = 0;
        Color c = imagenNegra.color;

        while (t < 1)
        {
            t += Time.deltaTime / duracion;
            imagenNegra.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0, 1, t));
            yield return null;
        }
    }

    /// <summary>
    /// Quita el color negro de la pantalla suavemente.
    /// </summary>
    public void QuitarFundido()
    {
        StopAllCoroutines(); // Evita conflictos si hay fundidos a medias
        StartCoroutine(RutinaFadeIn());
    }

    // ==========================================
    // LÓGICA INTERNA (CORRUTINAS)
    // ==========================================

    private IEnumerator RutinaFadeOutYLoad(string escenaDestino, float espera)
    {
        // Esperamos el tiempo de "drama" (viendo al jugador morir)
        yield return new WaitForSeconds(espera);

        imagenNegra.raycastTarget = true;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * velocidadFundido;
            imagenNegra.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t));
            yield return null;
        }

        // Una vez negro, cargamos
        SceneManager.LoadScene(escenaDestino);
    }

    private IEnumerator RutinaFadeIn()
    {
        float t = 0;
        Color c = imagenNegra.color;

        while (t < 1)
        {
            t += Time.deltaTime * velocidadFundido;
            imagenNegra.color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0, t));
            yield return null;
        }

        imagenNegra.raycastTarget = false;
    }
}