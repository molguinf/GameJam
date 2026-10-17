// =====================================================================
// Fundido.cs                             MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// Pone la pantalla en negro (o blanco) de a poco, y la vuelve a aclarar.
// Va en una Image que cubre TODA la pantalla, al final del Canvas
// (para que quede encima de todo). Desmarcar "Raycast Target".
//
// Uso desde otro script:
//   yield return StartCoroutine(Fundido.Instancia.FundirA(Color.black, 1f, 2f));  // a negro en 2 s
//   yield return StartCoroutine(Fundido.Instancia.FundirA(Color.black, 0f, 1f));  // aclarar en 1 s
// =====================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fundido : MonoBehaviour
{
    public static Fundido Instancia;

    public Image imagenFundido;

    [Tooltip("Si está marcado, el juego empieza en negro y se aclara solo.")]
    public bool empezarEnNegro = true;
    public float segundosAclararAlEmpezar = 1.5f;

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        if (imagenFundido == null)
        {
            return;
        }

        if (empezarEnNegro)
        {
            PonerColorInmediato(Color.black, 1f);
            StartCoroutine(FundirA(Color.black, 0f, segundosAclararAlEmpezar));
        }
        else
        {
            PonerColorInmediato(Color.black, 0f);
        }
    }

    public void PonerColorInmediato(Color color, float alfa)
    {
        if (imagenFundido == null)
        {
            return;
        }
        Color nuevoColor = new Color(color.r, color.g, color.b, alfa);
        imagenFundido.color = nuevoColor;
    }

    public IEnumerator FundirA(Color color, float alfaFinal, float duracion)
    {
        if (imagenFundido == null)
        {
            yield break;
        }

        float alfaInicial = imagenFundido.color.a;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo = tiempo + Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempo / duracion);
            float alfaActual = Mathf.Lerp(alfaInicial, alfaFinal, progreso);
            imagenFundido.color = new Color(color.r, color.g, color.b, alfaActual);
            yield return null;
        }

        imagenFundido.color = new Color(color.r, color.g, color.b, alfaFinal);
    }
}
