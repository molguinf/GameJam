// =====================================================================
// Anomalia.cs                         MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Una "cosa rara" del cuarto. Tiene una versión NORMAL y una versión RARA.
// Cuando el juego llega a su etapa, se apaga la normal y se prende la rara.
//
// Ejemplos:
//   - Foto familiar:  normal = Foto_Abuela_Normal,  rara = Foto_Abuela_Borrada   (etapa 2)
//   - Reloj:          normal = Aguja (gira normal), rara = Aguja (gira al revés) (etapa 1)
//   - Calendario:     normal = Calendario_1,        rara = Calendario_2          (etapa 2)
//
// NO necesita referencias a otros prefabs: ControlAnomalias encuentra
// solo todas las Anomalia de la escena. Así cada prefab es independiente.
// =====================================================================

using UnityEngine;

public class Anomalia : MonoBehaviour
{
    [Tooltip("En qué etapa aparece la versión rara (1, 2, 3 o 4).")]
    public int etapaEnQueAparece = 1;

    public GameObject versionNormal;
    public GameObject versionRara;

    [Tooltip("Sonido al aparecer (opcional). Solo suena si esta pared se está viendo.")]
    public AudioClip sonidoAlAparecer;

    private bool yaEstaRara = false;

    public void Aplicar(int etapaActual)
    {
        bool debeEstarRara = etapaActual >= etapaEnQueAparece;

        if (versionNormal != null)
        {
            versionNormal.SetActive(debeEstarRara == false);
        }
        if (versionRara != null)
        {
            versionRara.SetActive(debeEstarRara);
        }

        if (debeEstarRara && yaEstaRara == false)
        {
            yaEstaRara = true;
            if (gameObject.activeInHierarchy && GameManager.Instancia != null)
            {
                GameManager.Instancia.ReproducirSonido(sonidoAlAparecer);
            }
        }
    }
}
