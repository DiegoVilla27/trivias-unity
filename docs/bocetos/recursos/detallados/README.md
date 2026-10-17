# Referencias individuales de recursos

Fecha: 15 de octubre de 2026. **36 láminas individuales** generadas con la herramienta integrada de imágenes a partir de las referencias de [recursos](../recursos-modelado-01.png) y [ambiente](../../plato/ambiente-modelado-01.png).

Cada lámina reúne vistas del objeto y, cuando corresponde, perspectiva, despiece o secciones. No son modelos 3D ni planos dimensionales. Las vistas generadas requieren ajustar proyecciones y proporciones al mismo modelo; no asumir que todas son ortográficas exactas por su etiqueta.

Descargar el conjunto con imágenes y prompts (archivo retirado).

## Objetos principales

| Recurso | Lámina | Prompt |
| --- | --- | --- |
| Cabina | [Abrir imagen](01-cabina.png) | [Prompt](01-cabina-prompt.md) |
| Mesa central | [Abrir imagen](02-mesa.png) | [Prompt](02-mesa-prompt.md) |
| Boton de verificar | [Abrir imagen](03-boton.png) | [Prompt](03-boton-prompt.md) |
| Fajo de billetes | [Abrir imagen](04-fajo.png) | [Prompt](04-fajo-prompt.md) |
| Bolsa de dinero | [Abrir imagen](05-bolsa.png) | [Prompt](05-bolsa-prompt.md) |
| Lingote de oro | [Abrir imagen](06-lingote.png) | [Prompt](06-lingote-prompt.md) |
| Pantalla del concurso | [Abrir imagen](07-pantalla.png) | [Prompt](07-pantalla-prompt.md) |
| Estacion de cambio manual | [Abrir imagen](08-cambio-manual.png) | [Prompt](08-cambio-manual-prompt.md) |
| Podio | [Abrir imagen](09-podio.png) | [Prompt](09-podio-prompt.md) |
| Cheque gigante | [Abrir imagen](10-cheque.png) | [Prompt](10-cheque-prompt.md) |

## Cámaras e iluminación

| Recurso | Lámina | Prompt |
| --- | --- | --- |
| Camara de estudio | [Abrir imagen](11-camara.png) | [Prompt](11-camara-prompt.md) |
| Foco con viseras | [Abrir imagen](12-foco.png) | [Prompt](12-foco-prompt.md) |
| Foco pequeno | [Abrir imagen](13-foco-pequeno.png) | [Prompt](13-foco-pequeno-prompt.md) |
| Tira de luces | [Abrir imagen](14-tira-luces.png) | [Prompt](14-tira-luces-prompt.md) |

## Paredes y cortinas

| Recurso | Lámina | Prompt |
| --- | --- | --- |
| Panel de pared | [Abrir imagen](15-pared.png) | [Prompt](15-pared-prompt.md) |
| Esquina de pared | [Abrir imagen](16-esquina-pared.png) | [Prompt](16-esquina-pared-prompt.md) |
| Columna | [Abrir imagen](17-columna.png) | [Prompt](17-columna-prompt.md) |
| Cortina recta | [Abrir imagen](18-cortina-recta.png) | [Prompt](18-cortina-recta-prompt.md) |
| Cortina lateral recogida | [Abrir imagen](19-cortina-lateral.png) | [Prompt](19-cortina-lateral-prompt.md) |
| Cortina superior | [Abrir imagen](20-cortina-superior.png) | [Prompt](20-cortina-superior-prompt.md) |

## Suelo y escenario

| Recurso | Lámina | Prompt |
| --- | --- | --- |
| Baldosa de piso | [Abrir imagen](21-piso.png) | [Prompt](21-piso-prompt.md) |
| Inserto recto de piso | [Abrir imagen](22-inserto-recto.png) | [Prompt](22-inserto-recto-prompt.md) |
| Inserto curvo de piso | [Abrir imagen](23-inserto-curvo.png) | [Prompt](23-inserto-curvo-prompt.md) |
| Borde de escenario | [Abrir imagen](24-borde-escenario.png) | [Prompt](24-borde-escenario-prompt.md) |
| Escalera de escenario | [Abrir imagen](25-escalera.png) | [Prompt](25-escalera-prompt.md) |
| Barandilla | [Abrir imagen](26-barandilla.png) | [Prompt](26-barandilla-prompt.md) |

## Techo y accesorios

| Recurso | Lámina | Prompt |
| --- | --- | --- |
| Panel de techo | [Abrir imagen](27-techo.png) | [Prompt](27-techo-prompt.md) |
| Viga modular | [Abrir imagen](28-viga.png) | [Prompt](28-viga-prompt.md) |
| Conector de vigas | [Abrir imagen](29-conector-viga.png) | [Prompt](29-conector-viga-prompt.md) |
| Soporte de foco | [Abrir imagen](30-soporte-foco.png) | [Prompt](30-soporte-foco-prompt.md) |
| Cable de techo | [Abrir imagen](31-cable.png) | [Prompt](31-cable-prompt.md) |
| Altavoz | [Abrir imagen](32-altavoz.png) | [Prompt](32-altavoz-prompt.md) |
| Rejilla de ventilacion | [Abrir imagen](33-rejilla.png) | [Prompt](33-rejilla-prompt.md) |

## Componentes de cabina

| Recurso | Lámina | Prompt |
| --- | --- | --- |
| Tubo de expulsion modular | [Abrir imagen](34-tubo-expulsion.png) | [Prompt](34-tubo-expulsion-prompt.md) |
| Bandeja de dinero | [Abrir imagen](35-bandeja.png) | [Prompt](35-bandeja-prompt.md) |
| Plataforma de jugador | [Abrir imagen](36-plataforma-jugador.png) | [Prompt](36-plataforma-jugador-prompt.md) |

## Notas para modelar

- Definir una escala común tomando el personaje base antes de establecer medidas de mesa, botón y cabina. No hay medidas finales en estas imágenes.
- Construir un único modelo consistente: usar la vista que mejor describe cada volumen y corregir discrepancias entre las demás. Comparar siluetas además de etiquetas.
- Las partes de despiece pueden ser objetos separados para producción, pero no obligan a modelar mecanismos invisibles o tornillería interna. Priorizar superficies visibles en el juego.
- Cabina: según GDD v0.10, el jugador no entra en la carcasa ni en el tubo. Ignorar la «cámara del jugador de pie» del corte interno; el tubo es decorativo. La apuesta se hace en una zona delantera y encima de la cabina va un texto con los nombres de los apostados. El tubo está parcialmente recortado en algunas vistas; completar con la lámina modular 34.
- Fajo: la vista etiquetada INFERIOR repite una vista lateral; usar el contorno de SUPERIOR para la base, mantener la banda envolvente y definir cara inferior en el modelo.
- Pared: las dos proyecciones horizontales aparecen bajo un único rótulo SUPERIOR; interpretar por sus superficies y derivar espesor uniforme. Esquina: FRENTE y ESPALDA son similares; la trasera debe derivarse de los paneles del modelo, no copiarse como esquina interior duplicada.
- Focos: algunas vistas SUPERIOR/INFERIOR son oblicuas. Modelar cuerpo cilíndrico y soporte a partir de laterales y despiece.
- Piso: la lámina desarrolla un módulo cuadrado de cuatro baldosas. Puede separarse en una baldosa repetible; las vistas FRENTE/ESPALDA muestran caras planas y no una elevación de suelo colocado horizontalmente.
- Inserto curvo: la imagen propone un canal azul con borde dorado, más elaborado que la tira dorada original. Es una variante visual, no una regla nueva; simplificar a tira si facilita producción.
- Rejilla: la cantidad dibujada de lamas varía respecto al texto. Elegir un número fijo para el modelo; no tiene efecto de juego.
- Podio y cheque: tienen soportes distintos entre láminas. Elegir un único montaje coherente al ensamblarlos; el cheque individual también puede ser un tablero independiente.
- Cable: se desarrolla como lazo decorativo con soporte. No requiere sistema físico ni simulación de cables.
- Las etiquetas pueden contener errores menores. Las imágenes son bocetos visuales de modelado, no documentación técnica certificada.

Estas láminas amplían detalle visual y no añaden funciones de cámaras operativas, mecanismos reales, simulación de telas o voz del presentador.

