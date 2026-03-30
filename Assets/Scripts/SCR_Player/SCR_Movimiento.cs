using UnityEngine;
using System;
[RequireComponent(typeof(Rigidbody))]
public class SCR_Movimiento : MonoBehaviour
{
    public enum Estados { Idle, Move, Jump, DoubleJump, Fall }

    [Header("Máquina de Estados")]
    public Estados estadoActual = Estados.Idle;
    private string animacionActual = "";

    [Header("Referencias")]
    [SerializeField] private Animator animador;
    private Rigidbody rb;
    private Transform camaraTransform;

    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 8f;
    [SerializeField] private float suavizadoSuelo = 50f;
    [SerializeField] private float suavizadoAire = 5f;
    [SerializeField] private float velocidadRotacion = 20f;
    private float inputH, inputV;

    [Header("Salto Físico")]
    [SerializeField] private float fuerzaSalto = 14f;
    [SerializeField] private float gravedadAscenso = 2.5f;
    [SerializeField] private float multiplicadorCaida = 4.5f;
    [SerializeField] private float multiplicadorSaltoCorto = 4f;
    [SerializeField] private float velocidadTerminal = -20f;
    [SerializeField] private int saltosExtraMaximos = 1;
    private int saltosRestantes;

    [Header("Detección y Refinamiento")]
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private float longitudRayoSuelo = 1.1f;
    [SerializeField] private float tiempoCoyote = 0.15f;
    [SerializeField] private float tiempoBufferSalto = 0.15f;
    private bool enSuelo;
    private float contadorCoyote;
    private float contadorBufferSalto;

    [Header("Sistema de Respawn")]
    private Vector3 posRespawnPlayer;
    private Vector3 posRespawnEnemigo;
    public static event Action OnPlayerRespawn;

    private Transform plataformaActual;
    private Vector3 posicionPreviaPlataforma;
    private Vector3 velocidadPlataforma;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        camaraTransform = Camera.main.transform;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        posRespawnPlayer = transform.position;
    }
    private void Start()
    {
        posRespawnPlayer = transform.position;

        // BUSCAMOS AL ENEMIGO PARA GUARDAR SU SITIO ORIGINAL
        GameObject enemigo = GameObject.FindGameObjectWithTag("Enemigo"); // Asegúrate de que el enemigo tenga el Tag "Enemigo"
        if (enemigo != null)
        {
            posRespawnEnemigo = enemigo.transform.position;
        }
    }

    private void Update()
    {
        inputH = Input.GetAxisRaw("Horizontal");
        inputV = Input.GetAxisRaw("Vertical");

        ManejarTimers();
        VerificarSuelo();
        DeterminarEstadoLogico();
        ActualizarAnimaciones();
    }

    private void FixedUpdate()
    {
        CalcularVelocidadPlataforma();

        if (contadorBufferSalto > 0) ProcesarPeticionSalto();

        float suavizado = enSuelo ? suavizadoSuelo : suavizadoAire;
        AplicarMovimientoFisico(suavizado);

        if (!enSuelo) AplicarGravedadPersonalizada();
        AplicarRotacion();
    }

    private void ManejarTimers()
    {
        if (Input.GetButtonDown("Jump")) contadorBufferSalto = tiempoBufferSalto;
        else contadorBufferSalto -= Time.deltaTime;

        if (enSuelo) contadorCoyote = tiempoCoyote;
        else contadorCoyote -= Time.deltaTime;
    }

    private void DeterminarEstadoLogico()
    {
        if (!enSuelo)
        {
            if (rb.linearVelocity.y < -0.1f) estadoActual = Estados.Fall;
        }
        else
        {
            estadoActual = (Mathf.Abs(inputH) > 0.1f || Mathf.Abs(inputV) > 0.1f) ? Estados.Move : Estados.Idle;
        }
    }

    private void ProcesarPeticionSalto()
    {
        if (enSuelo || contadorCoyote > 0)
        {
            EjecutarFuerzaSalto();
            estadoActual = Estados.Jump;
            contadorBufferSalto = 0;
            contadorCoyote = 0;
        }
        else if (saltosRestantes > 0)
        {
            EjecutarFuerzaSalto();
            estadoActual = Estados.DoubleJump;
            contadorBufferSalto = 0;
        }
    }

    private void EjecutarFuerzaSalto()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        saltosRestantes--;
        enSuelo = false;
        plataformaActual = null;
    }

    private void AplicarMovimientoFisico(float suavizado)
    {
        Vector3 forward = camaraTransform.forward;
        Vector3 right = camaraTransform.right;
        forward.y = 0; right.y = 0;
        Vector3 dir = (forward * inputV + right * inputH).normalized;
        Vector3 velObj = dir * velocidadMovimiento;

        Vector3 velRelativa = rb.linearVelocity - velocidadPlataforma;
        float nX = Mathf.Lerp(velRelativa.x, velObj.x, suavizado * Time.fixedDeltaTime);
        float nZ = Mathf.Lerp(velRelativa.z, velObj.z, suavizado * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(nX + velocidadPlataforma.x, rb.linearVelocity.y, nZ + velocidadPlataforma.z);
    }

    private void AplicarGravedadPersonalizada()
    {
        float multi = 1f;
        if (rb.linearVelocity.y > 0)
            multi = !Input.GetButton("Jump") ? multiplicadorSaltoCorto : gravedadAscenso;
        else
            multi = multiplicadorCaida;

        rb.linearVelocity += Vector3.up * Physics.gravity.y * (multi - 1) * Time.fixedDeltaTime;

        if (rb.linearVelocity.y < velocidadTerminal)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, velocidadTerminal, rb.linearVelocity.z);
    }

    private void VerificarSuelo()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, longitudRayoSuelo, capaSuelo))
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < 45f)
            {
                if (!enSuelo) saltosRestantes = saltosExtraMaximos;
                enSuelo = true;
                if (plataformaActual != hit.collider.transform)
                {
                    plataformaActual = hit.collider.transform;
                    posicionPreviaPlataforma = plataformaActual.position;
                }
                return;
            }
        }
        enSuelo = false;
        plataformaActual = null;
    }

    private void CalcularVelocidadPlataforma()
    {
        if (enSuelo && plataformaActual != null)
        {
            velocidadPlataforma = (plataformaActual.position - posicionPreviaPlataforma) / Time.fixedDeltaTime;
            posicionPreviaPlataforma = plataformaActual.position;
        }
        else velocidadPlataforma = Vector3.zero;
    }

    private void AplicarRotacion()
    {
        if (Mathf.Abs(inputH) < 0.1f && Mathf.Abs(inputV) < 0.1f) return;
        Vector3 forward = camaraTransform.forward;
        Vector3 right = camaraTransform.right;
        forward.y = 0; right.y = 0;
        Vector3 direccion = (forward * inputV + right * inputH).normalized;
        if (direccion != Vector3.zero)
            rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(direccion), velocidadRotacion * Time.fixedDeltaTime);
    }

    public void EstablecerCheckpoint(Vector3 playerPos, Vector3 enemigoPos)
    {
        posRespawnPlayer = playerPos;
        posRespawnEnemigo = enemigoPos;
    }

    public Vector3 GetEnemigoRespawn() => posRespawnEnemigo;

    public void Respawn()
    {
        transform.position = posRespawnPlayer;
        rb.linearVelocity = Vector3.zero;
        estadoActual = Estados.Idle;
        saltosRestantes = saltosExtraMaximos;
        enSuelo = true;
        OnPlayerRespawn?.Invoke(); 
    }

    private void ActualizarAnimaciones()
    {
        if (animador == null) return;
        switch (estadoActual)
        {
            case Estados.Idle: CambiarAnimacion("Idle"); break;
            case Estados.Move: CambiarAnimacion("Run"); break;
            case Estados.Jump: CambiarAnimacion("Jump"); break;
            case Estados.DoubleJump: CambiarAnimacion("DoubleJump"); break;
            case Estados.Fall: CambiarAnimacion("Fall"); break;
        }
    }

    private void CambiarAnimacion(string nueva)
    {
        if (animacionActual == nueva) return;
        animador.Play(nueva);
        animacionActual = nueva;
    }
}