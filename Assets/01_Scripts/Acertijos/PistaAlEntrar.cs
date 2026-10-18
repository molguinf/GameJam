// =====================================================================
// PistaAlEntrar.cs                    MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Muestra una pista cortita cuando el jugador llega a una pared.
// Va en la raíz de cada Interactivos_ParedX (la arma el ConstructorB).
//
// Cada pista tiene condiciones:
//   - "Requiere Activo":   solo se muestra si ese objeto está prendido.
//   - "Requiere Inactivo": solo se muestra si ese objeto está apagado.
// Se muestra la PRIMERA pista de la lista que cumpla sus condiciones.
//
// Las rarezas van con "Solo Una Vez" marcado. Las pistas de acertijos
// pendientes se pueden repetir, pero no antes de "Segundos Para Repetir".
// =====================================================================

using System.Collections;
using UnityEngine;

[System.Serializable]
public class PistaPared
{
    [TextArea]
    public string mensaje;
    public GameObject requiereActivo;
    public GameObject requiereInactivo;
    public bool soloUnaVez = false;

    [HideInInspector] public bool yaMostrada = false;
    [HideInInspector] public float ultimaVez = -9999f;
}

public class PistaAlEntrar : MonoBehaviour
{
    public PistaPared[] pistas;

    [Tooltip("Espera antes de mostrar la pista (al girar, ControlParedes borra el mensaje).")]
    public float segundosDeEspera = 0.4f;
    public float segundosParaRepetir = 45f;

    private void OnEnable()
    {
        if (pistas != null && pistas.Length > 0)
        {
            StartCoroutine(MostrarPista());
        }
    }

    private IEnumerator MostrarPista()
    {
        yield return new WaitForSeconds(segundosDeEspera);

        // Durante la intro o el final no se muestran pistas.
        while (GameManager.Instancia != null && GameManager.Instancia.juegoBloqueado)
        {
            yield return null;
        }

        foreach (PistaPared pista in pistas)
        {
            if (pista == null || string.IsNullOrEmpty(pista.mensaje))
            {
                continue;
            }
            if (pista.soloUnaVez && pista.yaMostrada)
            {
                continue;
            }
            if (pista.requiereActivo != null && pista.requiereActivo.activeSelf == false)
            {
                continue;
            }
            if (pista.requiereInactivo != null && pista.requiereInactivo.activeSelf)
            {
                continue;
            }
            if (Time.time - pista.ultimaVez < segundosParaRepetir)
            {
                // Esta ya salió hace poco: no se repite, y tampoco se muestra otra.
                yield break;
            }

            pista.yaMostrada = true;
            pista.ultimaVez = Time.time;
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.MostrarMensaje(pista.mensaje);
            }
            yield break;
        }
    }
}
