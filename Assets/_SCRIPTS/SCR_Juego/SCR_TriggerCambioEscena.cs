using UnityEngine;

public class SCR_TriggerCambioEscena : MonoBehaviour
{
    [Header("Configuración de Meta")]
    [Tooltip("El índice de este nivel en la lista del Gestor de Niveles")]
    [SerializeField] private int indiceDeEsteNivel;
    private bool tocado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tocado)
        {
            tocado = true;

            SCR_Movimiento mov = other.GetComponent<SCR_Movimiento>();
            if (mov != null) mov.BloquearMovimiento();

            if (SCR_GestorNiveles.Instancia != null)
            {
                SCR_GestorNiveles.Instancia.AvanzarDesdeNivel(indiceDeEsteNivel);
            }
        }
    }
}