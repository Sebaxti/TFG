using UnityEngine;

public class SCR_AnimacionesJugador : MonoBehaviour
{
    // Creamos una lista desplegable para el Inspector
    public enum EstiloNivel { Nivel1 = 1, Nivel2 = 2 }

    [Header("Configuraci�n de Nivel")]
    [Tooltip("Elige qu� animaci�n de correr se usar� en esta escena")]

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

       //if (objetoAlas != null) objetoAlas.SetActive(false);
        if (scriptMovimiento != null) estadoAnterior = scriptMovimiento.estadoActual;

        // Le enviamos al Animator la elecci�n del Inspector nada m�s arrancar
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
        // Limpiar triggers pendientes para evitar que se acumulen y disparen en momentos incorrectos
        animador.ResetTrigger("tSalto");
        animador.ResetTrigger("tDobleSalto");
        animador.ResetTrigger("tEspera");
        animador.ResetTrigger("tMuerte");

        switch (nuevoEstado)
        {
            case SCR_Movimiento.Estados.Idle:
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false); // Apagamos doble salto
                animador.SetBool("bIsFalling", false);
                //if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.Move:
                animador.SetFloat("EstiloCorrer", (float)estiloDeCorrer);

                animador.SetBool("bIsRunning", true);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false); // Apagamos doble salto
                animador.SetBool("bIsFalling", false);
                //if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.Jump:
                // Dado aleatorio solo para el primer salto
                int saltoElegido = Random.Range(1, 3);
                animador.SetInteger("IndiceSalto", saltoElegido);

                animador.SetTrigger("tSalto");

                animador.SetBool("bIsJumping", true);
                animador.SetBool("bIsDoubleJumping", false); // Aseguramos que el doble salto est� apagado
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsFalling", false);
                //if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.DoubleJump:

                animador.SetTrigger("tDobleSalto");

                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", true);
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsFalling", false);
                //if (objetoAlas) objetoAlas.SetActive(true);
                break;

            case SCR_Movimiento.Estados.Fall:
                animador.SetBool("bIsFalling", true);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsRunning", false);
                //if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.Die:
                animador.SetTrigger("tMuerte");

                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsFalling", false);
                //if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.IdleWait:
                animador.SetTrigger("tEspera"); // Lanzamos el trigger de la animaci�n de aburrimiento

                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsFalling", false);
                break;
        }

    }

    public void ResetearAnimaciones()
    {
        if (animador == null) return;

        // Forzamos al Animator a regresar al estado "Idle" de golpe en el frame 0
        animador.Play("Idle_001", 0, 0f);

        // Limpiamos todos los par�metros para evitar que se queden atascados
        animador.ResetTrigger("tSalto");
        animador.ResetTrigger("tDobleSalto");
        animador.ResetTrigger("tEspera");
        animador.ResetTrigger("tMuerte");
        animador.SetBool("bIsRunning", false);
        animador.SetBool("bIsJumping", false);
        animador.SetBool("bIsDoubleJumping", false);
        animador.SetBool("bIsFalling", false);

        // Sincronizamos la m�quina de estados para el pr�ximo frame
        estadoAnterior = SCR_Movimiento.Estados.Idle;
    }
}