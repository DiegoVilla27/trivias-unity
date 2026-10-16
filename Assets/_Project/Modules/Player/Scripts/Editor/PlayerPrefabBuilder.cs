using System.Linq;
using Trivias.Player.Components;
using Trivias.Player.Data;
using Trivias.Player.Network;
using Trivias.Shared.Editor;
using Trivias.Shared.Events;
using Trivias.Shared.Physics;
using Trivias.Shared.Presentation;
using Trivias.Shared.Spawning;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Trivias.Player.Editor
{
    /// <summary>
    /// Construye el prefab del concursante con su modelo propio (<see cref="PlayerNewModelBuilder"/>, Humanoid) y las
    /// animaciones y sonidos de pasos de Starter Assets (ThirdParty), con scripts propios del módulo. Los terceros no se modifican.
    /// </summary>
    public static class PlayerPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Player";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/Player.prefab";
        private const string CONFIG_PATH = MODULE_ROOT + "/Config/PlayerConfig.asset";
        private const string CONTROLS_PATH = MODULE_ROOT + "/Config/PlayerControls.inputactions";

        private const string STARTER_CHARACTER = "Assets/ThirdParty/StarterAssets/Runtime/ThirdPersonController/Character";
        private const string ANIMATOR_PATH = STARTER_CHARACTER + "/Animations/StarterAssetsThirdPerson.controller";
        private const string SFX_FOLDER = STARTER_CHARACTER + "/Sfx";

        // Medidas del concursante chibi (1,65 m, cabeza grande de 1 m de ancho).
        private const float HEIGHT = 1.65f;
        private const float CAPSULE_RADIUS = 0.32f;
        private const float CAMERA_ROOT_HEIGHT = 1.5f;
        private const string HEAD_BONE = "Head";
        private const float NAME_TAG_HEIGHT = 1.95f;
        /// <summary>El dinero se lleva delante de la barriga, por debajo de la cabeza.</summary>
        private static readonly Vector3 CarryPointPosition = new(0f, 0.62f, 0.45f);
        /// <summary>
        /// Anclaje de los cosméticos: a la altura de los ojos, en el centro de la cabeza y a su escala (los cosméticos se
        /// diseñaron para una cabeza de unos 0,35 m; esta mide casi 1 m con el pelo).
        /// </summary>
        public static readonly Vector3 CosmeticsAnchorPosition = new(0f, 1.12f, 0.09f);
        public const float COSMETICS_SCALE = 2.6f;
        public const string COSMETICS_ANCHOR = "CosmeticsAnchor";

        [MenuItem("Tools/Trivias/Build/Player")]
        public static GameObject BuildFromMenu() =>
            Build(SharedAssets.StageSpawnPoints(), SharedAssets.LocalPlayerCameraTarget());

        /// <param name="extensions">Otros módulos añaden sus componentes al concursante (p. ej. Interaction) sin que Player los conozca.</param>
        public static GameObject Build(SpawnPointSet spawnPoints, TransformEventChannel localPlayerCameraTarget,
            params System.Action<GameObject>[] extensions)
        {
            var config = EditorAssets.GetOrCreateAsset<PlayerConfig>(CONFIG_PATH);
            var root = new GameObject("Player");

            var controller = root.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, HEIGHT * 0.5f + 0.02f, 0f);
            controller.height = HEIGHT;
            controller.radius = CAPSULE_RADIUS;
            controller.stepOffset = 0.3f; // Sube a las plataformas de apuesta andando.
            controller.skinWidth = 0.02f;
            controller.slopeLimit = 45f;
            controller.minMoveDistance = 0f;

            var animator = CreateModel(root.transform);
            var cameraRoot = EditorAssets.CreateEmpty("CameraRoot", root.transform, Vector3.up * CAMERA_ROOT_HEIGHT);
            var nameTag = CreateNameTag(root.transform);

            var carryPoint = EditorAssets.CreateEmpty("CarryPoint", root.transform, CarryPointPosition);
            var carrySocket = root.AddComponent<PlayerCarrySocket>();
            SerializedWiring.Set(carrySocket, "_carryPoint", carryPoint.transform);

            var expulsion = root.AddComponent<PlayerExpulsionView>();
            SerializedWiring.Set(expulsion, "_model", animator.transform);
            SerializedWiring.Set(expulsion, "_nameTag", nameTag.gameObject);
            SerializedWiring.Set(expulsion, "_controller", controller);
            SerializedWiring.Set(expulsion, "_tubes", SharedAssets.TubeRegistry());

            var input = root.AddComponent<PlayerInputReader>();
            WireInput(input);
            input.enabled = false;

            var look = root.AddComponent<PlayerLook>();
            SerializedWiring.Set(look, "_config", config);
            SerializedWiring.Set(look, "_input", input);
            SerializedWiring.Set(look, "_cameraRoot", cameraRoot.transform);
            SerializedWiring.Set(look, "_settings", SharedAssets.GameSettings());
            look.enabled = false;

            var motor = root.AddComponent<PlayerMotor>();
            SerializedWiring.Set(motor, "_config", config);
            SerializedWiring.Set(motor, "_input", input);
            SerializedWiring.Set(motor, "_controller", controller);
            SerializedWiring.Set(motor, "_animator", animator);
            SerializedWiring.Set(motor, "_cameraRoot", cameraRoot.transform);
            SerializedWiring.Set(motor, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(motor, "_movementLimit", SharedAssets.PlayerMovementLimit());
            motor.enabled = false;

            root.AddComponent<NetworkObject>();
            var networkTransform = root.AddComponent<NetworkTransform>();
            networkTransform.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
            networkTransform.SyncRotAngleX = false;
            networkTransform.SyncRotAngleZ = false;
            networkTransform.SyncScaleX = false;
            networkTransform.SyncScaleY = false;
            networkTransform.SyncScaleZ = false;

            var networkAnimator = root.AddComponent<OwnerNetworkAnimator>();
            SerializedWiring.Set(networkAnimator, "m_Animator", animator);

            var playerNetwork = root.AddComponent<PlayerNetwork>();
            SerializedWiring.Set(playerNetwork, "_input", input);
            SerializedWiring.Set(playerNetwork, "_motor", motor);
            SerializedWiring.Set(playerNetwork, "_look", look);
            SerializedWiring.Set(playerNetwork, "_nameTag", nameTag);
            SerializedWiring.Set(playerNetwork, "_spawnPoints", spawnPoints);
            SerializedWiring.Set(playerNetwork, "_localPlayerCameraTarget", localPlayerCameraTarget);
            SerializedWiring.Set(playerNetwork, "_carrySocket", carrySocket);
            SerializedWiring.Set(playerNetwork, "_expulsion", expulsion);
            SerializedWiring.Set(playerNetwork, "_avatars", SharedAssets.PlayerAvatars());
            SerializedWiring.Set(playerNetwork, "_profile", SharedAssets.PlayerProfile());
            SerializedWiring.Set(playerNetwork, "_headAnchor", CreateCosmeticsAnchor(root.transform, animator.transform));
            // Los trajes de colores tiñen solo el chándal (la cara, el pelo y las franjas blancas no).
            SerializedWiring.SetArray(playerNetwork, "_bodyRenderers",
                new Renderer[] { animator.transform.Find(PlayerNewModelBuilder.SUIT_MESH).GetComponent<SkinnedMeshRenderer>() });

            foreach (var extend in extensions)
                extend(root);

            SetLayerRecursively(root, GameLayers.PLAYER);
            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        private static Animator CreateModel(Transform parent)
        {
            PlayerNewModelBuilder.EnsureImported();
            var model = (GameObject)PrefabUtility.InstantiatePrefab(PlayerNewModelBuilder.Model(), parent);
            model.name = "Model";
            foreach (var skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>())
                skinned.updateWhenOffscreen = false;

            if (!model.TryGetComponent<Animator>(out var animator))
                animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ANIMATOR_PATH);
            animator.avatar = PlayerNewModelBuilder.Avatar();
            animator.applyRootMotion = false;

            var audioSource = model.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 20f;

            var events = model.AddComponent<PlayerAnimationEvents>();
            SerializedWiring.Set(events, "_audioSource", audioSource);
            SerializedWiring.SetArray(events, "_footstepClips", LoadClips("Player_Footstep"));
            SerializedWiring.Set(events, "_landingClip", LoadClips("Player_Land").FirstOrDefault());
            return animator;
        }

        /// <summary>
        /// Punto de la cabeza donde se enganchan los cosméticos: sigue al hueso de la cabeza, mira como el concursante y
        /// tiene la escala de esta cabeza, así gorras, gafas y pajaritas encajan sin tocarlas.
        /// </summary>
        private static Transform CreateCosmeticsAnchor(Transform root, Transform model)
        {
            var head = FindDeep(model, HEAD_BONE);
            var anchor = new GameObject(COSMETICS_ANCHOR).transform;
            anchor.SetParent(head, false);
            anchor.SetPositionAndRotation(root.TransformPoint(CosmeticsAnchorPosition), root.rotation);
            var headScale = head.lossyScale.x;
            anchor.localScale = Vector3.one * (COSMETICS_SCALE / headScale);
            return anchor;
        }

        private static PlayerNameTag CreateNameTag(Transform parent)
        {
            var label = EditorAssets.CreateWorldText("NameTag", parent, "Jugador", 3f, Color.white,
                Vector3.up * NAME_TAG_HEIGHT, Quaternion.identity, new Vector2(3f, 0.6f));
            label.outlineWidth = 0.2f;
            label.outlineColor = Color.black;
            label.gameObject.AddComponent<Billboard>();
            var nameTag = label.gameObject.AddComponent<PlayerNameTag>();
            SerializedWiring.Set(nameTag, "_label", label);
            return nameTag;
        }

        private static void WireInput(PlayerInputReader input)
        {
            var references = AssetDatabase.LoadAllAssetsAtPath(CONTROLS_PATH).OfType<InputActionReference>().ToArray();
            SerializedWiring.Set(input, "_move", FindAction(references, "Move"));
            SerializedWiring.Set(input, "_look", FindAction(references, "Look"));
            SerializedWiring.Set(input, "_jump", FindAction(references, "Jump"));
            SerializedWiring.Set(input, "_sprint", FindAction(references, "Sprint"));
        }

        /// <summary>Referencia a una acción de los controles del concursante, para otros módulos (p. ej. Interact).</summary>
        public static InputActionReference GetActionReference(string actionName) =>
            FindAction(AssetDatabase.LoadAllAssetsAtPath(CONTROLS_PATH).OfType<InputActionReference>().ToArray(), actionName);

        private static InputActionReference FindAction(InputActionReference[] references, string actionName) =>
            references.First(r => r.action.name == actionName);

        private static AudioClip[] LoadClips(string prefix) =>
            AssetDatabase.FindAssets($"{prefix} t:AudioClip", new[] { SFX_FOLDER })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p)
                .Select(AssetDatabase.LoadAssetAtPath<AudioClip>)
                .ToArray();

        private static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var found = FindDeep(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}
