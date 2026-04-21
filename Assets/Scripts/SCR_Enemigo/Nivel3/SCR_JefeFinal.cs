using UnityEngine;
using System.Collections;

public class SCR_JefeFinal : MonoBehaviour
{
    public enum EstadoJefe { Persiguiendo, Atacando }

    [Header("Estado Actual")]
    public EstadoJefe estadoActual = EstadoJefe.Persiguiendo;

    [Header("Referencias Generales")]
    public Transform jugador;
    public Transform puntoCentro;
    public GameObject objetoCorteLateral;
    [SerializeField] private Renderer renderizadorCuerpo;
    private GameObject proyectilActivo;

    [Header("Configuración Proyectil Maestro")]
    public GameObject prefabProyectilMaestro;
    [Tooltip("Altura desde el suelo a la que aparece el proyectil (Pecho)")]
    public float alturaSalidaProyectil = 1.8f;
    [Tooltip("Distancia por delante del jefe a la que aparece el proyectil")]
    public float distanciaSalidaProyectil = 2.5f;

    [Header("Estadísticas de Combate")]
    public int saludMax = 3;
    private int saludActual;
    public int ataquesParaElMaestro = 3;
    private int contadorAtaques = 0;

    [Header("Prefabs y Avisos")]
    public GameObject prefabObjetoCaida;
    public GameObject prefabAvisoSuelo;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAvisoAtaque = Color.red;
    [SerializeField] private Color colorAvisoMagia = Color.magenta;
    [SerializeField] private Color colorMaestro = Color.yellow;
    [SerializeField] private Color colorAturdido = Color.gray;

    [Header("Alturas de Arena")]
    public float alturaSueloArena = 0f;
    public float alturaCaidaObjetos = 15f;

    [Header("Movimiento y Flote")]
    public float velocidad = 4f;
    public float velocidadRotacion = 5f;
    [SerializeField] private float amplitudFlote = 0.3f;
    [SerializeField] private float frecuenciaFlote = 2.0f;
    private float alturaInicialY;

    [Header("Tiempos y Rangos")]
    public float distanciaAtaqueMelee = 3f;
    public float cooldownPostAtaque = 1.5f;
    public float tiempoParaActivarCaida = 3f;
    public int cantidadObjetosCaida = 4;
    public float radioDispersionCaida = 4f;

    private float timerCooldown = 0f;
    private float timerLejos = 0f;

    private void Start()
    {
        saludActual = saludMax;
        if (jugador == null) jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (objetoCorteLateral != null) objetoCorteLateral.SetActive(false);
        if (renderizadorCuerpo == null) renderizadorCuerpo = GetComponent<Renderer>();
        alturaInicialY = transform.position.y;
        AplicarColor(colorNormal);
        timerCooldown = 2f;
    }

    private void Update()
    {
        if (jugador == null) return;
        float nuevoDesfaseY = Mathf.Sin(Time.time * frecuenciaFlote) * amplitudFlote;
        transform.position = new Vector3(transform.position.x, alturaInicialY + nuevoDesfaseY, transform.position.z);

        if (estadoActual == EstadoJefe.Atacando) return;

        if (timerCooldown > 0f) timerCooldown -= Time.deltaTime;

        Vector3 dirJugador = (jugador.position - transform.position).normalized;
        dirJugador.y = 0;
        if (dirJugador != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirJugador), velocidadRotacion * Time.deltaTime);

        float distanciaActual = Vector3.Distance(transform.position, jugador.position);

        if (distanciaActual > distanciaAtaqueMelee)
        {
            transform.position += transform.forward * velocidad * Time.deltaTime;
            timerLejos += Time.deltaTime;
        }
        else timerLejos = 0f;

        if (timerCooldown <= 0f)
        {
            if (contadorAtaques >= ataquesParaElMaestro) StartCoroutine(RutinaAtaqueMaestro());
            else if (distanciaActual > distanciaAtaqueMelee && timerLejos >= tiempoParaActivarCaida) StartCoroutine(RutinaAtaqueCaida());
            else if (distanciaActual <= distanciaAtaqueMelee) StartCoroutine(RutinaCorteLateral());
        }
    }

    private IEnumerator RutinaAtaqueMaestro()
    {
        estadoActual = EstadoJefe.Atacando;
        contadorAtaques = 0;
        timerLejos = 0f;
        AplicarColor(colorMaestro);

        Vector3 destino = new Vector3(puntoCentro.position.x, transform.position.y, puntoCentro.position.z);
        while (Vector3.Distance(transform.position, destino) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * 1.5f * Time.deltaTime);
            yield return null;
        }

        Vector3 dirApunto = (jugador.position - transform.position).normalized;
        dirApunto.y = 0;
        transform.rotation = Quaternion.LookRotation(dirApunto);

        yield return new WaitForSeconds(1.5f);

        if (prefabProyectilMaestro != null)
        {
            // USAMOS LAS VARIABLES DEL INSPECTOR PARA EL SPAWN
            Vector3 spawnPos = transform.position + (transform.forward * distanciaSalidaProyectil) + (Vector3.up * alturaSalidaProyectil);
            proyectilActivo = Instantiate(prefabProyectilMaestro, spawnPos, transform.rotation);
            proyectilActivo.GetComponent<SCR_ProyectilJefe>().Disparar(jugador.position, this);
        }

        AplicarColor(colorNormal);

        float timeout = 0f;
        while (estadoActual == EstadoJefe.Atacando && timeout < 10f) { timeout += Time.deltaTime; yield return null; }
        if (estadoActual == EstadoJefe.Atacando) DesbloquearJefe();
    }

    private IEnumerator RutinaCorteLateral()
    {
        estadoActual = EstadoJefe.Atacando;
        AplicarColor(colorAvisoAtaque);
        if (objetoCorteLateral != null) objetoCorteLateral.transform.localRotation = Quaternion.Euler(0, 60, 0);
        yield return new WaitForSeconds(0.6f);
        if (objetoCorteLateral != null)
        {
            objetoCorteLateral.SetActive(true);
            float d = 0.3f; float t = 0f;
            while (t < d) { t += Time.deltaTime; objetoCorteLateral.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 60, 0), Quaternion.Euler(0, -60, 0), t / d); yield return null; }
            objetoCorteLateral.SetActive(false);
        }
        AplicarColor(colorNormal);
        yield return new WaitForSeconds(0.5f);
        FinalizarAtaque();
    }

    private IEnumerator RutinaAtaqueCaida()
    {
        estadoActual = EstadoJefe.Atacando;
        AplicarColor(colorAvisoMagia);
        for (int i = 0; i < cantidadObjetosCaida; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radioDispersionCaida;
            Vector3 pSuelo = new Vector3(jugador.position.x + offset.x, alturaSueloArena, jugador.position.z + offset.y);
            if (prefabAvisoSuelo != null) Destroy(Instantiate(prefabAvisoSuelo, pSuelo + Vector3.up * 0.1f, Quaternion.identity), 1.5f);
            yield return new WaitForSeconds(0.6f);
            if (prefabObjetoCaida != null) Instantiate(prefabObjetoCaida, new Vector3(pSuelo.x, alturaSueloArena + alturaCaidaObjetos, pSuelo.z), Quaternion.identity);
            yield return new WaitForSeconds(0.3f);
        }
        AplicarColor(colorNormal);
        yield return new WaitForSeconds(1f);
        FinalizarAtaque();
    }

    private void FinalizarAtaque() { contadorAtaques++; timerCooldown = cooldownPostAtaque; timerLejos = 0f; estadoActual = EstadoJefe.Persiguiendo; }

    public void DesbloquearJefe() { if (estadoActual == EstadoJefe.Atacando) { contadorAtaques = 0; timerCooldown = cooldownPostAtaque; AplicarColor(colorNormal); estadoActual = EstadoJefe.Persiguiendo; } }

    public void RecibirGolpe()
    {
        if (saludActual <= 0) return;
        saludActual--;
        StopAllCoroutines();
        if (proyectilActivo != null) Destroy(proyectilActivo);
        if (objetoCorteLateral != null) objetoCorteLateral.SetActive(false);

        if (saludActual <= 0) StartCoroutine(RutinaMuerte());
        else StartCoroutine(RutinaAturdido());
    }

    private IEnumerator RutinaAturdido()
    {
        estadoActual = EstadoJefe.Atacando;
        for (int i = 0; i < 5; i++) { if (renderizadorCuerpo) renderizadorCuerpo.enabled = false; yield return new WaitForSeconds(0.1f); if (renderizadorCuerpo) renderizadorCuerpo.enabled = true; AplicarColor(Color.red); yield return new WaitForSeconds(0.1f); }
        AplicarColor(colorAturdido); yield return new WaitForSeconds(2f);
        DesbloquearJefe();
    }

    private IEnumerator RutinaMuerte()
    {
        estadoActual = EstadoJefe.Atacando;
        for (int i = 0; i < 10; i++) { if (renderizadorCuerpo) renderizadorCuerpo.enabled = !renderizadorCuerpo.enabled; AplicarColor(Color.black); yield return new WaitForSeconds(0.05f); }
        if (renderizadorCuerpo) renderizadorCuerpo.enabled = true; AplicarColor(Color.black);
        SCR_ObjetoCaida[] rocas = FindObjectsByType<SCR_ObjetoCaida>(FindObjectsSortMode.None);
        foreach (var r in rocas) Destroy(r.gameObject);
    }

    private void AplicarColor(Color c) { if (renderizadorCuerpo) renderizadorCuerpo.material.color = c; }
}