using UnityEngine;

public class SCR_AnimacionesJugador : MonoBehaviour
{
    public enum EstiloNivel { Nivel1 = 1, Nivel2 = 2 }

    [Header("Configuraci�n de Nivel")]

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

        if (scriptMovimiento != null) estadoAnterior = scriptMovimiento.estadoActual;

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
        animador.ResetTrigger("tSalto");
        animador.ResetTrigger("tDobleSalto");
        animador.ResetTrigger("tEspera");
        animador.ResetTrigger("tMuerte");

        switch (nuevoEstado)
        {
            case SCR_Movimiento.Estados.Idle:
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsFalling", false);
                break;

            case SCR_Movimiento.Estados.Move:
                animador.SetFloat("EstiloCorrer", (float)estiloDeCorrer);

                animador.SetBool("bIsRunning", true);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsFalling", false);
                break;

            case SCR_Movimiento.Estados.Jump:
                int saltoElegido = Random.Range(1, 3);
                animador.SetInteger("IndiceSalto", saltoElegido);

                animador.SetTrigger("tSalto");

                animador.SetBool("bIsJumping", true);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsFalling", false);
                break;

            case SCR_Movimiento.Estados.DoubleJump:

                animador.SetTrigger("tDobleSalto");

                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", true);
                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsFalling", false);
                break;

            case SCR_Movimiento.Estados.Fall:
                animador.SetBool("bIsFalling", true);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsRunning", false);
                break;

            case SCR_Movimiento.Estados.Die:
                animador.SetTrigger("tMuerte");

                animador.SetBool("bIsRunning", false);
                animador.SetBool("bIsJumping", false);
                animador.SetBool("bIsDoubleJumping", false);
                animador.SetBool("bIsFalling", false);
                break;

            case SCR_Movimiento.Estados.IdleWait:
                animador.SetTrigger("tEspera");

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

        animador.Play("Idle_001", 0, 0f);

        animador.ResetTrigger("tSalto");
        animador.ResetTrigger("tDobleSalto");
        animador.ResetTrigger("tEspera");
        animador.ResetTrigger("tMuerte");
        animador.SetBool("bIsRunning", false);
        animador.SetBool("bIsJumping", false);
        animador.SetBool("bIsDoubleJumping", false);
        animador.SetBool("bIsFalling", false);

        estadoAnterior = SCR_Movimiento.Estados.Idle;
    }
}