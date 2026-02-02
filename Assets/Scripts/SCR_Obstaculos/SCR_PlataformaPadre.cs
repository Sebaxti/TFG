using UnityEngine;

public class SCR_PlataformaPadre : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        // Cuando el jugador toca la plataforma, hacerlo hijo
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Cuando el jugador sale de la plataforma, liberarlo
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}