using UnityEngine;
using System.Collections;

public class SCR_EnemigoPersecucion : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidadConstante = 7f;
    [SerializeField] private Vector3 direccionMundo = new Vector3(0, 0, -1);

    [Header("Ataque 1: Corte Lateral (Salto)")]
    [SerializeField] private GameObject objetoCorte;
    [SerializeField] private GameObject prefabAvisoLateral;
    [Range(0f, 1f)][SerializeField] private float probabilidadCorte = 0.5f;
    [SerializeField] private float duracionDelCorte = 0.5f;
    [SerializeField] private float tiempoDeAvisoLateral = 1.0f;

    // --- PUNTOS DE REFERENCIA LOCALES (PARA PRECISIÓN TOTAL) ---
    [Tooltip("Objeto vacío hijo del enemigo colocado exactamente en la pared IZQUIERDA (X, Y, Z exactas)")]
    [SerializeField] private Transform puntoReferenciaIzquierda;
    [Tooltip("Objeto vacío hijo del enemigo colocado exactamente en la pared DERECHA (X, Y, Z exactas)")]
    [SerializeField] private Transform puntoReferenciaDerecha;

    [Header("Ataque 2: Caída (3 Carriles)")]
    [SerializeField] private GameObject prefabIndicador;
    [SerializeField] private GameObject prefabObjetoCaida;
    [Range(0f, 1f)][SerializeField] private float probabilidadCaida = 0.5f;
    [SerializeField] private float[] posicionesCarriles = new float[3] { -3.5f, 0f, 3.5f };
    [SerializeField] private float distanciaAdelante = 8f;

    [Header("Tiempos Generales")]
    [SerializeField] private float tiempoEntreChequeos = 3f;
    [SerializeField] private float tiempoDeAvisoCaida = 1.2f;

    private SCR_Movimiento playerScript;
    private float alturaOriginal;

    // Control individual de cada ataque
    private bool cortando = false;
    private bool tirandoObjeto = false;

    // Referencias para limpieza (Evita bugs visuales al morir)
    private GameObject indicadorActivo;
    private GameObject trampaActiva;
    private GameObject avisoLateralActivo;

    // Variable para guardar el punto de referencia actual
    private Transform puntoReferenciaActual;

    // --- NUEVAS VARIABLES: Guardan la posición exacta del mundo al instanciar ---
    private float xMundoRetenida;
    private float yMundoRetenida;
    // Guardamos la Z del enemigo al instanciar para calcular el avance
    private float zEnemigoAlInstanciar;

    private void OnEnable() => SCR_Movimiento.OnPlayerRespawn += ResetPosicion;
    private void OnDisable() => SCR_Movimiento.OnPlayerRespawn -= ResetPosicion;

    private void Awake()
    {
        // MEMORIA ORGÁNICA: Guardamos la Y que tú configuraste manualmente en el Inspector
        alturaOriginal = transform.position.y;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerScript = player.GetComponent<SCR_Movimiento>();

        if (objetoCorte != null) objetoCorte.SetActive(false);
        StartCoroutine(BucleLogicaAtaques());
    }

    private void FixedUpdate()
    {
        transform.position += direccionMundo.normalized * velocidadConstante * Time.fixedDeltaTime;

        // El indicador de caída sigue al enemigo (en el suelo)
        if (indicadorActivo != null)
        {
            float sueloY = transform.position.y - alturaOriginal;
            indicadorActivo.transform.position = new Vector3(indicadorActivo.transform.position.x, sueloY + 0.1f, transform.position.z - distanciaAdelante);
        }

        // --- SEGUIMIENTO ORGÁNICO DEL AVISO LATERAL CON RETENCIÓN DE X e Y ---
        if (avisoLateralActivo != null)
        {
            float avanceZ = transform.position.z - zEnemigoAlInstanciar;
            float nuevaZPos = avisoLateralActivo.transform.position.z + avanceZ;

            // Reubicamos el aviso en el mundo, respetando X e Y originales
            avisoLateralActivo.transform.position = new Vector3(xMundoRetenida, yMundoRetenida, nuevaZPos);

            // Actualizamos la Z del enemigo al instanciar para el siguiente frame
            zEnemigoAlInstanciar = transform.position.z;
        }
    }

    IEnumerator BucleLogicaAtaques()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            if (!cortando && Random.value < probabilidadCorte)
            {
                StartCoroutine(EjecutarCorte());
            }

            if (!tirandoObjeto && Random.value < probabilidadCaida)
            {
                StartCoroutine(EjecutarCaida());
            }

            yield return new WaitForSeconds(tiempoEntreChequeos);
        }
    }

    // --- LÓGICA DE CORTE CORREGIDA: TOTALMENTE ORGÁNICA ---
    IEnumerator EjecutarCorte()
    {
        cortando = true;

        bool deIzquierda = Random.value > 0.5f;

        // Asignamos el punto de referencia correspondiente
        puntoReferenciaActual = deIzquierda ? puntoReferenciaIzquierda : puntoReferenciaDerecha;

        // Mostrar el Aviso Lateral
        if (prefabAvisoLateral != null && puntoReferenciaActual != null)
        {
            // CALCULAMOS POSICIÓN MUNDO PRECISA:
            // Usamos la posición exacta del mundo del punto de referencia (X, Y, Z completos).
            // Esto asegura que aparezca pegado a la pared, a la altura y profundidad que tú decidiste en el editor.
            Vector3 posAvisoMundo = puntoReferenciaActual.position;

            // ¡CAPTURA DE X, Y y Z PRECISA! Guardamos los valores exactos del mundo en este frame.
            xMundoRetenida = puntoReferenciaActual.position.x;
            yMundoRetenida = puntoReferenciaActual.position.y;
            zEnemigoAlInstanciar = transform.position.z; // Guardamos la Z del ENEMIGO para el cálculo de avance

            // Lo creamos con la rotación del Prefab (90 grados) y sin padre
            avisoLateralActivo = Instantiate(prefabAvisoLateral, posAvisoMundo, prefabAvisoLateral.transform.rotation, null);

            // Mantenemos la escala del prefab (Orgánico)
            avisoLateralActivo.transform.localScale = prefabAvisoLateral.transform.localScale;
        }

        // Esperar el tiempo de aviso (el FixedUpdate lo moverá hacia adelante)
        yield return new WaitForSeconds(tiempoDeAvisoLateral);

        if (avisoLateralActivo != null) Destroy(avisoLateralActivo);

        // Si el jugador murió durante el aviso, paramos
        if (Resetting) { cortando = false; yield break; }

        objetoCorte.SetActive(true);

        // --- NÚMEROS LOCALES HARDCODEADOS TEMPORALES PARA EL CORTE ---
        // Debes ajustar estos valores para que coincidan con la anchura real de tu pasillo.
        float startXLocal = deIzquierda ? -5f : 5f;
        float endXLocal = deIzquierda ? 5f : -5f;

        // Posicionamos la cuchilla (localPosition porque es hija)
        objetoCorte.transform.localPosition = new Vector3(startXLocal, objetoCorte.transform.localPosition.y, objetoCorte.transform.localPosition.z);

        float t = 0;
        while (t < duracionDelCorte)
        {
            t += Time.deltaTime;
            float progreso = t / duracionDelCorte;
            float x = Mathf.Lerp(startXLocal, endXLocal, progreso);
            objetoCorte.transform.localPosition = new Vector3(x, objetoCorte.transform.localPosition.y, objetoCorte.transform.localPosition.z);
            yield return null;
        }

        objetoCorte.SetActive(false);
        cortando = false;
    }

    IEnumerator EjecutarCaida()
    {
        tirandoObjeto = true;
        float xElegida = posicionesCarriles[Random.Range(0, posicionesCarriles.Length)];

        // Lo creamos en el mundo (null), respetando su propia escala
        Vector3 posAviso = transform.position + new Vector3(xElegida, -alturaOriginal + 0.1f, -distanciaAdelante);
        indicadorActivo = Instantiate(prefabIndicador, posAviso, Quaternion.identity, null);

        // Mantenemos la escala del prefab
        indicadorActivo.transform.localScale = prefabIndicador.transform.localScale;

        yield return new WaitForSeconds(tiempoDeAvisoCaida);

        if (indicadorActivo != null)
        {
            Vector3 posTrampa = indicadorActivo.transform.position + Vector3.up * 10f;
            trampaActiva = Instantiate(prefabObjetoCaida, posTrampa, Quaternion.identity, null);
            trampaActiva.transform.localScale = prefabObjetoCaida.transform.localScale;
            Vector3 destinoTrampa = indicadorActivo.transform.position;
            Destroy(indicadorActivo);

            float t = 0;
            while (t < 0.3f && trampaActiva != null)
            {
                t += Time.deltaTime;
                trampaActiva.transform.position = Vector3.Lerp(posTrampa, destinoTrampa, t / 0.3f);
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            if (trampaActiva != null) Destroy(trampaActiva);
        }
        tirandoObjeto = false;
    }

    private bool Resetting = false;

    private void ResetPosicion()
    {
        Resetting = true;
        StopAllCoroutines();


        if (indicadorActivo != null) Destroy(indicadorActivo);
        if (trampaActiva != null) Destroy(trampaActiva);
        if (avisoLateralActivo != null) Destroy(avisoLateralActivo);

        if (playerScript != null)
        {
            Vector3 puntoRespawn = playerScript.GetEnemigoRespawn();
            transform.position = new Vector3(puntoRespawn.x, alturaOriginal, puntoRespawn.z);
        }

        if (objetoCorte != null) objetoCorte.SetActive(false);
        cortando = false;
        tirandoObjeto = false;

        Resetting = false;
        StartCoroutine(BucleLogicaAtaques());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SCR_Movimiento mov = other.GetComponent<SCR_Movimiento>();
            if (mov != null) mov.Respawn();
        }
    }

    public void SumarDificultad(float extraCorte, float extraCaida)
    {
        probabilidadCorte = Mathf.Clamp(probabilidadCorte + extraCorte, 0f, 1f);
        probabilidadCaida = Mathf.Clamp(probabilidadCaida + extraCaida, 0f, 1f);
        Debug.Log($"Dificultad Actualizada. Corte: {probabilidadCorte} | Caída: {probabilidadCaida}");
    }
}