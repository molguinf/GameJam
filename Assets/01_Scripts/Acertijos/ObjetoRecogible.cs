// =====================================================================
// ObjetoRecogible.cs                  MÓDULO: ACERTIJOS (Integrante B)
// ---------------------------------------------------------------------
// Un objeto que el jugador puede recoger con un clic (fósforos, pedazos
// de escalera, flores...). Al recogerlo:
//   1. Se agrega al inventario.
//   2. Se muestra un mensaje.
//   3. Suena un efecto.
//   4. Desaparece del cuarto.
//
// Necesita: SpriteRenderer + BoxCollider2D en el mismo objeto.
// =====================================================================

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObjetoRecogible : MonoBehaviour, IClickeable
{
    [Header("Datos del objeto")]
    [Tooltip("Identificador sin espacios ni tildes. Ejemplo: fosforos, escalera_1")]
    public string idObjeto;
    [Tooltip("Nombre que ve el jugador. Ejemplo: Fósforos")]
    public string nombreObjeto;
    [Tooltip("Icono del inventario. Si se deja vacío, usa el mismo dibujo del objeto.")]
    public Sprite iconoInventario;

    [Header("Al recogerlo")]
    [TextArea]
    public string mensajeAlRecoger;
    public AudioClip sonidoAlRecoger;

    public void AlHacerClic()
    {
        Sprite icono = iconoInventario;
        if (icono == null)
        {
            SpriteRenderer dibujo = GetComponent<SpriteRenderer>();
            if (dibujo != null)
            {
                icono = dibujo.sprite;
            }
        }

        Inventario.Instancia.AgregarObjeto(idObjeto, nombreObjeto, icono);

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.MostrarMensaje(mensajeAlRecoger);
            GameManager.Instancia.ReproducirSonido(sonidoAlRecoger);
        }

        gameObject.SetActive(false);
    }
}
