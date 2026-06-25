using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SCR_Movimiento : MonoBehaviour
{
    public enum Estados { Idle, IdleWait, Move, Jump, DoubleJump, Fall, Die }

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

    [Header("Animación de Espera")]
    [SerializeField] private float tiempoParaEspera = 5f;
    private float contadorInactividad = 0f;

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

        if (direccionInput.magnitude > 0.1f || Input.anyKey)
        {
            contadorInactividad = 0f;
        }
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
        if (estadoActual == Estados.Die) return;

        if (enSuelo)
        {
            // No interrumpir un salto activo: el jugador puede seguir en contacto con el suelo
            // durante 1-2 frames tras el impulso. GestionarSalto ya fijo el estado a Jump.
            if (estadoActual == Estados.Jump || estadoActual == Estados.DoubleJump) return;

            // Aterrizaje: limpiar estado de caida
            if (estadoActual == Estados.Fall)
            {
                estadoActual = Estados.Idle;
                contadorInactividad = 0f;
            }

            if (direccionInput.magnitude > 0.1f)
            {
                contadorInactividad = 0f;
                estadoActual = Estados.Move;
            }
            else
            {
                contadorInactividad += Time.deltaTime;
                estadoActual = contadorInactividad >= tiempoParaEspera ? Estados.IdleWait : Estados.Idle;
            }
        }
        else
        {
            // En el aire: solo pasar a Fall cuando la velocidad descendente es significativa.
            // No reseteamos contadorInactividad aqui para evitar que el micro-jitter de
            // plataformas reinicie el temporizador de inactividad constantemente.
            if (rb.linearVelocity.y < -0.1f)
            {
                estadoActual = Estados.Fall;
                contadorInactividad = 0f;
            }
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
        contadorInactividad = 0f;
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
            if (camaraTransform == null) camaraTransform = Camera.main?.transform;

            float anguloObjetivo = Mathf.Atan2(direccionInput.x, direccionInput.z) * Mathf.Rad2Deg;
            if (camaraTransform != null) anguloObjetivo += camaraTransform.eulerAngles.y;
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
        // Añadimos el !enSuelo para evitar que el ascensor active la gravedad de salto corto
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump") && !enSuelo)
            gravedadFinal *= multiplicadorSaltoCorto;
        else if (rb.linearVelocity.y > 0 && Input.GetButton("Jump") && !enSuelo)
            gravedadFinal *= gravedadAscenso;

        rb.linearVelocity += Vector3.up * gravedadFinal * Time.fixedDeltaTime;

        if (rb.linearVelocity.y < velocidadTerminal)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, velocidadTerminal, rb.linearVelocity.z);
    }

    public void DesbloquearMovimiento()
    {
        controlesBloqueados = false;
        rb.isKinematic = false;
        estadoActual = Estados.Idle;
    }

    public void BloquearMovimiento()
    {
        controlesBloqueados = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        estadoActual = Estados.Idle;
    }

    // --- NUEVA FUNCIÓN ---
    public void BloquearPorMuerte()
    {
        controlesBloqueados = true;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        estadoActual = Estados.Die; // Forzamos el estado de muerte
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