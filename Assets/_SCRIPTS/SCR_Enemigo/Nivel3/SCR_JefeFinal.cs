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
        [Tooltip("Tiempo de respiro entre ataques del jefe")]
        public float cooldownAtaque = 1.5f;
        [Tooltip("Número de ataques básicos antes de lanzar el proyectil maestro")]
        public int ataquesParaMaestro = 3;
        [Tooltip("Duración del ataque de corte. Valores bajos = tajo más rápido y letal")]
        public float duracionGiroEspada = 0.4f;
        [Tooltip("Color base del jefe durante esta fase")]
        public Color colorFase = Color.white;
    }

    [Header("Configuración de Fases")]
    public AjustesFase fase0_Inicial    = new AjustesFase { nombreFase = "Fase 1 (100% Vida)",      cooldownAtaque = 2f,   ataquesParaMaestro = 3, duracionGiroEspada = 0.5f,  colorFase = Color.white };
    public AjustesFase fase1_UnGolpe    = new AjustesFase { nombreFase = "Fase 2 (Tras 1 Golpe)",   cooldownAtaque = 1.5f, ataquesParaMaestro = 4, duracionGiroEspada = 0.35f, colorFase = new Color(1f, 0.8f, 0.8f) };
    public AjustesFase fase2_DosGolpes  = new AjustesFase { nombreFase = "Fase 3 (Tras 2 Golpes)",  cooldownAtaque = 1f,   ataquesParaMaestro = 5, duracionGiroEspada = 0.2f,  colorFase = new Color(1f, 0.5f, 0.5f) };

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
    [Tooltip("Componente VisualEffect del hijo VFX_Orb. Arrastra el GameObject hijo aquí.")]
    [SerializeField] private VisualEffect vfxCuerpo;
    [Tooltip("Nombre exacto de la propiedad Vector4 expuesta en el VFX Graph")]
    [SerializeField] private string propiedadColorVFX = "ColorTint";

    [Header("Espada: Posición y Animación")]
    [Tooltip("Posición local de la espada respecto al jefe. Y=Altura, Z=Distancia frontal.")]
    [SerializeField] private Vector3 offsetEspada = new Vector3(0, 1.2f, 2.0f);
    [Tooltip("Arrastra aquí el Animator del modelo FBX de la espada. Si está vacío se auto-busca en los hijos de objetoCorteLateral.")]
    [SerializeField] private Animator animadorEspada;
    [Tooltip("Trigger para el corte desde la izquierda")]
    [SerializeField] private string triggerCorteIzquierda = "CorteIzquierda";
    [Tooltip("Trigger para el corte desde la derecha")]
    [SerializeField] private string triggerCorteDerecha = "CorteDerecha";
    [Tooltip("Duración real del clip FBX en segundos. El Animator ajusta su velocidad para encajar con la fase actual")]
    [SerializeField] private float duracionClipCorte = 1f;
    [Tooltip("Rotación local para el corte de IZQUIERDA")]
    [SerializeField] private Vector3 rotacionCorteIzquierda = Vector3.zero;
    [Tooltip("Rotación local para el corte de DERECHA")]
    [SerializeField] private Vector3 rotacionCorteDerecha = Vector3.zero;
    [Tooltip("Escala del objeto raíz de la espada")]
    [SerializeField] private Vector3 escalaCorte = Vector3.one;
    [Tooltip("Actívalo si el corte va al lado equivocado")]
    [SerializeField] private bool intercambiarSentidoCorte = false;

    [Header("Ataque Maestro (Proyectil)")]
    public GameObject prefabProyectilMaestro;
    public float alturaSalidaProyectil = 1.8f;
    public float distanciaSalidaProyectil = 2.5f;

    [Header("Ataque Caída (Lluvia de Espadas)")]
    [Tooltip("Prefab con PSS_Espadas + BoxCollider + SCR_LluviaEspadas")]
    [SerializeField] private GameObject prefabLluviaEspadas;
    [Tooltip("Prefab de aviso que aparece en el suelo antes de la lluvia")]
    public GameObject prefabAvisoSuelo;
    [Tooltip("Altura Y del suelo del arena")]
    public float alturaSueloArena = 0f;
    [Tooltip("Segundos que se muestra el aviso antes de que caigan las espadas")]
    [SerializeField] private float tiempoAvisoAntesCaida = 1f;
    [Tooltip("Segundos de espera entre cada zona de lluvia consecutiva")]
    [SerializeField] private float tiempoEntreLluvias = 0.4f;
    [Tooltip("Número de zonas de lluvia por ataque")]
    public int cantidadObjetosCaida = 4;
    [Tooltip("Radio de dispersión alrededor del jugador")]
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
    [Tooltip("Arrastra aquí las columnas reflectoras de la escena. Se eligen aleatoriamente al recibir un golpe.")]
    [SerializeField] private List<SCR_ColumnaReflectora> columnasArena = new List<SCR_ColumnaReflectora>();
    [Tooltip("Número de columnas que se hunden por cada golpe recibido")]
    [SerializeField] private int columnasPorGolpe = 2;

    [Header("Temblor de Cámara al Recibir Golpe")]
    [Tooltip("Duración del temblor de cámara cuando el proyectil impacta al jefe")]
    [SerializeField] private float duracionTemblorGolpe = 0.6f;
    [Tooltip("Intensidad del temblor de cámara cuando el proyectil impacta al jefe")]
    [SerializeField] private float magnitudTemblorGolpe = 0.5f;

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
            timerCooldown -= Time.deltaTime;
        else
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
            Vector3 posLocal = offsetEspada;
            posLocal.x = esDerecha ? Mathf.Abs(offsetEspada.x) : -Mathf.Abs(offsetEspada.x);
            objetoCorteLateral.transform.localPosition = posLocal;
            objetoCorteLateral.transform.localRotation = Quaternion.Euler(usarDerecha ? rotacionCorteDerecha : rotacionCorteIzquierda);
            objetoCorteLateral.transform.localScale = escalaCorte;
            objetoCorteLateral.SetActive(true);

            if (animadorEspada != null)
            {
                animadorEspada.speed = duracionClipCorte / Mathf.Max(faseActual.duracionGiroEspada, 0.01f);
                animadorEspada.SetTrigger(usarDerecha ? triggerCorteDerecha : triggerCorteIzquierda);
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

        for (int i = 0; i < cantidadObjetosCaida; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radioDispersionCaida;
            Vector3 posSuelo = new Vector3(jugador.position.x + offset.x, alturaSueloArena, jugador.position.z + offset.y);

            // Aviso en el suelo (se destruye solo cuando la lluvia arranca)
            GameObject aviso = null;
            if (prefabAvisoSuelo != null)
                aviso = Instantiate(prefabAvisoSuelo, posSuelo + Vector3.up * 0.05f, Quaternion.identity);

            yield return new WaitForSeconds(tiempoAvisoAntesCaida);

            // Quitar aviso y activar VFX + zona de daño simultáneamente
            if (aviso != null) Destroy(aviso);
            if (prefabLluviaEspadas != null)
                Instantiate(prefabLluviaEspadas, posSuelo, Quaternion.identity);

            yield return new WaitForSeconds(tiempoEntreLluvias);
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

        Destroy(gameObject, 1f);
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
