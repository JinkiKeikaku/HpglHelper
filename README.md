HpglHelper
---
HPGL読み取り保存ライブラリ。 
使い方の詳細はサンプルの[HpglViewer](https://github.com/JinkiKeikaku/HpglHelper/tree/master/HpglViewer)を見てください。

# 読み込み
`HpglReader`のインスタンスを作り、`Read`メソッドを呼びます。
~~~
var reader = new HpglReader();
using var r = new StreamReader(path);
reader.Read(r, 420, 297, 0.025);
~~~
`Read`メソッドの最初の引数は読み込むファイルなどのTextReaderオブジェクト、次の２つは用紙の大きさ（mm）、最後はhpglの解像度でひとまず0.025(単位はmm)を設定してください。用紙の大きさは読み込んだファイル内でスケーリングポイントの指定がない場合のP1,P2に影響します（塗りつぶし命令（FT）の間隔のデフォルト値などに利用されます）。  
`reader.Shapes`に読み込んで解析した図形が入ります。  
読み込まれた図形の座標や大きさの単位はmmで角度は度です。  
円と円弧には`Flatness`という扁平率を表すメンバがありますが、hpglの図形には扁平率はありません。この扁平率はhpglのコマンドのIPとSCを使い座標系の縦横比を変えると円が楕円になるために追加しました。実際には縦横比を変えたファイルはあまり無いかもしれません。

# 保存
`HpglWriter`のインスタンスを作り、`Shapes`プロパティに図形を追加した後`Write`メソッドを呼びます。
~~~
using var w = new StreamWriter(path);
writer.Shapes.Add(new HpglCircleShape()
{
    Center = new HpglPoint(50, 50),
    Radius = 50,
});
writer.Write(w);
~~~

## 注意点
- 円などの扁平率を表す`Flatness`プロパティは使えません（常に1.0として扱います）。
- 用紙サイズは保存時に指定できません。
- 出力されるファイルにIPとSCは含まれません。

# 未実装命令
未実装命令は読み込み時に無視します。
- LT(線種)
- 文字関係各種(DV,BL,PB,CS,CA,SS,SA)
- その他プロッタ制御関係など

# サンプルについて
 わかりやすさ優先で手抜きをしているので遅くて使い勝手は悪いです。
- 文字の間隔など簡単にGDI+で表現できないものは省略しました。
- ハッチングはSolidBrushで塗りつぶしています。
