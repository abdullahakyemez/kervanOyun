using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.SceneManagement;
using Kervan.Core;
#endif

namespace Kervan.DevTools
{
    /// <summary>
    /// Test ve geliştirme sürecinde sahneler arasında (Savaş, Seyahat, Pazar, Divan)
    /// tek tıkla geçiş yapmayı sağlayan geliştirici arayüzü.
    /// Yalnızca Editör ve Development Build sürümlerinde aktif olur.
    /// </summary>
    public class KervanSceneNavOverlay : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static bool _initialized = false;
        private bool _isCollapsed = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            if (_initialized) return;
            _initialized = true;

            var go = new GameObject("[Kervan_TestNavOverlay]");
            go.AddComponent<KervanSceneNavOverlay>();
            DontDestroyOnLoad(go);
        }

        private void OnGUI()
        {
            // Mobil veya Editör ekranının sağ üst köşesi
            float areaWidth = _isCollapsed ? 110 : 490;
            float areaHeight = 55;
            float posX = Screen.width - areaWidth - 15;
            float posY = 15;

            GUILayout.BeginArea(new Rect(posX, posY, areaWidth, areaHeight));
            GUILayout.BeginHorizontal("box");

            if (_isCollapsed)
            {
                if (GUILayout.Button("🛠️ Menü", GUILayout.Height(38), GUILayout.Width(95)))
                {
                    _isCollapsed = false;
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0.85f, 0.45f, 0.20f);
                if (GUILayout.Button("⚔️ Savaş", GUILayout.Height(38)))
                {
                    SceneLoader.Load(SceneIds.Combat);
                }

                GUI.backgroundColor = new Color(0.25f, 0.55f, 0.35f);
                if (GUILayout.Button("🐪 Seyahat", GUILayout.Height(38)))
                {
                    SceneLoader.Load(SceneIds.Travel);
                }

                GUI.backgroundColor = new Color(0.85f, 0.70f, 0.20f);
                if (GUILayout.Button("⚖️ Pazar", GUILayout.Height(38)))
                {
                    SceneLoader.Load(SceneIds.Market);
                }

                GUI.backgroundColor = new Color(0.35f, 0.45f, 0.75f);
                if (GUILayout.Button("🏛️ Divan", GUILayout.Height(38)))
                {
                    SceneLoader.Load(SceneIds.TownHub);
                }

                GUI.backgroundColor = Color.gray;
                if (GUILayout.Button("✖", GUILayout.Height(38), GUILayout.Width(35)))
                {
                    _isCollapsed = true;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
#endif
    }
}
