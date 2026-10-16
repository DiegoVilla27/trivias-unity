# Prompt de producción: personaje 3D base modular

Actúa como un artista profesional de personajes 3D para videojuegos, especializado en personajes estilizados cute, topología para animación, materiales y texturas para Unity. Crea un modelo 3D editable y terminado del personaje definido en las referencias de este proyecto. La prioridad es reproducir su identidad visual y construir una base sólida para animarlo y personalizarlo posteriormente.

Trabaja exclusivamente con herramientas locales. No uses Higgsfield ni envíes bocetos o modelos a ese servicio.

## Referencias y fidelidad

Analiza primero estas imágenes, utilizando rutas relativas a la raíz del proyecto:

- `docs/bocetos/player/poses/personaje-a-pose.png`: referencia principal de identidad, proporciones y silueta general.
- `docs/bocetos/player/poses/personaje-t-pose.png`: comprobación adicional de proporciones.
- `docs/bocetos/player/base/personaje-base-t-pose.png`: cuerpo completo sin ropa, pelo ni zapatos; sustituye al antiguo despiece retirado.

Actualización del 16 de octubre de 2026: el usuario retiró las láminas individuales del cuerpo. No utilizarlas ni recuperarlas como fuente activa. Este documento es un antecedente para una fase futura; la tarea actual consiste solo en bocetos.

Las láminas son referencias artísticas, no planos dimensionales perfectos. Algunas vistas laterales del brazo repiten información frontal y las del torso presentan pequeñas contradicciones. Resuelve esas diferencias construyendo un volumen coherente en tres dimensiones, conservando la identidad del personaje. No reproduzcas errores de proyección ni interpretes las superficies cortadas de las piezas como articulaciones visibles.

## Alcance central: cuerpo base sin vestir

El resultado de esta fase debe ser el personaje completo **sin ropa, sin cabello y sin zapatos**, preparado para recibir posteriormente diferentes peinados, camisas, pantalones, prendas y calzado intercambiables.

Modela cabeza, orejas, cuello, torso, pelvis, brazos completos, manos, piernas completas y pies. El cuerpo debe ser una figura neutra estilizada de juguete, con superficies suaves, sin genitales, pezones ni detalle íntimo. No añadas ropa interior, prendas pintadas sobre la piel ni volúmenes de vestuario integrados en el cuerpo.

Conserva ojos, cejas, nariz y sonrisa como parte de la identidad facial. El cuero cabelludo debe estar completo y suavemente redondeado debajo de los futuros peinados. Mantén la cabeza y los zapatos aprobados como guía de diseño sin reinterpretarlos; los zapatos y el pelo se producirán como accesorios en una fase posterior.

## Diseño y proporciones

Reproduce la estética chibi cute de las referencias: cabeza grande y ancha, mejillas suaves, mentón redondeado, cuello corto, torso compacto, barriga suave y extremidades cortas y redondeadas. Mide las relaciones de tamaño directamente en la referencia principal antes de modelar. Al retirar el volumen del pelo y de la ropa, conserva la escala del cráneo y del cuerpo subyacentes; no agrandes el cráneo para ocupar la silueta del cabello.

La cara debe conservar los grandes ojos marrones oscuros con zona blanca cálida, cejas amables, nariz pequeña y sonrisa discreta. Integra los ojos en el rostro con transiciones suaves y contorno de párpado; evita esferas excesivamente salientes. Mantén la lectura del personaje tanto de frente como de perfil y tres cuartos.

Los brazos incluyen hombro, brazo, codo, antebrazo, muñeca y mano. Las manos son pequeñas manoplas redondeadas, sin dedos individuales ni surcos que los simulen; admite únicamente un pequeño volumen de pulgar integrado y suave. Las piernas incluyen muslo, rodilla, pantorrilla, tobillo y pie. Los pies son volúmenes simples, sin dedos, dimensionados para encajar dentro de futuros zapatos.

Construye el cuerpo con simetría bilateral y una postura A neutra coherente con la referencia. Deja separación suficiente en axilas y entre piernas para trabajar el rig y la ropa. Evita alargar el personaje o darle anatomía adulta realista.

## Construcción y personalización futura

Construye superficies corporales continuas y coherentes. No sustituyas articulaciones por esferas, cilindros o cápsulas simplemente superpuestas. Los hombros, codos, muñecas, caderas, rodillas y tobillos deben mantener transiciones naturales dentro del estilo simplificado.

Conserva el cuerpo base completo bajo las futuras prendas. Esta versión maestra debe permitir crear después variantes que oculten zonas cubiertas para evitar intersecciones y reducir geometría, sin perder el original completo. No cortes ni elimines ahora torso, piernas, cuero cabelludo o pies.

Prepara el diseño para accesorios independientes: cabello que ajuste al cráneo, camisas sobre hombros y torso, pantalones sobre pelvis y piernas, zapatos alrededor de los pies. No crees todavía esos accesorios, sistemas de equipamiento ni variantes de vestuario. Documenta las zonas de ajuste y conserva una escala y postura comunes para las futuras piezas. La compatibilidad de prendas deformables requerirá posteriormente un esqueleto compartido y pesos adecuados; la separación de objetos por sí sola no la garantiza.

## Topología preparada para animación

Utiliza una malla de trabajo limpia, preferentemente de quads, con densidad razonable y flujo de aristas apropiado alrededor de hombros, axilas, codos, muñecas, caderas, entrepierna, rodillas y tobillos. Evita concentraciones de polos en zonas de flexión, caras muy estrechas y triangulación arbitraria. Prevé bucles faciales alrededor de ojos y boca para futuras expresiones, sin añadir expresiones o controles ahora.

Comprueba normales, continuidad, sombreado, vértices duplicados, caras internas accidentales e intersecciones. Las separaciones intencionales de ojos y otros componentes faciales son válidas si están bien integradas. No impongas una única malla a todos los componentes si perjudica el resultado.

Esta fase incluye modelado y preparación de topología; no incluye esqueleto, skinning, animaciones ni controladores. No afirmes haber validado deformaciones animadas si todavía no existe rig.

## Materiales y texturas

La piel debe ser de tono melocotón cálido, con acabado suave predominantemente mate y rubor muy sutil en mejillas. Los ojos deben tener mayor brillo y una lectura clara, sin parecer vidrio metálico. Mantén cejas marrones y una expresión amable. Evita poros realistas, ruido, sombras fuertes pintadas y brillo plástico uniforme.

Crea UVs ordenadas, con densidad de texel coherente, costuras discretas y sin estiramientos visibles. Usa mapas PBR solamente donde aporten valor. El color base no debe contener iluminación direccional ni reflejos horneados. Conserva la simetría donde sea útil y evita reflejar detalles faciales que necesiten tratamiento independiente.

Adapta los materiales al pipeline real del proyecto, verificándolo antes. Entrega materiales de Unity y texturas correctamente vinculados para que el modelo aparezca con su aspecto previsto al arrastrarlo al Hierarchy. No dependas únicamente de colores de vértice o materiales exclusivos del programa de modelado sin soporte en la importación.

## Organización y entrega

Inspecciona las convenciones existentes del proyecto antes de exportar. Guarda los recursos finales en `Assets/_Project/Modules/Player/Models/PlayerCharacter/`, con nombres claros para mallas, materiales y texturas. Usa unidades coherentes con Unity, orientación final +Y arriba y +Z adelante, y pivote centrado en el suelo entre los pies. Determina la altura a partir de la escala del proyecto y conserva las proporciones mediante escalado uniforme.

Entrega el archivo fuente editable del programa utilizado y una exportación apta para Unity que preserve UVs, normales y materiales. Si incluyes OBJ, acompáñalo de su MTL y texturas; no lo uses como sustituto de la fuente editable con quads. Incluye materiales nativos de Unity cuando el entorno permita generarlos. Informa del número de vértices, triángulos, materiales y resoluciones de texturas, así como de cualquier limitación real de exportación o importación.

No modifiques escenas, prefabs, lógica del juego ni recursos aprobados ajenos al personaje. No hagas commits como parte de esta tarea.

## Revisión de calidad

Revisa primero silueta y proporciones; después rostro y conexiones; finalmente topología, UVs, materiales y exportación. Produce vistas del modelo real de frente, espalda, ambos perfiles y tres cuartos con iluminación neutra, además de capturas de topología de las zonas de deformación. Compara esas vistas con las referencias y corrige diferencias antes de darlo por terminado. Las imágenes de presentación deben mostrar la geometría entregada.

El resultado se acepta cuando mantiene la identidad cute y las proporciones de los bocetos, tiene cabeza sin cabello, cuerpo neutral completo sin vestuario ni detalle íntimo, manos de manopla sin dedos, pies simples, conexiones limpias y materiales correctamente asignados. Debe servir como base editable para futuros peinados, ropa, zapatos y animaciones. Indica explícitamente qué comprobaciones se realizaron y cuáles quedaron pendientes por limitaciones de herramientas.
