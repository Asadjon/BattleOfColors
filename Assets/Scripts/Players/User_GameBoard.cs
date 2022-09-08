using Assets.Scripts.PuzzleSolvers;
using Assets.Scripts.Resource;
using UnityEngine;
using static Assets.Scripts.Players.ItemView;
using static Assets.Scripts.Players.ItemView.SwipeDirection;

namespace Assets.Scripts.Players
{
    sealed class User_GameBoard : GameBoard, IOnSwipe
    {
        private bool mPermissionToSwipe = true;
        private GameSettings mSettings;

        protected override void LoadData()
        {
            base.LoadData();
            mPermissionToSwipe = true;
            mSettings = GameSettings.Instance;
        }

        protected override ItemView CreateView(int index, ViewResource resource)
        {
            var view = base.CreateView(index, resource);
            view.OnSwipe = this;
            return view;
        }

        public bool OnSwipe(Vector2Int position, State state, SwipeDirection direction)
        {
            if (!mPermissionToSwipe && direction != Default) return false;

            Vector2 dir = position - mEmptyNode.PositionInTheArray;

            Vector2Int orientation;

            if (mSettings.ItemState == State.Swipe && state == State.Swipe)
                orientation = new Vector2Int(Vector2Int.right[((int)direction) % 2], Vector2Int.up[((int)direction) % 2]);

            else if (mSettings.ItemState == State.Click && state == State.Click)
                orientation = new Vector2Int((int)Mathf.Abs(dir.normalized.x), (int)Mathf.Abs(dir.normalized.y));

            else return false;

            if (dir[orientation.x] != 0 || mSettings.DistanceOfSwiping - dir.magnitude < 0) return false;

            SwitchSpecifiedPositions(dir);

            return true;
            
        }

        protected override void SwitchPositionOnece(Vector2Int position)
        {
            base.SwitchPositionOnece(position);

            if (CheckTheWin()) GameOver();
        }

        protected override void SwitchSpecifiedPositions(Vector2 direction)
        {
            base.SwitchSpecifiedPositions(direction);

            if (CheckTheWin()) GameOver();
        }

        protected override bool CheckTheWin()
        {
            if (mNodes[mNodes.Count - 1] != mEmptyNode) return false;
            return GameType == GameOptions.GameTypes.WithNumber && mItemViews.TrueForAll(item => mNodes[mItemViews.IndexOf(item)].ItemView.Resource == item.Resource) ||
                   GameType == GameOptions.GameTypes.WithColor && mItemViews.TrueForAll(item => mNodes[mItemViews.IndexOf(item)].ItemView.Resource.Color == item.Resource.Color);
        }

        public override void PauseGame() => mPermissionToSwipe = false;

        public override void PlayGame() => mPermissionToSwipe = true;

        public override void StartGame()
        {
            StartShuffle(GetPuzzle().Shuffle((PuzzleShuffle.ShuffleLevels)GameOptions.Instance.Level, mNumberOfArrays - GameOptions.MinNumberOfArrays));
            PlayGame();
        }
    }
}