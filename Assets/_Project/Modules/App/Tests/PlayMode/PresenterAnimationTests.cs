using System.Collections;
using NUnit.Framework;
using Trivias.Presenter.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Trivias.App.Tests
{
    /// <summary>
    /// En juego el presentador se mueve con sus animaciones: en reposo baja los brazos; al hablar se lleva el micro a la
    /// boca; se alegra (puño arriba) y se decepciona (cabeza abajo), y después vuelve al reposo.
    /// </summary>
    public sealed class PresenterAnimationTests
    {
        private static readonly Vector3 Origin = new(0f, -500f, 0f);
        private GameObject _presenter;
        private GameObject _camera;
        private RenderTexture _image;
        private Animator _animator;
        private PresenterAnimator _view;

        [SetUp]
        public void SetUp()
        {
            _presenter = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Modules/Presenter/Prefabs/Presenter.prefab"),
                Origin, Quaternion.identity);
            // Una cámara que lo vea: si nadie lo ve, no mueve los huesos. Pinta en una imagen porque en las pruebas por
            // consola no hay pantalla.
            _camera = new GameObject("PresenterTestCamera", typeof(Camera));
            _image = new RenderTexture(64, 64, 16);
            _camera.GetComponent<Camera>().targetTexture = _image;
            _camera.transform.SetPositionAndRotation(Origin + new Vector3(0f, 1f, 4f), Quaternion.Euler(0f, 180f, 0f));
            _animator = _presenter.GetComponentInChildren<Animator>();
            _view = _presenter.GetComponent<PresenterAnimator>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_presenter);
            Object.Destroy(_camera);
            if (_image != null)
                _image.Release();
        }

        private Transform Bone(HumanBodyBones bone) => _animator.GetBoneTransform(bone);

        private float Height(HumanBodyBones bone) => Bone(bone).position.y - Origin.y;

        [UnityTest]
        public IEnumerator ThePresenter_StandsAtEase_NotInTPose()
        {
            var upper = Bone(HumanBodyBones.LeftUpperArm);
            var lower = Bone(HumanBodyBones.LeftLowerArm);
            yield return new WaitForSeconds(0.3f);

            Assert.That((lower.position - upper.position).normalized.y, Is.LessThan(-0.6f), "Con los brazos abajo, en reposo.");
            Assert.That(_view.IsTalking, Is.False);
        }

        [UnityTest]
        public IEnumerator Talking_BringsTheMicToHisMouth_AndStopping_LowersIt()
        {
            var micHead = FindDeep(_presenter.transform, "Grille");
            var mouth = _presenter.transform.TransformPoint(new Vector3(0f, 1.15f, 0.47f));
            yield return new WaitForSeconds(0.3f);
            var resting = Vector3.Distance(micHead.position, mouth);

            _view.SetTalking(true);
            yield return new WaitForSeconds(0.9f);
            Assert.That(_view.IsTalking, Is.True);
            Assert.That(Vector3.Distance(micHead.position, mouth), Is.LessThan(0.14f), "El micro, en la boca.");
            Assert.That(resting, Is.GreaterThan(0.4f), "En reposo el micro está abajo.");

            _view.SetTalking(false);
            yield return new WaitForSeconds(0.8f);
            Assert.That(Vector3.Distance(micHead.position, mouth), Is.GreaterThan(0.3f), "Al callar, lo baja.");
        }

        [UnityTest]
        public IEnumerator WhenHappy_HeRaisesHisFist_ThenGoesBackToRest()
        {
            yield return new WaitForSeconds(0.3f);
            var shoulder = Height(HumanBodyBones.LeftShoulder);
            _view.React(happy: true);
            var highest = 0f;
            var rose = false;
            for (var t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                yield return null;
                highest = Mathf.Max(highest, Height(HumanBodyBones.LeftHand));
                rose |= Height(HumanBodyBones.Hips) > 0.36f + 0.06f;
            }
            Assert.That(highest, Is.GreaterThan(shoulder + 0.15f), "Puño arriba, por encima del hombro.");
            Assert.That(rose, Is.True, "Da un saltito.");

            yield return new WaitForSeconds(1.6f);
            Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), Is.True, "Y vuelve al reposo.");
        }

        [UnityTest]
        public IEnumerator WhenDisappointed_HeHangsHisHead()
        {
            yield return new WaitForSeconds(0.3f);
            _view.React(happy: false);
            yield return new WaitForSeconds(1.0f);
            Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName("Sad"), Is.True);
            var head = Bone(HumanBodyBones.Head);
            Assert.That(Vector3.Angle(head.up, Vector3.up), Is.GreaterThan(12f), "Cabeza gacha.");
            Assert.That(Vector3.Dot(head.up, _presenter.transform.forward), Is.GreaterThan(0.15f), "Hacia delante, mirando al suelo.");
        }

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
    }
}
