# Accesorios 3D

Diez modelos rígidos modelados localmente en Blender a partir de los [bocetos aprobados](../../bocetos/player/accessories/README.md), sin servicios externos.

![Renders reales](preview.png)

## Modelos y fuentes

Los OBJ se importan desde `Assets/_Project/Modules/Shop/Models/Accessories`, con MTL, texturas de color, normales para telas y materiales URP Lit remapeados explícitamente por el importador. Las fuentes editables, GLB portátiles y cinco renders reales por objeto están en las carpetas de esta documentación.

| Accesorio | OBJ Unity | Blender editable | Anclaje previsto |
| --- | --- | --- | --- |
| Corona | [Crown.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/Crown/Crown.obj) | [Crown.blend](Crown/Crown.blend) | Cabeza |
| Pajarita | [BowTie.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/BowTie/BowTie.obj) | [BowTie.blend](BowTie/BowTie.blend) | Cuello |
| Sombrero de copa | [TopHat.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/TopHat/TopHat.obj) | [TopHat.blend](TopHat/TopHat.blend) | Cabeza |
| Gafas de estrella | [StarGlasses.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/StarGlasses/StarGlasses.obj) | [StarGlasses.blend](StarGlasses/StarGlasses.blend) | Cara |
| Bigote | [Mustache.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/Mustache/Mustache.obj) | [Mustache.blend](Mustache/Mustache.blend) | Cara |
| Sombrero de pirata | [PirateHat.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/PirateHat/PirateHat.obj) | [PirateHat.blend](PirateHat/PirateHat.blend) | Cabeza |
| Antifaz | [SuperheroMask.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/SuperheroMask/SuperheroMask.obj) | [SuperheroMask.blend](SuperheroMask/SuperheroMask.blend) | Cara |
| Halo | [Halo.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/Halo/Halo.obj) | [Halo.blend](Halo/Halo.blend) | Cabeza |
| Alas de ángel | [AngelWings.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/AngelWings/AngelWings.obj) | [AngelWings.blend](AngelWings/AngelWings.blend) | Espalda |
| Alas de dragón | [DragonWings.obj](../../../Assets/_Project/Modules/Shop/Models/Accessories/DragonWings/DragonWings.obj) | [DragonWings.blend](DragonWings/DragonWings.blend) | Espalda |

## Colocación y alcance

Unidades: metros; OBJ con Y arriba y Z delante. Origen en la base de apoyo para corona y sombreros, puente nasal para gafas/antifaz, centro del labio para bigote, nudo para pajarita, centro del aro para halo y soporte central para alas. El halo se coloca por encima del pelo.

No se ha comprobado el encaje sobre los diez personajes: hay que ajustar posición/escala y ocultar el pelo que atraviese sombreros. El catálogo implementado no cambia. Su sistema actual coloca cabeza/cara/cuello en `HeadAnchor`; las alas requieren incorporar un anclaje de espalda cuando se integren. No se cambian precios, escenas ni equipamiento.

## Producción

[Generador Blender](build_accessories.py), [publicación de materiales URP](publish_accessories.py), [validación geométrica](validate_geometry.py), [resultado de geometría](geometry-validation.json) e [inventario técnico](manifest.json). El generador aplica modificadores, cierra costuras y calcula normales y UV antes de exportar. Los renders muestran la geometría real; no son imágenes de IA.

## Validación final

Unity 6000.6.0f1: Build All completado, 407/407 pruebas EditMode y 114/114 PlayMode aprobadas. Sin errores ni avisos asociados a `Assets/` en los tres logs. [Resumen de ejecución](unity-validation.json).

Los diez modelos pasan la [validación de exportaciones](export-validation.json): índices OBJ, UV, normales, materiales declarados, remapeos URP y estructura GLB. La comprobación Blender no detectó bordes abiertos, aristas no manifold ni caras degeneradas. Total: 184.944 triángulos repartidos entre los diez accesorios. Esto no sustituye la comprobación visual del encaje sobre cada personaje, que sigue pendiente.
