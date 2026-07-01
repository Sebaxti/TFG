using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class SCR_EnemigoPersecucion : MonoBehaviour
{
    [Header("Movimiento General")]
    [SerializeField] private float velocidadConstante = 7f;
    [SerializeField] private Vector3 direccionMundo = new Vector3(0, 0, -1);

    [Header("Probabilidades y Dificultad")]
    [Range(0, 1)] public float probabilidadCorte = 0.5f;
    [Range(0, 1)] public float probabilidadCaida = 0.5f;

    [Header("Ataque 1: Corte Lateral")]
    [SerializeField] private GameObject objetoCorte;
    [SerializeField] private float tiempoDeAvisoLateral = 1.0f;
    [SerializeField] private float duracionDelCorte = 0.8f;
    [SerializeField] private Vector3 offsetCorteIzquierda = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 offsetCorteDerecha = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 escalaCorte = Vector3.one;
    [SerializeField] private Vector3 rotacionCorteIzquierda = Vector3.zero;
    [SerializeField] private Vector3 rotacionCorteDerecha = Vector3.zero;
    [SerializeField] private float duracionClipCorte = 1f;
    [SerializeField] private string triggerCorteIzquierda = "CorteIzquierda";
    [SerializeField] private string triggerCorteDerecha = "CorteDerecha";
    [SerializeField] private bool intercambiarSentidoCorte = false;

    private Animator animadorCorte;
    private bool corteAnimandose = false;

    [Header("Ataque 2: Lluvia de Espadas")]
    [SerializeField] private GameObject prefabIndicadorCaida;
    [SerializeField] private GameObject prefabLluviaEspadas;
    [SerializeField] private float distanciaAdelante = 15f;
    [SerializeField] private float anchuraTercio = 4f;
    [SerializeField] private bool tercioIzquierda = true;
    [SerializeField] private bool tercioCentro = true;
    [SerializeField] private bool tercioDerecha = true;
    [SerializeField] private float tiempoAvisoCaida = 1.5f;
    [SerializeField] private float alturaAviso = 0.05f;
    [SerializeField] private float alturaLluvia = 0f;

    [Header("VFX y Colores de Ataque")]
    [SerializeField] private VisualEffect vfxCuerpo;
    [SerializeField] private string propiedadColorVFX = "ColorTint";
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAvisoCorte = Color.red;
    [SerializeField] private Color colorAvisoCaida = new Color(0.3f, 0.5f, 1f);

    [Header("Audio")]
    [SerializeField] private AudioClip clipAtaqueEspada;

    private float alturaOriginal;
    private GameObject indicadorActivo;

    private void OnEnable()
    {
        SCR_RespawnJugador.OnGlobalRespawn += ResetearPosicion;
    }
    private void OnDisable()
    {
        SCR_RespawnJugador.OnGlobalRespawn -= ResetearPosicion;
    }

    private void Start()
    {
        alturaOriginal = transform.position.y;
        if (objetoCorte != null)
        {
            animadorCorte = objetoCorte.GetComponentInChildren<Animator>();
            objetoCorte.SetActive(false);
        }
        AplicarColor(colorNormal);
        StartCoroutine(BucleLogicaAtaques());
    }

    private void AplicarColor(Color c)
    {
        if (vfxCuerpo != null && vfxCuerpo.HasVector4(propiedadColorVFX))
            vfxCuerpo.SetVector4(propiedadColorVFX, c);
    }

    private void RestablecerColor()
    {
        AplicarColor(colorNormal);
    }

    private void Update()
    {
        transform.Translate(direccionMundo * velocidadConstante * Time.deltaTime, Space.World);
    }

        public void SumarDificultad(float extraCorte, float extraCaida)
    {
        probabilidadCorte = Mathf.Clamp01(probabilidadCorte + extraCorte);
        probabilidadCaida = Mathf.Clamp01(probabilidadCaida + extraCaida);
    }

    private void ResetearPosicion()
    {
        StopAllCoroutines();
        corteAnimandose = false;

        if (objetoCorte != null) objetoCorte.SetActive(false);
        if (animadorCorte != null) animadorCorte.transform.localPosition = Vector3.zero;

        if (indicadorActivo != null) { Destroy(indicadorActivo); indicadorActivo = null; }

        foreach (var lluvia in FindObjectsByType<SCR_LluviaEspadas>(FindObjectsSortMode.None))
            Destroy(lluvia.gameObject);

        RestablecerColor();

        SCR_RespawnJugador respawn = FindFirstObjectByType<SCR_RespawnJugador>();
        if (respawn != null)
            transform.position = respawn.GetEnemigoRespawn();

        StartCoroutine(BucleLogicaAtaques());
    }

    private IEnumerator BucleLogicaAtaques()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));

            float totalProb = probabilidadCorte + probabilidadCaida;
            if (totalProb <= 0) continue;

            float r = Random.value;

            if (r < (probabilidadCorte / totalProb))
                yield return StartCoroutine(AtaqueCorte());
            else
                yield return StartCoroutine(AtaqueCaida());
        }
    }

    private IEnumerator AtaqueCorte()
    {
        if (objetoCorte == null) yield break;

        AplicarColor(colorAvisoCorte);

        bool esDerecha = Random.value > 0.5f;

        yield return new WaitForSeconds(tiempoDeAvisoLateral);

        bool usarDerecha = intercambiarSentidoCorte ? !esDerecha : esDerecha;
        Vector3 rotacion = usarDerecha ? rotacionCorteDerecha : rotacionCorteIzquierda;

        objetoCorte.transform.SetParent(transform, worldPositionStays: false);
        objetoCorte.transform.localPosition = usarDerecha ? offsetCorteDerecha : offsetCorteIzquierda;
        objetoCorte.transform.localRotation = Quaternion.Euler(rotacion);
        objetoCorte.transform.localScale = escalaCorte;
        objetoCorte.SetActive(true);

        SCR_GestorAudio.Instancia?.ReproducirSFX(clipAtaqueEspada);

        if (animadorCorte != null)
        {
            animadorCorte.speed = duracionClipCorte / Mathf.Max(duracionDelCorte, 0.01f);
            animadorCorte.SetTrigger(usarDerecha ? triggerCorteDerecha : triggerCorteIzquierda);
            yield return new WaitForSeconds(duracionDelCorte);
            animadorCorte.speed = 1f;
        }

        objetoCorte.SetActive(false);
        if (animadorCorte != null)
            animadorCorte.transform.localPosition = Vector3.zero;

        RestablecerColor();
    }

    private IEnumerator AtaqueCaida()
    {
        AplicarColor(colorAvisoCaida);

        int[] tercios = new int[3];
        int count = 0;
        if (tercioIzquierda) tercios[count++] = -1;
        if (tercioCentro)    tercios[count++] =  0;
        if (tercioDerecha)   tercios[count++] =  1;

        if (count == 0) { RestablecerColor(); yield break; }

        int tercioElegido = tercios[Random.Range(0, count)];

        Vector3 avance = direccionMundo.normalized;
        Vector3 derecha = Vector3.Cross(Vector3.up, avance).normalized;

        Vector3 posBase = transform.position
                        + avance  * distanciaAdelante
                        + derecha * (tercioElegido * anchuraTercio);

        Vector3 posAviso  = new Vector3(posBase.x, alturaAviso,  posBase.z);
        Vector3 posLluvia = new Vector3(posBase.x, alturaLluvia, posBase.z);

        if (prefabIndicadorCaida != null)
            indicadorActivo = Instantiate(prefabIndicadorCaida, posAviso, Quaternion.identity);

        yield return new WaitForSeconds(tiempoAvisoCaida);

        if (indicadorActivo != null) Destroy(indicadorActivo);

        if (prefabLluviaEspadas != null)
            Instantiate(prefabLluviaEspadas, posLluvia, Quaternion.identity);

        RestablecerColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<SCR_RespawnJugador>()?.Respawn();
    }
}
