# Scripts de Blender

Blender 4.3, en modo consola (`blender -b -P <script> -- <argumentos>`). No se ejecutan desde Unity: generan archivos que luego se copian al módulo.

| Script | Qué hace | Salida |
| --- | --- | --- |
| `rig_character.py` | Riggea un modelo completo del usuario tal cual (sin tocar malla ni textura): esqueleto con los nombres humanos de Unity sacado de su geometría, pesos en una copia ligera traspasados al modelo, pose T y FBX. | `<Nombre>_Rigged.fbx` (concursantes en `Player/Models/Characters`, presentador en `Presenter/Models/Presenter`) |
| `presenter_anims.py` | Construye el rig del presentador igual que el anterior y hornea sus animaciones (Hablar en bucle, Alegría y Decepción) con IK de brazos y piernas y curvas suaves. Con `render` saca láminas de comprobación. | `Presenter_Talk.fbx`, `Presenter_Happy.fbx`, `Presenter_Sad.fbx` (en `Presenter/Animations`) |
| `accessories.py` | Prepara los accesorios del usuario y el micrófono tal cual: solo su tamaño real y el origen en su punto de enganche (frente −Y, arriba +Z). Con nombres de carpeta al final, solo esas. | `Shop/Models/Accessories/<Nombre>/<Nombre>.fbx` y `Presenter/Models/Microphone/Microphone.fbx`, con su `shaded.png` |
| `portrait.py` | Retrato de frente del presentador con fondo transparente (luego se recorta en círculo). | `Presenter/UI/Portrait/Portrait_Presenter.png` |

Ejemplos:

```sh
blender -b -P rig_character.py -- ~/Downloads/PresentatorModel/base_basic_shaded.fbx Presenter_Rigged.fbx Presenter render/presenter
blender -b -P presenter_anims.py -- rig_character.py ~/Downloads/PresentatorModel/base_basic_shaded.fbx salida render
```

En Unity, las animaciones se importan como Humanoid con la pose T del modelo como referencia y conservando la jerarquía (`HumanoidModels.EnsureAnimationImported`); sin eso los brazos salen torcidos.
