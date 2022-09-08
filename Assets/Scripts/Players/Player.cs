using Assets.Scripts.Resource;
using Assets.Scripts.SaveGameDatas.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private string m_PlayerName = "Player";
        [SerializeField] private GameBoard m_GameBoard = null;
        [SerializeField] private DirectoryBoard m_DirectoryBoard = null;
        [SerializeField] private float m_Padding = 16f;
        [SerializeField] private UnityEvent<string> m_GameOver = null;

        public string PlayerName { get => m_PlayerName; set => m_PlayerName = value; }
        public UnityEvent<string> OnGameOver { get => m_GameOver; }
        public GameTypes GameType
        {
            set
            {
                if (m_GameBoard) m_GameBoard.GameType = value;
            }
        }


#if UNITY_EDITOR
        private UnityAction<int> mOnChange;
        private void Update()
        {
            if (mOnChange == null)
                GameOptions.Instance.OnChangeNumberOfArrays.AddListener(mOnChange = asd => InitializeGame());
        }
#endif

        [ContextMenu("Initialize Game")]
        private void InitializeGame()
        {
            var numberOfArray = GameOptions.Instance ? GameOptions.Instance.NumberOfArrays : DefaultNumberOfArrays;

            CalculateSize(numberOfArray);

            var resources = ViewResource.GenerateResources(numberOfArray);

            m_DirectoryBoard.LoadDataPreview(numberOfArray, resources);
            m_GameBoard.LoadDataPreview(numberOfArray,
                ViewResource.CreateMultiple(resources));
        }

        public void InitializeGame(int numberOfArrays, List<ViewResource> resources)
        {
            CalculateSize(numberOfArrays);

            m_DirectoryBoard.Initialize(numberOfArrays, resources);
            m_GameBoard.Initialize(numberOfArrays,
                ViewResource.CreateMultiple(resources),
                () => OnGameOver.Invoke(m_PlayerName));
        }

        [ContextMenu("Calculate Size")]
        private void CalculateSize() =>
            CalculateSize(FindObjectOfType<GameOptions>() is GameOptions gameOptions ? gameOptions.NumberOfArrays : DefaultNumberOfArrays);

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

        public void AddSwipeAction(UnityAction action) =>
            m_GameBoard.OnSwipeViews.AddListener(action);

        public void StartGame()
        {
            m_GameBoard.StartGame();
        }

        public void PlayGame() => m_GameBoard.PlayGame();

        public void PauseGame() => m_GameBoard.PauseGame();

        public void NewGame()
        {
            m_DirectoryBoard.StartShuffle();
            var count = m_DirectoryBoard.Resources.Count;
            m_GameBoard.Resources = ViewResource.CreateMultiple(m_DirectoryBoard.Resources);

            m_GameBoard.Restart();
        }
    }

    [Serializable] struct SerializationPlayer
    {
        [SerializedMember("m_PlayerName")] public string PlayerName;
        [SerializedMember("m_DirectoryBoard")] public SerializationDirectoryBoard DirectoryBoard;
        [SerializedMember("m_GameBoard")] public SerializationGameBoard GameBoard;
    }
}
