using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SCR_Movimiento : MonoBehaviour
{
    [Header("Configuración de movimiento")]
    [SerializeField] private float velocidadMovimiento;
    [SerializeField] private float aceleracion;
    [SerializeField] private float desaceleracion;

    [Header("Configuración de rotación")]
    [SerializeField] private float velocidadRotacion;

    [Header("Configuración de Salto")]
    [SerializeField] private float fuerzaSalto;
    [SerializeField] private float gravedad;
    [SerializeField] private float cayoteTiempo; //Esta es como un tiempo en que sales de la plataforma y aun puedes saltar (para que no se corte al instante la opcion de salto
    [SerializeField] private float bufferSaltoTiempo; //Esta es para cuando vas cayendoo y das espacio registre el salto antes de tocar el suelo (Bunny hop)

    [Header("Doble salto")]
    [SerializeField] private bool dobleSaltoHabilitado;
    [SerializeField] private float fuerzaDobleSalto = 9f;

    private CharacterController controlador;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector3 velocidadActual;
    private Vector2 moveInput;
    private float velocidadVertical;

    private bool enSuelo;
    private bool estabaEnSuelo;
    private float tiempoUltimoSuelo;
    private float tiempoUltimoSalto;
    private int saltosRestantes;

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();


        //obtener accion de movimiento del Input System
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];

    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {
        DetectarSuelo();
        MovimientoContolador();
        SaltoControlador();
        AplicarGravedad();
        AplicarMovimiento();
        RotacionControlador();
    }

    private void MovimientoContolador()
    {
        moveInput = moveAction.ReadValue<Vector2>();
    }

    private void AplicarMovimiento()
    {
        Vector3 direccionObjetivo = ObtenerDireccionMovimiento();
        Vector3 velocidadObjetivo = direccionObjetivo * velocidadMovimiento;

        //Interpolación suave entre velocidades

        float lerpVelocidad = direccionObjetivo.magnitude > 0 ? aceleracion : desaceleracion;
        velocidadActual = Vector3.Lerp (velocidadActual, velocidadObjetivo, lerpVelocidad * Time.deltaTime);

        Vector3 movimientoFinal = velocidadActual + Vector3.up * velocidadVertical;
        controlador.Move ( movimientoFinal * Time.deltaTime);
    }

    private Vector3 ObtenerDireccionMovimiento()
    {
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        //proyección en el plano horizontal
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        return(forward * moveInput.y + right * moveInput.x).normalized;
    }

    private void RotacionControlador()
    {
        if (velocidadActual.magnitude < 0.1f) return;

        Vector3 direccionObjetivo = new Vector3(velocidadActual.x,0f,velocidadActual.z);
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionObjetivo);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
            );
    }

    private void DetectarSuelo() 
    {
        enSuelo= controlador.isGrounded;

        if (enSuelo)
        {
            saltosRestantes = dobleSaltoHabilitado ? 2 : 1;
        }

        if (estabaEnSuelo && !enSuelo) 
        {
            tiempoUltimoSuelo = Time.time;
        }

        estabaEnSuelo = enSuelo;
    }

    private void SaltoControlador() 
    {
        //Registra intento de saltar (jumpe buffer)
        if (jumpAction.WasPressedThisFrame()) 
        {
            tiempoUltimoSalto = Time.time;
        }

        // Cayote Time
        float tiempoDesdeQueDejeSuelo = Time.time - tiempoUltimoSuelo;
        bool cayoteDisponible = !enSuelo && tiempoDesdeQueDejeSuelo < cayoteTiempo;

        // Jump Buffer
        float tiempoDesdeUltimoInput = Time.time - tiempoUltimoSalto;
        bool inputReciente = tiempoDesdeUltimoInput < bufferSaltoTiempo;

        // Condiciones para saltar
        bool primerSalto = (enSuelo || cayoteDisponible) && saltosRestantes > 0;
        bool dobleSalto = !enSuelo && !cayoteDisponible && saltosRestantes > 0 && dobleSaltoHabilitado;

        if (inputReciente && (primerSalto || dobleSalto))
        {
            EjecutarSalto(dobleSalto);
            tiempoUltimoSalto = -999f; // Invalida el buffer
        }

        // Salto variable: soltar espacio = caer más rápido
        if (jumpAction.WasReleasedThisFrame() && velocidadVertical > 0f)
        {
            velocidadVertical *= 0.5f;
        }
    }

    private void EjecutarSalto(bool esDobleSalto)
    {
        // Aplicar la fuerza correspondiente
        velocidadVertical = esDobleSalto ? fuerzaDobleSalto : fuerzaSalto;

        // Consumir un salto
        saltosRestantes--;
    }

    private void AplicarGravedad()
    {
        if (enSuelo && velocidadVertical < 0f)
        {
            velocidadVertical = -2f; // Pequeña fuerza para mantener pegado al suelo
        }
        else
        {
            // Aplicar gravedad constante
            velocidadVertical += gravedad * Time.deltaTime;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = enSuelo ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}
