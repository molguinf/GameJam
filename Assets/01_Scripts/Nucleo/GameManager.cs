// =====================================================================
// GameManager.cs                         MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// Estado general del juego:
//   - Si el juego está bloqueado (intro, final) o se puede hacer clic.
//   - Mostrar mensajes en pantalla ("Una mesa para las almas...").
//   - Reproducir efectos de sonido cortos.
//
// Va en el objeto "GameManager" dentro del prefab Sistema.prefab.
// Hay UNO solo por escena (patrón Singleton: GameManager.Instancia).
// =====================================================================

using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Mensajes en pantalla")]
    [Tooltip("Panel de abajo donde aparecen los textos. Desmarcar Raycast Target en su Image.")]
    public GameObject panelMensaje;
    public TextMeshProUGUI textoMensaje;
    public float segundosMensajeVisible = 4f;

    [Header("Sonido")]
    public AudioSource fuenteEfectos;

    // Cuando es true, el ClickManager no deja hacer clic en nada.
    [HideInInspector]
    public bool juegoBloqueado = false;

    private Coroutine corrutinaMensaje;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Debug.LogWarning("Hay dos GameManager en la escena. Se destruye el segundo.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (panelMensaje != null)
        {
            panelMensaje.SetActive(false);
        }
    }

    // ---------------------------------------------------------------
    // MENSAJES
    // ---------------------------------------------------------------
    public void MostrarMensaje(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return;
        }
        if (panelMensaje == null || textoMensaje == null)
        {
            Debug.Log("MENSAJE: " + texto);
            return;
        }

        textoMensaje.text = texto;
        panelMensaje.SetActive(true);

        if (corrutinaMensaje != null)
        {
            StopCoroutine(corrutinaMensaje);
        }
        corrutinaMensaje = StartCoroutine(OcultarMensajeDespues());
    }

    private IEnumerator OcultarMensajeDespues()
    {
        yield return new WaitForSeconds(segundosMensajeVisible);
        panelMensaje.SetActive(false);
        corrutinaMensaje = null;
    }

    public void OcultarMensaje()
    {
        if (corrutinaMensaje != null)
        {
            StopCoroutine(corrutinaMensaje);
            corrutinaMensaje = null;
        }
        if (panelMensaje != null)
        {
            panelMensaje.SetActive(false);
        }
    }

    // ---------------------------------------------------------------
    // SONIDO
    // ---------------------------------------------------------------
    public void ReproducirSonido(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }
        if (fuenteEfectos == null)
        {
            Debug.LogWarning("GameManager no tiene AudioSource asignado en 'Fuente Efectos'.");
            return;
        }
        fuenteEfectos.PlayOneShot(clip);
    }

    // ---------------------------------------------------------------
    // BLOQUEO (para la intro y las secuencias del final)
    // ---------------------------------------------------------------
    public void BloquearJuego(bool bloquear)
    {
        juegoBloqueado = bloquear;
    }
}
