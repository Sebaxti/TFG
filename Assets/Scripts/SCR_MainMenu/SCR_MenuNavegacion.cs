using UnityEngine;
using UnityEngine.Events;

public class SCR_MenuNavegacion : MonoBehaviour
{
    [System.Serializable]
    public struct OpcionMenu
    {
        public string nombre;
        public RectTransform transformObjeto;
        public UnityEvent accionAlSeleccionar;
    }

    [Header("Opciones del Menú")]
    public OpcionMenu[] opciones;

    [Header("Ajustes del Cilindro 3D")]
    public float radio = 150f;
    public float separacionAngular = 90f;
    public float velocidadGiro = 15f;
    public float escalaMinima = 0.5f;

    private int indiceActual = 0;
    private float posicionScroll = 0f;
    private float posicionScrollObjetivo = 0f;

    private void Start()
    {
        foreach (var op in opciones)
        {
            if (op.transformObjeto != null && op.transformObjeto.GetComponent<CanvasGroup>() == null)
            {
                op.transformObjeto.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) NavegarAbajo();
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) NavegarArriba();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) EjecutarSeleccion();

        posicionScroll = Mathf.Lerp(posicionScroll, posicionScrollObjetivo, Time.deltaTime * velocidadGiro);

        float lengthF = opciones.Length;
        float mitadF = lengthF / 2f;

        for (int i = 0; i < opciones.Length; i++)
        {
            if (opciones[i].transformObjeto == null) continue;

            float distanciaFloat = i - posicionScroll;
            float distanciaEnvuelve = ((distanciaFloat + mitadF) % lengthF + lengthF) % lengthF - mitadF;

            float anguloRelativo = distanciaEnvuelve * separacionAngular;

            float yPos = -radio * Mathf.Sin(anguloRelativo * Mathf.Deg2Rad);
            opciones[i].transformObjeto.anchoredPosition = new Vector2(0, yPos);

            opciones[i].transformObjeto.localEulerAngles = new Vector3(-anguloRelativo, 0, 0);

            float factorLejania = Mathf.Clamp01(Mathf.Abs(distanciaEnvuelve));
            float escala = Mathf.Lerp(1f, escalaMinima, factorLejania);
            opciones[i].transformObjeto.localScale = new Vector3(escala, escala, 1f);

            CanvasGroup cg = opciones[i].transformObjeto.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, factorLejania);
            }
        }
    }

    public void NavegarAbajo()
    {
        indiceActual++;
        if (indiceActual >= opciones.Length) indiceActual = 0;
        posicionScrollObjetivo += 1f;
    }

    public void NavegarArriba()
    {
        indiceActual--;
        if (indiceActual < 0) indiceActual = opciones.Length - 1;
        posicionScrollObjetivo -= 1f;
    }

    public void EjecutarSeleccion()
    {
        if (opciones.Length > 0 && opciones[indiceActual].accionAlSeleccionar != null)
        {
            opciones[indiceActual].accionAlSeleccionar.Invoke();
        }
    }
}