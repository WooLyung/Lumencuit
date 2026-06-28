using TMPro;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 블루프린트 UI 오브젝트입니다.
    /// </summary>
    public class BlueprintUI : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TextMeshProUGUI text;
        private InGameUIAdaptor inGameUIAdaptor = null;

        public EntityBlueprint Blueprint = null;
        public RectTransform RectTransform => rectTransform;
        public TextMeshProUGUI Text => text;

        public void SetInGameUIAdaptor(InGameUIAdaptor inGameUIAdaptor)
        {
            this.inGameUIAdaptor = inGameUIAdaptor;
        }

        public void Click()
        {
            inGameUIAdaptor.SelectBlueprint(Blueprint);
        }
    }
}
