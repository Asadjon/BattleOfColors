using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    class ColorChanger : MonoBehaviour
    {
        [SerializeField] private Image m_Image;
        [SerializeField] private RawImage m_RawImage;
        [SerializeField] private Gradient m_Gradient;
        [SerializeField, Range(0f, 1f)] private float m_Value = 0.0f;

        private void Update()
        {
            var color = m_Gradient.Evaluate(m_Value);
            if (m_RawImage) m_RawImage.color = color;
            if (m_Image) m_Image.color = new Color(color.r, color.g, color.b, m_Image.color.a);
        }
    }
}