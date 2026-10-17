// =====================================================================
// Radio.cs                            MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// La radio vieja de la cocina. Cada clic cambia de frecuencia.
// En las frecuencias incorrectas suena estática. En la correcta se
// escucha a la abuela: "¿Por qué nadie viene, wawa?... cuatro... uno... nueve..."
// y se ejecuta "Al Sintonizar" (por ejemplo, abrir la tapa de la radio y
// mostrar el tercer pedazo de la escalera de pan).
//
// Necesita: BoxCollider2D y un AudioSource propio (para la estática en loop).
// =====================================================================

using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class Radio : MonoBehaviour, IClickeable
{
    [Header("Dial")]
    public string[] textosFrecuencias = { "88.5", "92.1", "96.3", "101.7", "105.9" };
    [Tooltip("Posición de la frecuencia correcta en la lista (empieza en 0).")]
    public int indiceCorrecto = 3;
    public TextMeshPro textoDial;

    [Header("Sonido")]
    public AudioSource fuenteRadio;
    public AudioClip sonidoEstatica;
    public AudioClip vozAbuela;
    public AudioClip sonidoClicPerilla;

    [Header("Textos")]
    [TextArea]
    public string mensajeEstatica = "Solo se escucha estática...";
    [TextArea]
    public string mensajeSintonizada;

    [Header("Lo que pasa al sintonizar")]
    public UnityEvent alSintonizar;

    private int indiceActual = -1; // -1 = apagada
    private bool sintonizada = false;

    private void Start()
    {
        if (textoDial != null)
        {
            textoDial.text = "---";
        }
    }

    public void AlHacerClic()
    {
        // Si ya se sintonizó, repetir la voz y el mensaje.
        if (sintonizada)
        {
            if (fuenteRadio != null && vozAbuela != null)
            {
                fuenteRadio.Stop();
                fuenteRadio.PlayOneShot(vozAbuela);
            }
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.MostrarMensaje(mensajeSintonizada);
            }
            return;
        }

        // Pasar a la siguiente frecuencia.
        indiceActual = indiceActual + 1;
        if (indiceActual >= textosFrecuencias.Length)
        {
            indiceActual = 0;
        }

        if (textoDial != null)
        {
            textoDial.text = textosFrecuencias[indiceActual];
        }

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ReproducirSonido(sonidoClicPerilla);
        }

        if (indiceActual == indiceCorrecto)
        {
            Sintonizar();
        }
        else
        {
            PonerEstatica();
        }
    }

    private void PonerEstatica()
    {
        if (fuenteRadio != null && sonidoEstatica != null)
        {
            fuenteRadio.clip = sonidoEstatica;
            fuenteRadio.loop = true;
            if (fuenteRadio.isPlaying == false)
            {
                fuenteRadio.Play();
            }
        }
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.MostrarMensaje(mensajeEstatica);
        }
    }

    private void Sintonizar()
    {
        sintonizada = true;

        if (fuenteRadio != null)
        {
            fuenteRadio.Stop();
            fuenteRadio.loop = false;
            if (vozAbuela != null)
            {
                fuenteRadio.PlayOneShot(vozAbuela);
            }
        }

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.MostrarMensaje(mensajeSintonizada);
        }

        if (alSintonizar != null)
        {
            alSintonizar.Invoke();
        }
    }
}
