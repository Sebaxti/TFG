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
    [Tooltip("Posición local de la espada respecto al enemigo. Y negativo = abajo del cuerpo.")]
    [SerializeField] private Vector3 offsetPosicionCorte = new Vector3(0f, -0.5f, 0f);
    [Tooltip("Escala del objeto raíz de la espada. Cambia aquí en vez de en el prefab FBX hijo.")]
    [SerializeField] private Vector3 escalaCorte = Vector3.one;
    [Tooltip("Rotación local para el corte de IZQUIERDA")]
    [SerializeField] private Vector3 rotacionCorteIzquierda = Vector3.zero;
    [Tooltip("Rotación local para el corte de DERECHA")]
    [SerializeField] private Vector3 rotacionCorteDerecha = Vector3.zero;
    [Tooltip("Duración real del clip FBX. El Animator ajusta velocidad para encajar con Duracion Del Corte")]
    [SerializeField] private float duracionClipCorte = 1f;
    [SerializeField] private string triggerCorteIzquierda = "CorteIzquierda";
    [SerializeField] private string triggerCorteDerecha = "CorteDerecha";
    [Tooltip("Actívalo si el corte sale al lado equivocado")]
    [SerializeField] private bool intercambiarSentidoCorte = false;

    private Animator animadorCorte;
    private bool corteAnimandose = false;

    [Header("Ataque 2: Lluvia de Espadas")]
    [SerializeField] private GameObject prefabIndicadorCaida;
    [SerializeField] private GameObject prefabLluviaEspadas;
    [Tooltip("Distancia por delante del enemigo donde caen las espadas")]
    [SerializeField] private float distanciaAdelante = 15f;
    [Tooltip("Separación entre tercios del carril (eje X)")]
    [SerializeField] private float anchuraTercio = 4f;
    [Tooltip("Permite que caiga en el tercio izquierdo")]
    [SerializeField] private bool tercioIzquierda = true;
    [Tooltip("Permite que caiga en el tercio central")]
    [SerializeField] private bool tercioCentro = true;
    [Tooltip("Permite que caiga en el tercio derecho")]
    [SerializeField] private bool tercioDerecha = true;
    [Tooltip("Segundos que se muestra el aviso antes de que caigan las espadas")]
    [SerializeField] private float tiempoAvisoCaida = 1.5f;
    [Tooltip("Altura Y del suelo donde aparece el VFX y el collider")]
    [SerializeField] private float alturaSuelo = 0f;

    [Header("VFX y Colores de Ataque")]
    [Tooltip("Componente VisualEffect del hijo VFX_Orb. Arrastra el GameObject hijo aquí.")]
    [SerializeField] private VisualEffect vfxCuerpo;
    [Tooltip("Nombre exacto de la propiedad Vector4 expuesta en el VFX Graph")]
    [SerializeField] private string propiedadColorVFX = "ColorTint";
    [Tooltip("Color del VFX en estado normal (sin atacar). Blanco = colores originales del VFX.")]
    [SerializeField] private Color colorNormal = Color.white;
    [Tooltip("Color del VFX durante el aviso y ejecución del Corte Lateral")]
    [SerializeField] private Color colorAvisoCorte = Color.red;
    [Tooltip("Color del VFX durante el aviso y ejecución de la Caída de Rocas")]
    [SerializeField] private Color colorAvisoCaida = new Color(0.3f, 0.5f, 1f);

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
        SCR_RespawnJugador respawn = FindFirstObjectByType<SCR_RespawnJugador>();
        if (respawn != null)
            transform.position = respawn.GetEnemigoRespawn();
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

        // Colocar espada relativa al enemigo y activar
        objetoCorte.transform.SetParent(transform, worldPositionStays: false);
        objetoCorte.transform.localPosition = offsetPosicionCorte;
        objetoCorte.transform.localRotation = Quaternion.Euler(rotacion);
        objetoCorte.transform.localScale = escalaCorte;
        objetoCorte.SetActive(true);

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

        // Elegir tercio disponible al azar
        int[] tercios = new int[3];
        int count = 0;
        if (tercioIzquierda) tercios[count++] = -1;
        if (tercioCentro)    tercios[count++] =  0;
        if (tercioDerecha)   tercios[count++] =  1;

        if (count == 0) { RestablecerColor(); yield break; }

        int tercioElegido = tercios[Random.Range(0, count)];

        Vector3 avance = direccionMundo.normalized;
        Vector3 derecha = Vector3.Cross(Vector3.up, avance).normalized;

        Vector3 posSuelo = transform.position
                         + avance  * distanciaAdelante
                         + derecha * (tercioElegido * anchuraTercio);
        posSuelo.y = alturaSuelo;

        // Mostrar aviso en el suelo
        if (prefabIndicadorCaida != null)
            indicadorActivo = Instantiate(prefabIndicadorCaida, posSuelo, Quaternion.identity);

        yield return new WaitForSeconds(tiempoAvisoCaida);

        if (indicadorActivo != null) Destroy(indicadorActivo);

        // Activar VFX + collider de daño
        if (prefabLluviaEspadas != null)
            Instantiate(prefabLluviaEspadas, posSuelo, Quaternion.identity);

        RestablecerColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            other.GetComponent<SCR_RespawnJugador>()?.Respawn();
    }
}
