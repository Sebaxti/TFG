using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SCR_GestorEscena : MonoBehaviour
{
    public static SCR_GestorEscena Instancia;

    [Header("Configuraci�n de Escenas")]
    public string nombreEscenaMenu = "MainMenu";
    public string nombreEscenaJefe = "Nivel_3_Jefe";

    [Header("Efecto de Fundido (Fade)")]
    [SerializeField] private Image imagenFundido;
    [SerializeField] private float velocidadFade = 1.5f;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
            Resolution nativeRes = Screen.currentResolution;
            if (nativeRes.width > 0 && nativeRes.height > 0)
                Screen.SetResolution(nativeRes.width, nativeRes.height, FullScreenMode.FullScreenWindow);
            AsegurarCanvasFundido();
        }
        else { Destroy(gameObject); return; }
    }

    private void AsegurarCanvasFundido()
    {
        if (imagenFundido != null)
        {
            Canvas c = imagenFundido.GetComponentInParent<Canvas>();
            if (c != null)
            {
                if (c.transform.parent != transform)
                    c.transform.SetParent(transform, false);
                c.sortingOrder = 999;
            }
            return;
        }

        GameObject canvasGO = new GameObject("GE_CanvasFundido");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGO.AddComponent<CanvasScaler>();

        GameObject imgGO = new GameObject("GE_ImagenFundido");
        imgGO.transform.SetParent(canvasGO.transform, false);
        imagenFundido = imgGO.AddComponent<Image>();
        imagenFundido.color = new Color(0f, 0f, 0f, 0f);
        imagenFundido.raycastTarget = false;

        RectTransform rt = imgGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }

    private void OnEnable() { SceneManager.sceneLoaded += AlCargarEscena; }
    private void OnDisable() { SceneManager.sceneLoaded -= AlCargarEscena; }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        StopAllCoroutines();
        if (escena.name == nombreEscenaMenu)
        {
            FijarOpacidad(0f);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
            color.a = Mathf.MoveTowards(color.a, opacidadObjetivo, velocidadFade * Time.unscaledDeltaTime);
            imagenFundido.color = color;
            yield return null;
        }
        FijarOpacidad(opacidadObjetivo);
    }
}