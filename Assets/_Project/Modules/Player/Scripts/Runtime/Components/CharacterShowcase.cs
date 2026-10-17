using System.Collections.Generic;
using Trivias.Shared.Characters;
using Trivias.Shared.Profile;
using UnityEngine;

namespace Trivias.Player.Components
{
    /// <summary>
    /// Expositor del personaje para los menús: el modelo en reposo sobre su peana, balanceándose despacio, con su cámara
    /// y sus luces, lejos del plató. Pinta en una imagen que crea al cargar la escena (y libera al descargarla) y que el
    /// menú muestra. Solo funciona mientras un menú pide un personaje; si no, su cámara se apaga (no dibuja nada).
    /// Al cargar hace además los retratos de todos los personajes (cara y cuerpo entero) para listas y tarjetas.
    /// </summary>
    public sealed class CharacterShowcase : MonoBehaviour
    {
        private static readonly int GroundedId = Animator.StringToHash("Grounded");
        private static readonly int MotionSpeedId = Animator.StringToHash("MotionSpeed");

        [SerializeField] private CharacterPreview _preview;
        [SerializeField] private CharacterCatalog _catalog;
        [SerializeField] private PlayerCharacterView _view;
        [SerializeField] private Animator _animator;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _turntable;
        [SerializeField, Min(16)] private int _width = 640;
        [SerializeField, Min(16)] private int _height = 800;
        [Tooltip("Cuánto se gira a cada lado (grados) y cuánto tarda una ida y vuelta (segundos).")]
        [SerializeField, Range(0f, 90f)] private float _swayDegrees = 22f;
        [SerializeField, Min(1f)] private float _swaySeconds = 9f;
        [SerializeField, Min(16)] private int _faceSize = 256;
        [SerializeField] private Vector2Int _bodySize = new(256, 320);

        private RenderTexture _texture;
        private readonly Dictionary<string, Texture> _faces = new();
        private readonly Dictionary<string, Texture> _bodies = new();
        private readonly List<RenderTexture> _portraits = new();
        private readonly List<string> _pendingIds = new();
        private GameObject _worn;
        private float _swayTime;

        /// <summary>Giro de la peana para enseñar la espalda (de tres cuartos, para ver también la cara).</summary>
        public const float BACK_YAW = 155f;
        private float _baseYaw;

        /// <summary>Hacia dónde mira la peana sin contar su vaivén (0: de frente).</summary>
        public float BaseYaw => _baseYaw;

        public bool IsShowing => _camera.enabled;
        public string ShownId => _view.ShownId;
        public int PortraitCount => _faces.Count;

        private void Awake()
        {
            // La imagen se crea al cargar (no es un asset): pintar en un asset y descargar la escena cuelga Unity 6.
            _texture = new RenderTexture(_width, _height, 24, RenderTextureFormat.ARGB32) { name = "CharacterPreview", antiAliasing = 4 };
            _camera.targetTexture = _texture;
            _camera.enabled = false;
        }

        private void OnEnable()
        {
            _preview.Requested += Show;
            _preview.AccessoryChanged += Dress;
            _preview.AccessoryPortraitRequested += OnAccessoryPortraitRequested;
            _preview.SetTexture(_texture);
            Show(_preview.RequestedId);
            Dress();
        }

        private void Start()
        {
            TakePortraits();
            TakePendingAccessoryPortraits();
        }

        private void OnDisable()
        {
            _preview.Requested -= Show;
            _preview.AccessoryChanged -= Dress;
            _preview.AccessoryPortraitRequested -= OnAccessoryPortraitRequested;
            _camera.enabled = false;
            if (_preview.Texture == _texture)
                _preview.SetTexture(null);
        }

        private void OnDestroy()
        {
            _camera.targetTexture = null;
            _texture.Release();
            Destroy(_texture);
            foreach (var portrait in _portraits)
            {
                portrait.Release();
                Destroy(portrait);
            }
        }

        public void Show(string characterId)
        {
            var show = !string.IsNullOrEmpty(characterId);
            if (show && !_camera.enabled)
                StandStill();
            _camera.enabled = show;
            if (!show)
                return;
            var head = _view.HeadAnchor;
            _view.Show(characterId);
            if (_view.HeadAnchor != head)
            {
                StandStill(); // Otro modelo: el Animator vuelve a empezar.
                Dress(); // Otra cabeza: el complemento puesto se coloca en ella.
            }
        }

        private void Update()
        {
            if (!_camera.enabled)
                return;
            _swayTime += Time.unscaledDeltaTime;
            var angle = Mathf.Sin(_swayTime * Mathf.PI * 2f / _swaySeconds) * _swayDegrees;
            _turntable.localRotation = Quaternion.Euler(0f, _baseYaw + angle, 0f);
        }

        /// <summary>En el suelo y quieto: la animación de reposo.</summary>
        private void StandStill()
        {
            _animator.SetBool(GroundedId, true);
            _animator.SetFloat(MotionSpeedId, 1f);
        }

        /// <summary>
        /// Una foto de cada personaje (cara y cuerpo entero), de frente, con la misma luz que la vista grande. Se hace una
        /// sola vez al cargar; la cámara vuelve después a su sitio.
        /// </summary>
        private void TakePortraits()
        {
            var cameraTransform = _camera.transform;
            var position = cameraTransform.localPosition;
            var rotation = cameraTransform.localRotation;
            var fieldOfView = _camera.fieldOfView;
            var shown = _view.ShownId;

            _turntable.localRotation = Quaternion.identity;
            StandStill();
            _animator.Update(0.5f); // Postura de reposo, no la de atar el esqueleto.

            foreach (var character in _catalog.Characters)
            {
                _view.Show(character.Id);
                StandStill();
                _animator.Update(0.5f); // Tras cambiar de modelo el Animator vuelve a empezar: postura de reposo.
                // Cara: la cabeza y un poco de hombros.
                FrameFace(2.5f);
                _faces[character.Id] = Snap(_faceSize, _faceSize);

                // Cuerpo entero, como en la vista grande.
                cameraTransform.localPosition = position;
                cameraTransform.localRotation = rotation;
                _camera.fieldOfView = fieldOfView;
                _bodies[character.Id] = Snap(_bodySize.x, _bodySize.y);
            }

            cameraTransform.localPosition = position;
            cameraTransform.localRotation = rotation;
            _camera.fieldOfView = fieldOfView;
            _camera.targetTexture = _texture;
            _view.Show(shown);
            _preview.SetPortraits(_faces, _bodies);
        }

        /// <summary>Pone en la cabeza el complemento pedido (o lo quita), como lo coloca la tienda en el concursante.</summary>
        private void Dress()
        {
            if (_worn != null)
                Destroy(_worn);
            _worn = Attach(_preview.Worn.Prefab, _preview.Worn.Offset, _preview.Worn.Slot);
            // Lo que va a la espalda (alas) se enseña de tres cuartos por detrás.
            _baseYaw = _worn != null && _preview.Worn.Slot == CosmeticSlot.Back ? BACK_YAW : 0f;
        }

        /// <summary>Como la tienda lo pone en el concursante: en el anclaje de su hueco, con su desplazamiento.</summary>
        private GameObject Attach(GameObject prefab, Vector3 offset, CosmeticSlot slot)
        {
            var anchor = _view.AnchorFor(slot);
            if (prefab == null || anchor == null)
                return null;
            var instance = Instantiate(prefab, anchor, false);
            instance.transform.SetLocalPositionAndRotation(offset, Quaternion.identity);
            return instance;
        }

        private void OnAccessoryPortraitRequested(string _)
        {
            // Los retratos de los personajes van primero (al empezar); después, los de los complementos.
            if (_faces.Count > 0)
                TakePendingAccessoryPortraits();
        }

        /// <summary>Foto de la cabeza con cada complemento pedido, con el personaje por defecto.</summary>
        private void TakePendingAccessoryPortraits()
        {
            if (_preview.PendingAccessoryPortraits.Count == 0)
                return;
            _pendingIds.Clear();
            foreach (var id in _preview.PendingAccessoryPortraits.Keys)
                _pendingIds.Add(id);

            var cameraTransform = _camera.transform;
            var position = cameraTransform.localPosition;
            var rotation = cameraTransform.localRotation;
            var fieldOfView = _camera.fieldOfView;
            var turn = _turntable.localRotation;
            var shown = _view.ShownId;
            var wornActive = _worn != null && _worn.activeSelf;
            if (_worn != null)
                _worn.SetActive(false);

            _turntable.localRotation = Quaternion.identity;
            _view.Show(_catalog.DefaultId);
            StandStill();
            _animator.Update(0.5f); // Postura de reposo (también si acaba de cambiar de modelo).
            foreach (var id in _pendingIds)
            {
                var accessory = _preview.PendingAccessoryPortraits[id];
                var instance = Attach(accessory.Prefab, accessory.Offset, accessory.Slot);
                // Lo de la espalda se fotografía por detrás; lo demás, la cabeza de frente.
                _turntable.localRotation = accessory.Slot == CosmeticSlot.Back ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
                if (accessory.Slot == CosmeticSlot.Back)
                    FrameBack(4.2f);
                else if (accessory.Slot == CosmeticSlot.Head)
                    FrameFace(3.7f, 0.17f); // Lo que va encima del pelo (corona, sombrero, aureola), entero.
                else
                    FrameFace(3f);
                var portrait = Snap(_faceSize, _faceSize);
                if (instance != null)
                    DestroyImmediate(instance);
                _preview.SetAccessoryPortrait(id, portrait);
            }

            cameraTransform.localPosition = position;
            cameraTransform.localRotation = rotation;
            _camera.fieldOfView = fieldOfView;
            _camera.targetTexture = _texture;
            _turntable.localRotation = turn;
            _view.Show(shown);
            if (_worn != null)
                _worn.SetActive(wornActive);
        }

        /// <summary>Encuadre de la cara (la cabeza entera, cabezona); más lejos si hay complemento encima.</summary>
        private void FrameFace(float distance, float raise = 0f)
        {
            // El hueso de la cabeza está en su base: el centro de la cara queda algo más arriba.
            var face = _view.Head.position + Vector3.up * (0.38f + raise);
            var cameraTransform = _camera.transform;
            cameraTransform.position = face + _turntable.forward * distance + Vector3.up * 0.05f;
            cameraTransform.LookAt(face);
            _camera.fieldOfView = 30f;
        }

        /// <summary>Encuadre de la espalda (con lo que lleva puesto), desde detrás.</summary>
        private void FrameBack(float distance)
        {
            var back = _view.transform.position + Vector3.up * 1.05f;
            var cameraTransform = _camera.transform;
            cameraTransform.position = back + _turntable.forward * distance + Vector3.up * 0.25f;
            cameraTransform.LookAt(back);
            _camera.fieldOfView = 30f;
        }

        private RenderTexture Snap(int width, int height)
        {
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { name = "CharacterPortrait", antiAliasing = 4 };
            _portraits.Add(target);
            _camera.targetTexture = target;
            _camera.Render();
            return target;
        }
    }
}
