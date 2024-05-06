using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Assets.Scripts.GameOptions;
using Assets.Scripts.Resource;
using UnityEngine.EventSystems;
using Assets.Scripts.SaveGameDatas.Attributes;
using System.Linq;
using Assets.Scripts.AudioManagers;

namespace Assets.Scripts.Players
{
    [Serialization(typeof(SerializationGameBoard))]
    abstract class GameBoard : UIBehaviour
    {
        #region SerializeField Objects
        [SerializeField] private ItemView m_ItemViewPrefab = null;
        [SerializeField] private Node m_BlockPrefab = null;
        [SerializeField] private RectTransform m_ContentOfItemViews = null;
        [SerializeField] private RectTransform m_ContentOfNodes = null;
        [SerializeField] private float m_MinPadding = 1f;
        [SerializeField] private float m_MaxPadding = 3f;
        [SerializeField] protected UnityEvent<int> m_OnSwipeViews = null;
        #endregion

        #region Local Objects
        protected Node mEmptyNode;
        protected readonly List<ItemView> mItemViews = new List<ItemView>();
        protected readonly List<Node> mNodes = new List<Node>();
        private List<ViewResource> mResources = new List<ViewResource>();
        protected int mTotalSizeOfSquare = default;
        protected SizesOfSquare mSizeOfSquare = DefaultSizeOfSquare;
        protected GameTypes mGameType = DefaultGameType;
        protected GameLevels mGameLevel = DefaultGameLevel;
        protected sbyte[] mGoalState;
        protected sbyte[] mShuffledList;

        private float mSizeOfView = default;
        private int mMovesCount = 0;
        #endregion

        #region Getters And Setters

        #region Serialize
        private sbyte[] mSavedItemViewsPositions = null;
        private sbyte[] ItemViewPosition
        {
            get => mNodes.Select(node => node.ItemView).Select(view => (sbyte)(view ? mItemViews.IndexOf(view) + 1 : 0)).ToArray();
            set => mSavedItemViewsPositions = value;
        }

        public int MovesCount { get => mMovesCount; private set => mMovesCount = value; }
        public sbyte[] GoalState => mGoalState;
        #endregion

        public List<ViewResource> Resources
        {
            get => mResources;
            set
            {
                mResources = value;
                for (int i = 0; i < mGoalState.Length; i++)
                {
                    var stateIndex = mGoalState[i] - 1;
                    if (stateIndex == -1) continue;

                    mItemViews[stateIndex].Resource = mResources[i];
                }
            }
        }
        public UnityAction<int> OnGameOver { get; set; } = default;
        public Vector2 ContentPadding { get => GetComponent<RectTransform>().rect.size - (m_ContentOfItemViews ? m_ContentOfItemViews.rect.size : GetComponent<RectTransform>().rect.size); }
        public GameTypes GameType { get => Get_GameType(); set => Set_GameType(value); }
        public GameLevels GameLevel { get => Get_GameLevel(); set => Set_GameLevel(value); }
        public UnityEvent<int> OnSwipeViews { get => m_OnSwipeViews; set => m_OnSwipeViews = value; }

        protected virtual GameTypes Get_GameType() => mGameType;
        protected virtual void Set_GameType(GameTypes gameType)
        {
            mGameType = gameType;
            mItemViews.ForEach(view => view.GameType = mGameType);
        }

        protected virtual GameLevels Get_GameLevel() => mGameLevel;
        protected virtual void Set_GameLevel(GameLevels gameLevel) => mGameLevel = gameLevel;
        #endregion

        protected override void Awake() => LoadData();
        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        [ContextMenu("Calculate size")]
        public void CaluclateSize()
        {
            mSizeOfSquare = FindObjectOfType<GameOptions>() is GameOptions gameOptions ? gameOptions.SizeOfSquar : DefaultSizeOfSquare;
            ChangeSize(m_ContentOfNodes.GetComponent<RectTransform>().rect.size.x / mSizeOfSquare.Value());
        }

        public void ChangeSize(float sizeOfView)
        {
            mSizeOfView = sizeOfView;
            var i = 0;
            mNodes.ConvertAll(node => node.GetComponent<RectTransform>()).ForEach(node =>
            {
                var pos = new Vector2(i % mSizeOfSquare.Value(), i++ / mSizeOfSquare.Value());
                node.sizeDelta = Vector2.one * mSizeOfView;
                pos.y = mSizeOfSquare.Value() - pos.y - 1;
                node.anchoredPosition = pos * mSizeOfView;
            });

            mItemViews.ForEach(item => item.ChangePosition());
        }

        [ContextMenu("Initialize")]
        public void LoadDataPreview()
        {
            var sizeOfSquar = GameOptions.Instance ? GameOptions.Instance.SizeOfSquar : DefaultSizeOfSquare;
            LoadDataPreview(sizeOfSquar);
        }

        public void LoadDataPreview(SizesOfSquare sizeOfSquare)
        {
            LoadData();
            Initialize(sizeOfSquare, DefaultGameType, DefaultGameLevel, GameOptions.Instance.GoalState);
        }

        protected virtual void LoadData()
        {
            if (!m_ContentOfItemViews) m_ContentOfItemViews = (RectTransform)transform;
            if (!m_ContentOfNodes) m_ContentOfNodes = (RectTransform)transform;

            mTotalSizeOfSquare = (int)Math.Pow(mSizeOfSquare.Value(), 2f);
            mMovesCount = 0;
        }

        public virtual void Initialize(SizesOfSquare sizeOfSquare, GameTypes gameType, GameLevels gameLevel, sbyte[] goalState)
        {
            mSizeOfSquare = sizeOfSquare;
            GameType = gameType;
            GameLevel = gameLevel;
            mTotalSizeOfSquare = (int) Mathf.Pow(mSizeOfSquare.Value(), 2);
            mSizeOfView = m_ContentOfNodes.GetComponent<RectTransform>().rect.size.x / mSizeOfSquare.Value();
            m_ContentOfNodes.sizeDelta = m_ContentOfItemViews.sizeDelta;
            m_ContentOfNodes.anchoredPosition = m_ContentOfItemViews.anchoredPosition;
            mGoalState = goalState;

            NotifyViews();
        }

        protected virtual void NotifyViews()
        {
            RemoveAllViews();
            InitPositions();
            CreateViews();
        }

        private void RemoveAllViews()
        {
            Array.ForEach(m_ContentOfNodes.GetComponentsInChildren<Node>(), block => DestroyImmediate(block.gameObject));

            Array.ForEach(m_ContentOfItemViews.GetComponentsInChildren<ItemView>(), view => DestroyImmediate(view.gameObject));

            mItemViews.Clear();
            mNodes.Clear();
        }

        private void InitPositions()
        {
            for (byte i = 0; i < mTotalSizeOfSquare; i++)
            {
                var pos = new Vector2Int(i % mSizeOfSquare.Value(), i / mSizeOfSquare.Value());

                var node = Instantiate(m_BlockPrefab, m_ContentOfNodes);
                node.name = "Node " + i;
                node.PositionInTheArray = pos;
                var blockTransform = node.GetComponent<RectTransform>();
                blockTransform.sizeDelta = Vector2.one * mSizeOfView;
                pos.y = mSizeOfSquare.Value() - pos.y - 1;
                blockTransform.anchoredPosition = (Vector2) pos * mSizeOfView;
                mNodes.Add(node);
            }
        }

        protected virtual void CreateViews()
        {
            var offset = Mathf.Lerp(m_MaxPadding, m_MinPadding, (mSizeOfSquare - MinSizeOfSquare) / (MaxSizeOfSquare - MinSizeOfSquare));

            mNodes.ForEach(node =>
            {
                var viewIndex = mGoalState[mNodes.IndexOf(node)] - 1;
                if (viewIndex != -1)
                {
                    var view = CreateView(viewIndex);
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
            view.GameType = GameType;
            return view;
        }

        protected void SwitchPosition(Vector2Int position)
        {
            var index = position.y * (int)mSizeOfSquare + position.x;
            mEmptyNode &= mNodes[index];
        }

        protected virtual void SwitchPositionOnece(Vector2Int position)
        {
            SwitchPosition(position);
            mMovesCount++;
            m_OnSwipeViews.Invoke(mMovesCount);
        }

        protected virtual void SwitchSpecifiedPositions(Vector2 direction)
        {
            sbyte count = (sbyte)direction.magnitude;
            var dir = new Vector2Int((int)direction.normalized.x, (int)direction.normalized.y);

            for (sbyte i = 0; i < count; i++) SwitchPosition(mEmptyNode.PositionInTheArray + dir);

            mMovesCount++;
            m_OnSwipeViews.Invoke(mMovesCount);
        }

        protected virtual void Reset(sbyte[] state, bool withAnim = true)
        {
            for (var i = 0; i < state.Length; i++)
            {
                var posId = state[i];
                var node = mNodes[i];

                if (posId != 0)
                {
                    var view = mItemViews[posId - 1];

                    if (withAnim) node.SetItemViewWithAnim(view);
                    else node.SetItemViewWithoutAnim(view);
                }

                else (mEmptyNode = node).SetItemViewWithoutAnim(null);
            }
        }

        public virtual void StartGame()
        {
            Reset(mShuffledList);
            PlayGame();
        }

        public abstract void PauseGame();

        public abstract void PlayGame();

        public virtual void StopGame() =>
            PauseGame();

        protected void GameOver() =>
            OnGameOver.Invoke(mMovesCount);

        public virtual void Restart(sbyte[] shuffle)
        {
            mShuffledList = shuffle;
            Reset(mSavedItemViewsPositions ?? mGoalState, false);
            mSavedItemViewsPositions = null;
            mMovesCount = 0;
        }

        protected sbyte[] GetPuzzle()
        {
            var puzzle = new sbyte[mTotalSizeOfSquare];
            foreach (var node in mNodes)
            {
                var pos = node.PositionInTheArray;
                puzzle[pos.y * mSizeOfSquare.Value() + pos.x] = (sbyte)(node != mEmptyNode ? node.ItemView.Resource.Id : 0);
            }

            return puzzle;
        }

        protected Node GetNode(Vector2Int position) =>
            mNodes.Find(node => node.PositionInTheArray == position);
    }

    [Serializable] internal class SerializationGameBoard
    {
        [SerializedMember("ItemViewPosition")] public sbyte[] ItemViewsPosition;
        [SerializedMember("mMovesCount")] public int MovesCount;
    }
}
