using Trivias.Cameras.Components;
using Trivias.Shared.Editor;
using Trivias.Shared.Events;
using Trivias.Shared.Physics;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Trivias.Cameras.Editor
{
    /// <summary>Construye el rig de cámara: cámara de Unity con CinemachineBrain y cámara de seguimiento en tercera persona.</summary>
    public static class CameraRigPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Camera";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/CameraRig.prefab";

        [MenuItem("Tools/Trivias/Build/Camera Rig")]
        public static GameObject BuildFromMenu() => Build(SharedAssets.LocalPlayerCameraTarget());

        public static GameObject Build(TransformEventChannel localPlayerCameraTarget)
        {
            var root = new GameObject("CameraRig");

            var mainCamera = EditorAssets.CreateEmpty("MainCamera", root.transform, Vector3.zero);
            mainCamera.tag = "MainCamera";
            var unityCamera = mainCamera.AddComponent<UnityEngine.Camera>();
            unityCamera.nearClipPlane = 0.1f;
            unityCamera.farClipPlane = 200f;
            unityCamera.clearFlags = CameraClearFlags.SolidColor;
            unityCamera.backgroundColor = new Color(0.04f, 0.04f, 0.09f);
            // Posprocesado del plató (brillo de bombillas y focos, viñeta): lo define el volumen del módulo Stage.
            unityCamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            mainCamera.AddComponent<AudioListener>();
            mainCamera.AddComponent<CinemachineBrain>();

            var follow = EditorAssets.CreateEmpty("PlayerFollowCamera", root.transform, Vector3.zero);
            var cinemachineCamera = follow.AddComponent<CinemachineCamera>();
            cinemachineCamera.Lens.FieldOfView = 50f;
            var thirdPerson = follow.AddComponent<CinemachineThirdPersonFollow>();
            // Los concursantes son cabezones (cabeza de 1 m): la cámara va más al lado, algo más alta y más lejos para que
            // la cabeza no tape el plató.
            thirdPerson.ShoulderOffset = new Vector3(0.75f, 0.2f, 0f);
            thirdPerson.VerticalArmLength = 0.35f;
            thirdPerson.CameraSide = 1f;
            thirdPerson.CameraDistance = 4.2f;
            thirdPerson.Damping = new Vector3(0.1f, 0.4f, 0.3f);
            var obstacles = thirdPerson.AvoidObstacles;
            obstacles.Enabled = true;
            obstacles.CollisionFilter = GameLayers.STAGE_MASK | GameLayers.DEFAULT_MASK;
            obstacles.CameraRadius = 0.2f;
            thirdPerson.AvoidObstacles = obstacles;

            var cursorLock = root.AddComponent<CursorLockController>();
            SerializedWiring.Set(cursorLock, "_controlGate", SharedAssets.PlayerControlGate());
            SerializedWiring.Set(cursorLock, "_blockers", SharedAssets.CursorCaptureBlockers());
            var binder = root.AddComponent<LocalPlayerCameraBinder>();
            SerializedWiring.Set(binder, "_followCamera", cinemachineCamera);
            SerializedWiring.Set(binder, "_cursorLock", cursorLock);
            SerializedWiring.Set(binder, "_localPlayerCameraTarget", localPlayerCameraTarget);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }
    }
}
