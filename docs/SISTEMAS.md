# Sistemas — Reglas y responsabilidades

Referencia: [GDD](GDD.md), secciones 3–5 y 10–12.

## Partida

Flujo propuesto: preparación → inicio → pregunta/apuestas → bloqueo de tres segundos → resolución → siguiente pregunta o fin → resultados.

Hay 15 preguntas, cada una con tiempo límite para apostar (configurable); al agotarse se verifica sola. Las elige el host al empezar (`QuestionPlanner`, C# puro): tres bloques de dificultad, primero las que nadie del grupo ha visto según el historial (`QuestionHistory` en cada ordenador, combinado por `QuestionHistoryNetwork`) y sin dos seguidas de la misma categoría entre las igual de nuevas. Detalle en [CONTENIDO](CONTENIDO.md). Resolver puede producir derrota anticipada por dinero cero o ausencia de jugadores activos. Victoria exige resolver las 15 con dinero y supervivientes. El reloj total empieza al inicio común del concurso y termina al resolver el resultado final; incluye discusión, animaciones y cuentas atrás.

## Dinero

Responsabilidad: controlar valor, posición, portador y bandeja; convertir manualmente sin duplicar; registrar apuestas al pulsar verificar; aplicar pérdidas y multiplicador; regresar dinero a la mesa.

Capital propuesto: 100.000; denominaciones propuestas: fajo 5.000, bolsa 50.000 y lingote 500.000. Pendientes de validación, no constantes dispersas por el código.

Cada jugador lleva varios objetos en las manos, todos de la misma denominación, hasta el límite de ese tipo (propuesta: 5 fajos, 2 bolsas, 5 lingotes); se dejan en orden inverso al cogerlos. Objetos colocados en bandejas. La regla de perder dinero fuera de bandejas sigue siendo propuesta.

División confirmada: la estación también convierte un lingote en 10 bolsas y una bolsa en 10 fajos.

Emisión del bono: el dinero apostado en la respuesta correcta vuelve a la mesa tal cual; el bono se emite en la denominación más grande posible (lingotes, bolsas y fajos, en ese orden). Ejemplo: 6.400.000 → 12 lingotes y 8 bolsas.

## Comodines

Distribución propuesta con seis usos totales:

| Grupo inicial | Usos personales por jugador | Reserva |
| --- | --- | --- |
| 2 | 3 | 0 |
| 3 | 2 | 0 |
| 4 | 1 | 2 |
| 5 | 1 | 1 |
| 6 | 1 | 0 |

Apostarse exige estar en la zona de apuesta de la cabina y confirmar. Encima de la cabina se muestra el nombre de cada apostado; el estado lo replica el host. Solo usar reserva al agotarse usos personales. No recalcular tras eliminaciones. Cada persona se apuesta como máximo una vez por pregunta, confirma voluntariamente y consume el uso al verificar. Acertar conserva al jugador; fallar lo elimina y pierde sus usos restantes.

Recuperación: dinero en respuesta correcta × (1 + jugadores comprometidos en esa respuesta). Los usos disponibles o gastados en otras rondas no incrementan este multiplicador.

Propuesta técnica: reservar un uso compartido al preparar la apuesta y liberarlo al cancelar; confirmarlo al cierre. Dos jugadores no pueden comprometer el mismo uso restante simultáneamente.

## Fichas y compras

Preguntas superadas: 0–2 → 0 fichas; 3–5 → 50; 6–10 → 100; 11–14 → 150; 15 con victoria → 250. Cantidad completa por persona, no repartida ni acumulación de tramos. Incluye eliminados que permanecen hasta resultados. Entrega única.

Las fichas se guardan en un perfil local por jugador (JSON en la carpeta de datos del juego, escritura segura) y se gastan en cosméticos. Se entregan al ver los resultados, una sola vez por partida (identificador de partida). El dinero se reinicia cada partida y se registra como premio, no se transforma en fichas. Lo equipado se publica por red para que todos lo vean. Desconexiones/abandonos y sincronización en la nube pendientes.

## Ranking

Solo victorias, por tamaño inicial de grupo. Orden: dinero descendente, tiempo total ascendente; empate exacto comparte puesto. Animaciones y conversación cuentan. Menús o demoras no detienen el reloj. La ceremonia posterior al resultado no cuenta.

Primero resultado y récord local; servicio global después. Implementado el récord local (módulo `Ranking`): `LocalRecords` guarda en `trivias-records.json` las 10 mejores victorias de cada tamaño de grupo; `RankingRules` (C# puro, probado) decide quién entra (victoria desde la pregunta 1, de 2 a 6 jugadores), ordena por dinero y tiempo en milisegundos y hace que los empates exactos compartan puesto. Los resultados presentan cada partida una sola vez y muestran el puesto; el menú tiene la tabla por categorías. Identidad del equipo, tratamiento de preguntas de distinta dificultad, versionado de reglas, reconexión y validación de resultados pendientes.

## Red y comunicación

Tecnología: Netcode for GameObjects sobre Facepunch.Steamworks con transporte de la comunidad (relay de Steam, sin exponer IP). Un jugador actúa como host y es la autoridad de partida: valida conversiones, propiedad de objetos, cierre, consumo de usos, resolución y tiempo. Los demás jugadores muestran el mismo estado. En desarrollo se usa el AppID de pruebas 480.

Voz: API de voz de Steam. Cada cliente captura y comprime su voz y la envía al host, que la reenvía solo a los jugadores de su mismo canal. Desconexiones durante la partida: el dinero que llevaba vuelve a la mesa; su comodín se anula (si usaba la reserva y aún no se había verificado, el uso vuelve al equipo); deja de contar como activo y no figura como superviviente ni expulsado; el reparto de comodines no se recalcula. Si no queda ningún activo, la partida termina en derrota. Si se va el host, la sala se cierra y todos vuelven al menú con un aviso.

Nombre del concursante: lo pide el dueño desde su perfil (`PlayerProfile.PlayerName`) y el host lo acepta solo tras limpiarlo y hacerlo único (`PlayerNameRules`, C# puro: letras latinas, números, espacios y «- _ .», 16 caracteres como máximo; sin nombre, «Jugador N»; si se repite, « 2», « 3»…). Con Steam se tomará el nombre de la cuenta en lugar del perfil. Las listas de nombres en red (supervivientes y expulsados) usan textos de 512 bytes y se recortan sin fallar.

Los activos hablan entre sí; los eliminados entre sí. Todos se reúnen en resultados. No permitir nuevas incorporaciones como concursante a mitad de partida. Reconectar al mismo participante necesita una regla explícita.

## Espectador

El módulo `Spectator` actúa cuando el concursante local queda eliminado. Tras 2,5 s (para ver la expulsión) mueve la cámara al hombro de un activo por el mismo canal que usa el concursante local. A/D o flechas cambian de activo y Espacio alterna con la vista general. Si el que mira cae o se va, pasa al siguiente; si no queda nadie, vista general. El rótulo solo se ve mientras se juega (no en resultados) y todo se deshace al volver a la sala. Las restricciones del eliminado (no toca dinero, cabinas ni botón) siguen en Interaction, Match y SelfBet.

## Opciones del jugador

`GameSettings` (Shared) guarda en `trivias-settings.json` (carpeta de datos del juego) volúmenes, sensibilidad e inversión del ratón, pantalla completa y la última dirección usada para unirse. Un archivo dañado vuelve a las opciones de fábrica. Lo leen el sonido (Feedback), la mirada del concursante (Player) y el menú, sin conocerse entre sí. El menú de pausa cierra `PlayerControlGate` (el concursante se queda quieto) sin pausar la partida para los demás.

## Invitar a la sala

Al crear la sala, el host calcula una vez sus direcciones IPv4 y las ordena (`HostAddressRanker`): primero redes virtuales (ZeroTier, Radmin VPN, Tailscale, Hamachi; reconocidas por el adaptador o el rango), después la red local; nunca la interna, autoasignada ni de multidifusión. Si no hay red, ofrece `127.0.0.1` (solo este ordenador). Quien se une escribe la dirección y se valida (IPv4 o nombre de equipo) antes de conectar.

## Sonido y celebraciones

El módulo `Feedback` observa la partida, el dinero y los comodines sin intervenir: los avisos de red solo marcan qué cambió y el sonido se decide en `LateUpdate`, en cada equipo por separado (no viaja por red). Las reglas de qué suena (C# puro, probado) están en `FeedbackRules`. Coger/dejar dinero suena solo para quien lo hace; el resto lo oyen todos. Confeti (`StageConfetti`): de colores a los lados de la pantalla en cada acierto (el equipo conserva dinero) y lluvia dorada en toda la sala durante la victoria. Victoria: cheque gigante en la pantalla (a nombre de los supervivientes, con el premio final), podio dorado que sube delante de la pantalla con una placa por concursante (dorada para supervivientes, gris para expulsados; sin colisiones) y resultados tras 5 s de celebración.

Ambiente de luz (`StageLighting`): cuatro focos de colores, la luz ambiente y la principal cambian con fundido según el momento — sala neutra, preguntas fáciles cálidas, medias moradas, difíciles rojas y más oscuras, latido en la cuenta atrás, verde al acertar, rojo al fallar, dorado al ganar y apagado al perder. Valores en `Feedback/Config/StageMoodConfig.asset`. Detalle en [RECURSOS](RECURSOS.md#sonido).

## Verificaciones esenciales

- Dos jugadores intentan coger el mismo objeto: solo uno lo obtiene.
- Conversión simultánea o cercana al cierre: valor conservado, sin duplicados.
- Un objeto solo pertenece a una bandeja al bloquear.
- Botón pulsado varias veces: una resolución, un consumo y una recompensa.
- Dos apuestas usan la última reserva: solo una queda preparada válidamente.
- Dinero positivo pero cero supervivientes: derrota, sin ranking.
- Perder en la pregunta 15: 150 fichas, sin victoria.
- Resultado repetido: no duplica fichas.
- Tiempo de discusión y animación aumenta el tiempo total.
- Un bono grande se emite en lingotes y bolsas, sin generar cientos de fajos.
- Dividir un lingote o una bolsa conserva el valor exacto.
- Un eliminado no se oye en el canal de activos aunque manipule su cliente, porque el host filtra la voz.
