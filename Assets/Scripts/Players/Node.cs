using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Players
{
    internal class Node : UIBehaviour
    {
        [SerializeField] private Image m_Pallet = null;
        private ItemView mItemView = null;
        private Vector2Int mPosition = Vector2Int.zero;

        public ItemView ItemView => mItemView;

        public ItemView SetItemViewWithAnim(ItemView value)
        {
            if (!(mItemView = value)) return mItemView;

            mItemView.Node = this;
            mItemView.StartMove(GameSettings.Instance.SwipingSpeed);
            return mItemView;
        }

        public ItemView SetItemViewWithoutAnim(ItemView value)
        {
            if (!(mItemView = value)) return mItemView;

            mItemView.Node = this;
            mItemView.ChangePosition();
            return mItemView;
        }

        public Vector2Int PositionInTheArray { get => mPosition; set => mPosition = value; }

        public void ChangeChildOffset(float offset)
        {
            var rect = m_Pallet.GetComponent<RectTransform>();

            rect.offsetMin = offset * Vector2.one;
            rect.offsetMax = -1f * offset * Vector2.one;

            mItemView?.ChangeChildOffset(offset);
        }

        public static implicit operator SerializebleNode(Node node) => new SerializebleNode
            {
                Position = node.mPosition,
                ItemId = node.mItemView ? node.mItemView.Id : 0
            };

        public static explicit operator Node(ItemView itemView) => itemView?.Node;

        public static Node operator &(Node a, Node b)
        {
            var c = a.ItemView;
            a.SetItemViewWithAnim(b);
            b.SetItemViewWithAnim(c);
            return b;
        }
    }

    [Serializable] struct SerializebleNode { public int ItemId; public SerializableVector2Int Position; }
}