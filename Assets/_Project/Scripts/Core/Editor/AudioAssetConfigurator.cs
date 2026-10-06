#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using Kervan.Audio;

namespace Kervan.EditorTools
{
    public static class AudioAssetConfigurator
    {
        [MenuItem("Kervan/Varlıklar/Ses Kütüphanesini ve Yöneticisini Kur")]
        public static void SetupAudioSystem()
        {
            string audioSoFolder = "Assets/_Project/ScriptableObjects/Audio";
            if (!Directory.Exists(audioSoFolder))
            {
                Directory.CreateDirectory(audioSoFolder);
            }

            string soPath = $"{audioSoFolder}/AudioLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<AudioDataSO>(soPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioDataSO>();
                AssetDatabase.CreateAsset(library, soPath);
            }

            // Dosyaları tara ve bağla
            ScanAndLinkAudioFiles(library);

            // AudioManager Prefab'ı oluştur
            CreateAudioManagerPrefab(library);

            // Sahneleri bağla
            AttachAudioToAllScenesInternal();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AudioConfig] Ses sistemi, kütüphanesi ve tüm sahneler başarıyla yapılandırıldı.");
        }

        [MenuItem("Kervan/Varlıklar/Tüm Sahneler İçin Ses Sistemini Bağla")]
        public static void AttachAudioToAllScenes()
        {
            SetupAudioSystem();
        }

        private static void AttachAudioToAllScenesInternal()
        {
            string prefabPath = "Assets/_Project/Prefabs/Audio/AudioManager.prefab";
            var audioMgrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var sceneConfigs = new (string scenePath, SceneMusicType musicType)[]
            {
                ("Assets/_Project/Scenes/CombatScene.unity", SceneMusicType.Combat),
                ("Assets/_Project/Scenes/TravelScene.unity", SceneMusicType.Travel),
                ("Assets/_Project/Scenes/MarketScene.unity", SceneMusicType.Market),
                ("Assets/_Project/Scenes/TownHubScene.unity", SceneMusicType.TownHub),
            };

            foreach (var (scenePath, musicType) in sceneConfigs)
            {
                if (!File.Exists(scenePath)) continue;

                var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

                var existingMgr = Object.FindFirstObjectByType<AudioManager>();
                if (existingMgr == null && audioMgrPrefab != null)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(audioMgrPrefab);
                    instance.name = "AudioManager";
                }

                var existingPlayer = Object.FindFirstObjectByType<SceneAudioPlayer>();
                if (existingPlayer == null)
                {
                    var playerGo = new GameObject("SceneAudioPlayer");
                    existingPlayer = playerGo.AddComponent<SceneAudioPlayer>();
                }

                var soPlayer = new SerializedObject(existingPlayer);
                var musicProp = soPlayer.FindProperty("_musicType");
                if (musicProp != null)
                {
                    musicProp.enumValueIndex = (int)musicType;
                    soPlayer.ApplyModifiedPropertiesWithoutUndo();
                }

                // Sahnelerdeki tüm butonlara dokunma ses efekti (UIButtonSound) ekle
                var buttons = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
                foreach (var btn in buttons)
                {
                    if (btn.GetComponent<UIButtonSound>() == null)
                    {
                        btn.gameObject.AddComponent<UIButtonSound>();
                    }
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                Debug.Log($"[AudioConfig] Sahne güncellendi: {scenePath} (Müzik: {musicType}, Butonlar: {buttons.Length})");
            }
        }

        private static void ScanAndLinkAudioFiles(AudioDataSO library)
        {
            SerializedObject so = new SerializedObject(library);

            // Müzikler
            LinkClip(so, "_bgmCombat", "Assets/_Project/Audio/Music", "BGM_Combat");
            LinkClip(so, "_bgmTravel", "Assets/_Project/Audio/Music", "BGM_Travel");
            LinkClip(so, "_bgmMarket", "Assets/_Project/Audio/Music", "BGM_Market");
            LinkClip(so, "_bgmTownHub", "Assets/_Project/Audio/Music", "BGM_TownHub");

            // Efektler
            LinkClip(so, "_sfxSwordHit", "Assets/_Project/Audio/SFX", "SFX_SwordHit");
            LinkClip(so, "_sfxArrowShot", "Assets/_Project/Audio/SFX", "SFX_ArrowShot");
            LinkClip(so, "_sfxShieldBlock", "Assets/_Project/Audio/SFX", "SFX_ShieldBlock");
            LinkClip(so, "_sfxVictory", "Assets/_Project/Audio/SFX", "SFX_Victory");
            LinkClip(so, "_sfxDefeat", "Assets/_Project/Audio/SFX", "SFX_Defeat");

            LinkClip(so, "_sfxCoins", "Assets/_Project/Audio/SFX", "SFX_Coins");
            LinkClip(so, "_sfxParchment", "Assets/_Project/Audio/SFX", "SFX_Parchment");
            LinkClip(so, "_sfxButtonClick", "Assets/_Project/Audio/SFX", "SFX_ButtonClick");
            LinkClip(so, "_sfxTravelStep", "Assets/_Project/Audio/SFX", "SFX_TravelStep");

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
        }

        private static void LinkClip(SerializedObject so, string propertyName, string folder, string baseName)
        {
            var prop = so.FindProperty(propertyName);
            if (prop == null) return;

            string[] extensions = new string[] { ".wav", ".mp3", ".ogg" };
            foreach (var ext in extensions)
            {
                string fullPath = $"{folder}/{baseName}{ext}";
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(fullPath);
                if (clip != null)
                {
                    prop.objectReferenceValue = clip;
                    Debug.Log($"[AudioConfig] Ses bağlandı: {baseName}{ext} -> {propertyName}");
                    return;
                }
            }
        }

        private static void CreateAudioManagerPrefab(AudioDataSO library)
        {
            string prefabFolder = "Assets/_Project/Prefabs/Audio";
            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
            }

            string prefabPath = $"{prefabFolder}/AudioManager.prefab";
            var go = new GameObject("AudioManager");
            var mgr = go.AddComponent<AudioManager>();
            SetSerializedRef(mgr, "_library", library);

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"[AudioConfig] AudioManager prefabı kaydedildi: {prefabPath}");
        }

        private static void SetSerializedRef(Object target, string propName, Object val)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty prop = so.FindProperty(propName);
            if (prop != null)
            {
                prop.objectReferenceValue = val;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
#endif
