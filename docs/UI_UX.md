# Interfaz — Información y acciones

Referencia: [GDD](GDD.md), secciones 3–5, 9 y 11–12. Contenido funcional de la interfaz. Los [diez bocetos del menú](bocetos/ui/menu/README.md) y su [especificación visual](bocetos/ui/menu/DISENO.md) concretan la propuesta gráfica; no constituyen una implementación.

## Pantallas

Montadas como los [bocetos del menú](bocetos/ui/menu/README.md) (17 de octubre de 2026): piezas comunes en `UiKit` (paneles, botones, campos, pastillas, tarjetas, deslizadores e interruptores) y, detrás de todas, el plató real desenfocado (`UiBackdrop`, `MenuBackdropView`). Los botones crecen como mucho 5 px por lado al pasar el ratón (nunca se salen de su panel); las ilustraciones y medallas conservan su proporción; las piezas bajas (pistas de los deslizadores, pastillas, interruptores) tienen extremos de píldora.

| Pantalla | Información y acciones |
| --- | --- |
| Menú principal | Como el boceto 01: rótulo TRIVIAS; a la izquierda tu nombre (con «Así te verán los demás · n/16»), Crear sala, Unirme a una sala, Cómo se juega, Récords, Configuración y Salir; a la derecha «TU PERSONAJE» (el personaje en 3D en su peana, flechas, nombre, «Gratis · Elegido» y «N más en la tienda»; solo pasa por los que tiene, se guarda al momento y quien no elige lleva el de por defecto). Abajo, «2–6 jugadores · 15 preguntas» |
| Récords | Como el boceto 07: pestañas 2 a 6 jugadores y tabla de las 10 mejores victorias (con desplazamiento): puesto con medalla en los tres primeros (los empates lo comparten), premio, tiempo, fecha, participantes y supervivientes («2/3»). Vacía: «Todavía no hay victorias en esta categoría.» Esc o Cerrar |
| Unirme a una sala | Como el boceto 02 (sin puerto): dirección del anfitrión (recuerda la última), «Recordaremos la última dirección» o el error, «Conectando…», Conectar, Volver y tu nombre |
| Sala de espera | Como el boceto 03: tú (retrato · Anfitrión) y tus fichas arriba; seis plazas con retrato, nombre, personaje e insignia de anfitrión, o «Esperando jugador…»; «Invita a tus amigos» con la dirección y Copiar (anfitrión); a la derecha tu personaje en 3D, Empezar partida (anfitrión; si no, «Esperando al anfitrión»), Tienda, Cómo se juega y Salir de la sala (pregunta antes) |
| Cómo se juega | Como el boceto 06: tira de seis pasos (Objetivo, Dinero, Verificar, Comodín, Fichas, Controles) que lleva a cada página, «n / 6» con puntos, título de la página, su ilustración, el texto y Anterior / Siguiente («Entendido» en la última) y Cerrar. Como mucho 220 caracteres y 7 líneas por página. Las teclas del texto («[E]», «[Shift]»…) se dibujan como las teclas de los avisos del juego |
| Configuración | Como el boceto 08: Sonido (volumen general, música, efectos) y Controles y pantalla (sensibilidad del ratón en %, invertir eje vertical y pantalla completa con interruptor Sí/No); Restablecer y Cerrar. Se aplica al momento y se guarda al cerrar |
| Pausa (Esc en partida) | Como el boceto 09: «La partida continúa para los demás.» Continuar · Cómo se juega · Configuración · Salir de la sala (burdeos). Confirmación como el boceto 10: «Volverás al menú principal.» y, al anfitrión, «la sala se cerrará para todos»; Quedarme o Salir de la sala. Esc cierra primero el panel abierto. Cómo se juega, Configuración y Récords se dibujan encima de la pausa |
| Concurso | HUD sin saturar ([detalle](#hud-de-la-partida)). Pantalla del plató, con el estilo del menú (Lilita One y Poppins, recuadros redondeados de colores profundos como las cabinas): rótulo de pregunta y categoría, pregunta, respuestas A-D con su letra en un círculo y reloj en una placa; al revelar, la respuesta en grande con «✓ CORRECTA» y la explicación en lugar de las opciones. HUD: estado de la ronda, dinero y comodines. Rótulo de televisión del presentador abajo a la izquierda (su cara, «PRESENTADOR» y la frase letra a letra) |
| Conversión | Valor disponible y resultado de agrupar/dividir; acción manual |
| Espectador | Cámara por encima del hombro de un activo (A/D o flechas cambian, Espacio vista general) y rótulo abajo: «ELIMINADO · Eres espectador hasta la siguiente partida · Viendo a …». Sin acciones de ayuda |
| Resultados | Con la convención de los menús: plató desenfocado detrás, panel con marco dorado que aparece con rebote, «¡VICTORIA!» (dorado) o «DERROTA» (coral) en Lilita One con contorno, «Habéis superado X de 15 preguntas», tres tarjetas (preguntas, dinero final, tiempo), puesto en los récords si lo hay, pastilla dorada «+N fichas para cada jugador» con lo que tienes, supervivientes y expulsados, y «Volver a jugar» (dorado) · «Salir de la sala» (burdeos). Sin bombillas alrededor (están en el plató). Al ganar aparecen tras 5 s de celebración (cheque gigante en la pantalla, podio con placas, lluvia dorada) y dicen el puesto en los récords («¡NUEVO RÉCORD!», «Puesto 3…», «No entra en tus 10 mejores» o por qué no cuenta) |
| Tienda | Como los bocetos 04 y 05, desde la sala de espera: fichas arriba; pestañas Personajes (diez tarjetas con foto, «Gratis», «Elegido», «Comprado» o precio con candado) y Accesorios (foto con el complemento puesto, precio o «Adquirido»/«Puesto»); a la derecha la vista 3D del artículo (tu personaje con el complemento), su estado y el botón: Comprar y elegir, Elegir, Elegido, Comprar y equipar, Equipar, Quitar o «Te faltan N fichas». «Solo apariencia. Sin ventajas.» Volver a la sala. Se cierra al empezar la partida |
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

Tienda: compras cosméticas, confirmación de precio, objeto ya adquirido y saldo insuficiente. No vender gestos en la primera salida. Diez personajes: seis gratuitos y cuatro de pago a 1.200 fichas. La asignación definitiva de cuáles son gratuitos sigue pendiente, según GDD y PLAYER.

## Estilo

Toda la interfaz (menú, sala de espera, configuración, pausa, tienda, HUD, resultados, avisos) sigue el estilo del plató, aplicado al construir cada pantalla (`UiTheme`, en `Shared`), así que una pantalla nueva lo recibe sola:

- Tipografías: Lilita One en títulos (los dorados, con degradado como el rótulo del plató) y Poppins en el resto, igual que en el plató ([LEVEL](LEVEL.md)). Los textos encogen si no caben.
- Paneles redondeados con un leve degradado y marco dorado; los principales aparecen con un pequeño rebote (0,22 s).
- Botones redondeados: crecen un poco al pasar el ratón y se hunden al pulsar; los secundarios (fondo translúcido) llevan borde dorado.
- Detrás del menú principal y de «Unirme a una sala», un velo semioscuro a pantalla completa resalta el menú y deja ver el plató. En la sala de espera no hay velo.
- Menú principal y resultados con bombillas alrededor que persiguen, como la marquesina; el logo del menú late.
- Las animaciones usan tiempo real (siguen en pausa) y no reservan memoria por fotograma.

## Pendientes

Pendientes: ubicación de textos (cámara en tercera persona confirmada), asignación de controles, tamaño de tipografía, idiomas y compatibilidad de entrada. Validar legibilidad del texto más largo y usar letras/formas además del color. La interfaz no debe añadir sistemas de señalar ni votación obligatoria.

## Bocetos del menú — 17 de octubre de 2026

El [catálogo de diez pantallas](bocetos/ui/menu/README.md) cubre principal con selección integrada, unirse, sala de espera, tienda de personajes, tienda de accesorios, tutorial, récords, configuración, menú de partida y confirmación de salida. Crear sala lleva directamente a la sala de espera.

La [especificación de diseño](bocetos/ui/menu/DISENO.md) recoge jerarquía, tipografía, estados de conexión y compra, permisos del anfitrión, navegación con Esc y los seis textos propuestos del tutorial. La página de Verificar aclara el bloqueo inmediato: los tres segundos posteriores no permiten cambiar apuestas. El nombre TRIVIAS, los datos y la asignación visual de personajes gratuitos/de pago son ejemplos; no cierran decisiones pendientes.

## HUD de la partida

Implementado el 17 de octubre de 2026 a partir de los [bocetos](bocetos/ui/hud/README.md), con una regla: tres piezas fijas y, como mucho, un panel más según lo que se esté haciendo. El centro queda libre para jugar. Cada esquina va pegada a su borde en pantallas más anchas.

| Dónde | Qué | Cuándo |
| --- | --- | --- |
| Arriba a la izquierda | «PREGUNTA 5/15» y el estado con su símbolo: punto verde (apuestas abiertas), candado (cerradas), ✓ (revelada) | Siempre en la ronda |
| Debajo | Aviso rojo «Fuera de bandejas: 30.000 · Se pierde al verificar» | Solo si hay dinero fuera durante las apuestas |
| Arriba en el centro | Reloj «01:12» (rojo y latiendo los últimos 10 s). Al cerrar, candado, «APUESTAS CERRADAS», un 3-2-1 grande que salta y «Ya no se puede mover dinero ni cambiar comodines». Al revelar, «Conservado · Bono xN · Perdido» con las cifras subiendo y, si hay, «Expulsados: …» | Uno de los tres a la vez |
| Arriba a la derecha | Dinero del equipo (cuenta hasta la cifra nueva) y, centrado debajo, «Comodines 1 · reserva 2» | Siempre (sin comodines si eres espectador) |
| Abajo en el centro | Avisos con la tecla dibujada: «[E] Coger fajo (5.000)   [Q] Coger 5   [R] Devolver 2» | Solo cerca de algo y mientras se puede hacer |
| Encima de los avisos | «COMODÍN PREPARADO · B», «Si aciertas: 60.000 × 3 = 180.000» (previsión, no dinero ganado) y «Si fallas, quedas eliminado» | Solo con el comodín preparado |
| Abajo a la derecha | «LLEVAS 2 fajos · 10.000», «Capacidad 2/5» o «No puedes llevar más fajos» | Solo mientras llevas dinero |
| Abajo a la izquierda | Rótulo del presentador: su cara en un círculo dorado, «PRESENTADOR» (color según el ánimo) y la frase letra a letra; debajo, «[Esc] Menú» discreto | El rótulo, mientras habla; Esc, solo en la primera pregunta |
| Abajo, espectador | Cinta con pestaña burdeos «ELIMINADO», «Eres espectador hasta la siguiente partida», «Viendo a Ana» y las teclas A/D, Espacio y Esc | Tras quedar eliminado |

En el plató: el rótulo de apostados pasa a «APOSTADOS · BLOQUEADO» al cerrar y un ✓ grande aparece sobre la cabina correcta al revelar (además de sus bombillas). Se descartan de los bocetos el panel grande de cambio manual (la equivalencia va en el aviso: «Agrupar 10 fajos en 1 bolsa»), la tira del equipo, la tarjeta de comodines aparte y «Esc · Menú» fijo. Las cantidades siguen sin «$», como en el resto del juego, y los textos evitan «→» y «✓» porque Lilita One y Poppins no los tienen (el ✓ es un icono).

Rendimiento: todo se redibuja por eventos; por fotograma solo el reloj (al cambiar el segundo) y las animaciones mientras duran, con las cifras escritas en búferes de caracteres para no reservar memoria.

## Bocetos del HUD — 17 de octubre de 2026

[Siete estados visuales](bocetos/ui/hud/README.md): apuestas, transporte, comodín preparado, cambio manual, cierre, revelación y espectador. La pregunta y las opciones permanecen en la pantalla física del plató; el HUD usa información periférica y pistas contextuales por cercanía. La [especificación](bocetos/ui/hud/DISENO.md) fija comportamiento, importes ilustrativos, restricciones y puntos a validar. Montados en el juego como se describe en [HUD de la partida](#hud-de-la-partida); iconos en [HudUI](bocetos/ui/hud/recursos/README.md).

Recursos gráficos del menú: [18 PNG transparentes de MenuUI](bocetos/ui/menu/recursos/README.md), con títulos, hoja de iconos, ilustraciones del tutorial y personaje. Son recursos separados preparados para montaje posterior; no se ha modificado la interfaz de Unity.
