using UnityEngine;

public class SCR_AnimacionesJugador : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Se asignará automáticamente si están en el mismo objeto")]
    [SerializeField] private SCR_Movimiento scriptMovimiento;
    [SerializeField] private Animator animador;

    [SerializeField] private GameObject objetoAlas;

    [Header("Nombres de Animaciones")]
    public string animIdle = "Idle";
    public string animMove = "Move";
    public string animJump = "Jump";
    public string animDoubleJump = "DoubleJump";
    public string animFall = "Fall";

    [Header("Ajustes")]
    [Tooltip("Tiempo que tarda en mezclar una animación con otra")]
    [SerializeField] private float transicionSuave = 0.1f;

    // Se guarda el estado anterior para saber cuándo hay un cambio
    private SCR_Movimiento.Estados estadoAnterior;

    private void Start()
    {
        // Autoconfigurar referencias si se nos olvida arrastrarlas en Unity
        if (scriptMovimiento == null) scriptMovimiento = GetComponent<SCR_Movimiento>();
        if (animador == null) animador = GetComponentInChildren<Animator>();

        // apagar alas inicio
        if (objetoAlas != null) objetoAlas.SetActive(false);

        if (scriptMovimiento != null) estadoAnterior = scriptMovimiento.estadoActual;
    }

    private void Update()
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
                animador.CrossFade(animIdle, transicionSuave);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.Move:
                animador.CrossFade(animMove, transicionSuave);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.Jump:
                animador.CrossFade(animJump, transicionSuave);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;

            case SCR_Movimiento.Estados.DoubleJump:
                animador.CrossFade(animDoubleJump, transicionSuave);
                if (objetoAlas) objetoAlas.SetActive(true);
                break;

            case SCR_Movimiento.Estados.Fall:
                animador.CrossFade(animFall, transicionSuave);
                if (objetoAlas) objetoAlas.SetActive(false);
                break;
        }
    }
}