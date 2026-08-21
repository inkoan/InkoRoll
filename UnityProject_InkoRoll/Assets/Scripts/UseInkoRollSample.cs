using Inkoan.InkoRoll;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// **************************************************
/// 使用例です
/// スクロールビューを持っているクラス内で
/// 処理は全てやりくりします
/// SetData時に表示された、ボタンを押したなどの
/// アクションを登録しておくとアクションが実行されて
/// 対象のリスト要素が渡ってくるので
/// 自身で作成したリスト要素の操作クラスを受け取って
/// 処理します
/// **************************************************
public class UseInkoRollSample : MonoBehaviour
{
    [SerializeField] private GameObject _objAnotherSample;
    [SerializeField] private Button _btnChangeAnotherSample;

    [SerializeField] private InkoRoll _inkoRoll1;
    [SerializeField] private TMP_InputField _numData1;
    [SerializeField] private Button _btnSetCreateScrollView1;
    [SerializeField] private TMP_InputField _ifMoveTo1;
    [SerializeField] private Button _btnMoveTo1;

    [SerializeField] private InkoRoll _inkoRoll2;
    [SerializeField] private TMP_InputField _numData2;
    [SerializeField] private Button _btnSetCreateScrollView2;
    [SerializeField] private TMP_InputField _ifMoveTo2;
    [SerializeField] private Button _btnMoveTo2;

    [SerializeField] private InkoRoll _inkoRoll3;
    [SerializeField] private TMP_InputField _numData3;
    [SerializeField] private Button _btnSetCreateScrollView3;
    [SerializeField] private TMP_InputField _ifMoveTo3;
    [SerializeField] private Button _btnMoveTo3;

    private List<InkoRollItemSampleData> _data1 = new ();
    private List<InkoRollItemSampleData> _data2 = new ();
    private List<InkoRollItemSampleData> _data3 = new ();
    private List<int> _itemType = new();

    /// <summary>
    /// 破棄時
    /// </summary>
    private void OnDestroy()
    {
        _btnChangeAnotherSample.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 開始時処理
    /// </summary>
    private void Start()
    {
        // サンプル切り替え
        _btnChangeAnotherSample.onClick.AddListener(
            () =>
            {
                _objAnotherSample.SetActive(true);
                gameObject.SetActive(false);
            }
        );

        // ---------- サンプルスクロールビュー1個目:通常タイプ
        CreateSampleScrollView1();
        // 入力したindexの要素に移動する処理
        _btnMoveTo1.onClick.AddListener(
            () =>
            {
                if (!string.IsNullOrEmpty(_ifMoveTo1.text))
                {
                    _inkoRoll1.MoveTo(int.Parse(_ifMoveTo1.text));
                }
            }
        );
        // 入力したデータ数で再作成
        _btnSetCreateScrollView1.onClick.AddListener(
            () =>
            {
                if (!string.IsNullOrEmpty(_numData1.text))
                {
                    CreateSampleScrollView1();
                }
            }
        );

        // ---------- サンプルスクロールビュー2個目:混在タイプ
        CreateSampleScrollView2();
        // 入力したindexの要素に移動する処理
        _btnMoveTo2.onClick.AddListener(
            () =>
            {
                if (!string.IsNullOrEmpty(_ifMoveTo2.text))
                {
                    _inkoRoll2.MoveTo(int.Parse(_ifMoveTo2.text));
                }
            }
        );
        // 入力したデータ数で再作成
        _btnSetCreateScrollView2.onClick.AddListener(
            () =>
            {
                if (!string.IsNullOrEmpty(_numData2.text))
                {
                    CreateSampleScrollView2();
                }
            }
        );

        // ---------- スクロールビュー3個目:可変タイプ
        CreateSampleScrollView3();
        // 入力したindexの要素に移動する処理
        _btnMoveTo3.onClick.AddListener(
            () =>
            {
                if (!string.IsNullOrEmpty(_ifMoveTo3.text))
                {
                    _inkoRoll3.MoveTo(int.Parse(_ifMoveTo3.text));
                }
            }
        );
        // 入力したデータ数で再作成
        _btnSetCreateScrollView3.onClick.AddListener(
            () =>
            {
                if (!string.IsNullOrEmpty(_numData3.text))
                {
                    CreateSampleScrollView3();
                }
            }
        );
    }

    /// <summary>
    /// サンプルスクロールビュー1を作成する
    ///    ・複数列用:列数の変更はインスペクタで
    /// </summary>    
    private void CreateSampleScrollView1()
    {
        _itemType.Clear();
        _data1.Clear();
        // 表示に使うデータを用意します
        // ここではデータが何番目かを表すテキストだけを持つクラスを指定の数生成しています
        int num = int.Parse(_numData1.text);
        for (int i = 0; i < num; i++)
        {
            _data1.Add(new InkoRollItemSampleData("title" + i));
        }
        // スクロールビューのデータの設定:データの個数/データが表示されるときの処理/ボタン押したときの処理
        _inkoRoll1.Setup(
            _data1.Count, // データの個数
                          // 要素が描画される場合の処理:
                          //    要素にアタッチされた表示や挙動のための処理を取得して
                          //    実際にどう描画するかなどの指示をここで出します
            (index, cell) =>
            {
                cell.UpdateIndex(index);
                InkoRollItemSample sampleItem = cell.GetCellView<InkoRollItemSample>();
                if (sampleItem != null)
                {
                    sampleItem.SetData(_data1[index].Title);
                }
            },
            // 要素内のボタンが押された場合:
            //    何番目の要素が押されたか/要素内のボタンの内登録されている何個目のボタンが押されたかが
            //    返ってきますので、それに合わせて処理を行います
            //    サンプルではただ押されたボタンのInteractableをオフにするだけの処理をしています
            (index, buttonIndex, cell) =>
            {
                InkoRollItemSample sampleItem = cell.GetCellView<InkoRollItemSample>();
                if (sampleItem != null)
                {
                    sampleItem.SetButtonOff(buttonIndex);
                }
            }
        );
    }

    /// <summary>
    /// サンプルスクロールビュー2を作成する
    ///    ・複数要素混在型:列数が2以上でも無視されます
    /// </summary>
    private void CreateSampleScrollView2()
    {
        _itemType.Clear();
        _data2.Clear();
        // 表示に使うデータを用意します
        // ここではデータが何番目かを表すテキストだけを持つクラスを指定の数生成しています
        int num = int.Parse(_numData2.text);
        for (int i = 0; i < num; i++)
        {
            _data2.Add(new InkoRollItemSampleData("title" + i));
            // サンプルでは3個のプレハブを登録しているので
            // ここで適当に0～2のタイプを割り振っています
            var rand = UnityEngine.Random.Range(0, 100);
            if (rand < 10)
            {
                _itemType.Add(1);
            }
            else if (rand < 50)
            {
                _itemType.Add(2);
            }
            else
            {
                _itemType.Add(0);
            }
        }
        // スクロールビューのデータの設定:混在型はindex毎にどの要素を使うかの配列/データが表示されるときの処理/ボタン押したときの処理
        _inkoRoll2.Setup(
            // ここで各要素がどのタイプであるかの配列を渡します
            _itemType.ToArray(),
            // 要素が描画される場合の処理:
            //    要素にアタッチされた表示や挙動のための処理を取得して
            //    実際にどう描画するかなどの指示をここで出します
            (index, cell) =>
            {
                cell.UpdateIndex(index);
                InkoRollItemSample sampleItem = cell.GetCellView<InkoRollItemSample>();
                if (sampleItem != null)
                {
                    sampleItem.SetData(_data2[index].Title);
                }
            },
            // 要素内のボタンが押された場合:
            //    何番目の要素が押されたか/要素内のボタンの内登録されている何個目のボタンが押されたかが
            //    返ってきますので、それに合わせて処理を行います
            //    サンプルではただ押されたボタンのInteractableをオフにするだけの処理をしています
            (index, buttonIndex, cell) =>
            {
                InkoRollItemSample sampleItem = cell.GetCellView<InkoRollItemSample>();
                if (sampleItem != null)
                {
                    sampleItem.SetButtonOff(buttonIndex);
                }
            }
        );
    }

    /// <summary>
    /// サンプルスクロールビュー3を作成する
    ///    ・要素可変型:列数が2以上でも無視されます
    /// </summary>
    private void CreateSampleScrollView3()
    {
        _itemType.Clear();
        _data3.Clear();
        // 表示に使うデータを用意します
        // ここではデータが何番目かを表すテキストだけを持つクラスを指定の数生成しています
        int num3 = int.Parse(_numData3.text);
        for (int i = 0; i < num3; i++)
        {
            // 可変サイズ用にテキスト部に5～50文字のAを追加
            var rand = UnityEngine.Random.Range(5, 50);
            var txtAdd = " ";
            for (var t = 0; t < rand; t++) txtAdd += "A";
            _data3.Add(new InkoRollItemSampleData("title" + i + txtAdd));
        }
        _inkoRoll3.Setup(
            _data3.Count, // データの個数
                          // 要素が描画される場合の処理:
                          //    要素にアタッチされた表示や挙動のための処理を取得して
                          //    実際にどう描画するかなどの指示をここで出します
            (index, cell) =>
            {
                cell.UpdateIndex(index);
                InkoRollItemSample sampleItem = cell.GetCellView<InkoRollItemSample>();
                if (sampleItem != null)
                {
                    sampleItem.SetData(_data3[index].Title);
                }
            },
            // 要素内のボタンが押された場合:
            //    何番目の要素が押されたか/要素内のボタンの内登録されている何個目のボタンが押されたかが
            //    返ってきますので、それに合わせて処理を行います
            //    サンプルではただ押されたボタンのInteractableをオフにするだけの処理をしています
            (index, buttonIndex, cell) =>
            {
                InkoRollItemSample sampleItem = cell.GetCellView<InkoRollItemSample>();
                if (sampleItem != null)
                {
                    sampleItem.SetButtonOff(buttonIndex);
                }
            },
            // 要素のサイズがデータの内容によって変わる場合の処理:
            //    リスト要素のサイズを確定できる最低限の処理を登録します
            //    この処理はスクロールビューを描画する直前に画面外で各要素が一度描画され
            //    要素のサイズを確定してからスクロールビュー内での座標をマッピングします
            //    スクロールビューを表示しながらサイズを変更することはできないことにご注意ください
            //    アイコンなどの場合、ロードまでしてしまうと多分めちゃくちゃ重くなるので
            //    アイコンと同サイズの空要素を表示させるだけにするなど工夫してください
            //    サンプルではタイトルのテキストがランダムな長さになっているので
            //    それを一度表示させてサイズを確定しています
            (index, cell) =>
            {
                InkoRollItemSample sampleItem = cell.GetCellView<InkoRollItemSample>();
                if (sampleItem != null)
                {
                    sampleItem.TrySetData(_data3[index].Title);
                }
            }
        );
    }
}

/// <summary>
/// 仮表示用データクラス
/// </summary>
public class InkoRollItemSampleData
{
    public string Title { get; }

    public InkoRollItemSampleData(string title)
    {
        Title = title;
    }
}
