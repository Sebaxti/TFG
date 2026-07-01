using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SCR_Creditos : MonoBehaviour
{
    [Header("Nombres")]
    [SerializeField] private string nombre1 = "Nombre 1";
    [SerializeField] private string nombre2 = "Pablo";

    [Header("Textos")]
    [SerializeField] private string textoPrevio = "Un juego desarrollado por";
    [SerializeField] private string textoFinal = "Gracias por jugar";

    [Header("Fuente")]
    [SerializeField] private TMP_FontAsset fuente;

    [Header("Tiempos")]
    [SerializeField] private float tiempoEsperaFadeIn = 1.2f;
    [SerializeField] private float tiempoMostrado = 6f;

    [Header("Audio")]
    [SerializeField] private AudioClip clipMusica;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ConstruirUI();
        StartCoroutine(RutinaCreditos());
    }

    private void ConstruirUI()
    {
        GameObject canvasGO = new GameObject("Canvas_Creditos");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        canvasGO.AddComponent<CanvasScaler>();

        // Fondo negro
        GameObject fondoGO = new GameObject("Fondo");
        fondoGO.transform.SetParent(canvasGO.transform, false);
        Image fondo = fondoGO.AddComponent<Image>();
        fondo.color = Color.black;
        RectTransform rtFondo = fondoGO.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.sizeDelta = Vector2.zero;

        // Textos
        AnadirTexto(canvasGO.transform, textoPrevio, 0f,  200f, 28f, FontStyles.Normal, new Color(0.75f, 0.75f, 0.75f));
        AnadirTexto(canvasGO.transform, nombre1,      0f,   90f, 60f, FontStyles.Bold,   Color.white);
        AnadirTexto(canvasGO.transform, nombre2,      0f,  -90f, 60f, FontStyles.Bold,   Color.white);
        AnadirTexto(canvasGO.transform, textoFinal,   0f, -230f, 22f, FontStyles.Normal, new Color(0.5f, 0.5f, 0.5f));
    }

    private void AnadirTexto(Transform padre, string contenido, float x, float y, float tamano, FontStyles estilo, Color color)
    {
        GameObject go = new GameObject("Txt_" + contenido);
        go.transform.SetParent(padre, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = contenido;
        tmp.fontSize = tamano;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = estilo;
        if (fuente != null) tmp.font = fuente;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 110f);
        rt.anchoredPosition = new Vector2(x, y);
    }

    private IEnumerator RutinaCreditos()
    {
        yield return new WaitForSeconds(tiempoEsperaFadeIn);

        SCR_GestorAudio.Instancia?.PararMusica();
        if (clipMusica != null)
            SCR_GestorAudio.Instancia?.CambiarMusica(clipMusica);

        yield return new WaitForSeconds(tiempoMostrado);

        if (SCR_GestorEscena.Instancia != null)
            SCR_GestorEscena.Instancia.CargarEscenaConFade(SCR_GestorEscena.Instancia.nombreEscenaMenu);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
