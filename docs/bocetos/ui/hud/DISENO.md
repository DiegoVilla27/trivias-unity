# HUD — especificación visual

Propuesta del 17 de octubre de 2026, basada en [UI_UX](../../../UI_UX.md), [GDD](../../../GDD.md), [PLAYER](../../../PLAYER.md), [SISTEMAS](../../../SISTEMAS.md), el `MatchHudView` actual, el plató y los [menús aprobados](../menu/README.md). Son bocetos de documentación; no se modifica el HUD implementado.

## Composición

- Mantener el centro de la imagen libre para recorrer el plató, ver el dinero y las manos. Sin velo oscuro general, marcos exteriores de bombillas, retícula, minimapa ni barras de vida.
- La pregunta, las respuestas A–D y la explicación pertenecen a la pantalla física del plató. No duplicarlas como tarjeta flotante de HUD ni convertirlas en botones de voto.
- Cabinas y bandejas: letras y colores coherentes, importes y nombres apostados como rótulos en el mundo. El estilo del boceto no sustituye el plano dimensional de LEVEL.
- HUD periférico: ronda/estado arriba a la izquierda, reloj/cierre/resultado arriba en el centro, dinero y comodines arriba a la derecha. Pista contextual abajo; información de carga solo mientras se transporta dinero.
- Azul noche translúcido, texto marfil y bordes oro finos; menos ornamentación que los menús. Referencia 1920 × 1080, márgenes seguros 48–64 px, texto principal 24–30 px, reloj 44–56 px. Validar contraste y lectura con el juego en movimiento antes de implementar.
- Letras, iconos y texto acompañan al color. Los últimos diez segundos usan aviso rojo y reloj visible; sin destellos que impidan leer. Con tiempo desactivado se omite el contador y se mantiene el estado.

## Siete estados

| Estado | Contenido y comportamiento |
| --- | --- |
| Apuestas | Ronda de 15, reloj si aplica, total del equipo, comodines propios y reserva. Aviso de dinero fuera de bandejas antes del cierre. Cerca de la mesa: E uno y Q varios. |
| Transporte | Tipo, cantidad, valor y capacidad del tipo cargado. Cerca de una bandeja: E dejar uno, Q dejar varios. R devolver todo solo al estar en la mesa. Nunca mezclar denominaciones en una carga ni mostrar una capacidad única para todos los tipos. |
| Comodín preparado | Cabina comprometida, nombres de apostados y multiplicador previsto. E cancela antes del cierre. El importe mostrado es condicional a acertar, no saldo ganado. Uso consumido al verificar; reserva usada solo tras agotar los personales. No añadir señaladores, votación o listo obligatorio. |
| Cambio manual | Mostrar cantidad disponible, operación y resultado con igual valor. Ejemplo 10 fajos × 5.000 = 1 bolsa × 50.000. Agrupar/dividir son controles contextuales de la estación: el boceto no fija teclas nuevas para esas operaciones. No confundir el cambio con tienda o fichas. |
| Cerrado | Bloqueo inmediato, candado, cuenta 3–2–1 y aviso de que no se pueden cambiar apuestas, mover dinero o cancelar comodines. Se retiran las pistas de interacción. No interpretar el contador como tres segundos adicionales para depositar. |
| Revelación | Respuesta y explicación en la pantalla del plató; HUD con conservado, bono, perdido y nuevo total. Acierto con símbolo además de color. Confeti en los laterales. No mostrar fichas, premio final ni botón de continuar en una ronda normal. Transición automática según el flujo de partida. |
| Espectador | Tras la expulsión, cámara de un jugador activo y franja «ELIMINADO · Eres espectador hasta la siguiente partida · Viendo a…». A/D o flechas cambian, Espacio vista general, Esc menú. Sin carga, comodín ni acciones para ayudar a los activos. |

Los resultados finales y el menú de pausa son pantallas distintas. La pausa ya está [diseñada con los menús](../menu/09-pause.png); no se genera otra variante aquí. Este alcance no incluye una pantalla final de victoria/derrota.

## Coherencia de los ejemplos

La pregunta de muestra es «¿Qué planeta es conocido como el planeta rojo?». A Venus, B Marte, C Júpiter, D Saturno. No se modifica el banco de preguntas.

La secuencia económica ilustrativa usa 100.000 $: A 10.000, B 60.000, C 0, D 0 y 30.000 fuera de bandejas. Al transportar dos fajos, esos 30.000 se reparten en 20.000 sobre la mesa y 10.000 en manos; el total sigue siendo 100.000.

Si Diego y Ana comprometen sus comodines en B, el multiplicador de acierto es x3. Conservado 60.000 + bono 120.000 = nuevo total 180.000; perdido 40.000 (10.000 en A y 30.000 fuera). No sumar otra vez los 60.000 al saldo de 180.000 ni presentar el total como bono.

Con cuatro jugadores iniciales, el ejemplo muestra un uso personal por jugador y dos de reserva. Al preparar no se consume todavía el uso; al cerrar, Diego pasa de uno a cero. Es un ejemplo de disponibilidad, no un cambio de balance. Capital y denominaciones siguen siendo configurables según GDD.

La lámina de espectador representa otro caso de eliminación y no afirma que Diego haya sido expulsado tras acertar B. Los datos y nombres de los bocetos son ejemplos. El estado fuera de bandejas refleja la propuesta documentada; el boceto no confirma reglas pendientes de balance.

## Estados adicionales para implementación

- Capacidad llena: «No puedes llevar más fajos» con el límite del tipo, sin ocultar las manos.
- Sin comodines propios: mostrar cero y la reserva; si tampoco hay reserva, explicar que no se puede apostar el comodín.
- Sin operación de cambio válida: desactivar el control y mostrar qué cantidad falta, conservando el valor visible.
- Durante cierre, revelación y expulsión: ninguna pista debe invitar a una acción que ya no es válida.
- Presentador: viñeta breve junto a su cabeza, separada de la pregunta. No inventar voz ni controles de micrófono en el HUD.
- El menú Esc no detiene el concurso ni el reloj para los demás.

Validación pendiente en Unity: lectura desde los ángulos reales de cámara, textos largos, solapamiento de nombres, foco/contexto al acercarse a objetos y contraste sobre fondos variables. Los bocetos no prueban accesibilidad ni precisión dimensional por sí solos.
