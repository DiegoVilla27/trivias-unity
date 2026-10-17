using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trivias.Menu.Components
{
    /// <summary>Una fila de la tabla de récords: puesto (medalla en los tres primeros), premio, tiempo, fecha, participantes y supervivientes.</summary>
    public sealed class RecordRowView : MonoBehaviour
    {
        [SerializeField] private Image _medal;
        [SerializeField] private TextMeshProUGUI _position;
        [SerializeField] private TextMeshProUGUI _money;
        [SerializeField] private TextMeshProUGUI _time;
        [SerializeField] private TextMeshProUGUI _date;
        [SerializeField] private TextMeshProUGUI _players;
        [SerializeField] private TextMeshProUGUI _survivors;

        public void Show(int position, Sprite medal, string money, string time, string date, string players, string survivors)
        {
            gameObject.SetActive(true);
            _medal.sprite = medal;
            _medal.type = Image.Type.Simple;
            _medal.preserveAspect = true;
            _medal.enabled = medal != null;
            _position.text = medal != null ? string.Empty : position.ToString();
            _money.text = money;
            _time.text = time;
            _date.text = date;
            _players.text = players;
            _survivors.text = survivors;
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>La fila en una línea (para las pruebas y la accesibilidad).</summary>
        public string Describe() =>
            $"{(_medal.enabled ? "" : _position.text)} {_money.text} {_time.text} {_date.text} {_players.text} {_survivors.text}".Trim();
    }
}
