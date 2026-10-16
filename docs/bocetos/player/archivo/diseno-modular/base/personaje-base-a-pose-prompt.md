# Player base — A-pose: prompt y corrección

16 de octubre de 2026. Herramienta: image_gen integrada, edición con referencias.

- Identidad y proporciones: [base T-pose](personaje-base-t-pose.png).
- Postura de los brazos: [A-pose vestida original](../../../characters/01-blue-tracksuit/blue-tracksuit-a-pose.png), usada solo como guía de pose.
- Resultado: [personaje-base-a-pose.png](personaje-base-a-pose.png).

## Prompt inicial

```text
Use case: identity-preserve. Create the A-POSE version of input image 1, which is the exact approved PLAYER BASE T-POSE sheet. Input image 1 is the EDIT TARGET and sole authority for the character's identity, body shape, proportions, color, bald skull, face, expression, ears, torso, complete arms and legs, cute mitten hands and simple toeless feet. Input image 2 is ONLY a POSE guide: copy its relaxed neutral A-pose arm angles (arms lowered diagonally approximately 35–40 degrees away from the sides of the torso, about 50–55 degrees below horizontal), NOT its outfit, hair or shoes. Keep every feature of the base from input 1 unchanged except the arm pose and the sheet title. Same short compact chibi vinyl-toy body, broad round bald head, huge warm dark brown eyes, eyebrows, small peach button nose, tiny friendly smile and warm peach smooth material. No clothing, no underwear, no socks, no shoes, no hair, no accessories. Blank non-anatomical toy chest and pelvis, no nipples, no genitals, no intimate details, no navel or buttock cleft. Hands remain cute small smooth MITTENS with one small integrated thumb bump; NO individual fingers, NO finger grooves. Feet remain simple rounded small volumes with NO toes, no shoe soles or seams. Both arms must be complete from shoulder to hand, relaxed and mostly straight, palms oriented as in the A-pose guide. Legs retain exactly the same length, width, straight posture and spacing as input 1, feet planted at same floor level. Do not make the torso or limbs longer or more adult. Do not redesign the face or enlarge the skull to the hair volume from input 2.
Preserve the same landscape off-white sheet design, navy rounded title plaque, two rows of four views, same view order, generous margins, full feet and hands visible, fine horizontal alignment guides, matching scale. Exact title 'PLAYER — BASE A-POSE'. Top row: exact front, exact back, exact opposite 90-degree left and right profiles, all showing the SAME A-pose and coherent body. Bottom row: top view, bottom view, front three-quarter, rear three-quarter, all with arms lowered in A-pose rather than horizontal T-pose. Exact navy caption plaques: 'FRENTE', 'ESPALDA', 'PERFIL IZQUIERDO', 'PERFIL DERECHO', 'SUPERIOR', 'INFERIOR', 'TRES CUARTOS DELANTERO', 'TRES CUARTOS TRASERO'. No extra objects or repeated views. Crisp readable soft 3D concept-art toy shading matching input 1. The goal is a matching pair of T and A BASE sheets of exactly one character for later modular hair, clothing and shoes.
```

## Ajuste final de los brazos

Se bajaron los brazos para aproximarlos a la postura relajada de la referencia, manteniendo la cara y el resto del cuerpo.

```text
Edit input image 1 ONLY to lower both arms a little further in every view, matching the relaxed A-pose in input image 3. Input image 2 locks the exact base character identity and proportions. Keep the same bald blank vinyl toy base with its identical face, head, torso, legs, feet, mitten hands, warm peach material and all eight views. In front and back, arms should be about 35 degrees OUTWARD FROM VERTICAL (about 55 degrees BELOW HORIZONTAL), rather than the more spread arms in input 1. Bring both hands closer to the thighs and a little lower: the mitten tips should reach the upper thigh level, slightly below the pelvis/crotch height. Arms remain straight and relaxed, no elbow bend, no extra fingers. Apply this same neutral A-pose rotation coherently in the exact profiles, overhead, underside and both three-quarter views. Preserve body, head and limb sizes exactly; only rotate the arms at the shoulder. Keep title PLAYER — BASE A-POSE, all captions, guides, off-white background, navy plaques, layout and framing unchanged. Do NOT add the hair, clothing or shoes from input 3. No clothing or accessories; non-anatomical smooth toy chest/pelvis without intimate details. Same character, same sheet, only slightly lower arm angles.
```
