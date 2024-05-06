using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [ExecuteAlways]
    public class CustomGridLayoutGroup : LayoutGroup
    {
        [SerializeField] protected Vector2 m_Spacing = Vector2.zero;
        [SerializeField] protected bool m_ChildControlWidth = true;
        [SerializeField] protected bool m_ChildControlHeight = true;
        [SerializeField] protected bool m_ReverseArrangement = false;
        [SerializeField] protected Axis m_AxisDirection = Axis.Vertical;
        [SerializeField] protected Constraints m_Constraint = Constraints.Wrap;
        [SerializeField] protected int m_ConstraintCount = 1;

        private int m_Capacity = 10;
        private Vector2[] m_Sizes = new Vector2[10];
        private (Vector2 position, Vector2 size)[] m_Children = new (Vector2 position, Vector2 size)[0];

        public Vector2 Spacing { get => m_Spacing; set => SetProperty(ref m_Spacing, value); }
        public bool ChildControlWidth { get => m_ChildControlWidth; set => SetProperty(ref m_ChildControlWidth, value); }
        public bool ChildControlHeight { get => m_ChildControlHeight; set => SetProperty(ref m_ChildControlHeight, value); }
        public bool ReverseArrangement { get => m_ReverseArrangement; set => SetProperty(ref m_ReverseArrangement, value); }
        public Axis AxisDirection { get => m_AxisDirection; set => SetProperty(ref m_AxisDirection, value); }
        public Constraints Constraint { get => m_Constraint; set => SetProperty(ref m_Constraint, value); }
        public int ConstraintCount { get => m_ConstraintCount; set => SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); }

        public enum Axis
        {
            /// <summary>
            /// Horizontal axis
            /// </summary>
            Horizontal = 0,
            /// <summary>
            /// Vertical axis.
            /// </summary>
            Vertical = 1
        }

        public enum Constraints
        {
            /// <summary>
            /// Don't constrain the number of rows or columns.
            /// </summary>
            Wrap = 0,
            /// <summary>
            /// Constrain the number count a specified number.
            /// </summary>
            FixedCount = 1,
        }
        
        protected override void Start()
        {
            base.Start();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            m_Children = CalcChildren();
            CalcAlongAxis();
        }

        public override void CalculateLayoutInputVertical()
        {
            m_Children = CalcChildren();
            CalcAlongAxis();
        }

        public override void SetLayoutHorizontal()
        {
            SetChildren();
        }

        public override void SetLayoutVertical()
        {
            SetChildren();
        }

        private void CalcAlongAxis()
        {
            int inverseAxis = (int)GetInverse(AxisDirection);
            float padding = new Vector2(base.padding.horizontal, base.padding.vertical)[inverseAxis];
            float totalPreferred = padding + m_Children.Select(child => child.position[inverseAxis] + child.size[inverseAxis]).Aggregate(0f, Math.Max);

            SetLayoutInputForAxis(padding, totalPreferred, 0, inverseAxis);

            SetLayoutInputForAxis(LayoutUtility.GetMinSize(rectTransform, (int)AxisDirection), LayoutUtility.GetPreferredSize(rectTransform, (int)AxisDirection), 0, (int)AxisDirection);
        }

        private void SetChildren()
        {
            int count = m_Children.Length;
            Vector2 startPos = new Vector2(padding.left, padding.top);

            for (int i = 0; i < count; i++)
            {
                RectTransform rectTransform = rectChildren[m_ReverseArrangement ? (count - i - 1) : i];
                (Vector2 position, Vector2 size) = m_Children[i];

                if (m_ChildControlWidth) SetChildAlongAxis(rectTransform, 0, (startPos + position)[0], size[0]);
                else SetChildAlongAxis(rectTransform, 0, (startPos + position)[0]);

                if (m_ChildControlHeight) SetChildAlongAxis(rectTransform, 1, (startPos + position)[1], size[1]);
                else SetChildAlongAxis(rectTransform, 1, (startPos + position)[1]);
            }
        }

        protected (Vector2 position, Vector2 size)[] CalcChildren()
        {
            int count = rectChildren.Count;
            if (count == 0) return new (Vector2 position, Vector2 size)[0];

            int axis = (int)AxisDirection;
            int inverseAxis = (int)GetInverse(AxisDirection);
            (Vector2[] positions, Vector2[] sizes) = (new Vector2[count], new Vector2[count]);
            Vector2 alignment = new Vector2(GetAlignmentOnAxis(0), GetAlignmentOnAxis(1));
            Vector2 innerSize = rectTransform.rect.size - new Vector2(padding.horizontal, padding.vertical);

            if (m_Constraint == Constraints.Wrap)
            {
                int x = 0;
                Vector2 childPos = Vector2.zero;

                Enumerable.Range(0, count).ToList().ForEach(i =>
                {
                    (Vector2 min, Vector2 preferred, Vector2 flexible) = GetChildSizes(rectChildren[m_ReverseArrangement ? (count - i - 1) : i]);
                    sizes[i] = Vector2.Max(min, preferred);

                    if ((childPos + sizes[i])[axis] > innerSize[axis] && i != 0)
                    {
                        childPos[axis] = 0;
                        childPos[inverseAxis] += Enumerable.Range(i - x, x).Select(j => sizes[j][inverseAxis]).Max() + m_Spacing[inverseAxis];
                        x = 0;
                    }

                    positions[i][inverseAxis] = childPos[inverseAxis];

                    float alignmentSpaceAxis = ((innerSize - childPos - sizes[i]) * alignment)[axis];
                    positions[i - x][axis] = alignmentSpaceAxis;
                    if (x > 0) Enumerable.Range(i - x + 1, x).ToList()
                        .ForEach(j => positions[j][axis] = (positions[j - 1] + sizes[j - 1] + m_Spacing)[axis]);

                    childPos[axis] += (sizes[i] + m_Spacing)[axis];
                    x++;
                });
            }
            else
            {
                int fixedCount = Mathf.Clamp(m_ConstraintCount, 1, m_ConstraintCount);
                float axisSize = (innerSize - (m_Spacing * (fixedCount - 1)))[axis] / fixedCount;
                Vector2Int controlSize = new Vector2Int(Convert.ToInt16(m_ChildControlWidth), Convert.ToInt16(m_ChildControlHeight));

                Enumerable.Range(0, count).ToList().ForEach(i =>
                {
                    (int axisCount, int inverseAxisCount) = (i % fixedCount, i / fixedCount);
                    (Vector2 min, Vector2 preferred, Vector2 flexible) = GetChildSizes(rectChildren[m_ReverseArrangement ? (count - i - 1) : i]);

                    sizes[i][axis] = controlSize[axis] == 1 ? axisSize : preferred[axis];
                    sizes[i][inverseAxis] = controlSize[inverseAxis] == 1 ? preferred[inverseAxis] * axisSize / preferred[axis] : preferred[inverseAxis];

                    if (axisCount == 0)
                    {
                        positions[i][axis] = inverseAxisCount == 0 ? 0f : positions[i - fixedCount][axis];
                        positions[i][inverseAxis] = inverseAxisCount == 0 ? 0f : Enumerable.Range((inverseAxisCount - 1) * fixedCount, fixedCount)
                        .Select(j => (positions[j] + sizes[j])[inverseAxis]).Max() + m_Spacing[inverseAxis];
                    }
                    else
                    {
                        positions[i][axis] = (positions[i - 1] + sizes[i - 1])[axis] + m_Spacing[axis];
                        positions[i][inverseAxis] = positions[i - 1][inverseAxis];
                    }

                    float alignmentSpaceAxis = ((innerSize - positions[i] - sizes[i] + positions[i - axisCount]) * alignment)[axis];
                    positions[i - axisCount][axis] = alignmentSpaceAxis;
                    if (axisCount > 0) Enumerable.Range(i - axisCount + 1, axisCount).ToList()
                        .ForEach(j => positions[j][axis] = (positions[j - 1] + sizes[j - 1] + m_Spacing)[axis]);
                });
            }

            float alignmentSpaceInverseAxis = ((innerSize - positions[count - 1] - sizes[count - 1]) * alignment)[inverseAxis];
            Enumerable.Range(0, count).ToList().ForEach(i => positions[i][inverseAxis] += alignmentSpaceInverseAxis);

            return Enumerable.Range(0, count).Select(i => (positions[i], sizes[i])).ToArray();
        }

        private Axis GetInverse(Axis axis) =>
            (axis == Axis.Horizontal) ? Axis.Vertical : Axis.Horizontal;

        private (Vector2 min, Vector2 preferred, Vector2 flexible) GetChildSizes(RectTransform child)
        {
            Vector2 flexible = Vector2.zero;
            Vector2 min = Vector2.zero;
            Vector2 preferred = Vector2.zero;

            if (m_ChildControlWidth)
            {
                min[0] = LayoutUtility.GetMinSize(child, 0);
                preferred[0] = LayoutUtility.GetPreferredSize(child, 0);
                flexible[0] = LayoutUtility.GetFlexibleSize(child, 0);
            }
            else
            {
                min[0] = child.sizeDelta[0];
                preferred[0] = min[0];
                flexible[0] = 0f;
            }

            if (m_ChildControlHeight)
            {
                min[1] = LayoutUtility.GetMinSize(child, 1);
                preferred[1] = LayoutUtility.GetPreferredSize(child, 1);
                flexible[1] = LayoutUtility.GetFlexibleSize(child, 1);
            }
            else
            {
                min[1] = child.sizeDelta[1];
                preferred[1] = min[1];
                flexible[1] = 0f;
            }

            return (min, preferred, flexible);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            // For new added components we want these count be set count false,
            // so that the user's sizes won't be overwritten before they
            // have a chance count turn these settings off.
            // However, for existing components that were added before this
            // feature was introduced, we want it count be on be default for
            // backwardds compatibility.
            // Hence their default value is on, but we set count off in reset.
            m_ChildControlWidth = false;
            m_ChildControlHeight = false;
        }
        protected virtual void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            int childCount = transform.childCount;
            if (childCount > m_Capacity)
            {
                if (childCount > m_Capacity * 2)
                {
                    m_Capacity = childCount;
                }
                else
                {
                    m_Capacity *= 2;
                }

                m_Sizes = new Vector2[m_Capacity];
            }

            bool flag = false;
            for (int i = 0; i < childCount; i++)
            {
                RectTransform rectTransform = transform.GetChild(i) as RectTransform;
                if (rectTransform && rectTransform.sizeDelta != m_Sizes[i])
                {
                    flag = true;
                    m_Sizes[i] = rectTransform.sizeDelta;
                }
            }

            if (flag)
            {
                LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
            }
        }
#endif
    }
}
