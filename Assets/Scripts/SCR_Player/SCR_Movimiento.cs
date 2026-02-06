using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SCR_Movimiento : MonoBehaviour
{
    [Header("Referencias Inputs")]
    [SerializeField] private string nombreAccionMover = "Move";
    [SerializeField] private string nombreAccionSaltar = "Jump";

    [Header("Configuración Movimiento")]
    [SerializeField] private float velocidadMovimiento = 8f;
    [SerializeField] private float suavizadoMovimiento = 15f;
    [SerializeField] private float suavizadoAire = 5f;
    [SerializeField] private float velocidadRotacion = 20f;

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 14f;
    [SerializeField] private float gravedadAscenso = 2.5f;
    [SerializeField] private float multiplicadorCaida = 4.5f;
    [SerializeField] private float multiplicadorSaltoCorto = 4f;
    [SerializeField] private float velocidadTerminal = -20f;
    [SerializeField] private int saltosMaximos = 2;

    [Header("Detección de Suelo")]
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private float longitudRayoSuelo = 1.1f;

    [Header("Refinamiento")]
    [SerializeField] private float suavizadoAterrizaje = 5f;
    [SerializeField] private float tiempoCoyote = 0.15f;
    [SerializeField] private float tiempoBufferSalto = 0.1f;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Vector2 inputVector;
    private bool enSuelo;
    private int saltosRestantes;
    private Transform camaraTransform;
    private float contadorCoyote;
    private float contadorBufferSalto;

    // Variables de plataforma (Detectadas solo desde arriba vía Raycast)
    private Transform plataformaActual;
    private Vector3 posicionPreviaPlataforma;
    private Vector3 velocidadPlataforma;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        camaraTransform = Camera.main.transform;

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        moveAction = playerInput.actions[nombreAccionMover];
        jumpAction = playerInput.actions[nombreAccionSaltar];
    }

    private void OnEnable() => playerInput.actions.Enable();
    private void OnDisable() => playerInput.actions.Disable();

    private void Update()
    {
        inputVector = moveAction.ReadValue<Vector2>();
        VerificarSuelo();

        if (enSuelo) contadorCoyote = tiempoCoyote;
        else contadorCoyote -= Time.deltaTime;

        if (jumpAction.WasPressedThisFrame()) contadorBufferSalto = tiempoBufferSalto;
        else contadorBufferSalto -= Time.deltaTime;

        if (contadorBufferSalto > 0)
        {
            if (contadorCoyote > 0 || (jumpAction.WasPressedThisFrame() && saltosRestantes > 0))
            {
                RealizarSalto();
            }
        }
    }

    private void FixedUpdate()
    {
        CalcularMovimientoPlataforma();
        ProcesarMovimiento();
        ProcesarRotacion();
        AplicarFisicaSalto();
    }

    private void CalcularMovimientoPlataforma()
    {
        // Solo si estamos realmente apoyados encima (enSuelo) calculamos la velocidad
        if (enSuelo && plataformaActual != null)
        {
            velocidadPlataforma = (plataformaActual.position - posicionPreviaPlataforma) / Time.fixedDeltaTime;
            posicionPreviaPlataforma = plataformaActual.position;
        }
        else
        {
            velocidadPlataforma = Vector3.zero;
            // Reseteamos plataformaActual si no hay suelo para evitar arrastres fantasma
            if (!enSuelo) plataformaActual = null;
        }
    }

    private void ProcesarMovimiento()
    {
        Vector3 forward = camaraTransform.forward;
        Vector3 right = camaraTransform.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 direccionDeseada = (forward * inputVector.y + right * inputVector.x).normalized;
        Vector3 velocidadObjetivo = direccionDeseada * velocidadMovimiento;

        // --- VELOCIDAD RELATIVA ---
        // Restamos la velocidad de la plataforma para que el Lerp no se vuelva loco
        Vector3 velocidadRelativa = rb.linearVelocity - velocidadPlataforma;

        float lerpUso = enSuelo ? (inputVector.sqrMagnitude > 0.01f ? 50f : suavizadoAterrizaje) : suavizadoAire;

        float nuevaX = Mathf.Lerp(velocidadRelativa.x, velocidadObjetivo.x, lerpUso * Time.fixedDeltaTime);
        float nuevaZ = Mathf.Lerp(velocidadRelativa.z, velocidadObjetivo.z, lerpUso * Time.fixedDeltaTime);

        // Sumamos la velocidad de la plataforma al final para movernos CON ella
        rb.linearVelocity = new Vector3(nuevaX + velocidadPlataforma.x, rb.linearVelocity.y, nuevaZ + velocidadPlataforma.z);
    }

    private void ProcesarRotacion()
    {
        if (inputVector.sqrMagnitude < 0.01f) return;
        Vector3 forward = camaraTransform.forward;
        Vector3 right = camaraTransform.right;
        forward.y = 0f; right.y = 0f;
        Vector3 direccion = (forward * inputVector.y + right * inputVector.x).normalized;

        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            rb.rotation = Quaternion.Slerp(rb.rotation, rotacionObjetivo, velocidadRotacion * Time.fixedDeltaTime);
        }
    }

    private void RealizarSalto()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        saltosRestantes--;
        contadorCoyote = 0;
        contadorBufferSalto = 0;
        enSuelo = false;
        plataformaActual = null; // Al saltar nos desvinculamos
    }

    private void AplicarFisicaSalto()
    {
        float velY = rb.linearVelocity.y;
        if (velY > 0)
        {
            float multi = !jumpAction.IsPressed() ? multiplicadorSaltoCorto : (velY < 2f ? gravedadAscenso * 0.5f : gravedadAscenso);
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multi - 1) * Time.fixedDeltaTime;
        }
        else if (velY < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (multiplicadorCaida - 1) * Time.fixedDeltaTime;
            if (rb.linearVelocity.y < velocidadTerminal) rb.linearVelocity = new Vector3(rb.linearVelocity.x, velocidadTerminal, rb.linearVelocity.z);
        }
    }

    private void VerificarSuelo()
    {
        bool estabaEnSuelo = enSuelo;

        // Lanzamos el rayo un poco más corto y centrado
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, longitudRayoSuelo, capaSuelo))
        {
            // VERIFICACIÓN DE ÁNGULO: Solo es suelo si la superficie es plana (ángulo menor a 45 grados)
            if (Vector3.Angle(hit.normal, Vector3.up) < 45f)
            {
                enSuelo = true;
                if (plataformaActual != hit.collider.transform)
                {
                    plataformaActual = hit.collider.transform;
                    posicionPreviaPlataforma = plataformaActual.position;
                }
            }
            else // Si es una pared o lateral, no somos "hijos" ni estamos en el suelo
            {
                enSuelo = false;
                plataformaActual = null;
            }
        }
        else
        {
            enSuelo = false;
            plataformaActual = null;
        }

        if (enSuelo && !estabaEnSuelo) saltosRestantes = saltosMaximos - 1;
    }
}