using System.IO;
using System.Linq;
using TMPro;
using Trivias.Feedback.Components;
using Trivias.Feedback.Data;
using Trivias.Feedback.Logic;
using Trivias.Match.Data;
using Trivias.Money.Data;
using Trivias.SelfBet.Data;
using Trivias.Shared.Editor;
using UnityEditor;
using UnityEngine;

namespace Trivias.Feedback.Editor
{
    /// <summary>
    /// Construye el sonido y las celebraciones: sintetiza los .wav (tiempo de edición, coste 0 en juego), la biblioteca,
    /// el reproductor de sonido y la lluvia de confeti de la victoria.
    /// </summary>
    public static class FeedbackBuilder
    {
        public const string MODULE_ROOT = "Assets/_Project/Modules/Feedback";
        public const string LIBRARY_PATH = MODULE_ROOT + "/Config/FeedbackLibrary.asset";
        private const string AUDIO = MODULE_ROOT + "/Audio";
        private const string PREFABS = MODULE_ROOT + "/Prefabs";
        private const string CONFETTI_SHADER = "Universal Render Pipeline/Particles/Unlit";

        /// <summary>Volumen de cada sonido (los repetitivos, más bajos).</summary>
        private static float CueVolume(FeedbackCue cue) => cue switch
        {
            FeedbackCue.ClockTick => 0.45f,
            FeedbackCue.MoneyPick => 0.6f,
            FeedbackCue.MoneyDrop => 0.7f,
            FeedbackCue.CountdownBeep => 0.6f,
            _ => 0.9f,
        };

        public static FeedbackLibrary BuildLibrary()
        {
            var cues = new AudioClip[FeedbackCues.COUNT];
            var volumes = new float[FeedbackCues.COUNT];
            for (var i = 0; i < FeedbackCues.COUNT; i++)
            {
                var cue = (FeedbackCue)i;
                var synth = SoundRecipes.Cue(cue);
                cues[i] = WriteClip($"{AUDIO}/Sfx/{cue}.wav", synth.Normalized(), synth.SampleRate);
                volumes[i] = CueVolume(cue);
            }

            var music = new AudioClip[FeedbackRules.MUSIC_BLOCKS];
            for (var block = 0; block < music.Length; block++)
            {
                var synth = SoundRecipes.Music(block);
                music[block] = WriteClip($"{AUDIO}/Music/Block{block + 1}.wav", synth.Normalized(0.8f), synth.SampleRate);
            }

            var library = EditorAssets.GetOrCreateAsset<FeedbackLibrary>(LIBRARY_PATH);
            library.EditorSet(cues, volumes, music);
            EditorUtility.SetDirty(library);
            return library;
        }

        public static GameObject BuildPlayer(FeedbackLibrary library, MatchRuntime match, MoneyRuntime money, SelfBetRuntime selfBet)
        {
            var root = new GameObject("GameFeedback");
            var sfx = AddSource(root, "Sfx", loop: false);
            var musicA = AddSource(root, "MusicA", loop: true);
            var musicB = AddSource(root, "MusicB", loop: true);

            var player = root.AddComponent<GameFeedbackPlayer>();
            SerializedWiring.Set(player, "_library", library);
            SerializedWiring.Set(player, "_match", match);
            SerializedWiring.Set(player, "_money", money);
            SerializedWiring.Set(player, "_selfBet", selfBet);
            SerializedWiring.Set(player, "_settings", SharedAssets.GameSettings());
            SerializedWiring.Set(player, "_sfx", sfx);
            SerializedWiring.SetArray(player, "_musicSources", new Object[] { musicA, musicB });
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/GameFeedback.prefab");
        }

        /// <summary>Pantalla grande del plató (StageEnvironmentPrefabBuilder): centro y medio ancho.</summary>
        private static readonly Vector3 ScreenCenter = new(0f, 7.6f, 15f);
        private const float SCREEN_HALF_WIDTH = 7.1f;

        private static readonly Color[] PartyColors =
        {
            new(0.90f, 0.25f, 0.35f), new(0.35f, 0.55f, 1f), new(0.40f, 0.90f, 0.55f), new(0.75f, 0.40f, 0.95f), Color.white,
        };

        private static readonly Color[] GoldColors =
        {
            new(1f, 0.84f, 0.30f), new(0.95f, 0.70f, 0.22f), new(1f, 0.93f, 0.60f), new(0.85f, 0.60f, 0.15f), new(1f, 0.97f, 0.85f),
        };

        /// <summary>
        /// Confeti del plató: dos chorros de colores a los lados de la pantalla (cada acierto) y una lluvia dorada que
        /// cubre toda la sala (victoria).
        /// </summary>
        public static GameObject BuildStageConfetti(MatchRuntime match)
        {
            var root = new GameObject("StageConfetti");
            var material = ConfettiMaterial();

            var bursts = new ParticleSystem[2];
            for (var side = 0; side < 2; side++)
            {
                var sign = side == 0 ? -1f : 1f;
                var position = new Vector3(sign * (SCREEN_HALF_WIDTH + 0.7f), ScreenCenter.y - 3.4f, ScreenCenter.z - 0.4f);
                // Hacia arriba, un poco hacia el centro y hacia el público.
                var direction = new Vector3(-sign * 0.35f, 1f, -0.3f);
                var burst = CreateParticles(root.transform, side == 0 ? "CorrectLeft" : "CorrectRight", position,
                    Quaternion.LookRotation(direction), material, PartyColors);

                var main = burst.main;
                main.loop = false;
                main.duration = 1f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 4.5f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(7f, 11f);
                main.gravityModifier = 0.5f;
                main.maxParticles = 400;

                var emission = burst.emission;
                emission.rateOverTime = 0f;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 160), new ParticleSystem.Burst(0.25f, 80) });

                var shape = burst.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 18f;
                shape.radius = 0.3f;
                bursts[side] = burst;
            }

            var rain = CreateParticles(root.transform, "VictoryRain", new Vector3(0f, 12f, 0f), Quaternion.identity, material, GoldColors);
            var rainMain = rain.main;
            rainMain.loop = true;
            rainMain.duration = 5f;
            rainMain.startLifetime = new ParticleSystem.MinMaxCurve(6f, 8f);
            rainMain.startSpeed = new ParticleSystem.MinMaxCurve(0f, 1f);
            rainMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
            rainMain.gravityModifier = 0.08f;
            rainMain.maxParticles = 5000;

            var rainEmission = rain.emission;
            rainEmission.rateOverTime = 450f;
            rainEmission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1500) });

            var rainShape = rain.shape;
            rainShape.shapeType = ParticleSystemShapeType.Box;
            rainShape.scale = new Vector3(42f, 0.2f, 42f); // Todo el suelo del plató (44 m).

            var confetti = root.AddComponent<StageConfetti>();
            SerializedWiring.Set(confetti, "_match", match);
            SerializedWiring.SetArray(confetti, "_correctBursts", bursts);
            SerializedWiring.Set(confetti, "_victoryRain", rain);
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/StageConfetti.prefab");
        }

        /// <summary>Partículas de confeti: cuadraditos que giran y revolotean al caer.</summary>
        private static ParticleSystem CreateParticles(Transform parent, string name, Vector3 position, Quaternion rotation,
            Material material, Color[] colors)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.playOnAwake = false;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(Palette(colors)) { mode = ParticleSystemGradientMode.RandomColor };

            var spin = particles.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-4f, 4f);

            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = 0.6f;
            noise.frequency = 0.5f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;
            return particles;
        }

        /// <summary>Degradado de colores fijos: cada partícula toma uno al azar.</summary>
        private static Gradient Palette(Color[] colors)
        {
            var gradient = new Gradient { mode = GradientMode.Fixed };
            gradient.SetKeys(
                colors.Select((c, i) => new GradientColorKey(c, (i + 1f) / colors.Length)).ToArray(),
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }

        // ---- Podio de la victoria ----

        public static readonly Vector3 PodiumPosition = new(0f, 0f, 12f);
        public const float PODIUM_TOP = 1.35f;
        private const float PLAQUE_SPACING = 1.5f;

        /// <summary>Podio dorado delante de la pantalla con 6 placas; oculto hasta la victoria y sin colisiones.</summary>
        public static GameObject BuildPodium(MatchRuntime match)
        {
            var root = new GameObject("VictoryPodium");
            root.transform.position = PodiumPosition;
            var podium = EditorAssets.CreateEmpty("Podium", root.transform, Vector3.zero);
            var lift = EditorAssets.CreateEmpty("Lift", podium.transform, Vector3.zero).transform;

            var gold = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Podium_Gold.mat", new Color(0.95f, 0.72f, 0.25f), 0.75f, 0.9f);
            var dark = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Podium_Dark.mat", new Color(0.08f, 0.08f, 0.2f), 0.5f);
            var survivor = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Plaque_Survivor.mat", new Color(0.80f, 0.55f, 0.12f), 0.7f, 0.8f);
            var eliminated = EditorAssets.GetOrCreateMaterial($"{MODULE_ROOT}/Materials/Plaque_Eliminated.mat", new Color(0.32f, 0.32f, 0.36f), 0.3f);

            Part(lift, PrimitiveType.Cube, "Base", new Vector3(0f, 0.45f, 0f), new Vector3(9.4f, 0.9f, 2.2f), dark);
            Part(lift, PrimitiveType.Cube, "Trim", new Vector3(0f, 0.92f, 0f), new Vector3(9.5f, 0.06f, 2.3f), gold);
            Part(lift, PrimitiveType.Cube, "Top", new Vector3(0f, 1.13f, 0.1f), new Vector3(5.6f, 0.44f, 1.6f), gold);
            var title = EditorAssets.CreateWorldText("Title", lift, "¡CAMPEONES!", 3.2f, new Color(0.08f, 0.08f, 0.2f),
                new Vector3(0f, 1.13f, -0.72f), Quaternion.identity, new Vector2(5.4f, 0.45f));
            title.fontStyle = FontStyles.Bold;

            var plaques = new GameObject[PodiumRules.MAX_PLAQUES];
            var names = new TextMeshPro[PodiumRules.MAX_PLAQUES];
            var renderers = new Renderer[PodiumRules.MAX_PLAQUES];
            for (var i = 0; i < plaques.Length; i++)
            {
                var x = (i - (plaques.Length - 1) * 0.5f) * PLAQUE_SPACING;
                var plaque = EditorAssets.CreateEmpty($"Plaque{i + 1}", lift, new Vector3(x, 0.45f, -1.12f));
                var board = Part(plaque.transform, PrimitiveType.Cube, "Board", Vector3.zero, new Vector3(1.38f, 0.5f, 0.04f), survivor);
                var label = EditorAssets.CreateWorldText("Name", plaque.transform, $"Jugador {i + 1}", 2.2f, Color.white,
                    new Vector3(0f, 0f, -0.04f), Quaternion.identity, new Vector2(1.3f, 0.45f));
                label.fontStyle = FontStyles.Bold;
                label.enableAutoSizing = true;
                label.fontSizeMin = 1f;
                label.fontSizeMax = 2.2f;
                label.outlineWidth = 0.2f;
                label.outlineColor = Color.black;
                plaques[i] = plaque;
                names[i] = label;
                renderers[i] = board.GetComponent<Renderer>();
            }

            var view = root.AddComponent<VictoryPodium>();
            SerializedWiring.Set(view, "_match", match);
            SerializedWiring.Set(view, "_podium", podium);
            SerializedWiring.Set(view, "_lift", lift);
            SerializedWiring.SetArray(view, "_plaques", plaques);
            SerializedWiring.SetArray(view, "_plaqueNames", names);
            SerializedWiring.SetArray(view, "_plaqueRenderers", renderers);
            SerializedWiring.Set(view, "_survivorMaterial", survivor);
            SerializedWiring.Set(view, "_eliminatedMaterial", eliminated);
            podium.SetActive(false);
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/VictoryPodium.prefab");
        }

        private static GameObject Part(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material) =>
            EditorAssets.CreatePrimitive(type, name, parent, position, scale, material, 0, false, keepCollider: false);

        // ---- Ambiente de luz ----

        public const string MOODS_PATH = MODULE_ROOT + "/Config/StageMoodConfig.asset";
        private static readonly Vector3 SpotTarget = new(0f, 0f, 5f);
        private static readonly Vector3[] SpotPositions =
        {
            new(-10f, 11f, 1f), new(10f, 11f, 1f), new(-6f, 11.5f, 12f), new(6f, 11.5f, 12f),
        };

        /// <summary>Valores de fábrica de cada ambiente (luego se ajustan en el asset sin tocar código).</summary>
        public static StageMoodConfig BuildMoods()
        {
            var moods = new MoodLight[StageMoods.COUNT];
            moods[(int)StageMood.Lobby] = new MoodLight(new Color(1f, 0.95f, 0.85f), 30f, Color.white, 1f);
            moods[(int)StageMood.Easy] = new MoodLight(new Color(1f, 0.72f, 0.32f), 55f, new Color(1f, 0.95f, 0.85f), 1f);
            moods[(int)StageMood.Medium] = new MoodLight(new Color(0.55f, 0.42f, 1f), 60f, new Color(0.85f, 0.8f, 1f), 0.8f);
            moods[(int)StageMood.Hard] = new MoodLight(new Color(1f, 0.22f, 0.2f), 65f, new Color(0.7f, 0.55f, 0.6f), 0.55f);
            moods[(int)StageMood.Countdown] = new MoodLight(new Color(1f, 0.86f, 0.6f), 55f, new Color(0.65f, 0.6f, 0.6f), 0.5f);
            moods[(int)StageMood.Correct] = new MoodLight(new Color(0.3f, 1f, 0.45f), 70f, new Color(0.85f, 1f, 0.85f), 0.9f);
            moods[(int)StageMood.Wrong] = new MoodLight(new Color(1f, 0.15f, 0.15f), 70f, new Color(0.6f, 0.45f, 0.45f), 0.5f);
            moods[(int)StageMood.Victory] = new MoodLight(new Color(1f, 0.82f, 0.3f), 85f, new Color(1f, 0.9f, 0.7f), 1.1f);
            moods[(int)StageMood.Defeat] = new MoodLight(new Color(0.3f, 0.4f, 0.8f), 25f, new Color(0.5f, 0.5f, 0.6f), 0.4f);

            var config = EditorAssets.GetOrCreateAsset<StageMoodConfig>(MOODS_PATH);
            if (config.Count != StageMoods.COUNT || config.For(StageMood.Lobby).SpotIntensity <= 0f)
            {
                config.EditorSet(moods); // Solo la primera vez: si se ajustó a mano, se respeta.
                EditorUtility.SetDirty(config);
            }
            return config;
        }

        public static GameObject BuildStageLighting(MatchRuntime match, StageMoodConfig moods)
        {
            var root = new GameObject("StageLighting");
            var spots = new Light[SpotPositions.Length];
            for (var i = 0; i < spots.Length; i++)
            {
                var go = new GameObject($"Spot{i + 1}");
                go.transform.SetParent(root.transform, false);
                go.transform.SetPositionAndRotation(SpotPositions[i], Quaternion.LookRotation(SpotTarget - SpotPositions[i]));
                var spot = go.AddComponent<Light>();
                spot.type = LightType.Spot;
                spot.spotAngle = 60f;
                spot.innerSpotAngle = 35f;
                spot.range = 30f;
                spot.shadows = LightShadows.None;
                spot.color = moods.For(StageMood.Lobby).SpotColor;
                spot.intensity = moods.For(StageMood.Lobby).SpotIntensity;
                spots[i] = spot;
            }

            var lighting = root.AddComponent<StageLighting>();
            SerializedWiring.Set(lighting, "_match", match);
            SerializedWiring.Set(lighting, "_moods", moods);
            SerializedWiring.SetArray(lighting, "_spots", spots);
            return EditorAssets.SaveAsPrefab(root, $"{PREFABS}/StageLighting.prefab");
        }

        private static Material ConfettiMaterial()
        {
            const string path = MODULE_ROOT + "/Materials/Confetti.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            EditorAssets.EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            material = new Material(Shader.Find(CONFETTI_SHADER)) { name = "Confetti" };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static AudioSource AddSource(GameObject root, string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root.transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f; // Sonido del concurso: se oye igual en todo el plató.
            source.volume = loop ? 0f : 1f;
            return source;
        }

        /// <summary>Escribe el .wav solo si cambió (así los builds repetidos no tocan los archivos) y lo importa.</summary>
        private static AudioClip WriteClip(string path, float[] samples, int sampleRate)
        {
            EditorAssets.EnsureFolder(Path.GetDirectoryName(path)?.Replace('\\', '/'));
            var bytes = WavEncoder.Encode(samples, sampleRate);
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes))
            {
                File.WriteAllBytes(path, bytes);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
