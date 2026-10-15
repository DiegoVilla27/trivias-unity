using System.Reflection;
using Trivias.Networking.Components;
using Trivias.Networking.Data;
using Trivias.Shared.Editor;
using Unity.Netcode;
using Unity.Netcode.Editor.Configuration;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEngine;

namespace Trivias.Networking.Editor
{
    /// <summary>Construye en tiempo de edición la configuración de red y el prefab del NetworkManager.</summary>
    public static class NetworkManagerPrefabBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Networking";
        public const string PREFAB_PATH = MODULE_ROOT + "/Prefabs/NetworkManager.prefab";
        public const string NETWORK_PREFABS_LIST_PATH = MODULE_ROOT + "/Config/DefaultNetworkPrefabs.asset";
        private const string CONFIG_PATH = MODULE_ROOT + "/Config/NetworkSessionConfig.asset";

        [MenuItem("Tools/Trivias/Build/Networking")]
        public static GameObject Build()
        {
            ConfigureNetcodeProjectSettings();
            var config = EditorAssets.GetOrCreateAsset<NetworkSessionConfig>(CONFIG_PATH);

            var root = new GameObject("NetworkManager");
            var networkManager = root.AddComponent<NetworkManager>();
            var transport = root.AddComponent<UnityTransport>();
            networkManager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                ConnectionApproval = true,
            };

            var connection = root.AddComponent<ConnectionService>();
            SerializedWiring.Set(connection, "_networkManager", networkManager);
            SerializedWiring.Set(connection, "_transport", transport);
            SerializedWiring.Set(connection, "_config", config);

            var panel = root.AddComponent<DebugConnectionPanel>();
            SerializedWiring.Set(panel, "_connection", connection);

            return EditorAssets.SaveAsPrefab(root, PREFAB_PATH);
        }

        /// <summary>
        /// Lista de prefabs de red por defecto, guardada dentro del módulo y no en la raíz de Assets.
        /// Garantiza que contiene los prefabs indicados.
        /// </summary>
        public static NetworkPrefabsList GetOrCreateNetworkPrefabsList(params GameObject[] requiredPrefabs)
        {
            var list = EditorAssets.GetOrCreateAsset<NetworkPrefabsList>(NETWORK_PREFABS_LIST_PATH);
            SerializedWiring.Set(list, "IsDefault", true);
            foreach (var prefab in requiredPrefabs)
            {
                if (!list.Contains(prefab))
                    list.Add(new NetworkPrefab { Prefab = prefab });
            }

            EditorUtility.SetDirty(list);
            return list;
        }

        /// <summary>Objetos de red que el host crea al abrir la sala (en la instancia de la escena).</summary>
        public static void SetSessionPrefabs(ConnectionService connection, GameObject[] prefabs)
        {
            var networkObjects = new Object[prefabs.Length];
            for (var i = 0; i < prefabs.Length; i++)
                networkObjects[i] = prefabs[i].GetComponent<NetworkObject>();
            SerializedWiring.SetArray(connection, "_sessionPrefabs", networkObjects);
        }

        /// <summary>
        /// Calcula el GlobalObjectIdHash de un prefab de red ya guardado. NGO solo lo genera al validar el asset,
        /// y un prefab recién creado por código (sobre todo en modo consola) puede quedar con hash 0 y no aparecer en red.
        /// </summary>
        public static void EnsureGlobalObjectIdHash(GameObject prefab)
        {
            var networkObject = prefab.GetComponent<NetworkObject>();
            typeof(NetworkObject)
                .GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(networkObject, null);

            if (networkObject.PrefabIdHash == 0)
                throw new System.InvalidOperationException($"No se pudo generar el GlobalObjectIdHash de {prefab.name}.");

            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssetIfDirty(prefab);
        }

        private static void ConfigureNetcodeProjectSettings()
        {
            var settings = NetcodeForGameObjectsProjectSettings.instance;
            if (settings.NetworkPrefabsPath == NETWORK_PREFABS_LIST_PATH)
                return;

            settings.NetworkPrefabsPath = NETWORK_PREFABS_LIST_PATH;
            settings.TempNetworkPrefabsPath = NETWORK_PREFABS_LIST_PATH;
            settings.GenerateDefaultNetworkPrefabs = true;
            // SaveSettings es interno en NGO; se invoca por reflexión solo en tiempo de edición.
            typeof(NetcodeForGameObjectsProjectSettings)
                .GetMethod("SaveSettings", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(settings, null);
        }
    }
}
