// =====================================================================
// RotarAguja.cs                       MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Hace girar la aguja del reloj de pared.
//   - Aguja normal:    gradosPorSegundo = -6   (sentido del reloj)
//   - Aguja al revés:  gradosPorSegundo =  30  (rápido y al revés: la rareza)
//
// TRUCO: el "pivote" del sprite de la aguja tiene que estar en la base
// (Sprite Editor > Pivot = Bottom), si no gira desde el centro.
// =====================================================================

using UnityEngine;

public class RotarAguja : MonoBehaviour
{
    public float gradosPorSegundo = -6f;

    private void Update()
    {
        float giro = gradosPorSegundo * Time.deltaTime;
        transform.Rotate(0f, 0f, giro);
    }
}
