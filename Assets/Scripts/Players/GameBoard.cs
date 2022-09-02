using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using static Assets.Scripts.GameOptions;
using Assets.Scripts.Resource;
using System.Linq;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Players
{
    abstract class GameBoard : UIBehaviour
    {
        #region SerializeField Objects
        [SerializeField] private ItemView m_ItemViewPrefab = null;
        [SerializeField] private Node m_BlockPrefab = null;
        [SerializeField] private RectTransform m_ContentOfItemViews = null;
        [SerializeField] private RectTransform m_ContentOfNodes = null;
        [SerializeField] private float m_MinPadding = 1f;
        [SerializeField] private float m_MaxPadding = 3f;
        [SerializeField] protected UnityEvent m_OnSwipeViews = null;
        #endregion

        #region Local Objects
        protected Node mEmptyNode;
        protected readonly List<ItemView> mItemViews = new List<ItemView>();
        protected readonly List<Node> mNodes = new List<Node>();
        protected int mNumberOfArrays = default;
        protected int mTotalNumberOfArrays = default;
        protected List<ViewResource> mResources = new List<ViewResource>();

        private float mSizeOfView = default;
        #endregion

        #region Getters And Setters

        public virtual SerializationGameBoard Implicit()
        {
            var myGameBoard = CreateMyGameBoard();
            myGameBoard.EmptyNode = mEmptyNode;
            myGameBoard.Nodes = mNodes.ConvertAll(node => (SerializebleNode)node).ToArray();
            return myGameBoard;
        }

        protected virtual SerializationGameBoard CreateMyGameBoard() => new SerializationGameBoard();

        public virtual GameBoard Set(SerializationGameBoard myGameBoard)
        {
            var nodes = mNodes.ToDictionary(node => node.PositionInTheArray);
            var itemViews = mItemViews.ToDictionary(item => item.Id);

            mEmptyNode = nodes[(Vector2Int) myGameBoard.EmptyNode.Position];
            mEmptyNode.SetItemViewWithoutAnim(null);

            Array.ForEach(myGameBoard.Nodes, myNode => {
                if (nodes[(Vector2Int) myNode.Position] == mEmptyNode) return;
                nodes[(Vector2Int)myNode.Position].SetItemViewWithoutAnim(itemViews[myNode.ItemId]);
            });

            return this;
        }

        public List<ViewResource> Resources
        {
            set
            {
                mResources = value;
                mItemViews.ForEach(view => view.Resource = mResources[mItemViews.IndexOf(view)]);
            }
        }
        public UnityAction GameOver { get; set; } = default;
        public Vector2 ContentPadding { get => GetComponent<RectTransform>().rect.size - (m_ContentOfItemViews ? m_ContentOfItemViews.rect.size : GetComponent<RectTransform>().rect.size); }
        public GameTypes GameType { get; set; } = DefaultGameType;
        public UnityEvent OnSwipeViews { get => m_OnSwipeViews; set => m_OnSwipeViews = value; }
        #endregion

        protected override void Awake() => LoadData();
        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

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
                var pos = new Vector2(i % mNumberOfArrays, i++ / mNumberOfArrays);
                node.sizeDelta = Vector2.one * mSizeOfView;
                pos.y = mNumberOfArrays - pos.y - 1;
                node.anchoredPosition = pos * mSizeOfView;
            });

            mItemViews.ForEach(item => item.ChangePosition());
        }

        public void LoadDataPreview(int numberOfArrays, List<ViewResource> resources)
        {
            LoadData();
            Initialize(numberOfArrays, resources);
        }

        protected virtual void LoadData()
        {
            if (!m_ContentOfItemViews) m_ContentOfItemViews = (RectTransform)transform;
            if (!m_ContentOfNodes) m_ContentOfNodes = (RectTransform)transform;

            mNumberOfArrays = DefaultNumberOfArrays;
            mTotalNumberOfArrays = (int)Math.Pow(mNumberOfArrays, 2f);
            GameType = DefaultGameType;
        }

        public void Initialize(int numberOfArrays, List<ViewResource> resources, UnityAction gameOver)
        {
            GameOver = gameOver;
            Initialize(numberOfArrays, resources);
        }

        public virtual void Initialize(int numberOfArrays, List<ViewResource> resources)
        {
            mNumberOfArrays = numberOfArrays;
            mTotalNumberOfArrays = (int) Mathf.Pow(mNumberOfArrays, 2);
            mResources = resources;
            mSizeOfView = m_ContentOfNodes.GetComponent<RectTransform>().rect.size.x / mNumberOfArrays;
            m_ContentOfNodes.sizeDelta = m_ContentOfItemViews.sizeDelta;
            m_ContentOfNodes.anchoredPosition = m_ContentOfItemViews.anchoredPosition;

            InitPositions();

            // Remove all ItemView
            Array.ForEach(m_ContentOfItemViews.GetComponentsInChildren<ItemView>(), view => DestroyImmediate(view.gameObject));
            mItemViews.Clear();

            CreateViews();
        }

        private void InitPositions()
        {
            mNodes.Clear();
            Array.ForEach(m_ContentOfNodes.GetComponentsInChildren<Node>(), node => DestroyImmediate(node.gameObject));

            for (byte i = 0; i < mTotalNumberOfArrays; i++)
            {
                var pos = new Vector2Int(i % mNumberOfArrays, i / mNumberOfArrays);

                var node = Instantiate(m_BlockPrefab, m_ContentOfNodes);
                node.name = "Node " + i;
                node.PositionInTheArray = pos;
                var blockTransform = node.GetComponent<RectTransform>();
                blockTransform.sizeDelta = Vector2.one * mSizeOfView;
                pos.y = mNumberOfArrays - pos.y - 1;
                blockTransform.anchoredPosition = (Vector2) pos * mSizeOfView;
                mNodes.Add(node);
            }

            mEmptyNode = mNodes[mNodes.Count - 1];
        }

        protected virtual void CreateViews()
        {
            var offset = Mathf.Lerp(m_MaxPadding, m_MinPadding, (mNumberOfArrays - MinNumberOfArrays) / (MaxNumberOfArrays - MinNumberOfArrays));
            mNodes.ForEach(node =>
            {
                if (node != mEmptyNode)
                {
                    var view = CreateView(mNodes.IndexOf(node));
                    node.SetItemViewWithoutAnim(view);
                    mItemViews.Add(view);
                }
                node.ChangeChildOffset(offset);
            });
        }

        protected virtual ItemView CreateView(int index)
        {
            var view = Instantiate(m_ItemViewPrefab, m_ContentOfItemViews);

            view.name = "Item " + index;
            view.Id = index;
            view.Resource = mResources[index];
            view.IsShowText = GameType == GameTypes.WithNumber;
            return view;
        }

        protected void SwitchPosition(Vector2Int position)
        {
            var index = position.y * mNumberOfArrays + position.x;
            mEmptyNode &= mNodes[index];
        }

        protected virtual void SwitchPositionOnece(Vector2Int position)
        {
            SwitchPosition(position);
            m_OnSwipeViews.Invoke();
        }

        protected virtual void SwitchSpecifiedPositions(Vector2 direction)
        {
            sbyte count = (sbyte)direction.magnitude;
            var dir = new Vector2Int((int)direction.normalized.x, (int)direction.normalized.y);

            for (sbyte i = 0; i < count; i++) SwitchPosition(mEmptyNode.PositionInTheArray + dir);

            m_OnSwipeViews.Invoke();
        }

        protected virtual void StartShuffle(List<sbyte> shuffledList)
        {
            for (var i = 0; i < shuffledList.Count; i++)

                if (shuffledList[i] != 0)
                    mNodes[i].SetItemViewWithAnim(mItemViews.Find(view => view.Resource.Id == shuffledList[i]));

                else (mEmptyNode = mNodes[i]).SetItemViewWithoutAnim(null);
        }

        public abstract void PauseGame();

        public abstract void PlayGame();

        public abstract void StartGame();

        protected abstract bool CheckTheWin();

        public virtual void Restart()
        {
            for (var i = 0; i < mItemViews.Count; i++)
            {
                if (mNodes[i] != mItemViews[i])
                    mNodes[i].SetItemViewWithoutAnim(mItemViews[i]);
            }

            (mEmptyNode = mNodes[mNodes.Count - 1]).SetItemViewWithoutAnim(null);
        }

        protected sbyte[] GetPuzzle()
        {
            var puzzle = new sbyte[mTotalNumberOfArrays];
            foreach (var node in mNodes)
            {
                var pos = node.PositionInTheArray;
                puzzle[pos.y * mNumberOfArrays + pos.x] = (sbyte)(node != mEmptyNode ? node.ItemView.Resource.Id : 0);
            }

            return puzzle;
        }
    }

    [Serializable] internal class SerializationGameBoard { public SerializebleNode[] Nodes; public SerializebleNode EmptyNode; }
}
