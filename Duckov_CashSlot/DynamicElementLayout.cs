using Cysharp.Threading.Tasks;
using Duckov_CashSlot.Configs;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov_CashSlot
{
    public class DynamicElementLayout : MonoBehaviour
    {
        [SerializeField] private int maxRows = 2;

        private GridLayoutGroup _gridLayoutGroup = null!;
        private RectTransform _gridRectTransform = null!;
        private LayoutElement _layoutElement = null!;

        private bool _needToUpdateHeight;
        private ScrollRect _scrollRect = null!;
        private RectTransform _viewport = null!;

        public int MaxRows
        {
            get => maxRows;
            set => SetMaxRows(value);
        }

        private void Awake()
        {
            InitComponents();
        }

        private void LateUpdate()
        {
            if (!_needToUpdateHeight) return;
            _needToUpdateHeight = false;
            UniTask.DelayFrame(1).ContinueWith(UpdateHeight).Forget();
        }

        private void OnTransformChildrenChanged()
        {
            _needToUpdateHeight = true;
        }

        public void SetMaxRows(int rows)
        {
            maxRows = rows;
            _needToUpdateHeight = true;
        }

        private void InitComponents()
        {
            if (!CheckRectTransform()) return;
            if (!InitializeRectMask2D()) return;
            if (!InitializeLayoutElement()) return;
            if (!InitGridLayoutGroup()) return;
            if (!InitializeScrollRect()) return;

            _needToUpdateHeight = true;
        }

        private bool CheckRectTransform()
        {
            if (!Utility.GetComponent(gameObject, out RectTransform gridRectTransform))
            {
                ModLogger.LogError("[Dynamic Element Layout] Failed to find RectTransform in current object.");
                return false;
            }

            _gridRectTransform = gridRectTransform;

            if (!Utility.GetComponent(transform.parent.gameObject, out RectTransform viewport))
            {
                ModLogger.LogError("[Dynamic Element Layout] Failed to find RectTransform in parent object.");
                return false;
            }

            _viewport = viewport;
            return true;
        }

        private bool InitializeRectMask2D()
        {
            if (_viewport == null) return false;
            if (Utility.GetComponent<RectMask2D>(_viewport.gameObject, out _, true)) return true;

            ModLogger.LogError("[Dynamic Element Layout] Failed to add RectMask2D to parent object.");
            return false;
        }

        private bool InitializeLayoutElement()
        {
            if (_viewport == null) return false;
            if (!Utility.GetComponent<LayoutElement>(_viewport.gameObject, out var layoutElement, true))
            {
                ModLogger.LogError("[Dynamic Element Layout] Failed to add LayoutElement to parent object.");
                return false;
            }

            layoutElement.flexibleHeight = -1;

            _layoutElement = layoutElement;
            return true;
        }

        private bool InitGridLayoutGroup()
        {
            if (!Utility.GetComponent<GridLayoutGroup>(gameObject, out var gridLayoutGroup, true))
            {
                ModLogger.LogError("[Dynamic Element Layout] Failed to add GridLayoutGroup to current object.");
                return false;
            }

            _gridLayoutGroup = gridLayoutGroup;
            return true;
        }

        private bool InitializeScrollRect()
        {
            if (_viewport == null) return false;
            if (!Utility.GetComponent<ScrollRect>(_viewport.gameObject, out var scrollRect, true))
            {
                ModLogger.LogError("[Dynamic Element Layout] Failed to add ScrollRect to parent object.");
                return false;
            }

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.viewport = _viewport;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.content = _gridRectTransform;
            scrollRect.scrollSensitivity = 1f;

            _scrollRect = scrollRect;
            return true;
        }

        private void DisableScrollRect()
        {
            if (_scrollRect == null) return;
            _scrollRect.vertical = false;
            _scrollRect.normalizedPosition = Vector2.up;
        }

        private void EnableScrollRect()
        {
            if (_scrollRect == null) return;
            _scrollRect.vertical = true;
        }

        private void UpdateHeight()
        {
            if (this == null) return;
            if (_gridLayoutGroup == null || _layoutElement == null || _gridRectTransform == null) return;

            var childCount = _gridRectTransform.childCount;
            if (childCount == 0)
            {
                DisableScrollRect();
                _layoutElement.minHeight = 0;
                _layoutElement.preferredHeight = 0;
                return;
            }

            var cellH = _gridLayoutGroup.cellSize.y;
            var spacingY = _gridLayoutGroup.spacing.y;
            var pad = _gridLayoutGroup.padding;
            var totalRows = CalculateItemRows();

            var needScroll = totalRows > MaxRows;
            var showRows = needScroll ? MaxRows : totalRows;
            var needExtraSpace = needScroll && !SlotDisplaySetting.Instance.DontNeedMoreSlotReminder;

            var preferredHeight = pad.top + pad.bottom + showRows * cellH + (showRows - 1) * spacingY;
            if (needExtraSpace) preferredHeight += spacingY * 2; // show a bit of the next row to indicate more content

            if (needScroll)
                EnableScrollRect();
            else
                DisableScrollRect();

            _layoutElement.minHeight = preferredHeight;
            _layoutElement.preferredHeight = preferredHeight;
        }

        private int CalculateItemRows()
        {
            if (_gridLayoutGroup == null || _gridRectTransform == null) return 0;

            switch (_gridLayoutGroup.constraint)
            {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                {
                    var constraintCount = Mathf.Max(1, _gridLayoutGroup.constraintCount);
                    var childCount = _gridRectTransform.childCount;
                    return Mathf.CeilToInt((float)childCount / constraintCount);
                }
                case GridLayoutGroup.Constraint.FixedRowCount:
                    return _gridLayoutGroup.constraintCount;
                case GridLayoutGroup.Constraint.Flexible:
                default:
                    return CalculateFlexibleSize().y;
            }
        }

        private Vector2Int CalculateFlexibleSize()
        {
            if (_gridLayoutGroup == null || _gridRectTransform == null) return Vector2Int.zero;

            var itemCount = _gridRectTransform.childCount;
            if (itemCount == 0) return Vector2Int.zero;

            var xCount = 0;
            var previousX = float.MinValue;
            for (var i = 0; i < itemCount; i++)
            {
                var child = _gridRectTransform.GetChild(i) as RectTransform;
                if (child == null) continue;

                var childX = child.anchoredPosition.x;
                if (childX <= previousX) break;

                xCount++;
                previousX = childX;
            }

            var rows = Mathf.CeilToInt((float)itemCount / xCount);
            return new(xCount, rows);
        }
    }
}