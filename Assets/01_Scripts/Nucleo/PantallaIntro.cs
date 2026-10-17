// =====================================================================
// PantallaIntro.cs                       MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// Pantallas de texto al empezar el juego (fondo negro + texto).
// Cada clic pasa a la siguiente línea. Al terminar, se oculta el panel
// y el jugador ya puede hacer clic en el cuarto.
//
// La primera línea explica Todos Santos para los jurados de otros países.
// Los textos se escriben en el Inspector (campo "Lineas").
// =====================================================================

using System.Collections;
using UnityEngine;
using TMPro;

public class PantallaIntro : MonoBehaviour
{
    public GameObject panelIntro;
    public TextMeshProUGUI textoIntro;

    [TextArea(3, 6)]
    public string[] lineas;

    [Tooltip("Texto chiquito que avisa que hay que hacer clic (opcional).")]
    public GameObject indicadorClic;

    private void Start()
    {
        if (panelIntro == null || textoIntro == null || lineas == null || lineas.Length == 0)
        {
            if (panelIntro != null)
            {
                panelIntro.SetActive(false);
            }
            return;
        }

        StartCoroutine(MostrarIntro());
    }

    private IEnumerator MostrarIntro()
    {
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.BloquearJuego(true);
        }

        panelIntro.SetActive(true);

        for (int i = 0; i < lineas.Length; i++)
        {
            textoIntro.text = lineas[i];

            // Pequeña espera para que un clic no salte dos líneas de golpe.
            yield return new WaitForSeconds(0.4f);

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

            // Esperar un frame para que este mismo clic no llegue al ClickManager.
            yield return null;
        }

        panelIntro.SetActive(false);

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.BloquearJuego(false);
        }
    }
}
