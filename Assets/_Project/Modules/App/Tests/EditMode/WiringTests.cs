using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Comprueba que escena y prefabs están bien cableados: ningún componente del proyecto tiene scripts que faltan
    /// ni referencias [SerializeField] vacías. Detecta una escena desfasada respecto al código.
    /// </summary>
    public sealed class WiringTests
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Stage.unity";
        private const string PROJECT_NAMESPACE = "Trivias.";

        [Test]
        public void StageScene_HasNoMissingReferences()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            var problems = new List<string>();
            foreach (var root in scene.GetRootGameObjects())
                Inspect(root, problems);
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void ProjectPrefabs_HaveNoMissingReferences()
        {
            var problems = new List<string>();
            var prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" })
                .Select(AssetDatabase.GUIDToAssetPath);
            foreach (var path in prefabs)
                Inspect(AssetDatabase.LoadAssetAtPath<GameObject>(path), problems);
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void BuildStamp_MatchesCurrentCode()
        {
            var autoBuild = System.Type.GetType("Trivias.App.Editor.AutoBuild, Trivias.App.Editor");
            Assert.That(autoBuild, Is.Not.Null);
            var outdated = (bool)autoBuild.GetProperty("IsOutdated").GetValue(null);
            Assert.That(outdated, Is.False, "La escena no corresponde al código actual: hay que ejecutar Build All.");
        }

        private static void Inspect(GameObject root, List<string> problems)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var go = transform.gameObject;
                foreach (var component in go.GetComponents<Component>())
                {
                    if (component == null)
                    {
                        problems.Add($"{Path(go)}: script que falta.");
                        continue;
                    }

                    if (component.GetType().FullName?.StartsWith(PROJECT_NAMESPACE) != true)
                        continue;

                    var property = new SerializedObject(component).GetIterator();
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            property.name != "m_Script" && property.objectReferenceValue == null)
                        {
                            problems.Add($"{Path(go)} / {component.GetType().Name}.{property.propertyPath} está vacío.");
                        }
                    }
                }
            }
        }

        private static string Path(GameObject go)
        {
            var path = go.name;
            for (var parent = go.transform.parent; parent != null; parent = parent.parent)
                path = $"{parent.name}/{path}";
            return path;
        }
    }
}
