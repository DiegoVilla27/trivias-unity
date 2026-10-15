using System.IO;
using System.Linq;
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
