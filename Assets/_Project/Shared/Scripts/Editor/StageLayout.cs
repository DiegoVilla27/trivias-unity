using UnityEngine;

namespace Trivias.Shared.Editor
{
    /// <summary>
    /// Plano del plató (LEVEL.md): medidas y posiciones de todo lo que se coloca, en un solo sitio. Lo usan los
    /// constructores de Stage, Booth, Feedback y App para que nada se descuadre al cambiar una medida.
    /// <para>
    /// Distribución: pantalla al fondo y en el centro (+Z); mesa en el centro; cabinas en dos filas a los lados mirando
    /// al centro, como la cuadrícula de la pantalla (A y B al fondo, C y D delante; A y C a la izquierda). El pasillo
    /// central queda libre: nada tapa la pantalla desde la zona de inicio. Al pie de la pantalla, la estación de cambio
    /// y el botón a los lados y el presentador en medio; público en gradas detrás de la zona de inicio.
    /// Sala compacta: 25 × 29,5 × 11 m.
    /// </para>
    /// </summary>
    public static class StageLayout
    {
        // ---- Sala ----
        public const float HALF_WIDTH = 12.5f;     // Paredes en x = ±12,5.
        public const float FRONT_Z = -13f;         // Pared detrás del público.
        public const float BACK_Z = 16.5f;         // Pared detrás del telón.
        public const float CEILING_HEIGHT = 11f;
        public const float WALL_THICKNESS = 0.5f;
        public static float InnerHalfWidth => HALF_WIDTH - WALL_THICKNESS * 0.5f;
        public static float Depth => BACK_Z - FRONT_Z;
        public static Vector3 RoomCenter => new(0f, 0f, (FRONT_Z + BACK_Z) * 0.5f);

        // ---- Pantalla de preguntas ----
        public static readonly Vector3 ScreenCenter = new(0f, 6.4f, 13.6f);
        public const float SCREEN_MODEL_SCALE = 1.12f;
        /// <summary>Mitad del ancho del marco dorado (modelo de 14,8 m a escala).</summary>
        public const float SCREEN_FRAME_HALF_WIDTH = 7.4f * SCREEN_MODEL_SCALE;

        // ---- Centro del juego ----
        public static readonly Vector3 TablePosition = new(0f, 0f, 4.75f);
        // Al pie de la pantalla, a los lados del presentador (colocados así por el usuario en el editor).
        public static readonly Vector3 ExchangePosition = new(-2.7f, 0f, 9.39f);
        public static readonly Vector3 VerifyButtonPosition = new(2.7f, 0f, 9.39f);
        public const float STATIONS_YAW = 180f; // Miran a la zona de inicio.

        // ---- Cabinas: índice 0..3 = A, B, C, D ----
        public const float BOOTH_X = 8.6f;
        public const float BOOTH_BACK_ROW_Z = 7.5f;
        public const float BOOTH_FRONT_ROW_Z = 2f;

        public static Vector3 BoothPosition(int booth) =>
            new(booth % 2 == 0 ? -BOOTH_X : BOOTH_X, 0f, booth < 2 ? BOOTH_BACK_ROW_Z : BOOTH_FRONT_ROW_Z);

        /// <summary>El frente de la cabina (+Z local) mira al centro: las de la izquierda a +X, las de la derecha a -X.</summary>
        public static float BoothYaw(int booth) => booth % 2 == 0 ? 90f : -90f;

        // ---- Zona de inicio ----
        public const int SPAWN_COUNT = 6;
        public const float SPAWN_SPACING = 1.5f;
        public const float SPAWN_Z = -2.5f;

        // ---- Presentador: en el centro, entre la pantalla y las dos estaciones, mirando a la zona de inicio ----
        // A pie de suelo: así su cabeza y su viñeta (al lado de la cabeza) quedan por debajo de la pantalla vistas
        // desde la zona de inicio.
        public static readonly Vector3 PresenterSpot = new(0f, 0f, 11.3f);
        public const float PRESENTER_YAW = 180f;

        // ---- Plataforma con escalera y cámara de estudio, en la esquina derecha junto a la pantalla ----
        public static readonly Vector3 CameraDaisSpot = new(9.6f, 0f, 11.6f);

        // ---- Público ----
        public const float AUDIENCE_BARRIER_Z = -7f;

        // ---- Techo: dos líneas de vigas, fuera de la línea de visión de la pantalla ----
        public const float TRUSS_Y = 8.6f;
        public static readonly float[] TrussZ = { -1f, 6f };
        public const float TRUSS_HALF_LENGTH = 7.4f;

        /// <summary>Focos de verdad (con luz), colgados de las vigas con su abrazadera.</summary>
        public static readonly Vector3[] SpotPositions =
        {
            new(-5f, 7.1f, -1f), new(5f, 7.1f, -1f), new(-4f, 7.1f, 6f), new(4f, 7.1f, 6f),
        };
        public static readonly Vector3 SpotTarget = new(0f, 0f, 4.5f);

        // ---- Victoria: el podio sube delante de la mesa, entre ella y la zona de inicio ----
        public static readonly Vector3 PodiumPosition = new(0f, 0f, 0.6f);

        // ---- Vista general (menú, espectador sin nadie a quien seguir) ----
        public static readonly Vector3 OverviewPosition = new(0f, 6.6f, -10.4f);
        public static readonly Vector3 OverviewTarget = new(0f, 3.2f, 7f);

        /// <summary>Giro (grados) para que algo en <paramref name="from"/> mire hacia <paramref name="to"/>.</summary>
        public static float Yaw(Vector3 from, Vector3 to) => Mathf.Atan2(to.x - from.x, to.z - from.z) * Mathf.Rad2Deg;
    }
}
