using Assets.Scripts.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.GameOptions;

namespace Assets.Scripts.Players
{
    class DirectoryBoard : MonoBehaviour
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
        private int mNumberOfArrays = default;
        private float mSizeOfView = default;
        private List<ViewResource> mResources = new List<ViewResource>();
        #endregion

        #region Getters And Setters

        public static implicit operator MyDirectoryBoard(DirectoryBoard directoryBoard) =>
            new MyDirectoryBoard { Nodes = directoryBoard.mNodes.ConvertAll(node => (SerializebleNode)node).ToArray() };

        public void Set(MyDirectoryBoard myDirectoryBoard)
        {
            var nodes = mNodes.ToDictionary(node => node.PositionInTheArray);
            var itemViews = mItemViews.ToDictionary(item => item.Id);

            Array.ForEach(myDirectoryBoard.Nodes, myNode => nodes[(Vector2Int)myNode.Position].SetItemViewWithoutAnim(itemViews[myNode.ItemId]));
        }

        public List<ViewResource> Resources { 
            get => mNodes.ConvertAll(node => node.ItemView.Resource);
            set
            {
                mResources = value;
                mNodes.ForEach(node => node.ItemView.Resource = mResources[mNodes.IndexOf(node)]);
            }
        }
        public Vector2 ContentPadding { get => GetComponent<RectTransform>().rect.size - (m_ContentOfItemViews ? m_ContentOfItemViews.rect.size : GetComponent<RectTransform>().rect.size); }
        #endregion

        private void Awake() => LoadData();

        [ContextMenu("Initialize")]
        public void LoadDataPreview()
        {
            var numberOfArray = FindObjectOfType<GameOptions>() is GameOptions gameOptions ? gameOptions.NumberOfArrays : DefaultNumberOfArrays;
            LoadData();
            Initialize(numberOfArray, ViewResource.GenerateResources(numberOfArray));
        }

        [ContextMenu("Calculate size")]
        public void CaluclateSize()
        {
            mNumberOfArrays = FindObjectOfType<GameOptions>() is GameOptions gameOptions ? gameOptions.NumberOfArrays : DefaultNumberOfArrays;
            ChangeSize(m_ContentOfNodes.GetComponent<RectTransform>().rect.size.x / mNumberOfArrays);
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

        public void LoadDataPreview(int numberOfArrays, List<ViewResource> resources)
        {
            LoadData();
            Initialize(numberOfArrays, resources);
        }

        private void LoadData()
        {
            if (!m_ContentOfItemViews) m_ContentOfItemViews = (RectTransform) transform;

            mNumberOfArrays = DefaultNumberOfArrays;
        }

        public void Initialize(int numberOfArrays, List<ViewResource> resources)
        {
            mNumberOfArrays = numberOfArrays;
            mResources = resources;
            mSizeOfView = m_ContentOfNodes.GetComponent<RectTransform>().rect.size.x / numberOfArrays;
            m_ContentOfNodes.sizeDelta = m_ContentOfItemViews.sizeDelta;
            m_ContentOfNodes.anchoredPosition = m_ContentOfItemViews.anchoredPosition;

            NotifyViews();
        }

        private void CreateViews()
        {
            for (var i = 0; i < mNumberOfArrays; i++)
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

            var offset = Mathf.Lerp(m_MaxPadding, m_MinPadding, (mNumberOfArrays - MinNumberOfArrays) / (MaxNumberOfArrays - MinNumberOfArrays));
            mNodes.ForEach(node =>
            {
                var view = Instantiate(m_ItemViewPrefab, m_ContentOfItemViews);
                var index = mNodes.IndexOf(node);

                view.name = "Item " + index;
                node.SetItemViewWithoutAnim(view);
                view.Id = index;
                view.Resource = mResources[node.PositionInTheArray.x];
                mItemViews.Add(view);

                node.ChangeChildOffset(offset);
            });
        }

        public void StartShuffle()
        {
            var i = 0;
            mNodes.Shuffle().ForEach(node => node.SetItemViewWithAnim(mItemViews[i++]));
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
}
