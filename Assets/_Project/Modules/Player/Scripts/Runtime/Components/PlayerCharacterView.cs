using Trivias.Shared.Characters;
using Trivias.Shared.Profile;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Pone al concursante (o al expositor del menú) el modelo del personaje que lleva. Todos comparten esqueleto
    /// (huesos humanos de Unity) y animaciones: cambiar de personaje es cambiar el esqueleto y la malla que cuelgan del
    /// Animator, su avatar Humanoid, y volver a crear los anclajes de los cosméticos en sus puntos medidos (encima de su
    /// pelo, delante de sus ojos…). Solo ocurre al elegir personaje (en la sala de espera), nunca durante una ronda.
    /// </summary>
    public sealed class PlayerCharacterView : MonoBehaviour
    {
        public const string HEAD_BONE = "Head";
        public const string CHEST_BONE = "Chest";
        public const string ANCHOR_PREFIX = "CosmeticsAnchor_";

        [SerializeField] private CharacterCatalog _catalog;
        [Tooltip("Animator del modelo: de él cuelgan el esqueleto y la malla del personaje.")]
        [SerializeField] private Animator _animator;
        [Tooltip("Personaje con el que se construyó el prefab (ya puesto: no hace falta cambiarlo).")]
        [SerializeField] private string _builtWith;
        [SerializeField] private Transform _head;
        [Tooltip("Un anclaje por hueco de cosmético (orden de CosmeticSlot).")]
        [SerializeField] private Transform[] _anchors = new Transform[CosmeticSlots.COUNT];

        private string _modelId;

        public string ShownId { get; private set; } = string.Empty;

        /// <summary>Hueso de la cabeza del modelo que lleva.</summary>
        public Transform Head => _head;

        /// <summary>Encima del pelo: coronas, sombreros, aureolas.</summary>
        public Transform HeadAnchor => AnchorFor(CosmeticSlot.Head);

        /// <summary>Dónde se engancha un cosmético de ese hueco (sigue a su hueso; en metros, mirando como el concursante).</summary>
        public Transform AnchorFor(CosmeticSlot slot) => _anchors != null && (int)slot < _anchors.Length ? _anchors[(int)slot] : null;

        public Animator Animator => _animator;

        public void Show(string characterId)
        {
            if (characterId == ShownId)
                return;
            ShownId = characterId;
            var character = _catalog.Find(characterId);
            if (character == null || character.Model == null)
                return;
            if (string.IsNullOrEmpty(_modelId))
                _modelId = _builtWith;
            if (character.Id == _modelId)
                return;
            Swap(character);
        }

        private void Swap(CharacterDefinition character)
        {
            var model = _animator.transform;
            // Fuera el esqueleto y la malla anteriores (los componentes del Animator se quedan). Se descuelgan ya: si
            // siguieran bajo el Animator hasta el final del fotograma, el avatar y la cabeza se engancharían a ellos.
            for (var i = model.childCount - 1; i >= 0; i--)
            {
                var child = model.GetChild(i).gameObject;
                child.SetActive(false);
                child.transform.SetParent(null, false);
                Destroy(child);
            }

            // Dentro los del personaje, colgando directamente del Animator (las rutas del avatar empiezan ahí).
            var instance = Instantiate(character.Model, model.position, model.rotation);
            var layer = model.gameObject.layer;
            while (instance.transform.childCount > 0)
            {
                var part = instance.transform.GetChild(0);
                part.SetParent(model, false);
                SetLayer(part, layer);
            }
            Destroy(instance);
            foreach (var skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                skinned.updateWhenOffscreen = false;

            _animator.avatar = character.Avatar;
            _animator.Rebind();
            _modelId = character.Id;
            _head = FindDeep(model, HEAD_BONE);
            CreateAnchors(_anchors, transform, model, character);
        }

        /// <summary>
        /// Anclajes de los cosméticos en los puntos medidos del personaje (en pose T, respecto a sus pies): los de la cabeza
        /// siguen al hueso de la cabeza y los del cuello y la espalda al del pecho. Miran como el concursante y van en
        /// metros, así cada cosmético se coloca con su origen en su punto de enganche (lo usa también el constructor).
        /// </summary>
        public static void CreateAnchors(Transform[] anchors, Transform root, Transform model, CharacterDefinition character)
        {
            var head = FindDeep(model, HEAD_BONE);
            var chest = FindDeep(model, CHEST_BONE);
            for (var slot = 0; slot < anchors.Length; slot++)
            {
                var bone = (CosmeticSlot)slot is CosmeticSlot.Neck or CosmeticSlot.Back ? chest : head;
                if (bone == null)
                    continue;
                var anchor = new GameObject(ANCHOR_PREFIX + (CosmeticSlot)slot).transform;
                anchor.SetParent(bone, false);
                anchor.SetPositionAndRotation(root.TransformPoint(character.Anchor((CosmeticSlot)slot)), root.rotation);
                anchor.localScale = Vector3.one / Mathf.Max(0.0001f, bone.lossyScale.x);
                anchors[slot] = anchor;
            }
        }

        public static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var found = FindDeep(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void SetLayer(Transform target, int layer)
        {
            target.gameObject.layer = layer;
            foreach (Transform child in target)
                SetLayer(child, layer);
        }
    }
}
