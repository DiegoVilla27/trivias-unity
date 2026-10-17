using Trivias.Shared.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Shared.Presentation
{
    /// <summary>Muestra en una imagen de la interfaz la vista 3D del personaje (la crea el expositor al cargar la escena).</summary>
    public sealed class CharacterPreviewImage : MonoBehaviour
    {
        [SerializeField] private CharacterPreview _preview;
        [SerializeField] private RawImage _image;

        private void OnEnable()
        {
            _preview.TextureChanged += Show;
            Show();
        }

        private void OnDisable() => _preview.TextureChanged -= Show;

        private void Show()
        {
            _image.texture = _preview.Texture;
            _image.enabled = _preview.Texture != null;
        }
    }
}
