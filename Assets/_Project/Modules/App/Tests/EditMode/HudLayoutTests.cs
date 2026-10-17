using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Distribución del HUD a 1920 × 1080 (bocetos de docs/bocetos/ui/hud): ninguna pieza se pisa con otra que pueda
    /// verse a la vez, todas dentro de los márgenes y el centro libre para jugar.
    /// </summary>
    public sealed class HudLayoutTests
    {
        private const string PREFAB = "Assets/_Project/Modules/HUD/Prefabs/MatchHud.prefab";
        private const float MARGIN = 24f;

        private static readonly string[] Pieces =
        {
            "TopLeft/QuestionCard", "TopLeft/AtRisk", "Top/ClockCard", "Top/LockedCard", "Top/RevealCard", "Top/Eliminated",
            "TopRight/MoneyCard", "Bottom/SelfBetCard", "BottomRight/CarryCard", "BottomLeft/EscHint",
        };

        /// <summary>Piezas que nunca salen a la vez (se turnan en el mismo sitio).</summary>
        private static readonly (string, string)[] Exclusive =
        {
            ("Top/ClockCard", "Top/LockedCard"), ("Top/ClockCard", "Top/RevealCard"), ("Top/LockedCard", "Top/RevealCard"),
            ("Top/ClockCard", "Top/Eliminated"), ("Top/LockedCard", "Top/Eliminated"),
            // El aviso de dinero fuera de bandejas solo sale mientras se apuesta.
            ("TopLeft/AtRisk", "Top/LockedCard"), ("TopLeft/AtRisk", "Top/RevealCard"), ("TopLeft/AtRisk", "Top/Eliminated"),
        };

        /// <summary>Rectángulo en la pantalla de referencia (y hacia abajo desde arriba).</summary>
        private static Rect OnScreen(Transform hud, string path)
        {
            var piece = (RectTransform)hud.Find(path);
            Assert.That(piece, Is.Not.Null, path);
            var size = piece.sizeDelta;
            var left = piece.anchoredPosition.x - piece.pivot.x * size.x;
            var top = -piece.anchoredPosition.y - (1f - piece.pivot.y) * size.y;
            return new Rect(left, top, size.x, size.y);
        }

        [Test]
        public void NoPiece_OverlapsAnotherThatCanShowAtTheSameTime()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB).transform;
            var overlaps = new List<string>();
            for (var i = 0; i < Pieces.Length; i++)
            {
                for (var j = i + 1; j < Pieces.Length; j++)
                {
                    if (System.Array.IndexOf(Exclusive, (Pieces[i], Pieces[j])) >= 0 || System.Array.IndexOf(Exclusive, (Pieces[j], Pieces[i])) >= 0)
                        continue;
                    if (OnScreen(hud, Pieces[i]).Overlaps(OnScreen(hud, Pieces[j])))
                        overlaps.Add($"{Pieces[i]} pisa {Pieces[j]}");
                }
            }
            Assert.That(overlaps, Is.Empty, string.Join("\n", overlaps));
        }

        [Test]
        public void ThePresentersCaption_DoesNotOverlapTheHud_NorTheMiddle()
        {
            var caption = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Modules/Presenter/Prefabs/PresenterCaption.prefab").transform;
            var card = OnScreen(caption, "BottomLeft/Card");
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB).transform;
            foreach (var path in Pieces)
                Assert.That(card.Overlaps(OnScreen(hud, path)), Is.False, $"El rótulo del presentador pisa {path}.");
            Assert.That(card.yMax, Is.LessThan(1080f - 140f - 64f), "Por encima de los avisos de interacción.");
            Assert.That(card.xMin, Is.GreaterThanOrEqualTo(MARGIN));
            Assert.That(card.Overlaps(new Rect(1920f * 0.2f, 1080f * 0.22f, 1920f * 0.6f, 1080f * 0.4f)), Is.False, "El centro, libre.");
        }

        [Test]
        public void TheSelfBetsLine_IsCentredUnderTheTeamMoney()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB).transform;
            var card = OnScreen(hud, "TopRight/MoneyCard");
            var row = (RectTransform)hud.Find("TopRight/MoneyCard/SelfBets");
            Assert.That(row.sizeDelta.x, Is.EqualTo(card.width), "La fila ocupa todo el ancho de la tarjeta…");
            Assert.That(row.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>().childAlignment, Is.EqualTo(TextAnchor.MiddleCenter),
                "…y centra el icono y el texto juntos.");
        }

        [Test]
        public void EveryPiece_StaysInsideTheSafeMargins_AndTheMiddleIsFreeToPlay()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB).transform;
            var middle = new Rect(1920f * 0.2f, 1080f * 0.22f, 1920f * 0.6f, 1080f * 0.4f);
            foreach (var path in Pieces)
            {
                var rect = OnScreen(hud, path);
                Assert.That(rect.xMin >= MARGIN && rect.xMax <= 1920f - MARGIN && rect.yMin >= MARGIN && rect.yMax <= 1080f - MARGIN,
                    Is.True, $"{path} se sale de los márgenes: {rect}.");
                Assert.That(rect.Overlaps(middle), Is.False, $"{path} tapa el centro de la pantalla.");
            }
        }
    }
}
