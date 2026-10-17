using Trivias.Shared.Characters;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Pone al concursante (o al expositor del menú) el modelo del personaje que lleva. Todos comparten esqueleto
    /// (huesos humanos de Unity) y animaciones: cambiar de personaje es cambiar el esqueleto y la malla que cuelgan del
    /// Animator, su avatar Humanoid, y volver a crear en su cabeza el anclaje de los complementos. Solo ocurre al elegir
    /// personaje (en la sala de espera), nunca durante una ronda.
    /// </summary>
    public sealed class PlayerCharacterView : MonoBehaviour
    {
        public const string HEAD_BONE = "Head";
        public const string COSMETICS_ANCHOR = "CosmeticsAnchor";

        [SerializeField] private CharacterCatalog _catalog;
        [Tooltip("Animator del modelo: de él cuelgan el esqueleto y la malla del personaje.")]
        [SerializeField] private Animator _animator;
        [Tooltip("Personaje con el que se construyó el prefab (ya puesto: no hace falta cambiarlo).")]
        [SerializeField] private string _builtWith;
        [Tooltip("Anclaje de los complementos respecto a los pies del concursante: a la altura de los ojos, en el centro de la cabeza.")]
        [SerializeField] private Vector3 _anchorPosition = new(0f, 1.29f, 0f);
        [Tooltip("Escala de los complementos (se diseñaron para una cabeza de unos 0,35 m).")]
        [SerializeField, Min(0.01f)] private float _cosmeticsScale = 3f;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _headAnchor;

        private string _modelId;

        public string ShownId { get; private set; } = string.Empty;

        /// <summary>Hueso de la cabeza del modelo que lleva.</summary>
        public Transform Head => _head;

        /// <summary>Donde se enganchan gorras, gafas y pajaritas (sigue a la cabeza y tiene su escala).</summary>
        public Transform HeadAnchor => _headAnchor;

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
            _headAnchor = CreateAnchor(_head, transform, _anchorPosition, _cosmeticsScale);
        }

        /// <summary>
        /// Anclaje de los complementos en la cabeza: sigue al hueso, mira como el concursante y tiene la escala de esta
        /// cabeza, así gorras, gafas y pajaritas encajan sin tocarlas (lo usan también el constructor del prefab).
        /// </summary>
        public static Transform CreateAnchor(Transform head, Transform root, Vector3 position, float scale)
        {
            if (head == null)
                return null;
            var anchor = new GameObject(COSMETICS_ANCHOR).transform;
            anchor.SetParent(head, false);
            anchor.SetPositionAndRotation(root.TransformPoint(position), root.rotation);
            anchor.localScale = Vector3.one * (scale / Mathf.Max(0.0001f, head.lossyScale.x));
            return anchor;
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
