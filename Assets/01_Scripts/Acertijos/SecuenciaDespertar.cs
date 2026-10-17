// =====================================================================
// SecuenciaDespertar.cs               MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// PARTE 2 completa (escena "Despertar"). No necesita GameManager:
// funciona sola.
//
// Muestra una lista de "pantallas" (imagen + texto + sonido). Con cada
// clic avanza. Después de la última:
//   - Corte a NEGRO instantáneo + sonido del gato.
//   - Textos finales letra por letra.
//   - Botón "Volver al inicio".
//
// Pantallas sugeridas (en orden):
//   0  Cuarto, sábanas mojadas      "...no otra vez."
//   1  Celular                      (mensaje de la abuela de hace 4 días)
//   2  Puerta entreabierta          "Volviste, wawa..." (susurro)
//   3  Abuela de espaldas + gato    "¿Abuela?"   ← el clic aquí es el final
// =====================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class PantallaDespertar
{
    [Tooltip("Objeto con la imagen de esta pantalla (se prende y se apaga).")]
    public GameObject imagen;
    [TextArea]
    public string texto;
    public AudioClip sonido;
    [Tooltip("Si está marcado, espera un clic. Si no, espera 'Segundos Automatico'.")]
    public bool esperarClic = true;
    public float segundosAutomatico = 3f;
}

public class SecuenciaDespertar : MonoBehaviour
{
    [Header("Pantallas")]
    public PantallaDespertar[] pantallas;

    [Header("Interfaz")]
    public TextMeshProUGUI textoSubtitulo;
    [Tooltip("Image que cubre toda la pantalla (para blanco al empezar y negro al final).")]
    public Image imagenFundido;
    public GameObject indicadorClic;

    [Header("Sonido")]
    public AudioSource fuente;
    public AudioClip ambienteLoop;
    public AudioClip sonidoGatoFinal;

    [Header("Final")]
    public GameObject panelFinal;
    public TextMeshProUGUI textoFinal;
    [TextArea(2, 5)]
    public string[] lineasFinales;
    public float segundosPorLetra = 0.045f;
    public float segundosEntreLineas = 2.5f;
    public GameObject botonVolverAlInicio;

    private void Start()
    {
        foreach (PantallaDespertar pantalla in pantallas)
        {
            if (pantalla.imagen != null)
            {
                pantalla.imagen.SetActive(false);
            }
        }
        if (panelFinal != null)
        {
            panelFinal.SetActive(false);
        }
        if (botonVolverAlInicio != null)
        {
            botonVolverAlInicio.SetActive(false);
        }
        if (indicadorClic != null)
        {
            indicadorClic.SetActive(false);
        }
        if (textoSubtitulo != null)
        {
            textoSubtitulo.text = "";
        }

        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        // Empieza en BLANCO (viene del fundido blanco de la Parte 1).
        PonerFundido(Color.white, 1f);

        if (fuente != null && ambienteLoop != null)
        {
            fuente.clip = ambienteLoop;
            fuente.loop = true;
            fuente.Play();
        }

        GameObject imagenAnterior = null;

        for (int i = 0; i < pantallas.Length; i++)
        {
            PantallaDespertar pantalla = pantallas[i];

            if (imagenAnterior != null)
            {
                imagenAnterior.SetActive(false);
            }
            if (pantalla.imagen != null)
            {
                pantalla.imagen.SetActive(true);
            }
            imagenAnterior = pantalla.imagen;

            if (textoSubtitulo != null)
            {
                textoSubtitulo.text = pantalla.texto;
            }

            if (i == 0)
            {
                yield return StartCoroutine(Fundir(Color.white, 0f, 2f));
            }

            if (fuente != null && pantalla.sonido != null)
            {
                fuente.PlayOneShot(pantalla.sonido);
            }

            if (pantalla.esperarClic)
            {
                yield return StartCoroutine(EsperarClic());
            }
            else
            {
                yield return new WaitForSeconds(pantalla.segundosAutomatico);
            }
        }

        // ---------- EL FINAL ----------
        // Corte a negro INSTANTÁNEO (sin fundido: tiene que asustar).
        PonerFundido(Color.black, 1f);
        if (textoSubtitulo != null)
        {
            textoSubtitulo.text = "";
        }
        if (fuente != null)
        {
            fuente.Stop();
            if (sonidoGatoFinal != null)
            {
                fuente.PlayOneShot(sonidoGatoFinal);
            }
        }

        yield return new WaitForSeconds(4f);

        // Textos finales, letra por letra.
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
        }

        if (textoFinal != null)
        {
            string textoAcumulado = "";
            foreach (string linea in lineasFinales)
            {
                for (int letra = 0; letra < linea.Length; letra++)
                {
                    textoFinal.text = textoAcumulado + linea.Substring(0, letra + 1);
                    yield return new WaitForSeconds(segundosPorLetra);
                }
                textoAcumulado = textoAcumulado + linea + "\n\n";
                textoFinal.text = textoAcumulado;
                yield return new WaitForSeconds(segundosEntreLineas);
            }
        }

        if (botonVolverAlInicio != null)
        {
            botonVolverAlInicio.SetActive(true);
        }
    }

    private IEnumerator EsperarClic()
    {
        yield return new WaitForSeconds(0.5f);

        if (indicadorClic != null)
        {
            indicadorClic.SetActive(true);
        }

        while (EntradaMouse.SeHizoClicEsteFrame() == false)
        {
            yield return null;
        }

        if (indicadorClic != null)
        {
            indicadorClic.SetActive(false);
        }

        yield return null;
    }

    private void PonerFundido(Color color, float alfa)
    {
        if (imagenFundido != null)
        {
            imagenFundido.color = new Color(color.r, color.g, color.b, alfa);
        }
    }

    private IEnumerator Fundir(Color color, float alfaFinal, float duracion)
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
            PonerFundido(color, Mathf.Lerp(alfaInicial, alfaFinal, progreso));
            yield return null;
        }
        PonerFundido(color, alfaFinal);
    }

    // Lo llama el botón "Volver al inicio" (OnClick).
    public void VolverAlInicio()
    {
        SceneManager.LoadScene(0);
    }
}
