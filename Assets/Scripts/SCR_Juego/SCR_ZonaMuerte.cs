using UnityEngine;

public class SCR_ZonaMuerte : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            other.GetComponent<SCR_Movimiento>().Respawn();
        }
    }
}