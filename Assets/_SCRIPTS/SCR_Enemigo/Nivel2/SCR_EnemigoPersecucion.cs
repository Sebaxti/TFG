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
    [SerializeField] private GameObject prefabAvisoLateral;
    [SerializeField] private float duracionDelCorte = 0.5f;
    [SerializeField] private float tiempoDeAvisoLateral = 1.0f;
    [SerializeField] private Vector3 corregirRotacionAviso = Vector3.zero;
    [SerializeField] private Transform puntoReferenciaIzquierda;
    [SerializeField] private Transform puntoReferenciaDerecha;
    [Tooltip("Nombre del trigger en el Animator Controller que lanza la animación de corte")]
    [SerializeField] private string triggerAtacar = "Atacar";
    private Animator animadorCorte;

    [Header("Ataque 2: Caída (Rocas)")]
    [SerializeField] private GameObject prefabIndicador;
    [SerializeField] private GameObject prefabObjetoCaida;
    [SerializeField] private float distanciaAdelante = 15f;
    [SerializeField] private float separacionCarriles = 4f;
    [SerializeField] private float alturaCaida = 15f;
    [SerializeField] private float tiempoAvisoCaida = 1.5f;

    [Header("Ajustes de Caída (Sin Físicas)")]
    [SerializeField] private float alturaSueloRoca = 0.5f;
    [Tooltip("Velocidad constante a la que baja la roca (matemática, sin físicas)")]
    [SerializeField] private float velocidadCaidaRoca = 30f;
    [Tooltip("Segundos que la roca se queda en el suelo antes de desaparecer")]
    [SerializeField] private float tiempoVidaEnSuelo = 3f;

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
    private GameObject avisoLateralActivo, indicadorActivo, trampaActiva;

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
        {
            transform.position = respawn.GetEnemigoRespawn();

        }
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
            {
                yield return StartCoroutine(AtaqueCorte());
            }
            else
            {
                yield return StartCoroutine(AtaqueCaida());
            }
        }
    }

    private IEnumerator AtaqueCorte()
    {
        if (puntoReferenciaDerecha == null || puntoReferenciaIzquierda == null) yield break;

        AplicarColor(colorAvisoCorte);

        bool esDerecha = Random.value > 0.5f;
        Transform puntoInicio = esDerecha ? puntoReferenciaDerecha : puntoReferenciaIzquierda;
        Transform puntoFin = esDerecha ? puntoReferenciaIzquierda : puntoReferenciaDerecha;

        if (prefabAvisoLateral != null)
        {
            Quaternion rotacionFinal = puntoInicio.rotation * Quaternion.Euler(corregirRotacionAviso);
            avisoLateralActivo = Instantiate(prefabAvisoLateral, puntoInicio.position, rotacionFinal, transform);
        }

        yield return new WaitForSeconds(tiempoDeAvisoLateral);

        if (avisoLateralActivo != null) Destroy(avisoLateralActivo);

        if (objetoCorte != null)
        {
            objetoCorte.transform.position = puntoInicio.position;
            objetoCorte.transform.rotation = puntoInicio.rotation;

            // Espejear horizontalmente según la dirección del corte
            Vector3 escala = objetoCorte.transform.localScale;
            escala.x = esDerecha ? -Mathf.Abs(escala.x) : Mathf.Abs(escala.x);
            objetoCorte.transform.localScale = escala;

            objetoCorte.SetActive(true);

            if (animadorCorte != null)
            {
                animadorCorte.SetTrigger(triggerAtacar);
                yield return new WaitForSeconds(duracionDelCorte);
            }
            else
            {
                // Fallback procedural (placeholder cubo)
                float t = 0;
                while (t < 1)
                {
                    t += Time.deltaTime / duracionDelCorte;
                    objetoCorte.transform.position = Vector3.Lerp(puntoInicio.position, puntoFin.position, Mathf.SmoothStep(0, 1, t));
                    yield return null;
                }
            }

            objetoCorte.SetActive(false);
        }

        RestablecerColor();
    }

    private IEnumerator AtaqueCaida()
    {
        if (prefabIndicador == null || prefabObjetoCaida == null) yield break;

        AplicarColor(colorAvisoCaida);

        int carril = Random.Range(-1, 2);
        Vector3 avance = direccionMundo.normalized;
        Vector3 derecha = Vector3.Cross(Vector3.up, avance).normalized;

        Vector3 posSuelo = transform.position + (avance * distanciaAdelante) + (derecha * (carril * separacionCarriles));
        posSuelo.y = alturaSueloRoca;

        indicadorActivo = Instantiate(prefabIndicador, posSuelo, Quaternion.identity);

        yield return new WaitForSeconds(tiempoAvisoCaida);

        if (indicadorActivo != null) Destroy(indicadorActivo);

        Vector3 posCielo = posSuelo + (Vector3.up * alturaCaida);
        trampaActiva = Instantiate(prefabObjetoCaida, posCielo, Quaternion.identity);

        while (trampaActiva != null && trampaActiva.transform.position.y > posSuelo.y)
        {
            trampaActiva.transform.position = Vector3.MoveTowards(
                trampaActiva.transform.position,
                posSuelo,
                velocidadCaidaRoca * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(tiempoVidaEnSuelo);

        if (trampaActiva != null) Destroy(trampaActiva);

        RestablecerColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SCR_RespawnJugador>()?.Respawn();
        }
    }
}