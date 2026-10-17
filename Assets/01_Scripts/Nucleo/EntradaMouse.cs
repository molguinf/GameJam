// =====================================================================
// EntradaMouse.cs                        MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// Lee el mouse funcione el proyecto con el Input System NUEVO o con el
// Input Manager ANTIGUO. Así no importa cómo esté configurado Unity.
// Todos los scripts que necesiten saber si hubo un clic usan esta clase.
// =====================================================================

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class EntradaMouse
{
    // Devuelve true solo en el cuadro (frame) en que se presionó el botón izquierdo.
    public static bool SeHizoClicEsteFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
        {
            return false;
        }
        return Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    // Devuelve la posición del mouse en píxeles de pantalla.
    public static Vector2 PosicionEnPantalla()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
        {
            return Vector2.zero;
        }
        return Mouse.current.position.ReadValue();
#else
        Vector3 posicion = Input.mousePosition;
        return new Vector2(posicion.x, posicion.y);
#endif
    }
}
