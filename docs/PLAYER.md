# Jugador — Personaje y prefab base

Referencia: [GDD](GDD.md), secciones 5, 7, 8 y 10.

## Base compartida

Referencias de modelado: [T-pose y A-pose del personaje y presentador](bocetos/poses/README.md).

Un personaje de prototipo bien construido sirve de base para diez personajes disponibles desde el inicio del juego completo. Cabeza grande, cuerpo pequeño, formas redondeadas y apariencia de figura de vinilo, con diseños propios.

Las variantes cambian atuendo, pelo, accesorios, colores y apariencia. Comparten movimiento, capacidades, esqueleto, animaciones, colisiones e interacción. No tienen poderes ni estadísticas diferentes.

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
| Desplazarse | Cámara en tercera persona con Cinemachine; controles con Input System |
| Coger/soltar dinero | Un solo tipo por viaje: hasta 5 fajos, 2 bolsas o 5 lingotes (configurable), visibles en las manos. E: uno; Q: varios; R en la mesa: devolver todo. Registrado al depositar |
| Convertir dinero | Operación manual en la estación, conservando valor |
| Preparar comodín | Entrar en la zona de apuesta de la cabina y confirmar personalmente; su nombre aparece encima de la cabina |
| Cambiar/cancelar comodín | Permitido antes de verificar |
| Verificar | Solo jugadores activos; bloquea todas las apuestas |
| Hablar por micrófono | Separación de activos y eliminados durante el concurso |

Las interacciones se activan por cercanía con una tecla contextual (por ejemplo «E: coger», «E: apostarme»), sin apuntar con la cámara.

No hay señalar, gestos, salto, sprint o empujones confirmados. No introducir nuevas acciones de movimiento sin evaluar su necesidad.

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
