# Jugador — Personaje y prefab base

Referencia: [GDD](GDD.md), secciones 5, 7, 8 y 10.

## Base compartida

Referencias visuales: [personajes completos A-pose](bocetos/player/characters/README.md) y [presentador](bocetos/presentador/README.md).

Concursante en juego (17 de octubre de 2026): uno de los diez personajes completos del usuario, tal cual (un millón de triángulos y 1,9 m cada uno, su textura como emisión, igual que el original), en `Player/Models/Characters/<Nombre>/<Nombre>_Rigged.fbx`. Se riggearon en Blender con el mismo esqueleto (nombres de hueso Humanoid de Unity) y pesos automáticos, sin tocar la malla; en Unity son Humanoid y usan las animaciones de Starter Assets (reposo, andar, correr, saltar, caer). `CharacterModelBuilder` los importa y los asigna al catálogo; el prefab del concursante se construye con el de por defecto (Blue Tracksuit) y `PlayerCharacterView` cambia el esqueleto, la malla y el avatar al elegir otro (solo en la sala de espera), y vuelve a crear en la cabeza el anclaje de los complementos. Seis concursantes son unos seis millones de triángulos: hay que medir el rendimiento en equipos modestos. Se descartó vestir un cuerpo base con piezas sueltas (pelo, ropa y zapatos): cada pieza generada trae tantos triángulos como un personaje entero (unos tres millones por concursante vestido, frente a uno de un personaje completo), y encajar piezas de proporciones distintas deja huecos y piel que asoma. Por eso el catálogo son diez personajes completos, cada uno riggeado con el mismo esqueleto Humanoid.

Antecedentes visuales: las bases sin ropa y las poses de prueba fueron eliminadas. La referencia vigente son los personajes completos en A-pose. Esto no altera el concursante jugable descrito arriba.

Un personaje de prototipo bien construido sirve de base para diez personajes disponibles desde el inicio del juego completo. Cabeza grande, cuerpo pequeño, formas redondeadas y apariencia de figura de vinilo, con diseños propios.

Las variantes cambian atuendo, pelo, accesorios, colores y apariencia. Comparten movimiento, capacidades, esqueleto, animaciones, colisiones e interacción. No tienen poderes ni estadísticas diferentes.

Catálogo visual vigente: [diez personajes completos en A-pose](bocetos/player/characters/README.md), cada uno en su carpeta y con una única lámina de ocho vistas. Blue Tracksuit conserva la imagen A-pose original aprobada, ahora renombrada; los otros nueve integran sus atuendos, peinados y zapatos sobre la misma identidad visual. No se generan T-poses ni despieces como entregables del catálogo actual.

Los antiguos bocetos separados de ropa, pelo, zapatos, bases y T-poses fueron eliminados por el usuario. El [archivo histórico restante](bocetos/player/archivo/README.md) conserva únicamente otros antecedentes. Este cambio organiza los bocetos de documentación; no modifica los modelos, rigs, prefabs ni el sistema de piezas que esté implementado en Unity.

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

Elegir personaje (16 de octubre de 2026): en el panel «TU PERSONAJE» a la derecha del menú principal, con el personaje en 3D y flechas, entre los que tiene el jugador (los seis gratuitos y los comprados); se guarda en su perfil, y quien no elige lleva el de por defecto (el primero de los gratuitos). El 3D lo pinta un expositor (`CharacterShowcase`: el mismo modelo en reposo sobre una peana, girando, con su cámara y dos luces), 300 m por debajo del plató; dibuja en una imagen que crea al cargar la escena y que el menú muestra (`CharacterPreview`, en Shared), y apaga su cámara en cuanto el menú se cierra. Para ello se desactivó el GPU Resident Drawer de URP: con él, Unity 6000.6 se cuelga al descargar una escena con una cámara que pinta en una imagen. Catálogo en `Shared/Config/Characters/CharacterCatalog.asset`: diez personajes, la lista de gratuitos (`Free Character Ids`, se cambia en el inspector sin tocar código; ningún Build All la pisa) y el precio de los de pago (1.200, en la tienda). El dueño pide su personaje al host, que lo acepta si nadie de la sala lo lleva o le da el primer gratuito libre; con seis gratuitos y seis jugadores como máximo siempre hay uno. Cada uno tiene su modelo con el mismo esqueleto (`PlayerCharacterView`); gratuitos de momento: Blue Tracksuit, Casual, Gardener, Elegant, Explorer y Pajamas. El expositor siempre se anima (su cámara pinta en una imagen) y las fotos de las tarjetas se hacen en postura de reposo.

Probar una segunda apariencia sobre el prefab antes de producir las diez. Validar anclajes, deformaciones, alcance de manos, colisiones y legibilidad del nombre. La ropa no modifica velocidades, tamaños funcionales ni capacidad de carga.

Pendientes: asignación final de controles, colisión entre personajes, posición al bloquear una cabina y comportamiento ante desconexión. Evitar que un empujón comprometa involuntariamente el comodín.

<!-- FUTURO: gestos expresivos. Sin implementación inicial ni señales de eliminados hacia activos. -->
