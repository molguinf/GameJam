// =====================================================================
// CerraduraCombinacion.cs             MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Revisa si las ruedas están en la combinación correcta.
//
//   HORNO:  opciones = [sol, luna, estrella, corazon]
//           combinación correcta = 0, 1, 2   (sol, luna, estrella)
//
//   ROPERO: combinación correcta = 4, 1, 9
//
// Cuando se abre, ejecuta "Al Abrir" (UnityEvent): por ejemplo apagar el
// horno cerrado y prender el horno abierto con la t'antawawa adentro.
// =====================================================================

using UnityEngine;
using UnityEngine.Events;

public class CerraduraCombinacion : MonoBehaviour
{
    [Tooltip("Las ruedas EN ORDEN de izquierda a derecha.")]
    public RuedaCombinacion[] ruedas;

    [Tooltip("La posición correcta de cada rueda, en el mismo orden.")]
    public int[] combinacionCorrecta;

    [TextArea]
    public string mensajeAlAbrir;
    public AudioClip sonidoAlAbrir;

    public UnityEvent alAbrir;

    private bool estaAbierta = false;

    public bool EstaAbierta()
    {
        return estaAbierta;
    }

    public void Comprobar()
    {
        if (estaAbierta)
        {
            return;
        }

        if (ruedas.Length != combinacionCorrecta.Length)
        {
            Debug.LogWarning(name + ": la cantidad de ruedas no es igual a la cantidad de números de la combinación.");
            return;
        }

        for (int i = 0; i < ruedas.Length; i++)
        {
            if (ruedas[i].ObtenerIndiceActual() != combinacionCorrecta[i])
            {
                return;
            }
        }

        // Todas coinciden: se abre.
        estaAbierta = true;

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.MostrarMensaje(mensajeAlAbrir);
            GameManager.Instancia.ReproducirSonido(sonidoAlAbrir);
        }

        if (alAbrir != null)
        {
            alAbrir.Invoke();
        }
    }
}
