using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Trivias.App.Tests
{
    /// <summary>
    /// Comprueba la distribución del plató en la escena generada: la pantalla de preguntas es grande y nada
    /// (cabinas, tubos, textos, mesa, botón, presentador) la tapa desde las zonas de inicio; los objetos usan sus
    /// modelos y el decorado de concepto-01 está montado.
    /// </summary>
    public sealed class StageLayoutTests
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/Stage.unity";
        private const string ENVIRONMENT = "StageEnvironment";
        private const string SCREEN = "StageEnvironment/QuestionScreen";
        private const float EYE_HEIGHT = 1.7f;
        private static readonly Vector3 CameraOffset = new(0.75f, 2.62f, -4.2f); // Tercera persona: detrás y por encima.

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
            Assert.That(Find($"{SCREEN}/Display/Content/Question"), Is.Not.Null);
            Assert.That(Find($"{SCREEN}/Display/Content/Header"), Is.Not.Null);
            Assert.That(Find($"{SCREEN}/Display/Content/StatusBox/Status"), Is.Not.Null);
            Assert.That(Find($"{SCREEN}/Display/Message"), Is.Not.Null);
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                Assert.That(Find($"{SCREEN}/Display/Content/Options/Option{letter}/Box"), Is.Not.Null, $"Falta el recuadro {letter}.");
                Assert.That(Find($"{SCREEN}/Display/Content/Options/Option{letter}/Text"), Is.Not.Null, $"Falta el texto {letter}.");
                var tag = (RectTransform)Find($"{SCREEN}/Display/Content/Options/Option{letter}/Box/Tag").transform;
                var letterBox = (RectTransform)tag.Find("Letter");
                Assert.That(letterBox.sizeDelta, Is.EqualTo(tag.sizeDelta), $"La letra {letter} ocupa su círculo (centrada en él).");
                Assert.That(letterBox.anchoredPosition, Is.EqualTo(Vector2.zero));
            }
        }

        [Test]
        public void QuestionScreen_IsNotBlockedFromAnySpawnPoint()
        {
            var targets = new List<(string Name, Vector3 Point)>
            {
                ("pregunta", Find($"{SCREEN}/Display/Content/Question").transform.position),
                ("estado", Find($"{SCREEN}/Display/Content/StatusBox/Status").transform.position),
            };
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                targets.Add(($"opción {letter}", Find($"{SCREEN}/Display/Content/Options/Option{letter}/Box").transform.position));
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
        public void Presenter_StandsBetweenTheStationsAndTheScreen_WithoutCoveringIt()
        {
            var presenter = Find("Presenter").transform.position;
            var exchange = Find("ExchangeStation").transform.position;
            var button = Find("VerifyButton").transform.position;
            var frame = BoundsOf(Find($"{SCREEN}/Frame"));
            Assert.That(presenter.x, Is.EqualTo((exchange.x + button.x) * 0.5f).Within(0.05f), "En medio de cambio y verificar.");
            Assert.That(presenter.z, Is.GreaterThan(Mathf.Max(exchange.z, button.z) + 1f).And.LessThan(frame.min.z - 0.5f),
                "Entre las estaciones y la pantalla.");
            Assert.That(Find($"{ENVIRONMENT}/PresenterMark").transform.position.x, Is.EqualTo(presenter.x).Within(0.05f), "Sobre su marca dorada.");

            // Ni su cuerpo ni su cartel tapan el texto de la pantalla desde la zona de inicio.
            var body = Find("Presenter/Model").GetComponentInChildren<SkinnedMeshRenderer>().bounds;
            var screen = Find(SCREEN).transform.position;
            foreach (var spawn in SpawnPositions())
            {
                var eye = spawn + Vector3.up * EYE_HEIGHT;
                foreach (var x in new[] { -3.5f, 0f, 3.5f })
                {
                    var target = screen + new Vector3(x, -2.95f, -0.3f); // La línea más baja: reloj y respuesta correcta.
                    var direction = target - eye;
                    Assert.That(body.IntersectRay(new Ray(eye, direction.normalized), out var distance) && distance < direction.magnitude,
                        Is.False, $"Desde {spawn} el presentador tapa la línea de estado.");
                }
            }
        }

        [Test]
        public void Stations_StandAtTheFootOfTheScreen_OnEachSideOfThePresenter()
        {
            var exchange = Find("ExchangeStation").transform;
            var button = Find("VerifyButton").transform;
            Assert.That(exchange.position.x, Is.EqualTo(-2.7f).Within(0.01f));
            Assert.That(button.position.x, Is.EqualTo(2.7f).Within(0.01f));
            Assert.That(exchange.position.z, Is.EqualTo(9.39f).Within(0.01f), "Donde las dejó el usuario.");
            Assert.That(button.position.z, Is.EqualTo(9.39f).Within(0.01f));
            Assert.That(Vector3.Dot(exchange.forward, Vector3.back), Is.GreaterThan(0.99f), "Miran a la zona de inicio.");
            Assert.That(Vector3.Dot(button.forward, Vector3.back), Is.GreaterThan(0.99f));
        }

        [Test]
        public void CorrectAnswerConfetti_ShootsFromBothSidesOfTheScreen()
        {
            var frame = BoundsOf(Find($"{SCREEN}/Frame"));
            var left = Find("StageConfetti/CorrectLeft").transform;
            var right = Find("StageConfetti/CorrectRight").transform;

            Assert.That(left.position.x, Is.LessThan(-frame.extents.x), "El chorro izquierdo debe salir fuera de la pantalla, no delante.");
            Assert.That(right.position.x, Is.GreaterThan(frame.extents.x), "El chorro derecho debe salir fuera de la pantalla, no delante.");
            foreach (var emitter in new[] { left, right })
            {
                Assert.That(Mathf.Abs(emitter.position.z - frame.center.z), Is.LessThan(1.5f), "Junto a la pantalla.");
                Assert.That(emitter.forward.y, Is.GreaterThan(0.7f), "Dispara hacia arriba.");
                Assert.That(Mathf.Sign(emitter.forward.x), Is.EqualTo(-Mathf.Sign(emitter.position.x)), "Se abre hacia el centro.");
            }
        }

        [Test]
        public void VictoryConfetti_CoversTheWholeRoom()
        {
            var rain = Find("StageConfetti/VictoryRain").GetComponent<ParticleSystem>();
            var floor = Find($"{ENVIRONMENT}/Floor").GetComponent<Renderer>().bounds;
            Assert.That(rain.shape.scale.x, Is.GreaterThanOrEqualTo(floor.size.x * 0.9f), "La lluvia dorada debe cubrir la sala a lo ancho.");
            Assert.That(rain.shape.scale.z, Is.GreaterThanOrEqualTo(floor.size.z * 0.9f), "Y a lo largo.");
            var offset = rain.transform.position - floor.center;
            Assert.That(new Vector2(offset.x, offset.z).magnitude, Is.LessThan(2f), "Centrada sobre la sala.");
            Assert.That(rain.main.loop, Is.True, "Sigue lloviendo mientras se ven los resultados.");
        }

        [Test]
        public void VictoryPodium_RisesInFrontOfTheTable_WithoutBlockingTheScreen()
        {
            var podium = Find("VictoryPodium").transform.position;
            var frame = BoundsOf(Find($"{SCREEN}/Frame"));
            var table = Find("Table").transform.position;
            var spawnZ = SpawnPositions().Max(p => p.z);
            Assert.That(podium.z, Is.GreaterThan(spawnZ + 2f).And.LessThan(table.z - 2.6f), "Delante de la mesa, entre ella y la zona de inicio.");
            Assert.That(Mathf.Abs(podium.x), Is.LessThan(0.5f), "Centrado.");

            var lift = Find("VictoryPodium/Podium/Lift").transform;
            var top = float.MinValue;
            foreach (var renderer in lift.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is MeshRenderer)
                    top = Mathf.Max(top, lift.TransformPoint(renderer.transform.localPosition).y + renderer.transform.lossyScale.y * 0.5f);
            }
            Assert.That(top, Is.LessThan(frame.min.y - 0.3f), "El podio queda por debajo del marco de la pantalla.");
            Assert.That(Find("VictoryPodium/Podium").activeSelf, Is.False, "Oculto hasta la victoria.");
            Assert.That(lift.GetComponentsInChildren<Collider>(true), Is.Empty, "Sin colisiones: no empuja a nadie al subir.");
        }

        [Test]
        public void VictoryCheck_FitsInsideTheScreen()
        {
            var panel = Find($"{SCREEN}/Panel").GetComponent<Renderer>().bounds;
            var border = Find($"{SCREEN}/Check/Border").transform.lossyScale;
            Assert.That(border.x, Is.LessThanOrEqualTo(panel.size.x));
            Assert.That(border.y, Is.LessThanOrEqualTo(panel.size.y));
            Assert.That(Find($"{SCREEN}/Check").activeSelf, Is.False, "El cheque solo sale al ganar.");
        }

        [Test]
        public void StageSpotlights_PointDownAtTheStage()
        {
            var spots = Find("StageLighting").GetComponentsInChildren<Light>();
            Assert.That(spots.Length, Is.GreaterThanOrEqualTo(4));
            foreach (var spot in spots)
            {
                Assert.That(spot.type, Is.EqualTo(LightType.Spot));
                Assert.That(spot.transform.forward.y, Is.LessThan(-0.4f), $"{spot.name} debe apuntar hacia el plató.");
                Assert.That(spot.shadows, Is.EqualTo(LightShadows.None), "Sin sombras: baratos de dibujar.");
            }
        }

        [Test]
        public void Booths_UseTheirModels_WithTheTrayOnTheCounter()
        {
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                var root = Find($"Booths/Booth_{letter}");
                Assert.That(root, Is.Not.Null, $"Falta la cabina {letter}.");
                Assert.That(ModelPath(root.transform.Find("Body")), Does.EndWith($"BoothCabinet_{letter}.obj"), $"Cabina {letter}.");
                var cabinet = BoundsOf(root.transform.Find("Body").gameObject);
                var surface = root.transform.Find("Tray/Surface").position;
                Assert.That(surface.y, Is.InRange(1.1f, 1.35f), "El dinero se apila en la bandeja del mostrador.");
                Assert.That(cabinet.Contains(surface + Vector3.up * 0.05f), Is.True, "La bandeja está dentro del mueble.");
                Assert.That(root.GetComponentsInChildren<BoxCollider>().Length, Is.GreaterThanOrEqualTo(2), "El mueble no se atraviesa.");
            }
        }

        [Test]
        public void EachBooth_HasAMoneyTube_AndAPlayerTubeOverItsPlatform()
        {
            var ceiling = Find($"{ENVIRONMENT}/Ceiling").GetComponent<Renderer>().bounds.min.y;
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                var root = Find($"Booths/Booth_{letter}").transform;
                var cabinet = BoundsOf(root.Find("Body").gameObject);

                var money = BoundsOf(root.Find("MoneyTube").gameObject);
                Assert.That(money.min.y, Is.EqualTo(cabinet.max.y).Within(0.3f), $"{letter}: el tubo del dinero sale del techo del mueble.");
                Assert.That(money.max.y, Is.GreaterThanOrEqualTo(ceiling - 0.05f), $"{letter}: y sube hasta el techo del plató.");

                var zone = root.Find("BetZone").position;
                var player = BoundsOf(root.Find("PlayerTube").gameObject);
                Assert.That(new Vector2(player.center.x - zone.x, player.center.z - zone.z).magnitude, Is.LessThan(0.15f),
                    $"{letter}: el tubo del concursante está justo encima de su plataforma.");
                Assert.That(player.min.y, Is.InRange(3f, 3.8f), $"{letter}: su boca queda por encima de la cabeza, aun saltando.");
                Assert.That(player.max.y, Is.GreaterThanOrEqualTo(ceiling - 0.05f), $"{letter}: cuelga del techo.");
            }
        }

        [Test]
        public void MoneyTubes_AreTheSameInEveryBooth_WithOneGoldCollarPerJoint()
        {
            string Shape(Transform tube) => string.Join("\n", tube.GetComponentsInChildren<Transform>()
                .Where(t => t != tube)
                .Select(t => $"{t.name}|{tube.InverseTransformPoint(t.position).ToString("F2")}|{t.gameObject.activeSelf}"));

            var reference = Shape(Find("Booths/Booth_C/MoneyTube").transform);
            foreach (var letter in new[] { 'A', 'B', 'D' })
                Assert.That(Shape(Find($"Booths/Booth_{letter}/MoneyTube").transform), Is.EqualTo(reference),
                    $"El tubo del dinero de {letter} es igual que el de C.");

            foreach (var tubeName in new[] { "MoneyTube", "PlayerTube" })
            {
                var collars = Find($"Booths/Booth_C/{tubeName}").GetComponentsInChildren<Renderer>()
                    .Where(r => r.name.Contains("Collar")).Select(r => r.bounds.center).ToList();
                for (var i = 0; i < collars.Count; i++)
                    for (var j = i + 1; j < collars.Count; j++)
                        Assert.That(Vector3.Distance(collars[i], collars[j]), Is.GreaterThan(0.1f),
                            $"{tubeName}: dos aros dorados en la misma junta.");
            }
        }

        [Test]
        public void BetZones_AreThePlatformModel_OpenTowardsTheCenter()
        {
            var table = Find("Table").transform.position;
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                var root = Find($"Booths/Booth_{letter}").transform;
                var platform = root.Find("Platform");
                Assert.That(ModelPath(platform), Does.EndWith("PlayerPlatform.obj"), letter.ToString());
                var zone = root.Find("BetZone").position;
                Assert.That(zone.y, Is.InRange(0.2f, 0.3f), $"{letter}: el color de la zona está sobre la plataforma.");
                Assert.That(BoundsOf(platform.gameObject).max.y, Is.LessThan(1.2f), $"{letter}: barandilla baja.");
                // La barandilla está abierta por +Z del modelo: hacia el centro del plató.
                var open = platform.forward;
                var toCenter = table - platform.position;
                toCenter.y = 0f;
                Assert.That(Vector3.Dot(open, toCenter.normalized), Is.GreaterThan(0.6f), $"{letter}: se entra desde el centro.");
                Assert.That(root.GetComponentsInChildren<Collider>().Any(c => c.name == "PlatformCollider"), Is.True,
                    $"{letter}: la plataforma se pisa.");
            }
        }

        [Test]
        public void Booths_StandOnTheSides_LeavingTheViewOfTheScreenFree()
        {
            var frame = BoundsOf(Find($"{SCREEN}/Frame"));
            for (var booth = 0; booth < 4; booth++)
            {
                var letter = (char)('A' + booth);
                var position = Find($"Booths/Booth_{letter}").transform.position;
                Assert.That(Mathf.Abs(position.x), Is.GreaterThan(frame.extents.x * 0.9f), $"{letter}: a un lado del plató.");
                Assert.That(position.x < 0f, Is.EqualTo(booth % 2 == 0), $"{letter}: A y C a la izquierda, B y D a la derecha.");
                Assert.That(position.z > 5f, Is.EqualTo(booth < 2), $"{letter}: A y B al fondo, C y D delante (como la pantalla).");
            }
        }

        [Test]
        public void Room_IsCompact()
        {
            var floor = Find($"{ENVIRONMENT}/Floor").GetComponent<Renderer>().bounds.size;
            Assert.That(floor.x, Is.LessThanOrEqualTo(26f), "Sin espacio muerto a los lados.");
            Assert.That(floor.z, Is.LessThanOrEqualTo(31f), "Ni delante o detrás.");
            var ceiling = Find($"{ENVIRONMENT}/Ceiling").GetComponent<Renderer>().bounds.min.y;
            Assert.That(ceiling, Is.LessThanOrEqualTo(11.5f), "Techo bajo, de estudio de televisión.");
        }

        [Test]
        public void Floor_IsTiled_FlushWithTheGround()
        {
            var tiles = Find($"{ENVIRONMENT}/FloorTiles").transform;
            Assert.That(tiles.childCount, Is.GreaterThanOrEqualTo(30));
            foreach (Transform tile in tiles)
            {
                Assert.That(ModelPath(tile), Does.EndWith("StudioFloorTile.obj"));
                Assert.That(BoundsOf(tile.gameObject).max.y, Is.EqualTo(0f).Within(0.02f), $"{tile.name}: a ras del suelo.");
            }
            var collider = Find($"{ENVIRONMENT}/FloorCollider").GetComponent<BoxCollider>();
            Assert.That(collider.bounds.max.y, Is.EqualTo(0f).Within(0.001f), "Se pisa a nivel de las baldosas.");
        }

        [Test]
        public void CeilingPanels_ShowTheirFaceDownwards_UnderTheCeiling()
        {
            var ceiling = Find($"{ENVIRONMENT}/Ceiling").GetComponent<Renderer>().bounds.min.y;
            var panels = Find($"{ENVIRONMENT}/CeilingRig").GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("CeilingPanel")).ToList();
            Assert.That(panels.Count, Is.GreaterThanOrEqualTo(8));
            foreach (var panel in panels)
            {
                Assert.That(Vector3.Dot(panel.up, Vector3.up), Is.GreaterThan(0.99f), $"{panel.name}: soportes arriba, cara lisa abajo.");
                Assert.That(BoundsOf(panel.gameObject).max.y, Is.EqualTo(ceiling).Within(0.05f), $"{panel.name}: pegado al techo.");
            }
        }

        [Test]
        public void TableButtonAndExchange_UseTheirModels()
        {
            Assert.That(ModelPath(Find("Table/Model").transform), Does.EndWith("Table.obj"));
            Assert.That(ModelPath(Find("VerifyButton/Model").transform), Does.EndWith("VerifyButton.obj"));
            Assert.That(ModelPath(Find("ExchangeStation/Model").transform), Does.EndWith("ExchangeStation.obj"));
            Assert.That(ModelPath(Find($"{SCREEN}/Frame").transform), Does.EndWith("QuestionScreen.obj"));
        }

        [Test]
        public void MoneyPrefabs_UseTheirModels()
        {
            foreach (var name in new[] { "Bundle", "Bag", "GoldBar" })
            {
                var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Modules/Money/Prefabs/{name}.prefab");
                Assert.That(ModelPath(prefab.transform.Find("Model")), Does.EndWith($"{name}.obj"), name);
                Assert.That(prefab.GetComponentsInChildren<Renderer>().All(r => r.shadowCastingMode == ShadowCastingMode.Off), Is.True,
                    $"{name}: el dinero no proyecta sombras (hay mucho).");
            }
        }

        [Test]
        public void Stage_HasTheConceptDecor()
        {
            Assert.That(Find($"{ENVIRONMENT}/Backdrop").transform.childCount, Is.GreaterThanOrEqualTo(10), "Telón, bambalinas, columnas y altavoces.");
            Assert.That(Find($"{ENVIRONMENT}/CeilingRig").GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(100), "Vigas y focos.");
            Assert.That(Find($"{ENVIRONMENT}/WallDecor").transform.childCount, Is.GreaterThanOrEqualTo(20), "Paneles y columnas en las paredes.");
            Assert.That(Find($"{ENVIRONMENT}/StudioCameras"), Is.Not.Null);
            var people = Find($"{ENVIRONMENT}/Audience").GetComponentsInChildren<Transform>().Count(t => t.name == "Head");
            Assert.That(people, Is.GreaterThanOrEqualTo(40), "Público en las gradas.");
            foreach (var spot in Find("StageLighting").GetComponentsInChildren<Light>())
                Assert.That(spot.transform.Find("Fixture"), Is.Not.Null, $"{spot.name}: la luz sale de un foco colgado.");
        }

        [Test]
        public void CameraDais_HasStairsAndACamera_AndPlayersCannotReachTheAudience()
        {
            var dais = Find($"{ENVIRONMENT}/CameraDais").transform;
            Assert.That(ModelPath(dais.Find("Stairs")), Does.EndWith("StageStairs.obj"));
            Assert.That(ModelPath(dais.Find("Camera")), Does.EndWith("StudioCamera.obj"));
            Assert.That(dais.Find("Camera").position.y, Is.EqualTo(0.62f).Within(0.02f), "La cámara está encima de la plataforma.");
            var barrier = Find($"{ENVIRONMENT}/AudienceRailing/RailingCollider").GetComponent<BoxCollider>();
            var floorWidth = Find($"{ENVIRONMENT}/Floor").GetComponent<Renderer>().bounds.size.x;
            Assert.That(barrier.bounds.size.x, Is.GreaterThanOrEqualTo(floorWidth - 0.1f), "La barandilla cruza toda la sala.");
            foreach (var spawn in SpawnPositions())
                Assert.That(spawn.z, Is.GreaterThan(barrier.bounds.max.z), "Los concursantes empiezan en el plató, no en el público.");
        }

        [Test]
        public void PostProcessing_MakesBulbsGlow()
        {
            var volume = Find($"{ENVIRONMENT}/PostProcessing").GetComponent<Volume>();
            Assert.That(volume.isGlobal, Is.True);
            Assert.That(volume.sharedProfile.TryGet<Bloom>(out var bloom) && bloom.intensity.value > 0f, Is.True, "Con brillo (bloom).");
            var camera = Find("CameraRig/MainCamera").GetComponent<Camera>();
            Assert.That(camera.GetUniversalAdditionalCameraData().renderPostProcessing, Is.True, "La cámara aplica el posprocesado.");
        }

        [Test]
        public void PostProcessing_IsComfortable_NotDazzling()
        {
            var profile = Find($"{ENVIRONMENT}/PostProcessing").GetComponent<Volume>().sharedProfile;
            Assert.That(profile.TryGet<Bloom>(out var bloom), Is.True);
            Assert.That(bloom.threshold.value, Is.GreaterThanOrEqualTo(1.2f), "Solo brilla lo que de verdad está encendido.");
            Assert.That(bloom.intensity.value, Is.LessThanOrEqualTo(0.4f), "Sin velo de resplandor sobre todo.");
            Assert.That(profile.TryGet<ColorAdjustments>(out var color), Is.True);
            Assert.That(color.saturation.value, Is.LessThanOrEqualTo(5f), "Colores naturales, sin saturar.");
            Assert.That(color.postExposure.value, Is.LessThanOrEqualTo(0f));
            Assert.That(profile.TryGet<Tonemapping>(out var tonemapping) && tonemapping.mode.value != TonemappingMode.None, Is.True,
                "Las luces fuertes se suavizan en vez de quemarse.");
        }

        [Test]
        public void BetPlatforms_StandToTheRightOfTheirBooth_WithoutCoveringIt()
        {
            var platforms = new List<Bounds>();
            for (var booth = 0; booth < 4; booth++)
            {
                var root = Find($"Booths/Booth_{(char)('A' + booth)}").transform;
                var zone = root.Find("BetZone").position;
                // Mirando la cabina desde el centro del plató, su derecha.
                var towardsCenter = root.forward;
                var right = Vector3.Cross(Vector3.up, -towardsCenter);
                var offset = zone - root.position;
                Assert.That(Vector3.Dot(offset, right), Is.GreaterThan(2.2f), $"{root.name}: plataforma a la derecha de la cabina.");
                Assert.That(Mathf.Abs(Vector3.Dot(offset, towardsCenter)), Is.LessThan(1.2f), $"{root.name}: al lado, no delante.");

                var platform = BoundsOf(root.Find("Platform").gameObject);
                var cabinet = BoundsOf(root.Find("Body").gameObject);
                cabinet.Expand(new Vector3(0f, 10f, 0f));
                Assert.That(platform.Intersects(cabinet), Is.False, $"{root.name}: no se mete en el mueble.");
                platforms.Add(platform);

                // Desde el centro del plató, el mueble se ve entero: la plataforma no se cruza en medio.
                var eye = new Vector3(0f, 1.7f, root.position.z);
                var ray = new Ray(eye, (cabinet.center - eye).normalized);
                Assert.That(platform.IntersectRay(ray), Is.False, $"{root.name}: nada tapa la cabina desde el centro.");
            }
            for (var booth = 0; booth < 4; booth++)
            {
                var root = Find($"Booths/Booth_{(char)('A' + booth)}").transform;
                foreach (var other in new[] { "A", "B", "C", "D" })
                {
                    var cabinet = BoundsOf(Find($"Booths/Booth_{other}/Body"));
                    Assert.That(platforms[booth].Intersects(cabinet), Is.False, $"La plataforma de {root.name} no choca con la cabina {other}.");
                }
            }
        }

        [Test]
        public void BetPlatformRailing_Blocks_ExceptTheFrontOpening()
        {
            for (var booth = 0; booth < 4; booth++)
            {
                var root = Find($"Booths/Booth_{(char)('A' + booth)}").transform;
                var zone = root.Find("BetZone");
                var rails = root.Find("RailColliders").GetComponentsInChildren<BoxCollider>();
                Assert.That(rails.Length, Is.GreaterThanOrEqualTo(6), $"{root.name}: la barandilla tiene colisión.");
                foreach (var rail in rails)
                    Assert.That(rail.bounds.max.y, Is.GreaterThanOrEqualTo(1.3f), "Más alta que un salto desde la plataforma.");

                for (var angle = -180; angle < 180; angle += 5)
                {
                    var direction = root.rotation * Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                    var blocked = false;
                    for (var d = 0.6f; d <= 1.6f && !blocked; d += 0.05f)
                    {
                        var point = zone.position + Vector3.up * 0.6f + direction * d;
                        blocked = rails.Any(r => Inside(r, point));
                    }
                    if (Mathf.Abs(angle) <= 35)
                        Assert.That(blocked, Is.False, $"{root.name} a {angle}°: la abertura hacia el centro queda libre.");
                    else if (Mathf.Abs(angle) >= 55)
                        Assert.That(blocked, Is.True, $"{root.name} a {angle}°: la barandilla no deja pasar.");
                }
            }
        }

        private static bool Inside(BoxCollider box, Vector3 world)
        {
            var local = box.transform.InverseTransformPoint(world) - box.center;
            var half = box.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        [Test]
        public void EachTube_KnowsItsPath_ItsAir_AndItsSound()
        {
            var ceiling = Find($"{ENVIRONMENT}/Ceiling").GetComponent<Renderer>().bounds.min.y;
            for (var booth = 0; booth < 4; booth++)
            {
                var root = Find($"Booths/Booth_{(char)('A' + booth)}").transform;
                var cabinetTop = BoundsOf(root.Find("Body").gameObject).max.y;
                foreach (var tube in root.GetComponentsInChildren<Trivias.Shared.Tubes.TubePath>())
                {
                    Assert.That(tube.Booth, Is.EqualTo(booth), $"{root.name}/{tube.name}: sabe de qué cabina es.");
                    tube.Sample(0f, out var start, out _);
                    tube.Sample(tube.Length, out var end, out _);
                    Assert.That(end.y, Is.EqualTo(ceiling).Within(0.2f), $"{tube.name}: acaba en el techo.");
                    if (tube.Kind == Trivias.Shared.Tubes.TubeKind.Money)
                        Assert.That(start.y, Is.EqualTo(cabinetTop).Within(0.15f), "El del dinero empieza en el techo del mueble.");
                    else
                        Assert.That(start.y, Is.InRange(3f, 3.8f), "El del concursante empieza en su boca, sobre la plataforma.");

                    var air = tube.GetComponentInChildren<ParticleSystem>(true);
                    Assert.That(air, Is.Not.Null, "Aire que entra al succionar.");
                    Assert.That(air.main.loop, Is.False, "Una ráfaga por succión.");
                    Assert.That(air.main.playOnAwake, Is.False);
                    var audio = tube.GetComponentInChildren<AudioSource>(true);
                    Assert.That(audio.clip, Is.Not.Null, "Suena la succión.");
                    Assert.That(audio.playOnAwake, Is.False);
                }
                Assert.That(root.GetComponentsInChildren<Trivias.Shared.Tubes.TubePath>().Length, Is.EqualTo(2), "Dinero y concursante.");
            }
        }

        [Test]
        public void TubePaths_FlowSmoothly_WithoutSharpCorners()
        {
            var tube = Find("Booths/Booth_C/MoneyTube").GetComponent<Trivias.Shared.Tubes.TubePath>();
            tube.Sample(0f, out _, out var previous);
            for (var d = 0.1f; d < tube.Length; d += 0.1f)
            {
                tube.Sample(d, out _, out var direction);
                Assert.That(Vector3.Angle(previous, direction), Is.LessThan(20f), $"A {d:0.0} m gira de golpe: el codo debe ser curvo.");
                previous = direction;
            }
        }

        [Test]
        public void StageShow_HasItsBulbsAudienceCamerasAndFixtures()
        {
            var show = Find("StageShow");
            Assert.That(show, Is.Not.Null);
            var bulbs = show.GetComponent<Trivias.Feedback.Components.StageBulbs>();
            var perBooth = new int[4];
            var marquee = 0;
            for (var i = 0; i < bulbs.Count; i++)
            {
                if (bulbs.BoothOf(i) >= 0) perBooth[bulbs.BoothOf(i)]++;
                else marquee++;
            }
            foreach (var count in perBooth)
                Assert.That(count, Is.GreaterThanOrEqualTo(10), "Las bombillas de cada cabina.");
            Assert.That(marquee, Is.GreaterThanOrEqualTo(12), "Las de la marquesina de la pantalla.");

            var figures = (Transform[])typeof(Trivias.Feedback.Components.AudienceCrowd)
                .GetField("_figures", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(show.GetComponent<Trivias.Feedback.Components.AudienceCrowd>());
            Assert.That(figures.Length, Is.GreaterThanOrEqualTo(40));
            foreach (var moving in figures.Concat(Moving(show, typeof(Trivias.Feedback.Components.StudioCameraOperators), "_cameras"))
                         .Concat(Moving(show, typeof(Trivias.Feedback.Components.FixtureSway), "_fixtures")))
            {
                foreach (var part in moving.GetComponentsInChildren<Transform>())
                    Assert.That(part.gameObject.isStatic, Is.False, $"{part.name} se mueve: no puede ser estático (static batching).");
            }
            Assert.That(Moving(show, typeof(Trivias.Feedback.Components.StudioCameraOperators), "_cameras").Length, Is.EqualTo(4));
        }

        private static Transform[] Moving(GameObject show, System.Type component, string field) =>
            (Transform[])component.GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(show.GetComponent(component));

        [Test]
        public void StageTexts_UseTheStageFonts()
        {
            var texts = _roots.SelectMany(r => r.GetComponentsInChildren<TMPro.TMP_Text>(true)).ToList();
            Assert.That(texts.Count, Is.GreaterThan(20));
            foreach (var text in texts)
                Assert.That(text.font.name, Does.StartWith("Poppins").Or.StartWith("LilitaOne"), $"{text.name}: tipografía del plató.");
            Assert.That(Find($"{SCREEN}/Display/Message").GetComponent<TMPro.TMP_Text>().font.name, Does.StartWith("LilitaOne"), "El logo, con Lilita One.");
            var body = Find($"{SCREEN}/Display/Content/Question").GetComponent<TMPro.TMP_Text>().font;
            Assert.That(body.atlasPopulationMode, Is.EqualTo(TMPro.AtlasPopulationMode.Static), "Tabla fija: no cambia al jugar.");
            foreach (var c in "¿¡áéíóúñÑü€«»…")
                Assert.That(body.HasCharacter(c), Is.True, $"La fuente tiene «{c}».");
            Assert.That(body.fontWeightTable[7].regularTypeface, Is.Not.Null, "Negrita real (Poppins ExtraBold).");
        }

        // ---- Utilidades ----

        /// <summary>Caja que envuelve todas las piezas visibles de un objeto (los modelos tienen muchas).</summary>
        /// <summary>Volumen de las mallas (sin partículas: el aire de los tubos no ocupa sitio mientras no sopla).</summary>
        private static Bounds BoundsOf(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        /// <summary>Archivo del modelo de la primera malla del objeto.</summary>
        private static string ModelPath(Transform model) =>
            UnityEditor.AssetDatabase.GetAssetPath(model.GetComponentInChildren<MeshFilter>(true).sharedMesh);

        [Test]
        public void SpectatorBanner_IsHidden_AtTheBottom_AndDoesNotBlockClicks()
        {
            var spectator = Find("Spectator");
            Assert.That(spectator, Is.Not.Null, "Falta el espectador en la escena.");
            Assert.That(spectator.GetComponent<Trivias.Spectator.Components.SpectatorController>(), Is.Not.Null);
            Assert.That(spectator.GetComponent<UnityEngine.UI.GraphicRaycaster>(), Is.Null, "El rótulo no debe capturar clics.");

            var bottom = (RectTransform)spectator.transform.Find("Bottom");
            var banner = (RectTransform)bottom.Find("Banner");
            Assert.That(banner.gameObject.activeSelf, Is.False, "Solo aparece al quedar eliminado.");
            Assert.That(bottom.anchorMin.y, Is.Zero, "Pegada al borde de abajo…");
            Assert.That(-((RectTransform)banner.Find("Ribbon")).anchoredPosition.y, Is.GreaterThan(1080f * 0.7f), "…sin tapar la pantalla de preguntas.");
            foreach (var graphic in banner.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                Assert.That(graphic.raycastTarget, Is.False, graphic.name);
        }

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
