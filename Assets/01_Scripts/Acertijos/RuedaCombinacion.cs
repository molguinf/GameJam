// =====================================================================
// RuedaCombinacion.cs                 MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Una "rueda" de un candado. Cada clic pasa a la siguiente opción.
//
//   - Candado del HORNO (símbolos): llena "Opciones Sprites" con los
//     dibujos sol, luna, estrella, corazón. Deja "Texto Numero" vacío.
//
//   - Candado del ROPERO (números): deja "Opciones Sprites" VACÍO y
//     asigna un TextMeshPro en "Texto Numero". Va de 0 a 9.
//
// Cada rueda avisa a su CerraduraCombinacion para que revise si ya se abrió.
// Necesita: BoxCollider2D.
// =====================================================================

using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class RuedaCombinacion : MonoBehaviour, IClickeable
{
    public CerraduraCombinacion cerradura;

    [Header("Opción A: símbolos (horno)")]
    public Sprite[] opcionesSprites;
    public SpriteRenderer dibujoRueda;

    [Header("Opción B: números 0-9 (ropero)")]
    public TextMeshPro textoNumero;

    public AudioClip sonidoGirar;

    private int indiceActual = 0;

    private void Start()
    {
        if (dibujoRueda == null)
        {
            dibujoRueda = GetComponent<SpriteRenderer>();
        }
        ActualizarDibujo();
    }

    public void AlHacerClic()
    {
        if (cerradura != null && cerradura.EstaAbierta())
        {
            return;
        }

        indiceActual = indiceActual + 1;
        if (indiceActual >= CantidadDeOpciones())
        {
            indiceActual = 0;
        }

        ActualizarDibujo();

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ReproducirSonido(sonidoGirar);
        }

        if (cerradura != null)
        {
            cerradura.Comprobar();
        }
    }

    public int ObtenerIndiceActual()
    {
        return indiceActual;
    }

    private bool UsaSimbolos()
    {
        return opcionesSprites != null && opcionesSprites.Length > 0;
    }

    private int CantidadDeOpciones()
    {
        if (UsaSimbolos())
        {
            return opcionesSprites.Length;
        }
        return 10;
    }

    private void ActualizarDibujo()
    {
        if (UsaSimbolos())
        {
            if (dibujoRueda != null)
            {
                dibujoRueda.sprite = opcionesSprites[indiceActual];
            }
        }
        else
        {
            if (textoNumero != null)
            {
                textoNumero.text = indiceActual.ToString();
            }
        }
    }
}
