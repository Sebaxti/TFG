using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SCR_Movimiento : MonoBehaviour
{
    public enum Estados { Idle, Move, Jump, DoubleJump, Fall }

    [Header("Estado Actual")]
    public Estados estadoActual = Estados.Idle;

    [Header("Referencias")]
    private Rigidbody rb;
    private Transform camaraTransform;

    [Header("Movimiento")]
    [SerializeField] private float velocidadMovimiento = 8f;
    [SerializeField] private float suavizadoSuelo = 15f;
    [SerializeField] private float suavizadoAire = 5f;
    [SerializeField] private float velocidadRotacion = 20f;

    [Header("Salto Físico y Game Feel")]
    [SerializeField] private float fuerzaSalto = 14f;
    [SerializeField] private float gravedadAscenso = 2.5f;
    [SerializeField] private float multiplicadorCaida = 4.5f;
    [SerializeField] private float multiplicadorSaltoCorto = 4f;
    [SerializeField] private float velocidadTerminal = -20f;
    [SerializeField] private int saltosExtraMaximos = 1;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform puntoSuelo;
    [SerializeField] private float radioSuelo = 0.3f;
    [SerializeField] private LayerMask capaSuelo;

    private int saltosRestantes;
    private bool controlesBloqueados = false;
    private bool enSuelo;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private Vector3 direccionInput;

    // --- NUEVO: Control de Inercia de Plataformas ---
    private Rigidbody rbPlataformaActual;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (Camera.main != null) camaraTransform = Camera.main.transform;

        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (controlesBloqueados) return;

        ProcesarInputs();
        ComprobarSuelo();
        ControlarEstados();
        GestionarSalto();
    }

    private void FixedUpdate()
    {
        if (controlesBloqueados) return;

        AplicarMovimiento();
        AplicarGravedadDinamica();
    }

    private void ProcesarInputs()
    {
        float hor = Input.GetAxisRaw("Horizontal");
        float ver = Input.GetAxisRaw("Vertical");
        direccionInput = new Vector3(hor, 0f, ver).normalized;

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void ComprobarSuelo()
    {
        if (puntoSuelo == null) return;

        // Usamos OverlapSphere para saber EXACTAMENTE qué objeto estamos pisando
        Collider[] colisiones = Physics.OverlapSphere(puntoSuelo.position, radioSuelo, capaSuelo);
        enSuelo = colisiones.Length > 0;

        if (enSuelo)
        {
            coyoteTimeCounter = coyoteTime;
            saltosRestantes = saltosExtraMaximos;

            // Buscamos si alguno de los suelos es una plataforma móvil
            rbPlataformaActual = null;
            foreach (Collider col in colisiones)
            {
                if (col.GetComponent<SCR_Plataformas>() != null)
                {
                    rbPlataformaActual = col.GetComponent<Rigidbody>();
                    break;
                }
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            rbPlataformaActual = null;
        }
    }

    private void ControlarEstados()
    {
        if (enSuelo)
        {
            estadoActual = (direccionInput.magnitude > 0.1f) ? Estados.Move : Estados.Idle;
        }
        else
        {
            if (rb.linearVelocity.y < 0)
                estadoActual = Estados.Fall;
        }
    }

    private void GestionarSalto()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            EjecutarSalto();
            estadoActual = Estados.Jump;
        }
        else if (Input.GetButtonDown("Jump") && !enSuelo && saltosRestantes > 0)
        {
            EjecutarSalto();
            saltosRestantes--;
            estadoActual = Estados.DoubleJump;
        }
    }

    private void EjecutarSalto()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    private void AplicarMovimiento()
    {
        Vector3 velocidadSuelo = Vector3.zero;
        if (rbPlataformaActual != null)
        {
            velocidadSuelo = rbPlataformaActual.linearVelocity;
        }

        if (direccionInput.magnitude >= 0.1f)
        {
            float anguloObjetivo = Mathf.Atan2(direccionInput.x, direccionInput.z) * Mathf.Rad2Deg + camaraTransform.eulerAngles.y;
            Vector3 direccionMov = Quaternion.Euler(0f, anguloObjetivo, 0f) * Vector3.forward;

            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMov);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.fixedDeltaTime));

               Vector3 velocidadObjetivo = (direccionMov * velocidadMovimiento) + velocidadSuelo;
            float suavizadoActual = enSuelo ? suavizadoSuelo : suavizadoAire;

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(velocidadObjetivo.x, rb.linearVelocity.y, velocidadObjetivo.z), suavizadoActual * Time.fixedDeltaTime);
        }
        else
        {
            float suavizadoActual = enSuelo ? suavizadoSuelo : suavizadoAire;

               Vector3 velocidadObjetivo = velocidadSuelo;

            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(velocidadObjetivo.x, rb.linearVelocity.y, velocidadObjetivo.z), suavizadoActual * Time.fixedDeltaTime);
        }
    }

    private void AplicarGravedadDinamica()
    {
        float gravedadFinal = Physics.gravity.y;

        if (rb.linearVelocity.y < 0)
            gravedadFinal *= multiplicadorCaida;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            gravedadFinal *= multiplicadorSaltoCorto;
        else if (rb.linearVelocity.y > 0 && Input.GetButton("Jump"))
            gravedadFinal *= gravedadAscenso;

        rb.linearVelocity += Vector3.up * gravedadFinal * Time.fixedDeltaTime;

        if (rb.linearVelocity.y < velocidadTerminal)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, velocidadTerminal, rb.linearVelocity.z);
    }

    public void DesbloquearMovimiento()
    {
        controlesBloqueados = false;
        rb.isKinematic = false;
    }

    public void BloquearMovimiento()
    {
        controlesBloqueados = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        estadoActual = Estados.Idle;
    }

    private void OnDrawGizmosSelected()
    {
        if (puntoSuelo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(puntoSuelo.position, radioSuelo);
        }
    }
}