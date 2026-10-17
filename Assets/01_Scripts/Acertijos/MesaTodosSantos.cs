// =====================================================================
// MesaTodosSantos.cs                  MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Cuenta cuántas cosas se pusieron en la mesa (vela, t'antawawa,
// escalera de pan, flores). Cada vez que se pone una:
//   - Avanza una etapa de rarezas (ControlAnomalias).
// Cuando están todas:
//   - Aparece el retrato boca abajo para darle la vuelta (el final).
//
// Cada hueco de la mesa es una ZonaInteractiva. En su "Al Tener Exito"
// se arrastra este objeto y se elige MesaTodosSantos > ColocarObjeto.
// =====================================================================

using UnityEngine;

public class MesaTodosSantos : MonoBehaviour
{
    public int objetosNecesarios = 4;

    [Tooltip("La zona del retrato boca abajo. Empieza APAGADA.")]
    public GameObject retratoParaDarVuelta;

    [TextArea]
    public string mensajeMesaCompleta = "La mesa está lista. Solo falta el retrato del centro.";

    private int objetosColocados = 0;

    private void Start()
    {
        if (retratoParaDarVuelta != null)
        {
            retratoParaDarVuelta.SetActive(false);
        }
    }

    public void ColocarObjeto()
    {
        objetosColocados = objetosColocados + 1;

        if (ControlAnomalias.Instancia != null)
        {
            ControlAnomalias.Instancia.AvanzarEtapa();
        }

        if (objetosColocados >= objetosNecesarios)
        {
            if (retratoParaDarVuelta != null)
            {
                retratoParaDarVuelta.SetActive(true);
            }
            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.MostrarMensaje(mensajeMesaCompleta);
            }
        }
    }

    public int ObtenerObjetosColocados()
    {
        return objetosColocados;
    }
}
