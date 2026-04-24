using UnityEngine;

public class SCR_TriggerCambioEscena : MonoBehaviour
{
    [Header("Configuración de Meta")]
    [Tooltip("El índice de este nivel en la lista del Gestor de Niveles (Nivel 1 = 0, Nivel 2 = 1...)")]
    [SerializeField] private int indiceDeEsteNivel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Bloqueamos al jugador para que no se mueva durante el Fade
            other.GetComponent<SCR_Movimiento>()?.BloquearMovimiento();

            if (SCR_GestorNiveles.Instancia != null)
            {
                // Le decimos al Gestor de Niveles que hemos terminado este índice
                SCR_GestorNiveles.Instancia.AvanzarDesdeNivel(indiceDeEsteNivel);
            }
            else
            {
                Debug.LogError("¡No hay SCR_GestorNiveles en la escena!");
            }
        }
    }
}