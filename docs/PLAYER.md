# Jugador — Personaje y prefab base

Referencia: [GDD](GDD.md), secciones 5, 7, 8 y 10.

## Base compartida

Referencias visuales: [bocetos del player](bocetos/player/README.md), [base sin ropa, pelo ni zapatos](bocetos/player/base/README.md) y [presentador](bocetos/presentador/README.md).

Concursante en juego (16 de octubre de 2026): el cuerpo base oficial sin ropa, pelo ni zapatos (`Player/Models/PlayerBase`, `base_basic_shaded` y `shaded.png`) tal cual, sin perder polígonos ni altura: un millón de triángulos y 1,9 m. Se ve igual que al importarlo en Unity: color base negro y su textura (con la luz ya pintada) como emisión, así que no depende de las luces del plató. Esqueleto puesto en Blender por consola (`PlayerBase_Rigged.fbx`) con los nombres de huesos humanos de Unity (cadera, columna, pecho, cuello, cabeza, hombros, brazos, manos, piernas, pies y dedos de los pies) y pose T; los pesos se calculan en una copia ligera y se traspasan punto a punto al modelo completo. Se importa como Humanoid y usa las animaciones de Starter Assets (reposo, andar, correr, saltar, caer). Sobre este cuerpo irán después la ropa, el pelo y los zapatos como piezas; mientras tanto, los trajes de colores de la tienda no tiñen nada (nunca la piel) y los cosméticos se colocarán de nuevo cuando lleguen las piezas. La cámara va algo más alta y más al lado para que la cabeza no tape el plató. Sin dedos articulados. Seis concursantes son seis millones de triángulos animados: si el juego va lento en equipos modestos, es la primera causa a revisar. Los modelos anteriores con ropa (`PlayerNew`, `PlayerModel`) se eliminaron: `PlayerBase` es el único modelo del concursante.

Boceto base modular (16 de octubre de 2026): [T-pose y A-pose, ocho vistas cada una](bocetos/player/base/README.md), sin ropa, pelo ni zapatos y con manos de manopla. Son dos posturas del mismo diseño para futuras piezas intercambiables. El antiguo despiece se retiró y ya no se usa. Esta fase es de bocetos en documentación; no sustituye ni modifica el concursante jugable descrito arriba. Las referencias documentales a los antiguos modelos de base y pelo, cuyos archivos se retiraron, dejan de ser vigentes.

Un personaje de prototipo bien construido sirve de base para diez personajes disponibles desde el inicio del juego completo. Cabeza grande, cuerpo pequeño, formas redondeadas y apariencia de figura de vinilo, con diseños propios.

Las variantes cambian atuendo, pelo, accesorios, colores y apariencia. Comparten movimiento, capacidades, esqueleto, animaciones, colisiones e interacción. No tienen poderes ni estadísticas diferentes.

Referencia visual de vestuario: [catálogo clothes](bocetos/player/clothes/README.md). Su primer conjunto, Blue Tracksuit, recupera la chaqueta y el pantalón azules originales en una única lámina de ropa aislada, proporcionada para la base neutra. El calzado se trabaja por separado. Es documentación visual para una fase futura, sin cambios al modelo jugable. El catálogo también incluye Casual, Gardener, Elegant, Explorer, Pajamas, Raincoat, Chef, Retro Sport y Winter (diez estilos en total): una lámina de cuatro vistas por conjunto, solo ropa, en A-pose siguiendo la base aprobada.

Peinados del player: [catálogo hair](bocetos/player/hair/README.md), con Wavy Sweep, Short Crop, Curly Puff, Bob, High Ponytail y Twin Buns. Una lámina de pelo aislado por estilo, con cuatro vistas y referencia al volumen de la cabeza base; el encaje geométrico se verificará al modelar.

Calzado del player: [catálogo shoes](bocetos/player/shoes/README.md). Diez estilos a juego con los diez conjuntos de ropa, una lámina de cuatro vistas por diseño. Puntera, empeine y talón cerrados para cubrir el pie base completo; el encaje geométrico y el solape con los pantalones se comprobarán al modelar.

## Estructura propuesta del prefab

- Raíz del concursante: identidad, movimiento, colisión y estado de partida.
- Modelo visual: esqueleto y animaciones comunes.
- Apariencia: piezas y materiales de atuendo, pelo y accesorios.
- Puntos de anclaje: mano para dinero y puntos comunes para accesorios.
- Identificación visible: nombre y color del jugador. El nombre se elige en el menú principal (perfil local) y el host lo hace único en la sala; sin nombre, «Jugador N». Con Steam será el nombre de la cuenta.

Esta estructura no confirma motor ni nombres de componentes. El objetivo es cambiar la apariencia sin duplicar lógica o crear diez controladores.

## Acciones de salida

| Acción | Regla |
| --- | --- |
| Desplazarse | Cámara en tercera persona con Cinemachine; controles con Input System. Anda a 3,2 m/s y corre a 5,6 m/s manteniendo Shift (o pulsando el stick izquierdo del mando); velocidades en `PlayerConfig`. Sube escalones de hasta 30 cm (las plataformas de apuesta) andando, y solo por su abertura delantera: la barandilla no se atraviesa ni se salta |
| Saltar | Espacio (o B en el mando): 0,9 m, solo desde el suelo, con la partida en marcha y sin pausa, con una espera de 0,3 s entre saltos (configurable en `PlayerConfig`). Usa la animación de salto de Starter Assets, sincronizada por red |
| Coger/soltar dinero | Un solo tipo por viaje: hasta 5 fajos, 2 bolsas o 5 lingotes (configurable), visibles en las manos. E: uno; Q: varios; R en la mesa: devolver todo. Registrado al depositar |
| Convertir dinero | Operación manual en la estación, conservando valor |
| Preparar comodín | Entrar en la zona de apuesta de la cabina y confirmar personalmente; su nombre aparece encima de la cabina |
| Cambiar/cancelar comodín | Permitido antes de verificar |
| Verificar | Solo jugadores activos; bloquea todas las apuestas |
| Hablar por micrófono | Separación de activos y eliminados durante el concurso |

Las interacciones se activan por cercanía con una tecla contextual (por ejemplo «E: coger», «E: apostarme»), sin apuntar con la cámara.

No hay señalar, gestos ni empujones confirmados (el salto y correr con Shift se añadieron el 16 de octubre de 2026). No introducir nuevas acciones de movimiento sin evaluar su necesidad.

## Estados

| Estado | Capacidad |
| --- | --- |
| En sala de espera | Solo mirar alrededor y hablar; sin moverse ni tocar el dinero hasta que empieza la partida |
| Activo | Mover dinero, hablar, convertir y verificar |
| Apuesta preparada | Activo con apuesta voluntaria todavía cancelable |
| Apuesta bloqueada | Respuesta comprometida, uso consumido, sin cambiar apuesta |
| Expulsión | Animación breve, el personaje sale disparado hacia arriba; no entra en el tubo |
| Espectador | Observa por encima del hombro de un activo (A/D o flechas cambian, Espacio vista general); sin interactuar ni comunicarse con activos |
| Resultados | Voz del grupo reunida, recompensa de equipo y regreso al vestíbulo |

Los usos de comodín son un contador separado del estado: acertar puede dejar al personaje activo con usos disponibles o gastados. Fallar elimina aunque queden usos personales; estos no se transfieren.

## Animaciones mínimas propuestas

Reposo, locomoción, sujetar dinero, transición de coger/depositar y expulsión. Reutilizar postura y animaciones entre fajos, bolsas y lingotes cuando funcione visualmente. No producir gestos ni celebraciones comprables para la primera salida.

## Personalización y comprobación

Probar una segunda apariencia sobre el prefab antes de producir las diez. Validar anclajes, deformaciones, alcance de manos, colisiones y legibilidad del nombre. La ropa no modifica velocidades, tamaños funcionales ni capacidad de carga.

Pendientes: asignación final de controles, colisión entre personajes, posición al bloquear una cabina y comportamiento ante desconexión. Evitar que un empujón comprometa involuntariamente el comodín.

<!-- FUTURO: gestos expresivos. Sin implementación inicial ni señales de eliminados hacia activos. -->
