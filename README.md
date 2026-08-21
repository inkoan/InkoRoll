# Unity用お手軽スクロールビューInkoRoll
ちょっとしたリストなどを可能な限り手軽に実装できるようにした(つもりの)スクロールビューです
  - リスト要素は必要最低限の数を使いまわして表示します
  - リスト要素のサイズが固定の場合、複数行列表示に対応しています
  - サイズの違うリスト要素を複数登録して表示することもできます(この場合の複数行列は不可)
  - 可変するリスト要素のスクロールビューもできます(この場合の複数行列は不可)

## 作成時のUnityバージョン
Unity6000.3.18f1

## 構成
Assets\
　└ Prefabs // サンプル用のプレハブ入ってます\
　└ Scenes\
　　└ InkoRoll.unity // 実装サンプル用シーン:こちらを実行して動作をご確認いただけます\
　└ Scripts\
　　└ InkoRoll\
　　　└ InkoRoll.cs // InkoRoll本体\
　　　└ InkoRollitem.cs // リスト要素にアタッチするソース\
　　└ InkoRollItemSample.cs // 実装サンプルのリスト要素用ソース\
　　└ UseInkoRollSample.cs // 実装サンプルソース\
　└ TextMesh Pro

## 使い方
  1. Unityのメニューから通常のスクロールビューを作成します
  2. スクロールビューと同じ階層にInkoRollをアタッチします
     スクロール方向、余白、可変にするか、ログを出すかどうかを設定します
  3. リスト要素を作成してInkoRollItemをアタッチします\
      中身を更新するためのソースを作成してアタッチし、そのソースがアタッチされているオブジェクトをInkoRollItemのContentにアタッチします\
      また、ボタンがある場合ボタンも登録します\
      複数のボタンを登録可能です(なくても良い)
  5. 使用するリスト要素をInkoRollItemにアタッチします
  6. スクロールビューを参照しているソース側でデータのセットと、表示時の挙動、ボタン押下時の挙動などを登録します\
     要素が可変する場合、要素が確定する最低限の処理を登録します\
     UseInkoRollSample.csではリスト要素数を指定しての再作成、MoveToの実行ボタン、\
     縦横の切り替えなども実装しているのでそちらもご覧ください
```
using Inkoan.InkoRoll;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private InkoRoll _inkoRoll_type1;
    [SerializeField] private InkoRoll _inkoRoll_type2;
    [SerializeField] private InkoRoll _inkoRoll_type3;

    private List<InkoRollItemSampleData> _data1 = new();
    private List<InkoRollItemSampleData> _data2 = new();
    private List<InkoRollItemSampleData> _data3 = new();
    private List<int> _itemType = new();

    /// <summary>
    /// 開始時処理
    /// </summary>
    private void Start()
    {
        // --------------------------------------------------
        // 同じリスト要素で1～複数列のスクロールビュー
        // --------------------------------------------------
        // 表示に使うデータを用意します
        // ここではデータが何番目かを表すテキストだけを持つクラスを50個生成しています
        for (int i = 0; i < 50; i++)
        {
            _data1.Add(new InkoRollItemSampleData("title" + i));
        }
        _inkoRoll_type1.Setup(
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

        // --------------------------------------------------
        // 複数のリスト要素が混在しているスクロールビュー
        // --------------------------------------------------
        // 表示に使うデータを用意します
        // ここではデータが何番目かを表すテキストだけを持つクラスを50個生成しています
        for (int i = 0; i < 50; i++)
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
        _inkoRoll_type2.Setup(
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

        // --------------------------------------------------
        // リスト要素の内容でサイズが変わるスクロールビュー
        // --------------------------------------------------
        // 表示に使うデータを用意します
        // ここではデータが何番目かを表すテキストだけを持つクラスを50個生成しています
        for (int i = 0; i < 50; i++)
        {
            // 可変サイズ用にテキスト部に5～50文字のAを追加
            var rand = UnityEngine.Random.Range(5, 50);
            var txtAdd = " ";
            for (var t = 0; t < rand; t++) txtAdd += "A";
            _data3.Add(new InkoRollItemSampleData("title" + i + txtAdd));
        }
        _inkoRoll_type3.Setup(
            _data3.Count, // データの個数
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
            //    アイコンなどの場合、ロードまでしてしまうと多分めちゃくちゃ重くなると思うので
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

```
## その他
- 任意の要素に表示を移動するMoveToあります
- データの更新があってスクロールビューを更新する場合は再度Setupを実行してください
- 現時点では誰でも使えるようUniRxやR3には対応していないので必要に応じて対応を入れてください
