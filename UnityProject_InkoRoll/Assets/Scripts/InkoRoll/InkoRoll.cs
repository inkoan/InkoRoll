using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Inkoan.InkoRoll
{
    /// **************************************************
    /// 簡単に作れるスクロールビューが欲しくて作成した
    /// 「インコロール」です
    ///   ・1～複数行列リスト作れます
    ///   ・複数要素が混在したリストも作れます
    ///     区切り線入れたりとか
    ///   ・少し使い方にコツがいりますが
    ///     リスト要素のサイズが一定でない
    ///     スクロールビューも作れます
    /// 
    /// [使い方]
    /// 1) unityのUIからScrollViewを作成し、このソースを
    ///    アタッチしてください
    /// 2) リスト要素プレハブを設定してください
    ///    リスト要素が2種類以上ある場合/可変にする場合は
    ///    1個目のリストしか使われません
    /// 3) スクロール方向、行列数、要素の余白を
    ///    設定してください
    /// 4) 要素が可変する場合_flexibleにチェック
    /// 5) ログを表示するかどうかのフラグがありますので
    ///    バグってる場合などにご利用ください…
    /// 6) 表示したい時はSetData関数にデータを渡します
    ///    中身が変更される場合は再度SetDataを呼んで
    ///    ください
    ///    [引数1]
    ///      リスト要素が一個でいい場合はデータ数のみ
    ///      複数リスト要素があるどのリスト要素を
    ///      使用するかの配列を渡す
    ///    [引数2]
    ///      リスト要素の表示処理
    ///    [引数3](必要なら)
    ///      ボタンが押された場合の処理
    ///    [引数4](必要なら)
    ///      リスト要素が可変する場合、リスト要素の
    ///      サイズを決めるための最低限の処理
    ///      (_flexibleがtrueの場合のみ実行されます)
    /// 
    /// [注意]
    ///   縦の場合横の、横の場合縦のスクロールバーが
    ///   消える前提で自動サイズ調整をしていますので
    ///   スクロールしない側のサイズは要素が入り切る
    ///   よう設定してください
    /// **************************************************
    [RequireComponent(typeof(ScrollRect))]
    public class InkoRoll : MonoBehaviour
    {
        /// <summary>スクロール方向</summary>
        public enum Direction
        {
            Vertical, // 縦
            Horizontal // 横
        }

        // ==============================
        // 利用者設定用
        // ==============================

        /// <summary>リスト要素プレハブ</summary>
        [SerializeField] private RectTransform[] _prefabListItems = null;

        /// <summary>スクロールする方向</summary>
        [SerializeField] private Direction _direction = Direction.Vertical;

        /// <summary>スクロールする方向でない軸に表示する数</summary>
        [SerializeField] private int _line = 1;

        /// <summary>余白</summary>
        [SerializeField] private Vector2 _space = Vector2.zero;

        /// <summary>可変</summary>
        [SerializeField] private bool _flexible;

        /// <summary>ログ</summary>
        [SerializeField] private bool _drawLog;

        // ==============================
        // 定数
        // ==============================

        /// <summary>スクロールビュー枠にかぶる用の要素数(上下1ずつ)</summary>
        private const int NumItemOverlapFrame = 2;

        /// <summary>デフォルトで作成されるScrollViewのviewPortのmaskが角丸なのでその分を避ける用のマージン</summary>
        private const float Margin = 4.0f;

        // ==============================
        // メンバ変数
        // ==============================

        // ------------------------------ 内部保持用

        /// <summary>自分のRectTransform</summary>
        private RectTransform _rectTransform;

        /// <summary>ScrollRect</summary>
        private ScrollRect _scrollRect;

        /// <summary>要素表示部</summary>
        private RectTransform _rtContent;

        /// <summary>使いまわしオブジェクトの生成MAX数</summary>
        private int[] _maxItemReusable;

        /// <summary>生成済の使いまわしオブジェクト</summary>
        private List<InkoRollItem>[] _reusableItems;

        /// <summary>アイテムの座標</summary>
        private Vector2[] _posCoordinate;

        // ------------------------------ 監視用

        /// <summary>スクロール位置が更新された際のcontentの描画開始位置</summary>
        private float _drawAreaS;

        /// <summary>スクロール位置が更新された際のcontentの描画終了位置</summary>
        private float _drawAreaE;

        // ------------------------------ ユーザ設定関連

        /// <summary>スクロールビューに表示するアイテムの要素ごとのタイプ</summary>
        private int[] _itemType;

        /// <summary>移動によって新しく表示されるアイテムの表示処理</summary>
        private Action<int, InkoRollItem> _onDrawItem;

        /// <summary>アイテム内のボタンが押された時の処理</summary>
        private Action<int, int, InkoRollItem> _onClickItem;

        /// <summary>可変リストアイテムのサイズ決定用仮表示処理</summary>
        private Action<int, InkoRollItem> _tryDrawItem;

        // ------------------------------ 管理用

        /// <summary>スクロール方向に合わせたスクロールビューのHeight or Width</summary>
        private float SizeAreaHorW => _direction == Direction.Vertical ? _rectTransform.rect.height : _rectTransform.rect.width;

        /// <summary>スクロール方向に合わせた余白のサイズ</summary>
        private float SpaceXorY => _direction == Direction.Vertical ? _space.y : _space.x;

        /// <summary>
        /// スクロール方向に合わせた要素のサイズ:縦ならy/横ならx
        /// </summary>
        /// <param name="type"></param>
        private float ItemSizeWorH(int type)
        {
            return _direction == Direction.Vertical
                ? _prefabListItems[type].rect.size.y
                : _prefabListItems[type].rect.size.x;
        }

        /// <summary>
        /// ログ
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="isError"></param>
        private void DrawLog(string txt, bool isError = false)
        {
            if (!_drawLog) return;
            if (isError)
            {
                Debug.LogError(txt);
            }
            else
            {
                Debug.Log(txt);
            }
        }

        /// <summary>
        /// 破棄時
        /// </summary>
        private void OnDestroy()
        {
            _scrollRect.onValueChanged.RemoveAllListeners();
            _onDrawItem = null;
            _onClickItem = null;
            ClearReusableItems(); // 使いまわしオブジェクトをクリア
        }

        /// <summary>
        /// 起動時
        /// </summary>
        private void Awake()
        {
            if (_prefabListItems == null) return;

            // マイナス値対応
            if (_line <= 0) _line = 1;
            if (_space.x < 0) _space.x = 0;
            if (_space.y < 0) _space.y = 0;

            _scrollRect = GetComponent<ScrollRect>();
            _rectTransform = _scrollRect.transform as RectTransform;
            _rtContent = _scrollRect.content;
            _scrollRect.onValueChanged.AddListener(OnDrag);

            // スクロール方向設定
            _scrollRect.vertical = _direction == Direction.Vertical;
            _scrollRect.horizontal = _direction == Direction.Horizontal;

            // 不要な方のスクロールバー消す
            if (_direction == Direction.Vertical && _scrollRect.horizontalScrollbar != null)
            {
                _scrollRect.horizontalScrollbar.gameObject.SetActive(false);
            }
            else if (_scrollRect.verticalScrollbar != null)
            {
                _scrollRect.verticalScrollbar.gameObject.SetActive(false);
            }

            // contentの配置とアンカ
            _rtContent.anchorMin = new Vector2(0.0f, 1.0f);
            _rtContent.anchorMax = new Vector2(0.0f, 1.0f);
            _rtContent.pivot = new Vector2(0.0f, 1.0f);

            // リスト要素プレハブの配置
            _maxItemReusable = new int[_prefabListItems.Length];
            foreach (var prefab in _prefabListItems)
            {
                prefab.pivot = new Vector2(0.0f, 1.0f); // 左上配置にピボットを強制変更
                prefab.anchorMin = new Vector2(0.0f, 1.0f); // 左上配置にアンカを強制変更
                prefab.anchorMax = new Vector2(0.0f, 1.0f); // 左上配置にアンカを強制変更
            }
        }

        /// <summary>
        /// スクロール監視処理
        /// </summary>
        private void OnDrag(Vector2 v)
        {
            MonitoringScroll();
        }

        /// <summary>
        /// セットアップ処理:全て同じリスト要素を使う場合
        /// </summary>
        /// <param name="numItem">データの個数</param>
        /// <param name="onDrawItem">要素が更新された場合の処理</param>
        /// <param name="onClickItem">要素内のボタンが押された場合の処理</param>
        /// <param name="tryDrawItem">可変リスト時に要素サイズを決めるための最低限の処理</param>
        public void Setup(int numItem, Action<int, InkoRollItem> onDrawItem, Action<int, int, InkoRollItem> onClickItem, Action<int, InkoRollItem> tryDrawItem = null)
        {
            DrawLog("-------------------------------------------------- Setup(1 kind) : item count = " + numItem);
            if (_prefabListItems == null) return;

            var itemType = new int[numItem]; // 0だけの配列

            Setup(itemType, onDrawItem, onClickItem, tryDrawItem);
        }

        /// <summary>
        /// セットアップ処理:複数のリスト要素を使う場合を含む
        /// </summary>
        /// <param name="itemType">複数要素使用時のタイプ用リスト</param>
        /// <param name="onDrawItem">要素が更新された場合の処理</param>
        /// <param name="onClickItem">要素内のボタンが押された場合の処理</param>
        /// <param name="tryDrawItem">可変リスト時に要素サイズを決めるための最低限の処理</param>
        public void Setup(int[] itemType, Action<int, InkoRollItem> onDrawItem, Action<int, int, InkoRollItem> onClickItem, Action<int, InkoRollItem> tryDrawItem = null)
        {
            DrawLog("-------------------------------------------------- Setup : item count = " + itemType.Length);

            if (_prefabListItems == null) return;

            _itemType = itemType;
            _onDrawItem = onDrawItem;
            _onClickItem = onClickItem;
            _tryDrawItem = tryDrawItem;

            // 可変、または0以外の要素が含まれている場合複数行列不可
            if (_flexible || _itemType.Any(item => item != 0))
            {
                _line = 1;
            }

            CreateReusableItems();

            // 座標をマッピングする
            _posCoordinate = new Vector2[_itemType.Length];

            if (_flexible && _tryDrawItem != null) // 可変タイプ:サイズ決定用の処理が必要
            {
                PosMappingFlexible();
            }
            else // 通常
            {
                PosMappingNormal();
            }

            // スクロール有無
            _scrollRect.movementType = SizeAreaHorW <
                                       (_direction == Direction.Vertical
                                           ? Math.Abs(_posCoordinate.Last().y) + _prefabListItems[_itemType.Last()].rect.size.y
                                           : Math.Abs(_posCoordinate.Last().x) + _prefabListItems[_itemType.Last()].rect.size.x)
                ? ScrollRect.MovementType.Elastic
                : ScrollRect.MovementType.Clamped;

            MonitoringScroll();
        }

        /// <summary>
        /// 通常のリスト要素配置を決める
        /// </summary>
        private void PosMappingNormal()
        {
            DrawLog("-------------------------------------------------- PosMappingNormal()");
            for (var i = 0; i < _itemType.Length; i++)
            {
                if (i == 0) // 1要素目
                {
                    _posCoordinate[i] = new Vector2(Margin, -Margin);
                    continue;
                }
                if (_direction == Direction.Vertical)
                {
                    if (_line == 1) // lineが1の場合混在することがあるので前の要素を基準に位置を決める
                    {
                        _posCoordinate[i] = new Vector2(Margin, _posCoordinate[i - 1].y - _prefabListItems[_itemType[i - 1]].rect.size.y - _space.y);
                    }
                    else // 規則的に並べる:リスト要素は1個しか使わない
                    {
                        _posCoordinate[i] = new Vector2(
                            ((_prefabListItems[0].rect.size.x + _space.x) * (i % _line)) + Margin,
                            -(((_prefabListItems[0].rect.size.y + _space.y) * (i / _line)) + Margin)
                        );
                    }
                }
                else
                {
                    if (_line == 1) // lineが1の場合混在することがあるので前の要素を基準に位置を決める
                    {
                        _posCoordinate[i] =
                            new Vector2(
                                _posCoordinate[i - 1].x + _prefabListItems[_itemType[i - 1]].rect.size.x + _space.x,
                                -Margin
                            );
                    }
                    else // 規則的に並べる:リスト要素は1個しか使わない
                    {
                        _posCoordinate[i] = new Vector2(
                            ((_prefabListItems[0].rect.size.x + _space.x) * (i / _line)) + Margin,
                            -(((_prefabListItems[0].rect.size.y + _space.y) * (i % _line)) + Margin)
                        );
                    }
                }
                DrawLog("*** pos " + i + " : " + _posCoordinate[i]);
            }

            UpdateSizeForScrollContent(_direction == Direction.Vertical
                ? _prefabListItems[_itemType.Last()].rect.size.y
                : _prefabListItems[_itemType.Last()].rect.size.x);

            DrawLog("--------------------------------------------------");
        }

        /// <summary>
        /// 可変式のリスト要素配置を決める
        /// </summary>
        private void PosMappingFlexible()
        {
            DrawLog("-------------------------------------------------- PosMappingFlexible()");
            if (_tryDrawItem == null) return;

            RectTransform rt = _reusableItems[0][0].transform as RectTransform;
            _reusableItems[0][0].transform.localPosition = new Vector3(-10000.0f, -10000.0f, 0.0f);
            _reusableItems[0][0].gameObject.SetActive(true);
            Dictionary<int, float> sizeHorW = new();
            for (var i = 0; i < _itemType.Length; i++)
            {
                _tryDrawItem.Invoke(i, _reusableItems[0][0]);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                var size = _direction == Direction.Vertical ? rt!.rect.size.y : rt!.rect.size.x;
                DrawLog("* size : " + size);
                sizeHorW.Add(i, size);
            }

            _reusableItems[0][0].gameObject.SetActive(false);
            _reusableItems[0][0].transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

            // 座標決める
            for (var i = 0; i < _itemType.Length; i++)
            {
                if (i == 0) // 1要素目
                {
                    _posCoordinate[i] = new Vector2(Margin, -Margin);
                    DrawLog("* _pos " + i + " : " + _posCoordinate[i]);
                    continue;
                }

                if (_direction == Direction.Vertical)
                {
                    _posCoordinate[i] = new Vector2(Margin, _posCoordinate[i - 1].y - sizeHorW[i - 1] - _space.y);
                }
                else
                {
                    _posCoordinate[i] = new Vector2(_posCoordinate[i - 1].x + sizeHorW[i - 1] + _space.x, -Margin);
                }
                DrawLog("* _pos " + i + " : " + _posCoordinate[i]);
            }
            UpdateSizeForScrollContent(sizeHorW.Last().Value);
            DrawLog("--------------------------------------------------");
        }

        /// <summary>
        /// 使いまわし用オブジェクトの破棄
        /// </summary>
        private void ClearReusableItems()
        {
            if (_reusableItems == null || _reusableItems.Length <= 0) return;
            for (var i = 0; i < _reusableItems.Length; i++)
            {
                if (_reusableItems[i] == null || _reusableItems[i].Count <= 0) continue;
                for (var j = 0; j < _maxItemReusable[i]; j++)
                {
                    Destroy(_reusableItems[i][j].gameObject);
                }
                _reusableItems[i].Clear();
            }
            _reusableItems = null;
        }

        /// <summary>
        /// 使いまわし用オブジェクトの生成
        /// </summary>
        private void CreateReusableItems()
        {
            // 使いまわしオブジェクトをクリア
            ClearReusableItems();

            _reusableItems = new List<InkoRollItem>[_prefabListItems.Length];

            for (var i = 0; i < _prefabListItems.Length; i++)
            {
                // 混在リストだと指定した行列数が強制で1になる場合があるのでここでやる
                _maxItemReusable[i] = (((int)(SizeAreaHorW / (ItemSizeWorH(i) + SpaceXorY))) // きっちり入る数
                                       + ((SizeAreaHorW % (ItemSizeWorH(i) + SpaceXorY)) == 0 ? 0 : 1) // 見切れ分あれば
                                       + NumItemOverlapFrame // 予備数
                    ) * _line; // 指定の行列数

                _reusableItems[i] = new();

                var need = _itemType.Count(q => q == i); // 実際に使う数

                // 実際に使う分が少なければ使う分だけにする
                _maxItemReusable[i] = need < _maxItemReusable[i] ? need : _maxItemReusable[i];

                for (var j = 0; j < _maxItemReusable[i]; j++) // 準備する数分作る
                {
                    RectTransform item = Instantiate(_prefabListItems[i], _rtContent, false);
                    item.gameObject.SetActive(false);
                    var listItem = item.GetComponent<InkoRollItem>();
                    listItem.Initialize(this);
                    _reusableItems[i].Add(listItem);
                }
            }
        }

        /// <summary>
        /// 要素数に合わせたcontent(リスト要素置き場)のサイズ変更
        /// </summary>
        private void UpdateSizeForScrollContent(float lastObjSize)
        {
            float x, y;

            // contentのサイズと初期位置
            if (_direction == Direction.Vertical)
            {
                x = ((_prefabListItems[0].rect.size.x + _space.x) * _line) - _space.x + Margin;
                y = Math.Abs(_posCoordinate.Last().y) + lastObjSize + Margin;

                _scrollRect.verticalNormalizedPosition = 1; // 一番上
            }
            else
            {
                x = _posCoordinate.Last().x + lastObjSize + Margin;
                y = ((_prefabListItems[0].rect.size.y + _space.y) * _line) - _space.y + Margin;
                _scrollRect.horizontalNormalizedPosition = 0; // 左
            }

            _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
            _rtContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);

            DrawLog("***** content size : " + _rtContent.rect);
        }

        /// <summary>
        /// リスト要素のボタンが押された
        /// </summary>
        /// <param name="buttonIndex">登録されてるボタンの何番目か</param>
        /// <param name="item">リスト要素</param>
        public void OnClickScrollViewItem(int buttonIndex, InkoRollItem item)
        {
            DrawLog("!!! Onclick : index = " + item.Index + " / buttonIndex = " + buttonIndex);
            _onClickItem?.Invoke(item.Index, buttonIndex, item);
        }

        /// <summary>
        /// リスト要素の描画更新
        /// </summary>
        /// <param name="index">モデルのデータリストのIndex</param>
        /// <param name="item">更新するリスト要素</param>
        private void DrawListItem(int index, InkoRollItem item)
        {
            DrawLog("!!! DrawListItem : index = " + item.Index);
            _onDrawItem?.Invoke(index, item);
        }

        /// <summary>
        /// 任意の要素の位置にスクロールを移動
        /// </summary>
        public void MoveTo(int indexStart)
        {
            if (_scrollRect.movementType == ScrollRect.MovementType.Clamped) return; // 入りきってる場合はやらない
            if (indexStart < 0 || _itemType.Length <= indexStart) return; // 無い数値が来たらアウト

            _rtContent.anchoredPosition = _direction == Direction.Vertical
                                       ? new Vector2(0.0f, -(_posCoordinate[indexStart].y))
                                       : new Vector2(-(_posCoordinate[indexStart].x), 1.0f);

            MonitoringScroll(); // あとは移動したポジションに合わせて処理を行ってもらえばOK
        }

        /// <summary>
        /// スクロール監視処理
        /// </summary>
        private void MonitoringScroll() // 上基点から縦にスクロールするとyは+値/左基点から横にスクロールするとxは-値
        {
            DrawLog("-------------------------------------------------- MonitoringScroll()");
            if (_direction == Direction.Vertical)
            {
                _drawAreaS = _rtContent.anchoredPosition.y;
                if (_drawAreaS < 0) _drawAreaS = 0; // 上端超え補正
            }
            else
            {
                _drawAreaS = _rtContent.anchoredPosition.x;
                if (0 < _drawAreaS) _drawAreaS = 0; // 左端端超え補正
            }

            _drawAreaS = Math.Abs(_drawAreaS);
            DrawLog("*** _drawAreaS : " + _drawAreaS);

            // 終端
            _drawAreaE = _drawAreaS + SizeAreaHorW;
            if (_direction == Direction.Vertical)
            {
                if (_rtContent.rect.height < _drawAreaE) _drawAreaE = _rtContent.rect.height; // 下端超え補正
            }
            else
            {
                if (_rtContent.rect.width < _drawAreaE) _drawAreaE = _rtContent.rect.width; // 右端超え補正
            }
            _drawAreaE = Math.Abs(_drawAreaE);
            DrawLog("*** _drawAreaE : " + _drawAreaE);

            // -------------------------------------------------- 今表示したい行または列の開始終了を出す
            var indexStart = -1;
            var indexEnd = -1;
            float pos;
            float posNext;
            var nextLine = -1;

            for (var i = 0; i < _posCoordinate.Length; i += _line) // 複数ラインの場合端だけチェックする
            {
                // マージン分があるのでマージン内なら0
                if (_drawAreaS < Margin)
                {
                    indexStart = 0;
                    break;
                }
                pos = Math.Abs(_direction == Direction.Vertical ? _posCoordinate[i].y : _posCoordinate[i].x);
                nextLine = i + _line < _posCoordinate.Length - 1 ? i + _line : -1;
                if (nextLine < 0) // 次はない
                {
                    posNext = _direction == Direction.Vertical ? _rtContent.rect.height : _rtContent.rect.width;
                }
                else
                {
                    posNext = Math.Abs(_direction == Direction.Vertical ? _posCoordinate[nextLine].y : _posCoordinate[nextLine].x);
                }
                if (pos <= _drawAreaS && _drawAreaS <= posNext)
                {
                    indexStart = i;
                    break;
                }
            }
            DrawLog("*** indexStart : " + indexStart);

            for (var i = indexStart + _line; i < _posCoordinate.Length; i += _line) // 複数ラインの場合端だけチェックする
            {
                pos = Math.Abs(_direction == Direction.Vertical ? _posCoordinate[i].y : _posCoordinate[i].x);
                nextLine = i + _line < _posCoordinate.Length ? i + _line : -1;
                if (nextLine < 0) // 次はない
                {
                    posNext = _direction == Direction.Vertical ? _rtContent.rect.height : _rtContent.rect.width;
                }
                else
                {
                    posNext = Math.Abs(_direction == Direction.Vertical ? _posCoordinate[nextLine].y : _posCoordinate[nextLine].x);
                }
                if (pos <= _drawAreaE && _drawAreaE <= posNext)
                {
                    // 複数行列あればその最後まで必要
                    indexEnd = 0 == (i % _line) ? i + (_line - 1) : i;
                    indexEnd = Math.Min(_itemType.Length - 1, indexEnd); // 最大数を超えたら補正
                    break;
                }
            }
            DrawLog("*** indexEnd : " + indexEnd);

            DrawLog("--------------------------------------------------");
            UpdateScrollItem(indexStart, indexEnd); // スクロールアイテムの更新
        }

        /// <summary>
        /// スクロールアイテム更新
        /// </summary>
        private void UpdateScrollItem(int indexStart, int indexEnd)
        {
            DrawLog("-------------------------------------------------- UpdateScrollItem : indexStart " + indexStart + " > indexEnd " + indexEnd);
            // バグって描画位置設定できてなかったら抜ける
            if (indexStart < 0 || indexEnd < 0)
            {
                DrawLog("!!! Index collection failed !!!");
                return;
            }

            // 外出たチェック:スペース分の余白含む
            for (var i = 0; i < _reusableItems.Length; i++)
            {
                for (var j = 0; j < _maxItemReusable[i]; j++)
                {
                    _reusableItems[i][j].CheckActive(_direction, _drawAreaS - SpaceXorY, _drawAreaE + SpaceXorY);
                }
            }

            for (int i = indexStart; i <= indexEnd; i++)
            {
                // すでに設置済なら抜ける
                if (_reusableItems[_itemType[i]].Any(q => q.Index == i)) continue;
                // Indexが-1のやつを拾う
                var item = _reusableItems[_itemType[i]].FirstOrDefault(q => q.Index == -1);
                if (item == null)
                {
                    DrawLog("Unused items not found.", true);
                    continue; // 見つからなければ抜ける(何もしない)
                }

                item.gameObject.SetActive(true); // 表示
                item.RectTransform.anchoredPosition = _posCoordinate[i];
                DrawListItem(i, item);
            }
            DrawLog("--------------------------------------------------");
        }
    }
}
