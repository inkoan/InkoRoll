using System;
using UnityEngine;
using UnityEngine.UI;

namespace Inkoan.InkoRoll
{
    /// **************************************************
    /// このソースをリスト要素にアタッチします
    /// 実際の表示処理等を入れたソースを作成し
    /// リスト要素にアタッチした後
    /// 対象のソースがアタッチされているオブジェクトを
    /// _contentに設定してください
    /// また、ボタンがある場合はそちらも設定してください
    /// **************************************************
    public class InkoRollItem : MonoBehaviour
    {
        /// <summary>表示物Object</summary>
        [SerializeField] private GameObject _content;

        /// <summary>表示内ボタン</summary>
        [SerializeField] private Button[] _buttons;

        /// <summary>SimpleScrollView</summary>
        private InkoRoll _inkoRoll;

        /// <summary>インデックス</summary>
        public int Index { get; private set; } = -1;

        /// <summary>RectTransform</summary>
        public RectTransform RectTransform { get; private set; }

        /// <summary>
        /// データ表示用セルコンポーネント
        /// </summary>
        private Component _cellViewComponent;

        /// <summary>
        /// 指定のクラスを取得して返す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T GetCellView<T>() where T : Component
        {
            if (_cellViewComponent == null)
            {
                if (_content.TryGetComponent<T>(out var component))
                {
                    _cellViewComponent = component;
                }
            }

            if (_cellViewComponent != null)
            {
                return (T)_cellViewComponent;
            }

            return null;
        }

        /// <summary>
        /// 破棄時
        /// </summary>
        private void OnDestroy()
        {
            if (_buttons == null) return;
            foreach (var btn in _buttons)
            {
                btn?.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Initialize(InkoRoll inkoRoll)
        {
            _inkoRoll = inkoRoll;
            RectTransform = transform as RectTransform;
            if (_buttons == null || _buttons.Length <= 0) return;
            for (var i = 0; i < _buttons.Length; i++)
            {
                var buttonIndex = i;
                _buttons[i]?.onClick.AddListener(() => _inkoRoll?.OnClickScrollViewItem(buttonIndex, this));
            }
        }

        /// <summary>
        /// 自身のインデックスを更新
        /// </summary>
        /// <param name="index"></param>
        public void UpdateIndex(int index)
        {
            Index = index;
            transform.name = Index.ToString();
        }

        /// <summary>
        /// 自身が描画エリアの外に出ていたら非表示にする
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="posStart"></param>
        /// <param name="posEnd"></param>
        public void CheckActive(InkoRoll.Direction direction, float posStart, float posEnd)
        {
            if (Index == -1) return;
            var check = false;
            if (direction == InkoRoll.Direction.Vertical)
            {
                if ((Math.Abs(transform.localPosition.y) + RectTransform.rect.size.y) < posStart ||
                    posEnd < Math.Abs(transform.localPosition.y))
                {
                    check = true;
                }
            }
            else
            {
                if ((Math.Abs(transform.localPosition.x) + RectTransform.rect.size.x) < posStart ||
                    posEnd < Math.Abs(transform.localPosition.x))
                {
                    check = true;
                }
            }

            if (check)
            {
                Index = -1;
                transform.name = Index.ToString();
                gameObject.SetActive(false);
            }
        }
    }
}