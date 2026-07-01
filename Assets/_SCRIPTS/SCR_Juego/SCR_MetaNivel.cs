using UnityEngine;

public class SCR_MetaNivel : MonoBehaviour
{
    public int indiceNivelActual = 0;
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
                SCR_GestorNiveles.Instancia.AvanzarDesdeNivel(indiceNivelActual);
            }
        }
    }
}