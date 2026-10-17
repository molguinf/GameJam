// =====================================================================
// SecuenciaFinParte1.cs               MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Fin del sueño. Se activa al darle la vuelta al retrato de la mesa:
//   1. Bloquea los clics.
//   2. El retrato muestra la foto de LA ABUELA.
//   3. Mensaje: "Es la abuela..."
//   4. El reloj da 12 campanadas.
//   5. La pantalla se pone blanca.
//   6. Carga la escena "Despertar" (Parte 2).
//
// La zona del retrato es una ZonaInteractiva. En su "Al Tener Exito"
// se arrastra este objeto y se elige SecuenciaFinParte1 > Iniciar.
// =====================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SecuenciaFinParte1 : MonoBehaviour
{
    [Header("Retrato")]
    public SpriteRenderer dibujoRetrato;
    public Sprite fotoAbuela;

    [TextArea]
    public string mensajeRetrato = "Es la abuela...";

    [Header("Campanadas")]
    public AudioClip sonidoCampanada;
    public int cantidadCampanadas = 12;
    public float segundosEntreCampanadas = 0.8f;

    [Header("Siguiente escena")]
    public string nombreEscenaDespertar = "Despertar";
    public float segundosFundidoBlanco = 2.5f;

    private bool yaEmpezo = false;

    public void Iniciar()
    {
        if (yaEmpezo)
        {
            return;
        }
        yaEmpezo = true;
        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.BloquearJuego(true);
        }

        // 1. Mostrar la foto de la abuela en el retrato.
        if (dibujoRetrato != null && fotoAbuela != null)
        {
            dibujoRetrato.sprite = fotoAbuela;
        }

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.MostrarMensaje(mensajeRetrato);
        }

        yield return new WaitForSeconds(3f);

        // 2. Campanadas del reloj.
        for (int i = 0; i < cantidadCampanadas; i++)
        {
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.ReproducirSonido(sonidoCampanada);
            }
            yield return new WaitForSeconds(segundosEntreCampanadas);
        }

        // 3. Fundido a blanco.
        if (Fundido.Instancia != null)
        {
            yield return StartCoroutine(Fundido.Instancia.FundirA(Color.white, 1f, segundosFundidoBlanco));
        }

        yield return new WaitForSeconds(0.5f);

        // 4. Parte 2.
        SceneManager.LoadScene(nombreEscenaDespertar);
    }
}
