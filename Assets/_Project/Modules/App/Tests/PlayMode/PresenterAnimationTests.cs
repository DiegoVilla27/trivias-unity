using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>En juego, el presentador baja los brazos de la pose T: está en reposo, animado.</summary>
    public sealed class PresenterAnimationTests
    {
        private GameObject _presenter;
        private GameObject _camera;
        private RenderTexture _image;

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_presenter);
            Object.Destroy(_camera);
            if (_image != null)
                _image.Release();
        }

        [UnityTest]
        public IEnumerator ThePresenter_StandsAtEase_NotInTPose()
        {
            _presenter = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Modules/Presenter/Prefabs/Presenter.prefab"),
                new Vector3(0f, -500f, 0f), Quaternion.identity);
            // Una cámara que lo vea: si nadie lo ve, no mueve los huesos. Pinta en una imagen porque en las pruebas por
            // consola no hay pantalla.
            _camera = new GameObject("PresenterTestCamera", typeof(Camera));
            _image = new RenderTexture(64, 64, 16);
            _camera.GetComponent<Camera>().targetTexture = _image;
            _camera.transform.SetPositionAndRotation(new Vector3(0f, -499f, 4f), Quaternion.Euler(0f, 180f, 0f));
            var animator = _presenter.GetComponentInChildren<Animator>();
            var upper = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var lower = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Assert.That((lower.position - upper.position).normalized.y, Is.EqualTo(0f).Within(0.1f), "Sin animar está en pose T.");

            yield return new WaitForSeconds(0.3f);

            Assert.That((lower.position - upper.position).normalized.y, Is.LessThan(-0.6f), "Con los brazos abajo, en reposo.");
        }
    }
}
