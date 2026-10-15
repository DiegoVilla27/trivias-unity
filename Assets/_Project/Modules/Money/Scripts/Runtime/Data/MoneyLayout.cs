using UnityEngine;

namespace Trivias.Money.Data
{
    /// <summary>
    /// Dónde se colocan visualmente los objetos: huecos sobre la mesa y sobre cada bandeja.
    /// Las posiciones de mesa y bandejas las escribe el constructor de la escena en tiempo de edición;
    /// en runtime solo se calculan posiciones a partir del número de hueco, sin buscar objetos en la escena.
    /// </summary>
    [CreateAssetMenu(menuName = "Trivias/Money/Money Layout", fileName = "MoneyLayout")]
    public sealed class MoneyLayout : ScriptableObject
    {
        [Header("Mesa")]
        [SerializeField] private Vector3 _tableSurface;
        [SerializeField, Min(1)] private int _tableColumns = 5;
        [SerializeField, Min(1)] private int _tableRows = 4;
        [SerializeField] private Vector2 _tableSpacing = new(0.36f, 0.24f);

        [Header("Estación de cambio")]
        [SerializeField] private Vector3 _exchangeStation;

        [Header("Bandejas")]
        [SerializeField] private Vector3[] _traySurfaces = new Vector3[0];
        [SerializeField] private float[] _trayYaw = new float[0];
        [SerializeField, Min(1)] private int _trayColumns = 3;
        [SerializeField, Min(1)] private int _trayRows = 2;
        [SerializeField] private Vector2 _traySpacing = new(0.34f, 0.24f);

        [Header("Apilado")]
        [SerializeField, Min(0.01f)] private float _layerHeight = 0.09f;

        [Header("En las manos (por denominación: fajo, bolsa, lingote)")]
        [Tooltip("Posición del primer objeto respecto al punto de carga.")]
        [SerializeField] private Vector3[] _carryOrigin = { Vector3.zero, new(-0.16f, 0f, 0f), Vector3.zero };
        [Tooltip("Desplazamiento entre objetos: hacia arriba para fajos y lingotes, de lado para las bolsas.")]
        [SerializeField] private Vector3[] _carryStep = { new(0f, 0.075f, 0f), new(0.32f, 0f, 0f), new(0f, 0.085f, 0f) };

        public int TrayCount => _traySurfaces.Length;
        public Vector3 TableSurface => _tableSurface;
        public Vector3 ExchangeStation => _exchangeStation;
        public Vector3 TraySurface(int tray) => _traySurfaces[tray];

        public Vector3 TableSlot(int slot) =>
            GridSlot(_tableSurface, Quaternion.identity, slot, _tableColumns, _tableRows, _tableSpacing);

        public Vector3 TraySlot(int tray, int slot) =>
            GridSlot(_traySurfaces[tray], TrayRotation(tray), slot, _trayColumns, _trayRows, _traySpacing);

        public Quaternion TrayRotation(int tray) => Quaternion.Euler(0f, _trayYaw[tray], 0f);

        /// <summary>Posición local en el punto de carga del objeto número <paramref name="stackIndex"/> de la pila.</summary>
        public Vector3 CarryOffset(int denomination, int stackIndex) =>
            _carryOrigin[denomination] + _carryStep[denomination] * stackIndex;

        private Vector3 GridSlot(Vector3 origin, Quaternion rotation, int slot, int columns, int rows, Vector2 spacing)
        {
            var perLayer = columns * rows;
            var layer = slot / perLayer;
            var inLayer = slot % perLayer;
            var column = inLayer % columns;
            var row = inLayer / columns;

            var local = new Vector3(
                (column - (columns - 1) * 0.5f) * spacing.x,
                layer * _layerHeight,
                (row - (rows - 1) * 0.5f) * spacing.y);
            return origin + rotation * local;
        }

#if UNITY_EDITOR
        public void EditorSet(Vector3 tableSurface, Vector3 exchangeStation, Vector3[] traySurfaces, float[] trayYaw)
        {
            _tableSurface = tableSurface;
            _exchangeStation = exchangeStation;
            _traySurfaces = traySurfaces;
            _trayYaw = trayYaw;
        }
#endif
    }
}
