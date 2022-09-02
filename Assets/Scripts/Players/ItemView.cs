using Assets.Scripts.Resource;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static Assets.Scripts.GameSettings;
using static Assets.Scripts.Players.ItemView.SwipeDirection;
using static Assets.Scripts.Players.ItemView.State;

namespace Assets.Scripts.Players
{
    class ItemView : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
    {
        #region SerializeField Objects
        [SerializeField] private TextMeshProUGUI m_TextView = null;
        [SerializeField] private Image m_ColorView = null;
        [SerializeField] Ease m_Ease = Ease.Linear;
        #endregion

        #region Local Objects
        private Node mNode = null;
        private ViewResource m_Resources;
        private bool mIsOnTouchDown = false;
        private bool mIsAllowOnce = false;
        private Vector2 mTouchingPosition;
        private Vector4 mSwipingLimit;
        private bool mIsShowText = false;
        public enum SwipeDirection { Default = -1, Left = 4, Top = 5, Right = 6, Bottom = 7 }
        public enum SwipeOrientation { Default = -1, Horizontal = Left | Right, Vertical = Top | Bottom }
        public enum State { Swipe = 0, Click = 1 }
        #endregion

        #region Getters And Setters
        public bool IsMoving { get; set; } = true;
        public IOnSwipe OnSwipe { get; set; } = null;
        public ViewResource Resource
        {
            get => m_Resources;
            set
            {
                m_Resources = value;
                UpdateUI();
            }
        }
        public bool IsShowText { get => mIsShowText; 
            set
            {
                mIsShowText = value;
                UpdateUI();
            }
        }
        public int Id { get; set; }
        public Node Node { get => mNode; set => mNode = value; }
        #endregion

        public static implicit operator ItemView(Node node) => node?.ItemView;

        protected override void Awake() {
            base.Awake();
            LoadData();
        }

        private void LoadData()
        {
            var swipeSize = GameSettings.Instance.SwipeLimitSize * GetComponent<RectTransform>().rect.size;

            mSwipingLimit = new Vector4()
            {
                x = -swipeSize.x / 2f,
                y = -swipeSize.y / 2f,
                z = swipeSize.x / 2f,
                w = swipeSize.y / 2f,
            };
        }

        public ItemView ChangePosition()
        {
            var rectTrans = transform as RectTransform;
            var nodeRectTrans = mNode.transform as RectTransform;

            rectTrans.sizeDelta = nodeRectTrans.sizeDelta;
            rectTrans.SetPositionAndRotation(nodeRectTrans.position, nodeRectTrans.rotation);

            return this;
        }

        public void StartMove(float delay) =>
            (transform as RectTransform).DOAnchorPos((mNode.transform as RectTransform).anchoredPosition, delay).SetEase(m_Ease);

        private void UpdateUI()
        {
            (m_TextView.enabled, m_TextView.text, m_TextView.color, m_ColorView.color) =
                (mIsShowText, m_Resources.Text, m_Resources.TextColor, m_Resources.Color);
        }

        public void ChangeChildOffset(float offset)
        {
            var rect = m_ColorView.GetComponent<RectTransform>();

            rect.offsetMin = offset * Vector2.one;
            rect.offsetMax = -1f * offset * Vector2.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            mIsAllowOnce = true;
            mIsOnTouchDown = true;
            mTouchingPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData) =>
            mIsOnTouchDown = false;

        public void OnDrag(PointerEventData eventData)
        {
            if (IsMoving && OnSwipe != null && mIsOnTouchDown && mIsAllowOnce)
                CheckDirection(eventData.position);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(IsMoving && OnSwipe != null) OnSwipe.OnSwipe(mNode.PositionInTheArray, Click);
        }

        private void CheckDirection(Vector2 position)
        {
            var dir = mTouchingPosition - position;
            SwipeDirection direction;

            // horizontal movement
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                if (dir.x < mSwipingLimit.x) direction = Right;
                else if (dir.x > mSwipingLimit.z) direction = Left;
                else return;
            }
            // vertical movement
            else if (Mathf.Abs(dir.x) < Mathf.Abs(dir.y))
            {
                if (dir.y < mSwipingLimit.y) direction = Top;
                else if (dir.y > mSwipingLimit.w) direction = Bottom;
                else return;
            }

            else
            {
                if (dir.x < mSwipingLimit.x) direction = Right;
                else if (dir.x > mSwipingLimit.z) direction = Left;
                else if (dir.y < mSwipingLimit.y) direction = Top;
                else if (dir.y > mSwipingLimit.w) direction = Bottom;
                else return;
            }

            mIsAllowOnce = !OnSwipe.OnSwipe(mNode.PositionInTheArray, Swipe, direction);
        }

        public interface IOnSwipe
        {
            bool OnSwipe(Vector2Int position, State state, SwipeDirection direction = Default);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var size = (FindObjectOfType<GameSettings>() is GameSettings gameOptions ? gameOptions.SwipeLimitSize : DefaultSwipeLimitSize) * GetComponent<RectTransform>().rect.size / 2f;
            var centerPosition = m_ColorView.transform.position;

            Gizmos.color = Color.green;

            // Left line
            Gizmos.DrawLine(new Vector3 { x = centerPosition.x - size.x, y = centerPosition.y - size.y, z = centerPosition.z },
                            new Vector3 { x = centerPosition.x - size.x, y = centerPosition.y + size.y, z = centerPosition.z });

            // top line
            Gizmos.DrawLine(new Vector3 { x = centerPosition.x - size.x, y = centerPosition.y + size.y, z = centerPosition.z },
                            new Vector3 { x = centerPosition.x + size.x, y = centerPosition.y + size.y, z = centerPosition.z });

            // right line
            Gizmos.DrawLine(new Vector3 { x = centerPosition.x + size.x, y = centerPosition.y - size.y, z = centerPosition.z },
                            new Vector3 { x = centerPosition.x + size.x, y = centerPosition.y + size.y, z = centerPosition.z });

            // bottom line
            Gizmos.DrawLine(new Vector3 { x = centerPosition.x + size.x, y = centerPosition.y - size.y, z = centerPosition.z },
                            new Vector3 { x = centerPosition.x - size.x, y = centerPosition.y - size.y, z = centerPosition.z });
        }
#endif
    }
}
