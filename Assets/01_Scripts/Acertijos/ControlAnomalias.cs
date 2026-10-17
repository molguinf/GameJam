// =====================================================================
// ControlAnomalias.cs                 MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Lleva la cuenta de la etapa de rarezas (0 = todo normal).
// MesaTodosSantos llama a AvanzarEtapa() cada vez que se pone algo en la mesa.
//
// Busca TODAS las Anomalia de la escena, aunque estén en paredes apagadas,
// y les dice en qué etapa estamos.
//
// Va en el prefab Interactivos_Pared1 (junto a la mesa).
// =====================================================================

using System.Collections.Generic;
using UnityEngine;

public class ControlAnomalias : MonoBehaviour
{
    public static ControlAnomalias Instancia;

    [Tooltip("Solo para ver en el Inspector en qué etapa va el juego.")]
    public int etapaActual = 0;

    private List<Anomalia> anomalias = new List<Anomalia>();

    private void Awake()
    {
        Instancia = this;
    }

    private void Start()
    {
        BuscarAnomalias();
        AplicarATodas();
    }

    private void BuscarAnomalias()
    {
        anomalias.Clear();

        // Resources.FindObjectsOfTypeAll encuentra también los objetos APAGADOS.
        // Filtramos para quedarnos solo con los que están en la escena (no en la carpeta de prefabs).
        Anomalia[] todas = Resources.FindObjectsOfTypeAll<Anomalia>();
        foreach (Anomalia anomalia in todas)
        {
            if (anomalia.gameObject.scene.IsValid())
            {
                anomalias.Add(anomalia);
            }
        }

        Debug.Log("ControlAnomalias encontró " + anomalias.Count + " anomalías en la escena.");
    }

    public void AvanzarEtapa()
    {
        etapaActual = etapaActual + 1;
        AplicarATodas();
    }

    private void AplicarATodas()
    {
        foreach (Anomalia anomalia in anomalias)
        {
            if (anomalia != null)
            {
                anomalia.Aplicar(etapaActual);
            }
        }
    }
}
