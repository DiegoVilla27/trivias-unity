> Entrega generada: [18 PNG de MenuUI](recursos/README.md). Se conserva este pedido como especificación original.

# Pedido de imágenes del menú para ChatGPT

Recursos gráficos que faltan para montar el menú igual que los [bocetos](README.md). Se piden en ChatGPT, **una imagen por mensaje**, siempre en la **misma conversación** para mantener el estilo.

## Antes de empezar

- Crea en Descargas una carpeta `MenuUI` y guarda ahí cada imagen con el nombre indicado.
- Los archivos que hay que adjuntar están en el proyecto:
  - Bocetos del menú: `Trivias/docs/bocetos/ui/menu/` (`01-main-menu.png`, `02-join-room.png`…).
  - Personaje de referencia: `Trivias/docs/bocetos/player/characters/01-blue-tracksuit/blue-tracksuit-a-pose.png`.
- Total: 19 mensajes y 18 imágenes (1 mensaje de contexto, 9 títulos, 1 hoja de iconos, 6 ilustraciones del tutorial y 2 del personaje).

---

## Mensaje 0 · Contexto (pegar primero)

Adjuntar: `01-main-menu.png`, `05-shop-accessories.png`, `06-how-to-play.png`, `10-exit-confirmation.png` y `blue-tracksuit-a-pose.png`.

```text
Vas a crear recursos gráficos sueltos para la interfaz de mi videojuego (un concurso de trivias en un plató de televisión, estilo 3D cartoon tipo figura de vinilo chibi). Te adjunto los bocetos de las pantallas del menú y el personaje de referencia. Todo lo que te pida debe tener EXACTAMENTE el mismo estilo que esos bocetos: mismos dorados, mismo azul noche, misma iluminación cálida de plató, mismo acabado 3D brillante.

Reglas para TODAS las imágenes que te pida:
1. Fondo 100 % transparente (PNG con canal alfa real). NO dibujes un fondo de cuadros grises/blancos simulando transparencia, NO pongas fondo de color, escenario, panel ni marco.
2. Solo el elemento pedido, centrado, con margen vacío alrededor (que nada toque los bordes).
3. Sin texto extra, firmas ni marcas de agua. Si el elemento lleva texto, escríbelo letra por letra exactamente como te lo indique, con sus tildes y signos.
4. El personaje siempre es el de la referencia «Blue Tracksuit»: niño chibi de pelo castaño ondulado, ojos grandes marrones, chándal azul con franjas blancas y zapatillas color crema. No cambies su cara, pelo, proporciones ni ropa.
5. Sin sombras proyectadas sobre un suelo (no hay suelo); como mucho una sombra suave pegada al objeto.

Confirma que lo has entendido y espera a mi primer pedido.
```

---

## Títulos (9 mensajes, uno por título)

En cada mensaje, adjuntar el boceto indicado en la tabla y pegar este texto cambiando `[TEXTO]`:

```text
Crea el título «[TEXTO]» como letrero de letras gruesas doradas en relieve 3D, igual que el título «[TEXTO]» del boceto adjunto: tipografía redondeada y gruesa (estilo Lilita One), relleno dorado con degradado de amarillo claro arriba a dorado anaranjado abajo, brillo en el borde superior, contorno grueso azul marino muy oscuro y una sombra suave detrás. Solo las letras, fondo transparente, formato horizontal 1536x1024 con el título centrado ocupando el 80 % del ancho. Texto exacto: [TEXTO]
```

| Guardar como | [TEXTO] | Adjuntar |
| --- | --- | --- |
| `titulo-trivias.png` | TRIVIAS (y debajo, en letra blanca más pequeña con contorno oscuro: «Un concurso para jugar en equipo») | `01-main-menu.png` |
| `titulo-unirme.png` | UNIRME A UNA SALA | `02-join-room.png` |
| `titulo-sala-espera.png` | SALA DE ESPERA | `03-lobby.png` |
| `titulo-tienda.png` | TIENDA | `04-shop-characters.png` |
| `titulo-como-se-juega.png` | CÓMO SE JUEGA | `06-how-to-play.png` |
| `titulo-records.png` | RÉCORDS | `07-records.png` |
| `titulo-configuracion.png` | CONFIGURACIÓN | `08-settings.png` |
| `titulo-menu-partida.png` | MENÚ DE PARTIDA | `09-pause.png` |
| `titulo-salir-sala.png` | ¿SALIR DE LA SALA? | `10-exit-confirmation.png` |

Revisar las tildes antes de guardar. Si el texto sale mal: «Repítelo, el texto debe ser exactamente: …».

---

## Hoja de iconos (1 mensaje)

Adjuntar: `03-lobby.png`, `05-shop-accessories.png`, `07-records.png` y `08-settings.png`. Guardar como `iconos.png`.

```text
Crea una hoja de iconos en una cuadrícula de 4 columnas x 4 filas, en un lienzo cuadrado 1024x1024, fondo transparente. Cada icono centrado en su celda, todos del mismo tamaño visual, con margen entre ellos, mismo estilo 3D brillante dorado y azul marino de los bocetos adjuntos. Sin texto debajo de los iconos. En este orden (de izquierda a derecha, fila por fila):
Fila 1: (1) ficha del juego: moneda dorada gruesa con una estrella dorada sobre un círculo azul marino en el centro y borde con muescas, como la de «1.500 fichas» del boceto de la tienda de accesorios; (2) círculo verde con un check blanco; (3) candado dorado cerrado; (4) corona dorada pequeña de anfitrión.
Fila 2: (5) icono de copiar: dos hojas superpuestas, trazo azul marino oscuro, sin relleno de fondo; (6) jugador vacío: silueta de persona simple blanca y gris dentro de un círculo de línea discontinua blanca; (7) altavoz con ondas de sonido blanco dentro de un círculo azul marino con borde dorado; (8) engranaje blanco dentro de un círculo azul marino con borde dorado.
Fila 3: (9) botón redondo azul marino con borde dorado grueso y una flecha blanca apuntando a la IZQUIERDA; (10) el mismo botón con la flecha apuntando a la DERECHA; (11) medalla de oro redonda con el número 1; (12) medalla de plata con el número 2.
Fila 4: (13) medalla de bronce con el número 3; las celdas 14, 15 y 16 vacías.
```

---

## Ilustraciones del tutorial (6 mensajes)

En todos, adjuntar `06-how-to-play.png` y `blue-tracksuit-a-pose.png` y pegar este texto cambiando `[N]` y `[ESCENA]`:

```text
Crea la ilustración de la página [N] del tutorial «Cómo se juega», con el mismo estilo que la escena del boceto adjunto (personaje y elementos del plató en 3D cartoon brillante). Es solo la ilustración, sin el panel, sin títulos, sin textos de explicación, sin botones y SIN fondo de plató: fondo transparente. Formato horizontal 1536x1024, todo el contenido dentro de una franja horizontal centrada (aprox. 1536x560), con margen arriba y abajo. Escena: [ESCENA]
```

| Guardar como | [N] | [ESCENA] |
| --- | --- | --- |
| `tutorial-1-objetivo.png` | 1 | Igual que el boceto: el personaje Blue Tracksuit corriendo contento con un fajo de billetes verdes en las manos, a la izquierda; a la derecha, cuatro cabinas de concurso con arco de bombillas: A roja, B azul, C naranja y D verde, cada una con su letra en un círculo arriba y una bandeja vacía delante. |
| `tutorial-2-dinero.png` | 2 | El personaje llevando un fajo de billetes hacia una bandeja dorada. A la derecha, de izquierda a derecha: un fajo de billetes verdes, una flecha, una bolsa de dinero, una flecha y un lingote de oro, mostrando que se cambian unos por otros. Sin texto ni números. |
| `tutorial-3-verificar.png` | 3 | El personaje pulsando con la mano un gran botón rojo de concurso sobre un pedestal dorado, con el texto «VERIFICAR» en el pedestal. Junto a él, tres bocadillos redondos dorados con los números 3, 2 y 1, de grande a pequeño. |
| `tutorial-4-comodin.png` | 4 | El personaje de pie, valiente y sonriente, sobre una plataforma redonda iluminada delante de una cabina dorada con bombillas, con un foco de luz cálida sobre él y un gran «×2» dorado brillante flotando a su lado. |
| `tutorial-5-fichas.png` | 5 | El personaje feliz con los brazos llenos de fichas doradas (las monedas con estrella azul del juego); a su lado, flotando, una gorra azul, unas gafas de sol negras y una pajarita roja, como premios de una tienda. Sin billetes. |
| `tutorial-6-controles.png` | 6 | El personaje en el centro con gesto de «¡vamos!»; alrededor, flotando, teclas de teclado 3D blancas con borde dorado con estas letras exactas: W, A, S, D (en cruz), Shift, Espacio, E, Q, R, Esc, y un ratón de ordenador blanco. |

---

## Personaje para «Unirme a una sala» (1 mensaje)

Adjuntar: `02-join-room.png` y `blue-tracksuit-a-pose.png`. Guardar como `personaje-senalando.png`.

```text
Crea solo al personaje Blue Tracksuit de cuerpo entero en la misma pose que en el boceto adjunto: una mano en la cadera y el otro brazo extendido hacia la derecha con la palma abierta, señalando amablemente, sonriendo. Mismo estilo 3D. Fondo transparente, formato vertical 1024x1536, personaje completo de pies a cabeza centrado, con margen alrededor. Sin suelo ni escenario.
```

## Personaje abriendo la puerta (1 mensaje)

Adjuntar: `10-exit-confirmation.png` y `blue-tracksuit-a-pose.png`. Guardar como `personaje-puerta.png`.

```text
Crea la ilustración del boceto adjunto: el personaje Blue Tracksuit caminando y abriendo una puerta azul de madera con pomo dorado, con luz cálida saliendo por la rendija. Igual que en el boceto pero solo personaje y puerta: fondo transparente, sin panel, sin título, sin textos. Formato cuadrado 1024x1024, todo centrado con margen alrededor.
```

---

## Comprobar antes de guardar cada imagen

- **Fondo transparente de verdad:** al abrirla no se ven cuadros grises dibujados. Si los hay: «El fondo debe ser transparente de verdad, no un patrón de cuadros».
- **Texto exacto,** con tildes y signos.
- **Mismo personaje** que la referencia.

Con la carpeta `MenuUI` completa (18 archivos), avisar en el chat del proyecto: se recorta la hoja de iconos, se ajustan los tamaños y se monta cada pantalla.

[Volver a los bocetos del menú](README.md).
