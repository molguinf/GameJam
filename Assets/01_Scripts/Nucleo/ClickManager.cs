// =====================================================================
// ClickManager.cs                        MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// Detecta el clic del mouse sobre los objetos del mundo 2D y le avisa al
// objeto que fue clickeado (el que tenga un script IClickeable).
//
// Si hay varios objetos uno encima de otro, gana el que tiene el
// "Order in Layer" más alto en su SpriteRenderer (el que se ve adelante).
//
// Va en el mismo objeto que el GameManager (dentro de Sistema.prefab).
// =====================================================================

using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : MonoBehaviour
{
    [Tooltip("Si se deja vacío, usa la cámara principal (Main Camera).")]
    public Camera camara;

    private void Start()
    {
        if (camara == null)
        {
            camara = Camera.main;
        }
    }

    private void Update()
    {
        // 1. ¿El juego está bloqueado? (intro, final, etc.)
        if (GameManager.Instancia != null && GameManager.Instancia.juegoBloqueado)
        {
            return;
        }

        // 2. ¿Hubo clic en este frame?
        if (EntradaMouse.SeHizoClicEsteFrame() == false)
        {
            return;
        }

        // 3. Si el clic fue sobre un botón de la interfaz (flechas, inventario), no tocamos el mundo.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (camara == null)
        {
            Debug.LogWarning("ClickManager no encuentra la cámara. Pon la etiqueta MainCamera a la cámara.");
            return;
        }

        // 4. Convertir la posición del mouse (píxeles) a posición del mundo.
        Vector2 posicionPantalla = EntradaMouse.PosicionEnPantalla();
        Vector3 posicionMundo = camara.ScreenToWorldPoint(new Vector3(posicionPantalla.x, posicionPantalla.y, 0f));
        Vector2 punto = new Vector2(posicionMundo.x, posicionMundo.y);

        // 5. Buscar todos los colliders debajo del mouse.
        Collider2D[] collidersTocados = Physics2D.OverlapPointAll(punto);

        IClickeable objetoElegido = null;
        int ordenMasAlto = int.MinValue;

        foreach (Collider2D colliderTocado in collidersTocados)
        {
            IClickeable clickeable = colliderTocado.GetComponent<IClickeable>();
            if (clickeable == null)
            {
                continue;
            }

            int orden = 0;
            SpriteRenderer dibujo = colliderTocado.GetComponent<SpriteRenderer>();
            if (dibujo != null)
            {
                orden = dibujo.sortingOrder;
            }

            if (orden > ordenMasAlto)
            {
                ordenMasAlto = orden;
                objetoElegido = clickeable;
            }
        }

        // 6. Avisarle al objeto elegido.
        if (objetoElegido != null)
        {
            objetoElegido.AlHacerClic();
        }
    }
}
