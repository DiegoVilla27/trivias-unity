using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Comprueba la distribución del plató en la escena generada: la pantalla de preguntas es grande y nada
    /// (cabinas, tubos, textos, mesa, botón, presentador) la tapa desde las zonas de inicio.
    /// </summary>
    public sealed class StageLayoutTests
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Stage.unity";
        private const string ENVIRONMENT = "StageEnvironment";
        private const string SCREEN = "StageEnvironment/QuestionScreen";
        private const float EYE_HEIGHT = 1.7f;
        private static readonly Vector3 CameraOffset = new(0.5f, 2.2f, -3.8f); // Tercera persona: detrás y por encima.

        private GameObject[] _roots;

        [SetUp]
        public void OpenStage() =>
            _roots = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single).GetRootGameObjects();

        [Test]
        public void QuestionScreen_IsLarge()
        {
            var panel = Find($"{SCREEN}/Panel").GetComponent<Renderer>().bounds.size;
            Assert.That(panel.x, Is.GreaterThanOrEqualTo(14f), "La pantalla debe tener al menos 14 m de ancho.");
            Assert.That(panel.y, Is.GreaterThanOrEqualTo(7f), "La pantalla debe tener al menos 7 m de alto.");
        }

        [Test]
        public void QuestionScreen_HasQuestionFourOptionsAndStatus()
        {
            Assert.That(Find($"{SCREEN}/Content/Question"), Is.Not.Null);
            Assert.That(Find($"{SCREEN}/Content/Header"), Is.Not.Null);
            Assert.That(Find($"{SCREEN}/Content/Status"), Is.Not.Null);
            Assert.That(Find($"{SCREEN}/Message"), Is.Not.Null);
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                Assert.That(Find($"{SCREEN}/Content/Option{letter}/Box"), Is.Not.Null, $"Falta el recuadro {letter}.");
                Assert.That(Find($"{SCREEN}/Content/Option{letter}/Text"), Is.Not.Null, $"Falta el texto {letter}.");
            }
        }

        [Test]
        public void QuestionScreen_IsNotBlockedFromAnySpawnPoint()
        {
            var targets = new List<(string Name, Vector3 Point)>
            {
                ("pregunta", Find($"{SCREEN}/Content/Question").transform.position),
                ("estado", Find($"{SCREEN}/Content/Status").transform.position),
            };
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                targets.Add(($"opción {letter}", Find($"{SCREEN}/Content/Option{letter}/Box").transform.position));
            }

            var blockers = Blockers();
            var problems = new List<string>();
            foreach (var spawn in SpawnPositions())
            {
                foreach (var viewpoint in new[] { spawn + Vector3.up * EYE_HEIGHT, spawn + CameraOffset })
                {
                    foreach (var (name, point) in targets)
                    {
                        var blocker = FirstBlocker(blockers, viewpoint, point);
                        if (blocker != null)
                            problems.Add($"Desde {viewpoint}: «{blocker}» tapa la {name}.");
                    }
                }
            }

            Assert.That(problems, Is.Empty, string.Join("\n", problems.Distinct()));
        }

        [Test]
        public void PresenterBubble_DoesNotOverlapTheScreen()
        {
            var bubble = Find("Presenter/Bubble/Background").transform;
            var bubbleLeftEdge = bubble.position.x - bubble.lossyScale.x * 0.5f;
            var screenHalfWidth = Find($"{SCREEN}/Frame").GetComponent<Renderer>().bounds.extents.x;
            Assert.That(bubbleLeftEdge, Is.GreaterThan(screenHalfWidth), "La viñeta del presentador no debe tapar la pantalla.");
        }

        [Test]
        public void Presenter_StandsBesideTheScreen()
        {
            var presenter = Find("Presenter").transform.position;
            var screenHalfWidth = Find($"{SCREEN}/Frame").GetComponent<Renderer>().bounds.extents.x;
            Assert.That(Mathf.Abs(presenter.x), Is.GreaterThan(screenHalfWidth), "El presentador no debe estar delante de la pantalla.");
        }

        // ---- Utilidades ----

        private GameObject Find(string path)
        {
            var parts = path.Split('/');
            var root = _roots.FirstOrDefault(r => r.name == parts[0]);
            if (root == null)
                return null;
            var transform = parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
            return transform != null ? transform.gameObject : null;
        }

        /// <summary>Todo lo visible fuera del entorno (suelo, paredes y la propia pantalla no cuentan).</summary>
        private List<Renderer> Blockers() =>
            _roots.Where(r => r.name != ENVIRONMENT)
                .SelectMany(r => r.GetComponentsInChildren<Renderer>(true))
                .ToList();

        private static string FirstBlocker(List<Renderer> blockers, Vector3 from, Vector3 to)
        {
            var direction = to - from;
            var ray = new Ray(from, direction.normalized);
            foreach (var renderer in blockers)
            {
                if (renderer.bounds.IntersectRay(ray, out var distance) && distance < direction.magnitude - 0.5f)
                    return renderer.transform.parent != null ? $"{renderer.transform.parent.name}/{renderer.name}" : renderer.name;
            }
            return null;
        }

        private static IEnumerable<Vector3> SpawnPositions()
        {
            var spawnPoints = UnityEditor.AssetDatabase.LoadAssetAtPath<Trivias.Shared.Spawning.SpawnPointSet>(
                "Assets/_Project/Shared/Config/Spawning/StageSpawnPoints.asset");
            for (var i = 0; i < spawnPoints.Count; i++)
            {
                spawnPoints.Get(i, out var position, out _);
                yield return position;
            }
        }
    }
}
