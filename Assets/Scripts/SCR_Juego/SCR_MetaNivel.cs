using UnityEngine;

public class SCR_MetaNivel : MonoBehaviour
{
    private bool tocado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !tocado)
        {
            tocado = true;
            other.GetComponent<SCR_Movimiento>()?.BloquearMovimiento();

            // Avanzamos automáticamente al siguiente nivel en la lista
            SCR_GestorNiveles.Instancia.AvanzarSiguienteNivel();
        }
    }
}