using TMPro;
using Trivias.Match.Components;
using Trivias.Match.Data;
using Trivias.Match.Network;
using Trivias.Money.Data;
using Trivias.Questions.Data;
using Trivias.SelfBet.Data;
using Trivias.Shared.Editor;
using Trivias.Shared.Events;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace Trivias.Match.Editor
{
    /// <summary>Construye la configuración de la partida, el prefab de red de la sesión y conecta cabinas y pantalla.</summary>
    public static class MatchBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Match";
        private const string CONFIG = MODULE_ROOT + "/Config";
        public const string SESSION_PREFAB_PATH = MODULE_ROOT + "/Prefabs/MatchSession.prefab";

        public static MatchConfig Config() => EditorAssets.GetOrCreateAsset<MatchConfig>($"{CONFIG}/MatchConfig.asset");
        public static MatchRuntime Runtime() => EditorAssets.GetOrCreateAsset<MatchRuntime>($"{CONFIG}/MatchRuntime.asset");

        public static GameObject Build(QuestionBank bank, MoneyRuntime money, SelfBetRuntime selfBet,
            VoidEventChannel matchStartRequested)
        {
            EditorUtility.SetDirty(Config());

            var root = new GameObject("MatchSession");
            root.AddComponent<NetworkObject>();
            var session = root.AddComponent<MatchSessionNetwork>();
            SerializedWiring.Set(session, "_config", Config());
            SerializedWiring.Set(session, "_bank", bank);
            SerializedWiring.Set(session, "_runtime", Runtime());
            SerializedWiring.Set(session, "_money", money);
            SerializedWiring.Set(session, "_matchStartRequested", matchStartRequested);
            SerializedWiring.Set(session, "_playerControl", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(session, "_selfBet", selfBet);
            SerializedWiring.Set(session, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(session, "_verifyFromZoneRequested", SharedAssets.VerifyFromZoneRequested());
            SerializedWiring.Set(session, "_returnedToLobby", SharedAssets.ReturnedToLobby());

            return EditorAssets.SaveAsPrefab(root, SESSION_PREFAB_PATH);
        }

        public static void AttachBoothLabel(GameObject booth, int boothIndex, TextMeshPro label)
        {
            var view = booth.AddComponent<BoothAnswerLabel>();
            SerializedWiring.Set(view, "_runtime", Runtime());
            SerializedWiring.Set(view, "_label", label);
            SerializedWiring.Set(view, "_booth", boothIndex);
        }

        public static void AttachQuestionScreen(GameObject screen, GameObject content, TextMeshPro message,
            TextMeshPro header, TextMeshPro question, TextMeshPro[] optionTexts, GameObject[] optionBoxes, TextMeshPro status)
        {
            var view = screen.AddComponent<QuestionScreenView>();
            SerializedWiring.Set(view, "_runtime", Runtime());
            SerializedWiring.Set(view, "_content", content);
            SerializedWiring.Set(view, "_message", message);
            SerializedWiring.Set(view, "_header", header);
            SerializedWiring.Set(view, "_question", question);
            SerializedWiring.SetArray(view, "_optionTexts", optionTexts);
            SerializedWiring.SetArray(view, "_optionBoxes", optionBoxes);
            SerializedWiring.Set(view, "_status", status);
        }
    }
}
