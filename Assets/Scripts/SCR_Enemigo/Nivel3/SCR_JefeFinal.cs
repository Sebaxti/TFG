using UnityEngine;
using System.Collections;

public class SCR_JefeFinal : MonoBehaviour
{
    // ==========================================
    // 1. ESTRUCTURA DE FASES (DIFICULTAD EVOLUTIVA)
    // ==========================================
    [System.Serializable]
    public class AjustesFase
    {
        public string nombreFase;
        [Tooltip("Tiempo de respiro entre ataques del jefe")]
        public float cooldownAtaque = 1.5f;
        [Tooltip("Número de ataques básicos antes de lanzar el proyectil maestro")]
        public int ataquesParaMaestro = 3;
        [Tooltip("Duración de la animación del corte. Valores bajos = tajo más rápido y letal")]
        public float duracionGiroEspada = 0.4f;
        [Tooltip("Velocidad (m/s) a la que caen las rocas desde el cielo")]
        public float velocidadCaidaRocas = 15f;
        [Tooltip("Color base del jefe durante esta fase")]
        public Color colorFase = Color.white;
    }

    [Header("Configuración de Fases")]
    public AjustesFase fase0_Inicial = new AjustesFase { nombreFase = "Fase 1 (100% Vida)", cooldownAtaque = 2f, ataquesParaMaestro = 3, duracionGiroEspada = 0.5f, velocidadCaidaRocas = 10f, colorFase = Color.white };
    public AjustesFase fase1_UnGolpe = new AjustesFase { nombreFase = "Fase 2 (Tras 1 Golpe)", cooldownAtaque = 1.5f, ataquesParaMaestro = 4, duracionGiroEspada = 0.35f, velocidadCaidaRocas = 15f, colorFase = new Color(1f, 0.8f, 0.8f) };
    public AjustesFase fase2_DosGolpes = new AjustesFase { nombreFase = "Fase 3 (Tras 2 Golpes)", cooldownAtaque = 1f, ataquesParaMaestro = 5, duracionGiroEspada = 0.2f, velocidadCaidaRocas = 22f, colorFase = new Color(1f, 0.5f, 0.5f) };

    private AjustesFase faseActual;
    private int golpesRecibidos = 0;

    // ==========================================
    // 2. REFERENCIAS Y VARIABLES GENERALES
    // ==========================================
    public enum EstadoJefe { Persiguiendo, Atacando }

    [Header("Estado Actual")]
    public EstadoJefe estadoActual = EstadoJefe.Persiguiendo;

    [Header("Referencias")]
    public Transform jugador;
    public Transform puntoCentro;
    public GameObject objetoCorteLateral;
    [SerializeField] private Renderer renderizadorCuerpo;
    private GameObject proyectilActivo;

    [Header("Configuración Espada (Corte Lateral)")]
    [Tooltip("Ajusta la posición local de la espada. Y=Altura, Z=Distancia frontal.")]
    [SerializeField] private Vector3 offsetEspada = new Vector3(0, 1.2f, 2.0f);

    [Header("Ataque Maestro (Proyectil)")]
    public GameObject prefabProyectilMaestro;
    public float alturaSalidaProyectil = 1.8f;
    public float distanciaSalidaProyectil = 2.5f;

    [Header("Ataque Caída (Rocas)")]
    public GameObject prefabObjetoCaida;
    public GameObject prefabAvisoSuelo;
    public float alturaSueloArena = 0f;
    public float alturaCaidaObjetos = 15f;
    public int cantidadObjetosCaida = 4;
    public float radioDispersionCaida = 4f;

    [Header("Efecto de Flote y Rotación")]
    public float velocidadRotacion = 5f;
    [SerializeField] private float amplitudFlote = 0.3f;
    [SerializeField] private float frecuenciaFlote = 2.0f;
    private float alturaInicialY;

    [Header("Colores Visuales (Avisos)")]
    [SerializeField] private Color colorAvisoMelee = Color.red;
    [SerializeField] private Color colorAvisoMagia = Color.magenta;
    [SerializeField] private Color colorMaestro = Color.yellow;
    [SerializeField] private Color colorAturdido = Color.gray;

    // Variables internas
    private int contadorAtaques = 0;
    private float timerCooldown;

    // ==========================================
    // 3. MÉTODOS PRINCIPALES
    // ==========================================
    private void Start()
    {
        if (jugador == null) jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (objetoCorteLateral != null) objetoCorteLateral.SetActive(false);

        alturaInicialY = puntoCentro != null ? puntoCentro.position.y : transform.position.y;

        faseActual = fase0_Inicial;
        ActualizarParametrosFase();
    }

    private void ActualizarParametrosFase()
    {
        timerCooldown = faseActual.cooldownAtaque;
        AplicarColor(faseActual.colorFase);
        Debug.Log("--- EL JEFE ENTRA EN: " + faseActual.nombreFase + " ---");
    }

    private void Update()
    {
        if (jugador == null || puntoCentro == null) return;

        // Anclaje al centro + Flote vertical
        float desfaseY = Mathf.Sin(Time.time * frecuenciaFlote) * amplitudFlote;
        transform.position = new Vector3(puntoCentro.position.x, alturaInicialY + desfaseY, puntoCentro.position.z);

        if (estadoActual == EstadoJefe.Atacando) return;

        // Rotar constantemente hacia el jugador
        Vector3 dirJugador = (jugador.position - transform.position).normalized;
        dirJugador.y = 0;
        if (dirJugador != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dirJugador), velocidadRotacion * Time.deltaTime);

        // Temporizador de combate
        if (timerCooldown > 0f)
        {
            timerCooldown -= Time.deltaTime;
        }
        else
        {
            DecidirSiguienteMovimiento();
        }
    }

    private void DecidirSiguienteMovimiento()
    {
        // Si ya ha hecho suficientes ataques básicos, lanza el Maestro
        if (contadorAtaques >= faseActual.ataquesParaMaestro)
        {
            StartCoroutine(RutinaAtaqueMaestro());
        }
        else
        {
            // Elige aleatoriamente entre espada (0) y rocas (1)
            int aleatorio = Random.Range(0, 2);
            if (aleatorio == 0) StartCoroutine(RutinaCorteLateral());
            else StartCoroutine(RutinaAtaqueCaida());
        }
    }

    // ==========================================
    // 4. RUTINAS DE ATAQUE
    // ==========================================
    private IEnumerator RutinaCorteLateral()
    {
        estadoActual = EstadoJefe.Atacando;
        AplicarColor(colorAvisoMelee);

        // Colocar la espada donde marca el Inspector
        if (objetoCorteLateral != null)
        {
            objetoCorteLateral.transform.localPosition = offsetEspada;
            objetoCorteLateral.transform.localRotation = Quaternion.Euler(0, 60, 0);
        }

        yield return new WaitForSeconds(0.7f); // Tiempo de aviso (telegrafiado)

        if (objetoCorteLateral != null)
        {
            objetoCorteLateral.SetActive(true);

            float t = 0f;
            // El giro usa la velocidad configurada en la fase actual
            while (t < faseActual.duracionGiroEspada)
            {
                t += Time.deltaTime;
                objetoCorteLateral.transform.localRotation = Quaternion.Lerp(
                    Quaternion.Euler(0, 60, 0),
                    Quaternion.Euler(0, -60, 0),
                    t / faseActual.duracionGiroEspada
                );
                yield return null;
            }
            objetoCorteLateral.SetActive(false);
        }

        FinalizarAtaque(true);
    }

    private IEnumerator RutinaAtaqueCaida()
    {
        estadoActual = EstadoJefe.Atacando;
        AplicarColor(colorAvisoMagia);

        for (int i = 0; i < cantidadObjetosCaida; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radioDispersionCaida;
            Vector3 posSuelo = new Vector3(jugador.position.x + offset.x, alturaSueloArena, jugador.position.z + offset.y);

            if (prefabAvisoSuelo != null)
                Destroy(Instantiate(prefabAvisoSuelo, posSuelo + Vector3.up * 0.05f, Quaternion.identity), 1.2f);

            yield return new WaitForSeconds(0.5f); // Tiempo de aviso en el suelo

            if (prefabObjetoCaida != null)
            {
                GameObject roca = Instantiate(prefabObjetoCaida, posSuelo + Vector3.up * alturaCaidaObjetos, Quaternion.identity);
                // Le pasamos la velocidad de caída de la fase actual a la roca
                SCR_ObjetoCaida scriptRoca = roca.GetComponent<SCR_ObjetoCaida>();
                if (scriptRoca != null) scriptRoca.velocidadDescenso = faseActual.velocidadCaidaRocas;
            }
            yield return new WaitForSeconds(0.3f);
        }

        FinalizarAtaque(true);
    }

    private IEnumerator RutinaAtaqueMaestro()
    {
        estadoActual = EstadoJefe.Atacando;
        AplicarColor(colorMaestro);
        yield return new WaitForSeconds(1.2f); // El jefe carga energía

        if (prefabProyectilMaestro != null)
        {
            Vector3 spawnPos = transform.position + (transform.forward * distanciaSalidaProyectil) + (Vector3.up * alturaSalidaProyectil);
            proyectilActivo = Instantiate(prefabProyectilMaestro, spawnPos, transform.rotation);
            proyectilActivo.GetComponent<SCR_ProyectilJefe>().Disparar(jugador.position, this);
        }

        // Espera a que el jugador refleje el proyectil (o caduque)
        float safetyTimer = 0f;
        while (estadoActual == EstadoJefe.Atacando && safetyTimer < 8f)
        {
            safetyTimer += Time.deltaTime;
            yield return null;
        }

        if (estadoActual == EstadoJefe.Atacando) FinalizarAtaque(false);
    }

    private void FinalizarAtaque(bool esBasico)
    {
        if (esBasico) contadorAtaques++;
        else contadorAtaques = 0;

        timerCooldown = faseActual.cooldownAtaque;
        AplicarColor(faseActual.colorFase);
        estadoActual = EstadoJefe.Persiguiendo;
    }

    // ==========================================
    // 5. SISTEMA DE DAÑO Y FASES
    // ==========================================
    public void RecibirGolpe()
    {
        golpesRecibidos++;
        StopAllCoroutines();

        if (objetoCorteLateral != null) objetoCorteLateral.SetActive(false);
        if (proyectilActivo != null) Destroy(proyectilActivo);

        // Cambio de fase según golpes
        if (golpesRecibidos == 1)
        {
            faseActual = fase1_UnGolpe;
            ActualizarParametrosFase();
            StartCoroutine(RutinaAturdido());
        }
        else if (golpesRecibidos == 2)
        {
            faseActual = fase2_DosGolpes;
            ActualizarParametrosFase();
            StartCoroutine(RutinaAturdido());
        }
        else if (golpesRecibidos >= 3)
        {
            StartCoroutine(RutinaMuerte());
        }
    }

    private IEnumerator RutinaAturdido()
    {
        estadoActual = EstadoJefe.Atacando;

        // Efecto visual de daño
        for (int i = 0; i < 5; i++)
        {
            if (renderizadorCuerpo) renderizadorCuerpo.enabled = false;
            yield return new WaitForSeconds(0.1f);
            if (renderizadorCuerpo) renderizadorCuerpo.enabled = true;
            AplicarColor(Color.red);
            yield return new WaitForSeconds(0.1f);
        }

        AplicarColor(colorAturdido);
        yield return new WaitForSeconds(2f); // Tiempo aturdido

        DesbloquearJefe();
    }

    private IEnumerator RutinaMuerte()
    {
        estadoActual = EstadoJefe.Atacando;
        Debug.Log("¡JEFE DERROTADO!");

        for (int i = 0; i < 15; i++)
        {
            if (renderizadorCuerpo) renderizadorCuerpo.enabled = !renderizadorCuerpo.enabled;
            AplicarColor(Color.black);
            yield return new WaitForSeconds(0.05f);
        }
        if (renderizadorCuerpo) renderizadorCuerpo.enabled = false;

        // Limpieza de rocas residuales
        SCR_ObjetoCaida[] rocas = FindObjectsByType<SCR_ObjetoCaida>(FindObjectsSortMode.None);
        foreach (var r in rocas) Destroy(r.gameObject);

        // Aquí puedes añadir código para ganar el nivel (ej. GestorNiveles.Win())
        Destroy(gameObject, 1f);
    }

    public void DesbloquearJefe()
    {
        FinalizarAtaque(false);
    }

    private void AplicarColor(Color c)
    {
        if (renderizadorCuerpo) renderizadorCuerpo.material.color = c;
    }
}