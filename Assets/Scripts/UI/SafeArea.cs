using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class SafeArea : UIBehaviour
    {
        [SerializeField] protected Color m_SkyBoxColor;

        protected override void Awake()
        {
            base.Awake();
            ChangeSafeArea();
            ChangeSkyBoxColor();
        }

        private void ChangeSkyBoxColor()
        {
            if (!RenderSettings.skybox) return;

            RenderSettings.skybox.SetColor("_Tint", m_SkyBoxColor);
        }

        public virtual void ChangeSafeArea()
        {
            var safeArea = Screen.safeArea;
            var screenSize = new Vector2(Screen.width, Screen.height);
            var minAnchor = safeArea.position / screenSize;
            var maxAnchor = (safeArea.position + safeArea.size) / screenSize;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = minAnchor;
            rectTransform.anchorMax = maxAnchor;
        }
    }
}