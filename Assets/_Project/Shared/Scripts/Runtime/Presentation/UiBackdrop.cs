using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Presentation
{
    /// <summary>
    /// Fondo de los menús: el plató real desenfocado y algo oscurecido.
    /// <list type="bullet">
    /// <item>Las pantallas del menú (principal, unirse, sala) lo piden mientras se ven (<see cref="Show"/>).</item>
    /// <item>Las ventanas que van encima (tienda, tutorial, récords, configuración) además <b>tapan</b> esas pantallas
    /// (<see cref="Cover"/>): mientras alguna esté abierta, el menú de detrás se esconde, como en los bocetos.</item>
    /// </list>
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/UI/Backdrop", fileName = "UiBackdrop")]
    public sealed class UiBackdrop : ScriptableObject
    {
        private readonly HashSet<UnityEngine.Object> _shown = new();
        private readonly HashSet<UnityEngine.Object> _covers = new();

        public event Action Changed;

        public bool IsShown => _shown.Count > 0 || _covers.Count > 0;

        /// <summary>Hay una ventana encima: las pantallas del menú se esconden.</summary>
        public bool IsCovered => _covers.Count > 0;

        public void Show(UnityEngine.Object owner, bool shown) => Apply(_shown, owner, shown);

        public void Cover(UnityEngine.Object owner, bool covering) => Apply(_covers, owner, covering);

        private void Apply(HashSet<UnityEngine.Object> set, UnityEngine.Object owner, bool add)
        {
            var changed = add ? set.Add(owner) : set.Remove(owner);
            if (changed)
                Changed?.Invoke();
        }

        private void OnEnable()
        {
            _shown.Clear();
            _covers.Clear();
        }
    }
}
