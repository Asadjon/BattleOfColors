using Assets.Scripts.Resource;
using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static Assets.Scripts.GameOptions;
using static UnityEngine.Vector2;

namespace Assets.Scripts.Players
{
    [ExecuteInEditMode, Serialization(typeof(SerializationPlayer))]
    class Player : UIBehaviour
    {
        [SerializeField] private GameBoard m_GameBoard = null;
        [SerializeField] private DirectoryBoard m_DirectoryBoard = null;
        [SerializeField] private float m_Padding = 16f;
        [SerializeField] private UnityEvent<Player> m_GameOver = null;

        public UnityEvent<Player> OnGameOver => m_GameOver;
        public GameTypes GameType { set 
            {
                if (m_GameBoard) m_GameBoard.GameType = value;
            }
        }
        public GameLevels GameLevel { set { if (m_GameBoard) m_GameBoard.GameLevel = value; } }
        public int MovesCount => m_GameBoard.MovesCount;

        private List<ViewResource> mResources = new List<ViewResource>();
        public List<ViewResource> Resources
        {
            get => mResources;
            set
            {
                mResources = value;
                m_DirectoryBoard.Resources = mResources;
                m_GameBoard.Resources = mResources.CreateMultiple();
            }
        }

        [ContextMenu("Initialize Game")]
        private void InitializeGame()
        {
            var sizeOfSquare = GameOptions.Instance ? GameOptions.Instance.SizeOfSquar : DefaultSizeOfSquare;

            CalculateSize((int)sizeOfSquare);

            m_DirectoryBoard.LoadDataPreview(sizeOfSquare);
            m_GameBoard.LoadDataPreview(sizeOfSquare);
        }

        public void InitializeGame(sbyte[] goalState, SizesOfSquare sizeOfSquare = DefaultSizeOfSquare, GameTypes gameType = DefaultGameType, GameLevels gameLevel = DefaultGameLevel)
        {
            CalculateSize(sizeOfSquare.Value());

            m_DirectoryBoard.Initialize(sizeOfSquare);
            m_GameBoard.Initialize(sizeOfSquare, gameType, gameLevel, goalState);
            m_GameBoard.OnGameOver = movesCount => OnGameOver.Invoke(this);
        }

        [ContextMenu("Calculate Size")]
        private void CalculateSize() =>
            CalculateSize(FindObjectOfType<GameOptions>() is GameOptions gameOptions ? (int)gameOptions.SizeOfSquar : (int)DefaultSizeOfSquare);

        public void CalculateSize(int count)
        {
            // get size of Player
            var playerRectSize = (transform as RectTransform).rect.size;

            var padding = one * m_Padding;

            // calculate one SwipeView size
            var givenSize = playerRectSize - 2f * padding - (Max(m_GameBoard.ContentPadding, m_DirectoryBoard.ContentPadding) * right + (m_GameBoard.ContentPadding + m_DirectoryBoard.ContentPadding) * up);
            var viewSize = Mathf.Min(givenSize.x / count, givenSize.y / (count + 1));

            // calculate anchored sizes
            var gbAnchoredSize = (count * viewSize * one + m_GameBoard.ContentPadding) / playerRectSize;
            var dbAnchoredSize = (viewSize * (count * right + up) + m_DirectoryBoard.ContentPadding) / playerRectSize;
            padding /= playerRectSize;

            // get size of GameBoard and DirectoryBoard
            var gbRect = m_GameBoard.transform as RectTransform;
            var dbRect = m_DirectoryBoard.transform as RectTransform;

            // change sizes
            gbRect.anchorMin = (right - right * gbAnchoredSize + up * padding) / 2f;
            gbRect.anchorMax = (right + right * gbAnchoredSize) / 2f + up * (gbRect.anchorMin + gbAnchoredSize);

            dbRect.anchorMin = (right - right * dbAnchoredSize + up * 2f * (padding + gbRect.anchorMax)) / 2f;
            dbRect.anchorMax = (right + right * dbAnchoredSize) / 2f + up * (dbRect.anchorMin + dbAnchoredSize);

            m_GameBoard.ChangeSize(viewSize);
            m_DirectoryBoard.ChangeSize(viewSize);
        }

        public void AddSwipeAction(UnityAction<int> action) =>
            m_GameBoard.OnSwipeViews.AddListener(action);

        public void StartGame() =>
            m_GameBoard.StartGame();

        public void PlayGame() => m_GameBoard.PlayGame();

        public void PauseGame() => m_GameBoard.PauseGame();

        public void StopGame() => m_GameBoard.StopGame();

        public void NewGame(List<ViewResource> resources, sbyte[] shuffle)
        {
            m_GameBoard.Restart(shuffle);
            Resources = resources;
        }
    }

    [Serializable] struct SerializationPlayer
    {
        [SerializedMember("m_DirectoryBoard")] public SerializationDirectoryBoard DirectoryBoard;
        [SerializedMember("m_GameBoard")] public SerializationGameBoard GameBoard;
    }
}
