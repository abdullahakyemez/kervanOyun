using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kervan.TradeAndTravel
{
    /// <summary>
    /// Dünya Haritası üzerindeki bir şehir veya kervansaray düğümünü (UI Button)
    /// kendi <see cref="LocationNodeSO"/> verisine Inspector referansıyla bağlayan görünüm bileşeni.
    /// İsimle arama (GameObject.name) yerine ASCII <see cref="NodeId"/> kimliği sağlar.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class MapNodeView : MonoBehaviour
    {
        [Header("Düğüm Verisi (ScriptableObject)")]
        [Tooltip("Bu harita düğümünün temsil ettiği şehir veya kervansaray verisi (LocationNodeSO)")]
        [SerializeField] private LocationNodeSO _nodeData;

        private Button _button;
        private RectTransform _rectTransform;

        /// <summary>
        /// Düğüme tıklandığında tetiklenen olay.
        /// </summary>
        public event Action<MapNodeView> Clicked;

        /// <summary>
        /// Düğüme bağlı ScriptableObject verisi.
        /// </summary>
        public LocationNodeSO NodeData => _nodeData;

        /// <summary>
        /// Düğümün benzersiz ASCII kimliği (ör. "node_sogut").
        /// </summary>
        public string NodeId => _nodeData != null ? _nodeData.NodeId : string.Empty;

        /// <summary>
        /// Düğümün kullanıcıya gösterilecek Türkçe adı (ör. "Söğüt").
        /// </summary>
        public string DisplayName => _nodeData != null ? _nodeData.NodeName : gameObject.name;

        /// <summary>
        /// Düğümün harita üzerindeki RectTransform bileşeni.
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            _rectTransform = GetComponent<RectTransform>();

            if (_nodeData == null)
            {
                Debug.LogError($"[MapNodeView] '{gameObject.name}' objesinde LocationNodeSO referansı (_nodeData) atanmamış! Lütfen Inspector'dan ilgili SO varlığını sürükleyin.", this);
            }
            else if (string.IsNullOrWhiteSpace(_nodeData.NodeId))
            {
                Debug.LogError($"[MapNodeView] '{gameObject.name}' objesine bağlı '{_nodeData.name}' varlığında NodeId alanı boş!", this);
            }
        }

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(HandleButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleButtonClicked);
            }
        }

        private void HandleButtonClicked()
        {
            if (_nodeData == null)
            {
                Debug.LogError($"[MapNodeView] '{gameObject.name}' düğümüne tıklandı ancak LocationNodeSO referansı boş!", this);
                return;
            }

            Clicked?.Invoke(this);
        }
    }
}
