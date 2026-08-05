using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// **************************************************
/// 実際に表示するためのリスト要素処理のサンプルです
/// リスト要素を可変にする場合、要素のサイズを確定する
/// 処理を実行します
/// 実際に表示したいサイズが決まりさえすればばよいので
/// 最低限のものをセットします
/// このサンプルの場合、テキストの設定のみです
/// 複数のアイコンなどを可変で設定する場合
/// 実際のアイコンをロードすると処理が遅くなるので
/// 一時的に同サイズの空箱だけ表示するなど
/// 工夫する必要があります
/// **************************************************
public class InkoRollItemSample : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _txtTitle;
    [SerializeField] private Button[] _btns;

    /// <summary>
    /// リスト要素の表示処理
    /// </summary>
    /// <param name="title"></param>
    public void SetData(string title)
    {
        _txtTitle.text = title;
        foreach (var btn in _btns)
        {
            btn.interactable = true;
        }
    }

    /// <summary>
    /// 押された時にボタンをオフするだけの処理
    /// </summary>
    /// <param name="buttonIndex"></param>
    public void SetButtonOff(int buttonIndex)
    {
        if (_btns.Length < buttonIndex && _btns[buttonIndex] != null) return;
        _btns[buttonIndex].interactable = false;
    }

    /// <summary>
    /// リスト要素サイズが可変の場合にサイズを確定するための処理
    /// </summary>
    /// <param name="title"></param>
    public void TrySetData(string title)
    {
        _txtTitle.text = title;
    }
}
