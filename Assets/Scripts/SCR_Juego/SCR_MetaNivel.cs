using UnityEngine;

public class SCR_MetaNivel : MonoBehaviour
{
    [Tooltip("Índice de este nivel (Nivel 1 = 0, Nivel 2 = 1...)")]
    public int indiceNivelActual = 0;
    private bool tocado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tocado)
        {
            tocado = true;

            other.GetComponent<SCR_Movimiento>()?.BloquearMovimiento();

            if (SCR_GestorNiveles.Instancia != null)
            {
                SCR_GestorNiveles.Instancia.AvanzarDesdeNivel(indiceNivelActual);
            }
        }
    }
}