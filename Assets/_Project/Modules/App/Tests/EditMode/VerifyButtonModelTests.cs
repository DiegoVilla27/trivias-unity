using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Trivias.App.Tests
{
    /// <summary>El botón VERIFICAR usa su modelo sin la placa (decisión del usuario); el resto de piezas, tal cual.</summary>
    public sealed class VerifyButtonModelTests
    {
        private static GameObject Prefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Modules/VerifyButton/Prefabs/VerifyButton.prefab");

        [Test]
        public void TheButton_HasNoPlaque()
        {
            foreach (var part in Prefab().GetComponentsInChildren<Transform>(true))
                Assert.That(part.name, Is.Not.EqualTo("VerifyPlaque"));
        }

        [Test]
        public void TheRestOfTheModel_IsThere()
        {
            var model = Prefab().transform.Find("Model");
            foreach (var part in new[] { "Pedestal", "BottomGoldTrim", "UpperGoldCollar", "ButtonSocket", "Cap" })
                Assert.That(model.Find(part), Is.Not.Null, $"Falta {part}.");
        }
    }
}
