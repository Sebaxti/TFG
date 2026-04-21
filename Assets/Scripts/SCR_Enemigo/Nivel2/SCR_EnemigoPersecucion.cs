using UnityEngine;
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

    private float alturaOriginal;
    private GameObject avisoLateralActivo, indicadorActivo, trampaActiva;

    private void OnEnable() { SCR_Movimiento.OnGlobalRespawn += ResetPosicion; }
    private void OnDisable() { SCR_Movimiento.OnGlobalRespawn -= ResetPosicion; }

    private void Start()
    {
        alturaOriginal = transform.position.y;
        if (objetoCorte != null) objetoCorte.SetActive(false);
        StartCoroutine(BucleLogicaAtaques());
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

    private void ResetPosicion()
    {
        StopAllCoroutines();

        if (avisoLateralActivo != null) Destroy(avisoLateralActivo);
        if (indicadorActivo != null) Destroy(indicadorActivo);
        if (trampaActiva != null) Destroy(trampaActiva);
        if (objetoCorte != null) objetoCorte.SetActive(false);

        SCR_Movimiento player = Object.FindFirstObjectByType<SCR_Movimiento>();
        if (player != null)
        {
            Vector3 p = player.GetEnemigoRespawn();
            transform.position = new Vector3(p.x, alturaOriginal, p.z);
        }

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
            objetoCorte.SetActive(true);
            objetoCorte.transform.rotation = puntoInicio.rotation;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / duracionDelCorte;
                objetoCorte.transform.position = Vector3.Lerp(puntoInicio.position, puntoFin.position, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }

            objetoCorte.SetActive(false);
        }
    }

    private IEnumerator AtaqueCaida()
    {
        if (prefabIndicador == null || prefabObjetoCaida == null) yield break;

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

        // --- CAÍDA POR MATEMÁTICAS (Cero bugs físicos) ---
        while (trampaActiva != null && trampaActiva.transform.position.y > posSuelo.y)
        {
            // La bajamos a la velocidad exacta indicada
            trampaActiva.transform.position = Vector3.MoveTowards(
                trampaActiva.transform.position,
                posSuelo,
                velocidadCaidaRoca * Time.deltaTime
            );
            yield return null;
        }

        // --- TIEMPO DE ESPERA SOBRE LA SUPERFICIE ---
        yield return new WaitForSeconds(tiempoVidaEnSuelo);

        if (trampaActiva != null) Destroy(trampaActiva);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SCR_Movimiento mov = other.GetComponent<SCR_Movimiento>();
            if (mov != null) mov.Respawn();
        }
    }
}