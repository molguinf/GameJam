// =====================================================================
// IClickeable.cs                         MÓDULO: NÚCLEO (Integrante A)
// ---------------------------------------------------------------------
// "Contrato" que cumplen todos los objetos a los que se les puede hacer
// clic en el juego. El ClickManager detecta el clic del mouse y le avisa
// al objeto clickeado llamando a su método AlHacerClic().
//
// IMPORTANTE: el objeto necesita un Collider2D (BoxCollider2D, por ejemplo)
// para que el clic lo pueda encontrar.
// =====================================================================

public interface IClickeable
{
    void AlHacerClic();
}
