using Assets.Scripts.PuzzleSolvers;
using Assets.Scripts.Resource;
using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Players
{
    [Serialization(typeof(SerializationDirectoryBoard))]
    class DirectoryBoard : UIBehaviour
    {
        #region SerializeField Objects
        [SerializeField] private ItemView m_ItemViewPrefab = null;
        [SerializeField] private Node m_BlockPrefab = null;
        [SerializeField] private RectTransform m_ContentOfItemViews = null;
        [SerializeField] private RectTransform m_ContentOfNodes = null;
        [SerializeField] private float m_MinPadding = 1f;
        [SerializeField] private float m_MaxPadding = 3f;
        #endregion

        #region Local Objects
        private readonly List<ItemView> mItemViews = new List<ItemView>();
        protected List<Node> mNodes = new List<Node>();
        private SizesOfSquare mSizeOfSquare = DefaultSizeOfSquare;
        private float mSizeOfView = default;
        private List<ViewResource> mResources = new List<ViewResource>();
        #endregion

        #region Getters And Setters

        private List<Vector2Int> mSavedItemViewsPositions = null;
        private List<SerializableVector2Int> ItemViewsPosition
        {
            get => mItemViews.ConvertAll(view => (SerializableVector2Int)view.Node.PositionInTheArray);
            set => mSavedItemViewsPositions = value.ToList().ConvertAll(pos => (Vector2Int)pos);
        }

        public List<ViewResource> Resources { 
            get => mResources;
            set
            {
                mResources = value;
                mNodes.ForEach(node => node.ItemView.Resource = mResources[mNodes.IndexOf(node)]);
            }
        }
        public Vector2 ContentPadding { get => GetComponent<RectTransform>().rect.size - (m_ContentOfItemViews ? m_ContentOfItemViews.rect.size : GetComponent<RectTransform>().rect.size); }
        #endregion

        protected override void Awake() => LoadData();

        [ContextMenu("Calculate size")]
        public void CaluclateSize()
        {
            mSizeOfSquare = FindObjectOfType<GameOptions>() is GameOptions gameOptions ? gameOptions.SizeOfSquar : DefaultSizeOfSquare;
            ChangeSize(m_ContentOfNodes.GetComponent<RectTransform>().rect.size.x / (int)mSizeOfSquare);
        }

        public void ChangeSize(float sizeOfView)
        {
            mSizeOfView = sizeOfView;
            var i = 0;
            mNodes.ConvertAll(node => node.GetComponent<RectTransform>()).ForEach(node =>
            {
                var pos = Vector2.right * i++;
                node.sizeDelta = Vector2.one * mSizeOfView;
                node.anchoredPosition = pos * mSizeOfView;
            });

            mItemViews.ForEach(item => item.ChangePosition());
        }

        [ContextMenu("Initialize")]
        public void LoadDataPreview()
        {
            var sizeOfSquare = FindObjectOfType<GameOptions>() is GameOptions gameOptions ? gameOptions.SizeOfSquar : DefaultSizeOfSquare;
            LoadDataPreview(sizeOfSquare);
        }

        public void LoadDataPreview(SizesOfSquare sizeOfSquare)
        {
            LoadData();
            Initialize(sizeOfSquare);
        }

        private void LoadData()
        {
            if (!m_ContentOfItemViews) m_ContentOfItemViews = (RectTransform) transform;

            mSizeOfSquare = DefaultSizeOfSquare;
        }

        public void Initialize(SizesOfSquare sizeOfSquare)
        {
            mSizeOfSquare = sizeOfSquare;
            mSizeOfView = m_ContentOfNodes.GetComponent<RectTransform>().rect.size.x / (int)sizeOfSquare;
            m_ContentOfNodes.sizeDelta = m_ContentOfItemViews.sizeDelta;
            m_ContentOfNodes.anchoredPosition = m_ContentOfItemViews.anchoredPosition;

            NotifyViews();
        }

        private void CreateViews()
        {
            for (var i = 0; i < (int)mSizeOfSquare; i++)
            {
                var pos = Vector2Int.right * i;

                var node = Instantiate(m_BlockPrefab, m_ContentOfNodes);
                node.name = "Node " + i;
                node.PositionInTheArray = pos;
                var blockTransform = node.GetComponent<RectTransform>();
                blockTransform.sizeDelta = Vector2.one * mSizeOfView;
                blockTransform.anchoredPosition = Vector2.one * pos * mSizeOfView;
                mNodes.Add(node);
            }

            var offset = Mathf.Lerp(m_MaxPadding, m_MinPadding, (mSizeOfSquare - MinSizeOfSquare) / (MaxSizeOfSquare - MinSizeOfSquare));
            mNodes.ForEach(node =>
            {
                var view = Instantiate(m_ItemViewPrefab, m_ContentOfItemViews);
                var index = mNodes.IndexOf(node);

                view.name = "Item " + index;
                node.SetItemViewWithoutAnim(view);
                view.Id = index;
                view.GameType = GameTypes.WithColor;
                mItemViews.Add(view);

                node.ChangeChildOffset(offset);
            });

            if (mSavedItemViewsPositions != null) for (int i = 0; i < mSavedItemViewsPositions.Count; i++)
                {
                    var savPos = mSavedItemViewsPositions[i];
                    mNodes.Find(node => node.PositionInTheArray == savPos).SetItemViewWithoutAnim(mItemViews[i]);
                }
            mSavedItemViewsPositions = null;
        }

        public void NotifyViews()
        {
            RemoveAllViews();
            CreateViews();
        }

        private void RemoveAllViews()
        {
            Array.ForEach(m_ContentOfNodes.GetComponentsInChildren<Node>(), block => DestroyImmediate(block.gameObject));

            Array.ForEach(m_ContentOfItemViews.GetComponentsInChildren<ItemView>(), view => DestroyImmediate(view.gameObject));

            mItemViews.Clear();
            mNodes.Clear();
        }
    }

    [Serializable] struct MyDirectoryBoard { public SerializebleNode[] Nodes; }
    [Serializable] struct SerializationDirectoryBoard
    {
        [SerializedMember("ItemViewsPosition")] public List<SerializableVector2Int> ItemViewsPosition;
    }
}
