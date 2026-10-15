# Interfaz — Información y acciones

Referencia: [GDD](GDD.md), secciones 3–5, 9 y 11–12. Diseños de pantallas pendientes; aquí se define su contenido.

## Pantallas

| Pantalla | Información y acciones |
| --- | --- |
| Menú principal | Crear sala · Unirme a una sala · Cómo se juega · Récords · Configuración · Salir |
| Récords | Una pestaña por tamaño de grupo (2 a 6 jugadores) con las 10 mejores victorias de este ordenador: puesto (los empates lo comparten), premio, tiempo, fecha, participantes y supervivientes. Esc o Cerrar |
| Unirme a una sala | Dirección del host (recuerda la última), Conectar, Volver; valida el formato antes de intentarlo y explica los errores |
| Sala de espera | Jugadores n/6, fichas, «Invita a tus amigos» (host: dirección a compartir y Copiar), Empezar partida (host), Tienda, Cómo se juega, Salir |
| Cómo se juega | Tutorial de 6 páginas (objetivo, dinero, verificar, comodín, fichas y tienda, controles) con Anterior/Siguiente. Texto en `Menu/Content/howtoplay_es.json` |
| Configuración | Volumen general, música y efectos; sensibilidad del ratón; invertir eje vertical; pantalla completa; Restablecer. Se aplica al momento y se guarda al cerrar |
| Pausa (Esc en partida) | Continuar · Cómo se juega · Configuración · Salir de la sala (con confirmación). La partida sigue para los demás: tu concursante queda quieto y el ratón libre. Esc cierra primero el panel abierto |
| Concurso | Pantalla del plató: pregunta, respuestas A-D en recuadros de color, ronda, reloj y explicación. HUD: estado de la ronda, dinero y comodines. Viñeta del presentador sobre su cabeza |
| Conversión | Valor disponible y resultado de agrupar/dividir; acción manual |
| Espectador | Cámara por encima del hombro de un activo (A/D o flechas cambian, Espacio vista general) y rótulo abajo: «ELIMINADO · Eres espectador hasta la siguiente partida · Viendo a …». Sin acciones de ayuda |
| Resultados | Victoria/derrota, preguntas superadas, premio, tiempo total, supervivientes y fichas por persona. Al ganar aparecen tras 5 s de celebración (cheque gigante en la pantalla, podio con placas, lluvia dorada) y dicen el puesto en los récords («¡NUEVO RÉCORD!», «Puesto 3…», «No entra en tus 10 mejores» o por qué no cuenta) |
| Tienda | Desde la sala de espera: fichas, precio, propiedad y comprar/equipar/quitar cosméticos. Se cierra al empezar la partida |
| Ranking de salida | Categoría por tamaño inicial, dinero, tiempo y participantes |

El vestíbulo puede ser una pantalla inicialmente; no obliga a construir otra escena 3D.

## Durante la apuesta

- Ronda actual de 15 y pregunta legible.
- Cuatro opciones identificadas A/B/C/D, correspondencia clara con cabinas.
- Total en cada bandeja y valor de objeto transportado.
- Usos personales y reserva compartida de comodines, si aplica.
- Jugadores activos, comprometidos y eliminados distinguibles. Encima de cada cabina, texto con los nombres de los apostados en ella.
- Acción contextual para coger, colocar, convertir o preparar comodín.
- Presentador de texto separado de las respuestas.

Cuenta atrás grande del tiempo para apostar, en rojo en los últimos 10 segundos. Al llegar a 0 se verifica sola.

## Cierre y resolución

El botón indica que cierra todas las apuestas. Al pulsarlo: bloqueo inmediato, cuenta 3–2–1 y estado visible de cierre. No sugerir que esos tres segundos admiten cambios.

Comunicar antes del cierre la regla provisional de perder dinero sin apostar. Señalar correctamente usos de comodín sin recurrir a una nueva acción de marcadores del jugador.

Sonido (implementado): golpe al cerrar, pitido en cada segundo del 3-2-1, tic de reloj en los últimos 10 s de las apuestas con tiempo. Detalle en [RECURSOS](RECURSOS.md#sonido).

Después: respuesta correcta y explicación breve. Al acertar, confeti de colores a los lados de la pantalla (sin taparla); al ganar, lluvia dorada en toda la sala. Total perdido/recuperado y eliminaciones claros. La lectura no debe quedar oculta por la animación de expulsión.

## Resultados y compras

Fichas personales y dinero del concurso usan nombres/iconos distintos. Mostrar «250 fichas para cada jugador» o el tramo correspondiente; nunca sugerir un reparto entre integrantes. Una derrota no muestra envío de ranking.

Tienda: compras cosméticas, confirmación de precio, objeto ya adquirido y saldo insuficiente. No vender gestos en la primera salida. Diez personajes básicos disponibles sin compra.

## Pendientes

Ubicación de textos (cámara en tercera persona confirmada), asignación de controles, tamaño de tipografía, idiomas y compatibilidad de entrada. Validar legibilidad del texto más largo y usar letras/formas además del color. La interfaz no debe añadir sistemas de señalar ni votación obligatoria.
