using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class SCR_JefeFinal : MonoBehaviour
{
    // ==========================================
    // 1. ESTRUCTURA DE FASES (DIFICULTAD EVOLUTIVA)
    // ==========================================
    [System.Serializable]
    public class AjustesFase
    {
        public string nombreFase;
        public float cooldownAtaque = 1.5f;
        public int ataquesParaMaestro = 3;
        public float duracionGiroEspada = 0.4f;
        public Color colorFase = Color.white;
        public int cantidadLluvias = 3;
        public int cantidadSimultanea = 1;
        public float tiempoEntreOleadas = 0.5f;
    }

    [Header("Configuración de Fases")]
    public AjustesFase fase0_Inicial   = new AjustesFase { nombreFase = "Fase 1 (100% Vida)",     cooldownAtaque = 2f,   ataquesParaMaestro = 3, duracionGiroEspada = 0.5f,  colorFase = Color.white,              cantidadLluvias = 3, cantidadSimultanea = 1, tiempoEntreOleadas = 0.5f  };
    public AjustesFase fase1_UnGolpe   = new AjustesFase { nombreFase = "Fase 2 (Tras 1 Golpe)",  cooldownAtaque = 1.5f, ataquesParaMaestro = 4, duracionGiroEspada = 0.35f, colorFase = new Color(1f, 0.8f, 0.8f), cantidadLluvias = 5, cantidadSimultanea = 2, tiempoEntreOleadas = 0.35f };
    public AjustesFase fase2_DosGolpes = new AjustesFase { nombreFase = "Fase 3 (Tras 2 Golpes)", cooldownAtaque = 1f,   ataquesParaMaestro = 5, duracionGiroEspada = 0.2f,  colorFase = new Color(1f, 0.5f, 0.5f), cantidadLluvias = 8, cantidadSimultanea = 3, tiempoEntreOleadas = 0.25f };

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

    [Header("VFX del Cuerpo")]
    [SerializeField] private VisualEffect vfxCuerpo;
    [SerializeField] private string propiedadColorVFX = "ColorTint";

    [Header("Espada: Posición y Animación")]
    [SerializeField] private Vector3 offsetEspadaIzquierda = new Vector3(0, 1.2f, 2.0f);
    [SerializeField] private Vector3 offsetEspadaDerecha = new Vector3(0, 1.2f, 2.0f);
    [SerializeField] private Animator animadorEspada;
    [SerializeField] private string triggerCorteIzquierda = "CorteIzquierda";
    [SerializeField] private string triggerCorteDerecha = "CorteDerecha";
    [SerializeField] private float duracionClipCorte = 1f;
    [SerializeField] private Vector3 rotacionCorteIzquierda = Vector3.zero;
    [SerializeField] private Vector3 rotacionCorteDerecha = Vector3.zero;
    [SerializeField] private Vector3 escalaCorte = Vector3.one;
    [SerializeField] private bool intercambiarSentidoCorte = false;

    [Header("Ataque Maestro (Proyectil)")]
    public GameObject prefabProyectilMaestro;
    public float alturaSalidaProyectil = 1.8f;
    public float distanciaSalidaProyectil = 2.5f;

    [Header("Ataque Caída (Lluvia de Espadas)")]
    [SerializeField] private GameObject prefabLluviaEspadas;
    public GameObject prefabAvisoSuelo;
    public float alturaSueloArena = 0f;
    [SerializeField] private float tiempoAvisoAntesCaida = 1f;
    [SerializeField] private float alturaAviso = 0.05f;
    [SerializeField] private float alturaLluvia = 0f;
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

    [Header("Columnas del Arena")]
    [SerializeField] private List<SCR_ColumnaReflectora> columnasArena = new List<SCR_ColumnaReflectora>();
    [SerializeField] private int columnasPorGolpe = 2;

    [Header("Temblor de Cámara al Recibir Golpe")]
    [SerializeField] private float duracionTemblorGolpe = 0.6f;
    [SerializeField] private float magnitudTemblorGolpe = 0.5f;

    [Header("Final del Juego")]
    [SerializeField] private string nombreEscenaCreditos = "SCN_Creditos";

    [Header("Audio")]
    [SerializeField] private AudioClip clipGolpeRecibido;
    [SerializeField] private AudioClip clipLanzamientoMaestro;
    [SerializeField] private AudioClip clipAtaqueEspada;
    [SerializeField] private AudioSource fuenteEspada;

    // Variables internas
    private int contadorAtaques = 0;
    private float timerCooldown;

    // ==========================================
    // 3. MÉTODOS PRINCIPALES
    // ==========================================
    private void Start()
    {
        if (jugador == null) jugador = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (objetoCorteLateral != null)
        {
            if (animadorEspada == null)
                animadorEspada = objetoCorteLateral.GetComponentInChildren<Animator>();
            objetoCorteLateral.SetActive(false);
        }

        if (fuenteEspada == null)
        {
            fuenteEspada = gameObject.AddComponent<AudioSource>();
            fuenteEspada.playOnAwake = false;
        }

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

        // Temporizador de combate (se pausa si el jugador ya está muerto)
        if (timerCooldown > 0f)
            timerCooldown -= Time.deltaTime;
        else if (!SCR_RespawnJugador.EstaMuerto)
            DecidirSiguienteMovimiento();
    }

    private void DecidirSiguienteMovimiento()
    {
        if (contadorAtaques >= faseActual.ataquesParaMaestro)
            StartCoroutine(RutinaAtaqueMaestro());
        else
        {
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

        bool esDerecha = Random.value > 0.5f;
        bool usarDerecha = intercambiarSentidoCorte ? !esDerecha : esDerecha;

        yield return new WaitForSeconds(0.7f); // Tiempo de aviso (telegrafiado)

        if (objetoCorteLateral != null)
        {
            objetoCorteLateral.transform.localPosition = usarDerecha ? offsetEspadaDerecha : offsetEspadaIzquierda;
            objetoCorteLateral.transform.localRotation = Quaternion.Euler(usarDerecha ? rotacionCorteDerecha : rotacionCorteIzquierda);
            objetoCorteLateral.transform.localScale = escalaCorte;
            objetoCorteLateral.SetActive(true);

            if (animadorEspada != null)
            {
                animadorEspada.speed = duracionClipCorte / Mathf.Max(faseActual.duracionGiroEspada, 0.01f);
                animadorEspada.SetTrigger(usarDerecha ? triggerCorteDerecha : triggerCorteIzquierda);
                if (fuenteEspada != null && clipAtaqueEspada != null)
                {
                    float speedBase = duracionClipCorte / Mathf.Max(fase0_Inicial.duracionGiroEspada, 0.01f);
                    fuenteEspada.pitch = Mathf.Clamp(animadorEspada.speed / speedBase, 0.5f, 3f);
                    fuenteEspada.PlayOneShot(clipAtaqueEspada);
                }
                yield return new WaitForSeconds(faseActual.duracionGiroEspada);
                animadorEspada.speed = 1f;
            }

            objetoCorteLateral.SetActive(false);
            if (animadorEspada != null)
                animadorEspada.transform.localPosition = Vector3.zero;
        }

        FinalizarAtaque(true);
    }

    private IEnumerator RutinaAtaqueCaida()
    {
        estadoActual = EstadoJefe.Atacando;
        AplicarColor(colorAvisoMagia);

        int total      = faseActual.cantidadLluvias;
        int porOleada  = Mathf.Max(1, faseActual.cantidadSimultanea);

        for (int i = 0; i < total; i += porOleada)
        {
            int enEstaOleada = Mathf.Min(porOleada, total - i);

            GameObject[] avisas    = new GameObject[enEstaOleada];
            Vector3[]    posiciones = new Vector3[enEstaOleada];

            // Mostrar todos los avisas de esta oleada a la vez
            for (int j = 0; j < enEstaOleada; j++)
            {
                Vector2 desplazamiento = Random.insideUnitCircle * radioDispersionCaida;
                posiciones[j] = new Vector3(
                    jugador.position.x + desplazamiento.x,
                    alturaLluvia,
                    jugador.position.z + desplazamiento.y);

                if (prefabAvisoSuelo != null)
                avisas[j] = Instantiate(prefabAvisoSuelo,
                    new Vector3(posiciones[j].x, alturaAviso, posiciones[j].z),
                    Quaternion.identity);
            }

            yield return new WaitForSeconds(tiempoAvisoAntesCaida);

            // Caer todos al mismo tiempo
            for (int j = 0; j < enEstaOleada; j++)
            {
                if (avisas[j] != null) Destroy(avisas[j]);
                if (prefabLluviaEspadas != null)
                    Instantiate(prefabLluviaEspadas, posiciones[j], Quaternion.identity);
            }

            // Pausa entre oleadas (no esperar tras la última)
            if (i + porOleada < total)
                yield return new WaitForSeconds(faseActual.tiempoEntreOleadas);
        }

        FinalizarAtaque(true);
    }

    private IEnumerator RutinaAtaqueMaestro()
    {
        estadoActual = EstadoJefe.Atacando;
        AplicarColor(colorMaestro);
        yield return new WaitForSeconds(1.2f);

        if (prefabProyectilMaestro != null)
        {
            SCR_GestorAudio.Instancia?.ReproducirSFX(clipLanzamientoMaestro);
            Vector3 spawnPos = transform.position + (transform.forward * distanciaSalidaProyectil) + (Vector3.up * alturaSalidaProyectil);
            proyectilActivo = Instantiate(prefabProyectilMaestro, spawnPos, transform.rotation);
            proyectilActivo.GetComponent<SCR_ProyectilJefe>().Disparar(jugador.position, this);
        }

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

        SCR_GestorAudio.Instancia?.ReproducirSFX(clipGolpeRecibido);

        if (objetoCorteLateral != null) objetoCorteLateral.SetActive(false);
        if (proyectilActivo != null) Destroy(proyectilActivo);

        SCR_TemblorCamara.Instancia?.AgitarCamaraPersonalizada(duracionTemblorGolpe, magnitudTemblorGolpe);
        HundirColumnasAleatorias();

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

    private void HundirColumnasAleatorias()
    {
        columnasArena.RemoveAll(c => c == null);

        int cantidad = Mathf.Min(columnasPorGolpe, columnasArena.Count);
        for (int i = 0; i < cantidad; i++)
        {
            int indice = Random.Range(0, columnasArena.Count);
            columnasArena[indice].IniciarHundimiento();
            columnasArena.RemoveAt(indice);
        }
    }

    private IEnumerator RutinaAturdido()
    {
        estadoActual = EstadoJefe.Atacando;

        for (int i = 0; i < 5; i++)
        {
            if (renderizadorCuerpo) renderizadorCuerpo.enabled = false;
            yield return new WaitForSeconds(0.1f);
            if (renderizadorCuerpo) renderizadorCuerpo.enabled = true;
            AplicarColor(Color.red);
            yield return new WaitForSeconds(0.1f);
        }

        AplicarColor(colorAturdido);
        yield return new WaitForSeconds(2f);

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

        SCR_ObjetoCaida[] rocas = FindObjectsByType<SCR_ObjetoCaida>(FindObjectsSortMode.None);
        foreach (var r in rocas) Destroy(r.gameObject);

        yield return new WaitForSeconds(1f);
        SCR_GestorEscena.Instancia?.CargarEscenaConFade(nombreEscenaCreditos);
    }

    public void DesbloquearJefe()
    {
        FinalizarAtaque(false);
    }

    private void AplicarColor(Color c)
    {
        if (vfxCuerpo != null && vfxCuerpo.HasVector4(propiedadColorVFX))
            vfxCuerpo.SetVector4(propiedadColorVFX, c);

        if (renderizadorCuerpo != null)
            renderizadorCuerpo.material.color = c;
    }
}
