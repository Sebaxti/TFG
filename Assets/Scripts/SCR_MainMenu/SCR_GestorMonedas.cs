using UnityEngine;
using TMPro;

public class SCR_GestorMonedas : MonoBehaviour
{
    public static SCR_GestorMonedas Instancia;

    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI textoContador;
    private int monedasTotales = 0;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
    }

    public void SumarMoneda(int cantidad)
    {
        monedasTotales += cantidad;
        ActualizarTexto();
    }

    public void ResetearMonedas()
    {
        monedasTotales = 0;
        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        if (textoContador != null)
            textoContador.text = monedasTotales.ToString();
    }
}