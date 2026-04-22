using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SCR_Transiciones : MonoBehaviour
{
    public static SCR_Transiciones Instancia;

    [Header("Configuración de UI")]
    [Tooltip("Arrastra aquí la Imagen negra que cubre toda la pantalla")]
    public Image imagenNegra;

    [Tooltip("Velocidad a la que ocurre el fundido (más alto = más rápido)")]
    public float velocidadFundido = 1.5f;

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
            return;
        }

        if (imagenNegra != null)
        {
            imagenNegra.color = new Color(0, 0, 0, 0);
            imagenNegra.raycastTarget = false;
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += AlCargarEscena; }
    private void OnDisable() { SceneManager.sceneLoaded -= AlCargarEscena; }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        QuitarFundido();
    }


    public void ReiniciarEscenaConEspera(float espera)
    {
        StartCoroutine(RutinaFadeOutYLoad(SceneManager.GetActiveScene().name, espera));
    }

    public void CargarEscenaConEspera(string nombreEscena, float espera)
    {
        StartCoroutine(RutinaFadeOutYLoad(nombreEscena, espera));
    }

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


    public void QuitarFundido()
    {
        StopAllCoroutines(); 
        StartCoroutine(RutinaFadeIn());
    }


    private IEnumerator RutinaFadeOutYLoad(string escenaDestino, float espera)
    {
        yield return new WaitForSeconds(espera);

        imagenNegra.raycastTarget = true;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * velocidadFundido;
            imagenNegra.color = new Color(0, 0, 0, Mathf.Lerp(0, 1, t));
            yield return null;
        }

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