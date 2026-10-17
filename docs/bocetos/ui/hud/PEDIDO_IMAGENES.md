# Pedido de imágenes del HUD para ChatGPT

Recursos gráficos que faltan para montar el HUD de la partida a partir de los [bocetos](README.md), en su versión simplificada: tres piezas fijas arriba y paneles que solo aparecen cuando hacen falta. Solo hace falta **una imagen**: una hoja con seis iconos. El resto (teclas, placas, cintas, candado y ✓) sale del kit de los menús y de su [hoja de iconos](../menu/recursos/MenuUI/iconos.png).

## Antes de empezar

- Crea en Descargas una carpeta `HudUI` y guarda ahí la imagen con el nombre indicado.
- Puedes usar la misma conversación de ChatGPT en la que se hizo el menú (mantiene el estilo) o una nueva empezando por el mensaje 0.
- Los archivos que hay que adjuntar están en el proyecto:
  - Bocetos del HUD: `Trivias/docs/bocetos/ui/hud/01-betting.png` y `03-self-bet.png`.
  - Iconos del menú (estilo que hay que igualar): `Trivias/docs/bocetos/ui/menu/recursos/MenuUI/iconos.png`.
  - Dinero del plató: `Trivias/docs/bocetos/recursos/detallados/04-fajo.png`, `05-bolsa.png` y `06-lingote.png`.
- Total: 2 mensajes y 1 imagen.

---

## Mensaje 0 · Contexto (pegar primero)

Adjuntar: `01-betting.png`, `03-self-bet.png` e `iconos.png`.

```text
Vas a crear recursos gráficos sueltos para el HUD de mi videojuego (un concurso de trivias en un plató de televisión, estilo 3D cartoon tipo figura de vinilo). Te adjunto dos bocetos del HUD y la hoja de iconos que ya usa el menú. Todo lo que te pida debe tener EXACTAMENTE el mismo estilo que esa hoja de iconos: mismo acabado 3D brillante, mismos dorados, mismo azul marino, misma iluminación cálida.

Reglas para TODAS las imágenes que te pida:
1. Fondo 100 % transparente (PNG con canal alfa real). NO dibujes un fondo de cuadros grises/blancos simulando transparencia, NO pongas fondo de color, escenario, panel ni marco.
2. Cada icono centrado en su celda, con margen vacío alrededor (que nada toque los bordes de la celda ni se meta en la de al lado).
3. Sin texto, letras, números, firmas ni marcas de agua.
4. Formas grandes y simples: se verán pequeños (unos 64 píxeles) en una esquina de la pantalla, sobre un panel azul marino. Tienen que entenderse de un vistazo a ese tamaño.
5. Sin sombras proyectadas sobre un suelo (no hay suelo); como mucho una sombra suave pegada al objeto.

Confirma que lo has entendido y espera a mi primer pedido.
```

---

## Mensaje 1 · Hoja de iconos del HUD

Adjuntar: `iconos.png`, `04-fajo.png`, `05-bolsa.png`, `06-lingote.png` y `03-self-bet.png`. Guardar como `iconos-hud.png`.

```text
Crea una hoja de iconos en una cuadrícula de 3 columnas x 2 filas, en un lienzo horizontal de 1536x1024 (celdas cuadradas de 512x512), fondo transparente. Cada icono centrado en su celda ocupando aproximadamente el 75 % de ella, todos del mismo tamaño visual, mismo estilo 3D brillante que la hoja de iconos adjunta. Sin texto. En este orden (de izquierda a derecha, fila por fila):
Fila 1: (1) un fajo de billetes verdes atado con una faja beige, visto en perspectiva de tres cuartos, igual que el fajo de la imagen adjunta 04-fajo.png; (2) una bolsa de dinero de tela beige atada con cuerda, igual que la de 05-bolsa.png, SIN el símbolo del dólar; (3) un lingote de oro brillante, igual que el de 06-lingote.png, sin grabados ni texto.
Fila 2: (4) el comodín: una carta azul brillante con borde azul claro y una estrella blanca grande en el centro, ligeramente inclinada, igual que el icono de la tarjeta «COMODÍN PREPARADO» del boceto 03-self-bet.png; (5) un reloj: esfera blanca redonda con borde dorado grueso y dos agujas azul marino; (6) un aviso: círculo rojo brillante con un signo de exclamación blanco grueso en el centro.
```

Si el dinero sale distinto de las referencias: «El fajo/la bolsa/el lingote debe ser igual que la imagen adjunta, solo que como icono».

---

## Comprobar antes de guardar

- **Fondo transparente de verdad:** al abrirla no se ven cuadros grises dibujados. Si los hay: «El fondo debe ser transparente de verdad, no un patrón de cuadros».
- **Seis iconos, en su orden,** cada uno dentro de su celda y sin texto.
- **Se entienden en pequeño:** al alejar la imagen, cada icono se reconoce sin dudar.

Con `HudUI/iconos-hud.png` en Descargas, avisar en el chat del proyecto: se recorta la hoja y se monta el HUD.

[Volver a los bocetos del HUD](README.md).
