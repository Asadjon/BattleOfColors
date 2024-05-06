using Assets.Scripts.PuzzleSolvers;
using Assets.Scripts.Resource;
using System;
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

        protected override ItemView CreateView(int index)
        {
            var view = base.CreateView(index);
            view.OnSwipe = this;
            return view;
        }

        public bool OnSwipe(Vector2Int position, State state, SwipeDirection swipeDir)
        {
            var touchState = state & mSettings.ItemState;
            var orientation = -1;
            Vector2 dir = position - mEmptyNode.PositionInTheArray;

            if (!mPermissionToSwipe || touchState == State.None || mSettings.DistanceOfSwiping - dir.magnitude < 0) return false;
            else if (touchState == State.Click) orientation = (int)Mathf.Abs(dir.normalized.x);
            else if (touchState == State.Swipe)
            {
                orientation = Convert.ToInt32((Horizontal | swipeDir) == Horizontal);
                var orientVertical = Convert.ToInt32((Vertical | swipeDir) == Vertical);

                if ((((Right | Bottom) & swipeDir) != swipeDir || dir[orientVertical] > 0) &&
                    (((Left  | Top)    & swipeDir) != swipeDir || dir[orientVertical] < 0))
                    return false;
            }

            if (dir[orientation] != 0) return false;

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

        private bool CheckTheWin()
        {
            if (mNodes[mNodes.Count - 1] != mEmptyNode) return false;
            return GameType == GameOptions.GameTypes.WithNumber && mItemViews.TrueForAll(item => mNodes[mItemViews.IndexOf(item)].ItemView.Resource == item.Resource) ||
                   GameType == GameOptions.GameTypes.WithColor && mItemViews.TrueForAll(item => mNodes[mItemViews.IndexOf(item)].ItemView.Resource.Color == item.Resource.Color);
        }

        public override void PauseGame() => mPermissionToSwipe = false;

        public override void PlayGame() => mPermissionToSwipe = true;
    }
}