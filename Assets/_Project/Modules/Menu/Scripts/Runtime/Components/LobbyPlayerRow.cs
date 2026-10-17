using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>Una plaza de la sala de espera: el jugador (retrato, nombre, personaje y si es el anfitrión) o «Esperando jugador…».</summary>
    public sealed class LobbyPlayerRow : MonoBehaviour
    {
        [SerializeField] private GameObject _filled;
        [SerializeField] private GameObject _empty;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RawImage _portrait;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _character;
        [SerializeField] private GameObject _hostBadge;

        public bool IsFilled => _filled.activeSelf;
        public string NameText => _name.text;
        public string CharacterText => _character.text;
        public bool IsHost => _hostBadge.activeSelf;

        public void Show(string playerName, string characterName, Texture portrait, bool host)
        {
            _filled.SetActive(true);
            _empty.SetActive(false);
            _group.alpha = 1f;
            _name.text = playerName;
            _character.text = characterName;
            _portrait.texture = portrait;
            _portrait.enabled = portrait != null;
            _hostBadge.SetActive(host);
        }

        public void ShowEmpty()
        {
            _filled.SetActive(false);
            _empty.SetActive(true);
            _group.alpha = 0.6f;
        }
    }
}
