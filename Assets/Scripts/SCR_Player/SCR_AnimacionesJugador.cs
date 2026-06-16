using UnityEngine;

public class SCR_AnimacionesJugador : MonoBehaviour
{
    // Creamos una lista desplegable para el Inspector
    public enum EstiloNivel { Nivel1 = 1, Nivel2 = 2 }

    [Header("Configuración de Nivel")]
    [Tooltip("Elige qué animación de correr se usará en esta escena")]

    //Esto es para elegir el correr diferente en lvl1 y diferente en lvl2
    public EstiloNivel estiloDeCorrer = EstiloNivel.Nivel1;

    [Header("Referencias")]
    [SerializeField] private SCR_Movimiento scriptMovimiento;
    [SerializeField] private Animator animador;
    [SerializeField] private GameObject objetoAlas;

    private SCR_Movimiento.Estados estadoAnterior;

    private void Start()
    {
        if (scriptMovimiento == null) scriptMovimiento = GetComponent<SCR_Movimiento>();
        if (animador == null) animador = GetComponentInChildren<Animator>();

        if (objetoAlas != null) objetoAlas.SetActive(false);
        if (scriptMovimiento != null) estadoAnterior = scriptMovimiento.estadoActual;

        // Le enviamos al Animator la elección del Inspector nada más arrancar
        if (animador != null)
        {
            animador.SetFloat("EstiloCorrer", (float)estiloDeCorrer);
        }
    }

    private void LateUpdate()
    {
        if (scriptMovimiento == null || animador == null) return;

        SCR_Movimiento.Estados estadoActual = scriptMovimiento.estadoActual;

        if (estadoActual != estadoAnterior)
        {
            CambiarAnimacion(estadoActual);
            estadoAnterior = estadoActual;
        }
    }

    private void CambiarAnimacion(SCR_Movimiento.Estados nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case SCR_Movimiento.Estados.Idle:
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false); // Apagamos doble salto
                animador.SetBool("bIsFalling", false);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.Move:
                animador.SetFloat("EstiloCorrer", (float)estiloDeCorrer);

                animador.SetBool("bIsRunning", true);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false); // Apagamos doble salto
                animador.SetBool("bIsFalling", false);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.Jump:
                // Dado aleatorio solo para el primer salto
                int saltoElegido = Random.Range(1, 3);
                animador.SetInteger("IndiceSalto", saltoElegido);

                animador.SetBool("bIsJumping", true);
                animador.SetBool("bIsDoubleJumping", false); // Aseguramos que el doble salto esté apagado
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsFalling", false);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.DoubleJump:
                // APAGAMOS el salto normal y ENCENDEMOS el doble
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", true);
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsFalling", false);
                if (objetoAlas) objetoAlas.SetActive(true);
                break;

            case SCR_Movimiento.Estados.Fall:
                animador.SetBool("bIsFalling", true);
                animador.SetBool("bIsRunning", false);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;
        }
    }
}