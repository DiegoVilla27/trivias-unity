using NUnit.Framework;
using Trivias.App.Editor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Trivias.App.Tests
{
    /// <summary>La reconstrucción automática no cambia la escena que el usuario tiene abierta.</summary>
    public sealed class OpenScenesKeeperTests
    {
        private const string STAGE = "Assets/_Project/Scenes/Stage.unity";

        [Test]
        public void SceneThatChangesScenes_GivesBackTheOpenScene()
        {
            EditorSceneManager.OpenScene(STAGE, OpenSceneMode.Single);
            OpenScenesKeeper.Run(() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single));
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(STAGE));
            Assert.That(SceneManager.sceneCount, Is.EqualTo(1));
        }

        [Test]
        public void UnsavedScene_DoesNotBreakTheRebuild()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var ran = false;
            Assert.DoesNotThrow(() => OpenScenesKeeper.Run(() => ran = true));
            Assert.That(ran, Is.True);
        }
    }
}
