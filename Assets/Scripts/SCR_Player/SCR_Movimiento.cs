using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class SCR_Movimiento : MonoBehaviour
{
    // El evento global que reinicia al enemigo
    public static event Action OnGlobalRespawn;

    public enum Estados { Idle, Move, Jump, DoubleJump, Fall }

    [Header("Estado")]
    public Estados estadoActual = Estados.Idle;
    private Estados estadoAnterior = Estados.Idle;

    [Header("Referencias")]
    [SerializeField] private Animator animador;
    [SerializeField] private GameObject objetoAlas;
    private Rigidbody rb;
    private Transform camaraTransform;

    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 8f;
    [SerializeField] private float suavizadoSuelo = 15f;
    [SerializeField] private float suavizadoAire = 5f;
    [SerializeField] private float velocidadRotacion = 20f;

    [Header("Salto Físico (Restaurado)")]
    [SerializeField] private float fuerzaSalto = 14f;
    [SerializeField] private float gravedadAscenso = 2.5f;
    [SerializeField] private float multiplicadorCaida = 4.5f;
    [SerializeField] private float multiplicadorSaltoCorto = 4f;
    [SerializeField] private float velocidadTerminal = -20f;
    [SerializeField] private int saltosExtraMaximos = 1;
    private int saltosRestantes;

    [Header("Game Feel (Control Perfecto)")]
    [SerializeField] private float coyoteTime = 0.15f;
    private float coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Detección Suelo Automática")]
    [SerializeField] private Transform checkSuelo;
    [SerializeField] private float radioSuelo = 0.3f;
    private LayerMask capaSuelo;
    private bool enSuelo;

    [Header("Checkpoints")]
    private Vector3 posRespawnPlayer;
    private Vector3 posRespawnEnemigo;

    private bool controlesBloqueados = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (Camera.main != null) camaraTransform = Camera.main.transform;

        posRespawnPlayer = transform.position;
        capaSuelo = LayerMask.GetMask("Suelo");
    }

    private void Update()
    {

        if (controlesBloqueados) return;

        enSuelo = Physics.CheckSphere(checkSuelo.position, radioSuelo, capaSuelo);

        if (enSuelo)
        {
            coyoteTimeCounter = coyoteTime;
            saltosRestantes = saltosExtraMaximos;
            if (objetoAlas != null) objetoAlas.SetActive(false);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f) EjecutarSalto();
        else if (Input.GetButtonDown("Jump") && saltosRestantes > 0 && !enSuelo) EjecutarDobleSalto();

        
        ActualizarEstado();
        ProcesarAnimaciones();
    }

    private void FixedUpdate()
    {
        MoverPersonaje();
        AplicarGravedadPersonalizada();
    }

    private void MoverPersonaje()
    {
        Vector3 adelante = Vector3.ProjectOnPlane(camaraTransform.forward, Vector3.up).normalized;
        Vector3 derecha = Vector3.ProjectOnPlane(camaraTransform.right, Vector3.up).normalized;
        Vector3 dir = (adelante * Input.GetAxisRaw("Vertical") + derecha * Input.GetAxisRaw("Horizontal")).normalized;

        Vector3 velObj = dir * velocidadMovimiento;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(velObj.x, rb.linearVelocity.y, velObj.z), Time.fixedDeltaTime * (enSuelo ? suavizadoSuelo : suavizadoAire));

        if (dir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.fixedDeltaTime * velocidadRotacion);
    }

    private void AplicarGravedadPersonalizada()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorSaltoCorto - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravedadAscenso - 1) * Time.fixedDeltaTime;

        if (rb.linearVelocity.y < velocidadTerminal)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, velocidadTerminal, rb.linearVelocity.z);
    }

    private void EjecutarSalto()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        jumpBufferCounter = 0f; coyoteTimeCounter = 0f;
    }

    private void EjecutarDobleSalto()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        saltosRestantes--;
        if (objetoAlas) { objetoAlas.SetActive(false); objetoAlas.SetActive(true); }
    }

    private void ActualizarEstado()
    {
        if (enSuelo) estadoActual = (rb.linearVelocity.magnitude > 0.1f) ? Estados.Move : Estados.Idle;
        else estadoActual = (rb.linearVelocity.y > 0.1f) ? (saltosRestantes < saltosExtraMaximos ? Estados.DoubleJump : Estados.Jump) : Estados.Fall;
    }

    private void ProcesarAnimaciones()
    {
        if (animador && estadoActual != estadoAnterior)
        {
            animador.CrossFade(estadoActual.ToString(), 0.1f);
            estadoAnterior = estadoActual;
        }
    }

    public void EstablecerCheckpoint(Vector3 p, Vector3 e)
    {
        posRespawnPlayer = p;
        posRespawnEnemigo = e;
    }

    public Vector3 GetEnemigoRespawn() => posRespawnEnemigo;

    public void Respawn()
    {
        transform.position = posRespawnPlayer;
        rb.linearVelocity = Vector3.zero;
        OnGlobalRespawn?.Invoke();
    }

    public void BloquearMovimiento()
    {
        controlesBloqueados = true;
        rb.linearVelocity = Vector3.zero; 
        rb.isKinematic = true; 
        estadoActual = Estados.Idle; 
    }

    private void OnDrawGizmos()
    {
        if (checkSuelo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(checkSuelo.position, radioSuelo);
        }
    }
}