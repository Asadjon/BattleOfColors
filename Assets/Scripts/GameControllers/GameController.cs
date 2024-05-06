using Assets.Scripts.AnimControllers;
using Assets.Scripts.AudioManagers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.GameControllers
{
    class GameController : UIBehaviour
    {
        [SerializeField] private GameAnimController m_PauseController = null;
        [SerializeField] private GameAnimController m_GameOverController = null;
        [SerializeField] private AnimController m_CounterAnimController = null;

        private bool mIsPauseGame = false;
        private bool mIsGameOver = false;
        private bool mIsCountStart = false;
        private IOnGameActions mOnGameActions = null;

        protected override void Awake()
        {
            base.Awake();
            mIsGameOver = false;
            m_GameOverController.ShowIs(mIsGameOver);

            mIsPauseGame = false;
            m_PauseController.ShowIs(mIsPauseGame);
        }

        public bool IsPauseGame => mIsPauseGame && mIsGameOver; 
        public bool IsGameOver => mIsGameOver;
        public bool IsCountStart => mIsCountStart;

        public enum Controllers { Pause, GameOver }

        public void PlaySound(string soundName) => AudioManager.Instance.Play(soundName);

        public void StartGame()
        {
            if (!mIsPauseGame)
            {
                m_CounterAnimController.SetParam(0, default(char));

                mIsGameOver = false;
                m_GameOverController.ShowIs(mIsGameOver);
            }
        }

        public void GameOver()
        {
            if (!mIsPauseGame)
            {
                mIsGameOver = true;
                m_GameOverController.ShowIs(mIsGameOver);
            }

            ResumeGame();
        }

        public void PauseGame()
        {
            if (!mIsGameOver)
            {
                mIsPauseGame = true;
                m_PauseController.ShowIs(mIsPauseGame);
            }
        }

        public void ResumeGame()
        {
            if (!mIsGameOver)
            {
                mIsPauseGame = false;
                m_PauseController.ShowIs(mIsPauseGame);
            }
        }

        public void OnClick_CloseGame()
        {
            mOnGameActions.OnCloseGame();
        }

        public void OnClick_ResumeGame()
        {
            ResumeGame();
            if(mOnGameActions != null)
                mOnGameActions.OnResumeGame();
        }

        public void OnClick_Restart()
        {
            StartGame();

            if (mOnGameActions != null)
                mOnGameActions.OnRestartGame();
        }

        public void OnClick_Next()
        {
            if (mOnGameActions != null)
                mOnGameActions.OnNextGame();
        }

        public void SetMessage(string message) => m_GameOverController.SetMessage(message);
        public void SetMessage(string message, Color messageColor) => m_GameOverController.SetMessage(message, messageColor);

        public void SetGameActions(IOnGameActions onGameActions) => mOnGameActions = onGameActions;

        public void Action_StartCounter()
        {
            mIsCountStart = true;
            if (mOnGameActions != null)
                mOnGameActions.OnStartCount();
        }

        public void Action_EndCounter()
        {
            mIsCountStart = false;
            if (mOnGameActions != null)
                mOnGameActions.OnEndCount();
        }

        public interface IOnGameActions
        {
            void OnRestartGame();
            void OnNextGame();

            void OnResumeGame();
            void OnCloseGame();

            void OnStartCount();
            void OnEndCount();
        }
    }
}