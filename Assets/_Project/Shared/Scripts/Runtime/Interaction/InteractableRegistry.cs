using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Interaction
{
    /// <summary>
    /// Conjunto en runtime de objetos interactivos de la escena. Los interactivos se registran al activarse;
    /// el interactor recorre esta lista pequeña en vez de lanzar consultas de física.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Interaction/Interactable Registry", fileName = "InteractableRegistry")]
    public sealed class InteractableRegistry : ScriptableObject
    {
        private readonly List<IInteractable> _items = new();

        public IReadOnlyList<IInteractable> Items => _items;

        public void Register(IInteractable interactable)
        {
            if (!_items.Contains(interactable))
                _items.Add(interactable);
        }

        public void Unregister(IInteractable interactable) => _items.Remove(interactable);

        private void OnEnable() => _items.Clear();
    }
}
