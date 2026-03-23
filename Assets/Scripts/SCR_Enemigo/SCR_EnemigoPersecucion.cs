using UnityEngine;

public class SCR_EnemigoPersecucion : MonoBehaviour
{
    [SerializeField] private float velocidad = 7f;
    [SerializeField] private Vector3 direccion = new Vector3(0, 0, -1);
    private SCR_Movimiento playerScript;

    private void OnEnable() => SCR_Movimiento.OnPlayerRespawn += ResetPosicion;
    private void OnDisable() => SCR_Movimiento.OnPlayerRespawn -= ResetPosicion;

    private void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<SCR_Movimiento>();
    }

    private void FixedUpdate()
    {
        transform.Translate(direccion.normalized * velocidad * Time.fixedDeltaTime);
    }

    private void ResetPosicion()
    {
        transform.position = playerScript.GetEnemigoRespawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SCR_Movimiento>().Respawn();
        }
    }
}