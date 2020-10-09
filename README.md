# BeamAnalysis

grasshopperで梁の解析をするコンポーネントです。MITライセンスのもとで公開しています。詳細はLICENSE.mdを見てください。  
It is a component analyzing the beam in grasshopper.
This software is released under the MIT License, see LICENSE.md

![コンポーネント画像](https://github.com/hiro-n-rgkr/BeamAnalysis/blob/master/BeamAnalysis/BeamAnalysis/images/md_image.jpg)

## コンポーネントについて 
+ コンポーネント一覧

![コンポーネント一覧](https://github.com/hiro-n-rgkr/BeamAnalysis/blob/master/BeamAnalysis/BeamAnalysis/images/ListOfComponents.JPG)

+ Analysisタブ
  + コンポーネントごとの荷重の条件に対して曲げモーメントと変位、許容応力度と検定比を出力します。
  許容応力度は学会式ではなく、告示式で計算しています。検定比は最大曲げモーメントに対して計算しています。
  + Any Moment コンポーネント：任意の直接入力のモーメントに対して計算
  + Centralized Load コンポーネント：中央集中荷重に対して計算
  + Trapezoid Load コンポーネント：台形分布荷重に対して計算
  + Cantilever Ponit Load コンポーネント：片持ち梁先端荷重に対して計算

![Analysisタブコンポーネント一覧](https://github.com/hiro-n-rgkr/BeamAnalysis/blob/master/BeamAnalysis/BeamAnalysis/images/AnalysisTab.JPG)
+ CrossSectionタブ
  + Box Shape コンポーネント：箱型断面の断面に関する諸元を計算
  + H Shape コンポーネント：H型断面の断面に関する諸元を計算
  + L Shape コンポーネント：L型断面の断面に関する諸元を計算

![Analysisタブコンポーネント一覧](https://github.com/hiro-n-rgkr/BeamAnalysis/blob/master/BeamAnalysis/BeamAnalysis/images/CrossSectioTab.JPG)

+ Resultタブ
  + Moment View コンポーネント：入力されたモーメントをRhino上表示。描画は1/4点ごとのモーメントを使用して行っているので、
  曲線状のモーメント分布になる等分布荷重の場合でも、台形の組み合わせで表示しています。

![Analysisタブコンポーネント一覧](https://github.com/hiro-n-rgkr/BeamAnalysis/blob/master/BeamAnalysis/BeamAnalysis/images/ResultTab.JPG)
