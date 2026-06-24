using UnityEngine;

public class SCR_Moneda : MonoBehaviour
{
    [Header("Animación Visual")]
    [SerializeField] private float velocidadRotacion = 100f;
    [SerializeField] private float amplitudOscilacion = 0.5f;
    [SerializeField] private float velocidadOscilacion = 2f;

    private float posicionInicialY;

    private void Start()
    {
        posicionInicialY = transform.position.y;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime, Space.World);
        float nuevaY = posicionInicialY + Mathf.Sin(Time.time * velocidadOscilacion) * amplitudOscilacion;
        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SCR_GestorMonedas.Instancia != null)
            {
                SCR_GestorMonedas.Instancia.SumarMoneda(1);
            }
            Destroy(gameObject);
        }
    }
}