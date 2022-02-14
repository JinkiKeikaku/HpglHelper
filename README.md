# HpglHelper
HPGL読み取り保存ライブラリです。 
HpglViewerはこのライブラリを使ったサンプルです（ただし、わかりやすさを重視するために表示速度が遅く、操作性も悪いです）。
## 読み込み
`HpglReader`のインスタンスを作り、`Read`メソッドを呼びます。
~~~
var reader = new HpglReader();
using var r = new StreamReader(path);
reader.Read(r, 420, 297, 0.025);
~~~
`Read`メソッドの最初の引数は読み込む」ファイルなどのTextReaderオブジェクト、次の２つは用紙の大きさ（mm）、最後はhpglの解像度でひとまず0.025(単位はmm)を設定してください。
`reader.Shapes`に読み込んで解析した図形が入ります。

読み込まれた図形の座標や大きさの単位はmmで角度は度です。

円と円弧には`Flatness`という扁平率を表すメンバがありますが、hpglの図形には扁平率はありません。この扁平率はhpglのコマンドのIPとSCを使い座標系の縦横比を変えると円が楕円になるために追加しました。実際には縦横比を変えたファイルはあまり無いかもしれません（今のところ見たことがありません）。扁平率は保存時は効果がないので注意してください。

その他詳細はサンプルの[HpglViewer](https://github.com/JinkiKeikaku/HpglHelper/tree/master/HpglViewer)を見てください。
