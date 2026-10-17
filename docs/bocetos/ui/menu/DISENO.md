# Menús — decisiones de diseño y comportamiento

Concepto visual del 17 de octubre de 2026. Fuentes: [UI_UX](../../../UI_UX.md), [GDD](../../../GDD.md), [PLAYER](../../../PLAYER.md), [SISTEMAS](../../../SISTEMAS.md), [RECURSOS](../../../RECURSOS.md), el catálogo de personajes y los bocetos del plató. Contrastado con `UiTheme`, `CharacterCatalog`, `CosmeticCatalog` y `howtoplay_es.json` del proyecto. Son bocetos, no pantallas implementadas.

## Lenguaje visual

- Formato PC 16:9, referencia de implementación 1920 × 1080; márgenes seguros de 64 px y separación de 24–32 px. Las imágenes generadas pueden variar unos píxeles en proporción.
- Panel azul noche `#19243D`, texto marfil `#F5EFE1`, oro suave `#D6AC61`. Telones burdeos y bombillas cálidas pertenecen al fondo; el contenido tiene prioridad.
- Lilita One en títulos y Poppins en cuerpo. Referencia: títulos 44–60 px, botones 26–32 px, texto 22–26 px; ayudas importantes no menores de 18 px. Verificar contraste en la implementación, sin afirmar que una imagen lo certifica.
- Bordes finos, esquinas de 20–28 px, sombras suaves. Un botón principal por acción contextual, secundarios discretos y salida destructiva separada. No convertir cada botón en una marquesina.
- Áreas interactivas de al menos 48 px. Foco visible con contorno y cambio de estado; propiedad, selección y bloqueo combinan texto, icono y color.
- Movimiento previsto: aparición breve, hover y pulsación sutiles, rotación lenta del personaje. Los bocetos son estáticos y no verifican animaciones.

## Navegación

Menú principal → Crear sala → Sala de espera. Crear sala es una acción directa, no otro formulario ni otra imagen del mismo menú.

Menú principal → Unirme → Dirección del anfitrión → Conectar → Sala de espera.

La selección de personaje está dentro del menú principal. Las flechas recorren solo los personajes gratuitos o adquiridos; el cambio se guarda al momento. La peana muestra un personaje completo, sin editores separados de ropa, pelo o zapatos. Si el elegido está ocupado en la sala, mostrar el aviso de sustitución por otro gratuito disponible, conservando la preferencia para futuras salas.

Sala de espera → Tienda → Personajes / Accesorios → Volver a la sala. La tienda se cierra al empezar la partida. No se añade acceso directo desde el menú principal como función confirmada.

Cómo se juega, Récords y Configuración se abren desde sus puntos definidos en UI_UX. Cerrar o Esc devuelve al origen; Esc cierra primero el panel superior. Durante la partida el menú no detiene al resto de jugadores.

## Pantallas y estados

| Pantalla | Comportamiento y estados que deben implementarse |
| --- | --- |
| Principal | Nombre de hasta 16 caracteres, vista de cómo se verá y guardado al perder foco o crear/unirse. Sin nombre se usa el valor de reserva del juego. Crear sala es principal; unirse es secundario. Las flechas no ofrecen personajes sin adquirir. |
| Unirme | Recordar última dirección, validar antes de conectar, desactivar doble envío y mostrar «Conectando…». Error junto al campo con explicación y Reintentar; no prometer servidores públicos, códigos de sala ni Steam ya integrado. La dirección del boceto es ficticia. |
| Sala de espera | 2–6 jugadores, máximo seis plazas. Empezar solo visible/habilitado para el anfitrión y con al menos dos jugadores para una partida normal. Invitación con dirección y Copiar; confirmación breve «Copiado». El cliente ve «Esperando al anfitrión». No añadir sistema de listo ni votación. |
| Tienda: personajes | Seis gratuitos y cuatro de pago a 1.200 fichas. Comprar personaje lo deja elegido; adquirido muestra Elegir, seleccionado muestra Elegido. La asignación concreta de los cuatro personajes de pago en el boceto es solo ilustrativa: sigue pendiente decidirla. No actualizar el catálogo real a partir de la imagen. |
| Tienda: accesorios | Gorra, gafas y pajarita a 300 fichas; corona y chistera a 1.200. Compra con precio visible y confirmación; adquirido permite Equipar o Quitar según estado. La previsualización no equivale a equipado. Saldo insuficiente mantiene el precio legible y explica cuántas fichas faltan; no ofrece comprar moneda. |
| Cómo se juega | Seis páginas, una plantilla visual común. Anterior deshabilitado en la primera; Siguiente pasa de página y en la última se sustituye por Entendido. Indicador 1/6 y Cerrar siempre accesible. La lámina muestra la primera página, no seis imágenes repetidas. |
| Récords | Tabla local de hasta diez victorias por tamaño inicial, pestañas 2–6. Premio descendente; empate resuelto por menor tiempo, empate exacto comparte puesto. Participantes y supervivientes visibles. Estado vacío «Todavía no hay victorias en esta categoría». Datos del boceto son ficticios. |
| Configuración | Tres volúmenes, sensibilidad, invertir eje vertical, pantalla completa y Restablecer. Aplicación inmediata; guardado al cerrar, sin botón Aplicar redundante. Valores del boceto son ejemplos. No añadir calidad gráfica, idioma o remapeo como funciones confirmadas. |
| Menú de partida | Continuar, Cómo se juega, Configuración, Salir de la sala. Aviso «La partida continúa para los demás». El concursante local queda quieto y el ratón libre. |
| Confirmación de salida | La lámina muestra salir de una sala siendo anfitrión: Quedarme y Salir de la sala; avisa que la sala se cerrará para todos. Para un cliente se omite esa frase. Para salir de la aplicación, si se usa confirmación, adaptar título y acción a «¿Salir del juego?» / «Salir del juego»; no mostrar consecuencias de host fuera de una sala. |

Las fichas usan icono propio y la palabra «fichas»; el premio del concurso usa dinero. No se añaden dinero real, cajas aleatorias, ventajas, niveles, recompensas diarias ni ranking global.

## Texto propuesto del tutorial completo

Revisión editorial para que el texto de ayuda no contradiga el bloqueo de apuestas ni el catálogo actual. Cada cuerpo tiene como máximo 220 caracteres y siete líneas; estos textos no sustituyen todavía el JSON del juego.

1. **El objetivo:** De 2 a 6 jugadores contra 15 preguntas. Cada cabina es una respuesta. Llevad el dinero a la correcta y llegad al final con dinero.
2. **El dinero:** E: coge o deja uno. Q: varios. R: devuelve todo en la mesa. Cambia 10 fajos por 1 bolsa y 10 bolsas por 1 lingote. El dinero fuera de las bandejas se pierde al verificar.
3. **Verificar:** Pulsa VERIFICAR con E: las apuestas se bloquean al instante. Tras 3 segundos se revela la respuesta. Si se acaba el reloj, se verifica automáticamente.
4. **Apuéstate a ti mismo:** Entra en la plataforma y pulsa E para apostar tu comodín. Si aciertas, multiplicas el dinero de esa cabina. Si fallas, quedas eliminado hasta la siguiente partida. Antes del cierre, E cancela.
5. **Fichas y tienda:** Las preguntas superadas dan fichas a cada jugador. Se guardan para comprar personajes y accesorios en la tienda de la sala de espera. Solo cambian la apariencia: no dan ventajas.
6. **Controles:** WASD: moverte. Shift: correr. Ratón: mirar. Espacio: saltar. E: acción. Q: varios. R: devolver en la mesa. Esc: menú; la partida continúa para los demás.

## Límites del concepto

«TRIVIAS» es un rótulo provisional: el nombre oficial sigue pendiente. Nombres, fichas, dirección y resultados son datos de ejemplo. El catálogo de ilustraciones define apariencia, no qué personajes se venden. La implementación actual usa personajes provisionales; los bocetos muestran el acabado visual deseado.

La revisión cubre contenido, jerarquía, flujo y consistencia visual. La legibilidad final, accesibilidad, estados interactivos y navegación con teclado requieren comprobarse en la interfaz implementada.
