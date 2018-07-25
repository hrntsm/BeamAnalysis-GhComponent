# BeamAnalysis
grasshopperで梁の解析をするコンポーネントです。MITライセンスで公開しています。詳細はLICENSEを見てください。  
It is a component analyzing the beam in grasshopper.
This software is released under the MIT License, see LICENSE.

![コンポーネント画像](https://github.com/hiro-n-rgkr/BeamAnalysis/blob/master/BeamAnalysis/BeamAnalysis/images/md_image.jpg)

#### コンポーネントについて 
+ コンポーネント一覧

![コンポーネント一覧](https://github.com/hiro-n-rgkr/BeamAnalysis/blob/master/BeamAnalysis/BeamAnalysis/images/ListOfComponents.JPG)

+ Analysisタブ
  + コンポーネントごとの荷重の条件に対して曲げモーメントと変位、許容応力度と検定比を出力します。
  許容応力度は学会式ではなく、告示式で計算しています。検定比は最大曲げモーメントに対して計算しています。
  + Any Moment コンポーネント：任意の直接入力のモーメントに対して計算
  + Centralized Load コンポーネント：中央集中荷重に対して計算
  + Trapezoid Load コンポーネント：台形分布荷重に対して計算

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

#### 開発状況
+ ~2018/07/25
  + ver0.1.02 ライセンスに関する表記を追加(MITライセンス) 
+ ~2018/07/22
  + ver0.1.01 台形分布荷重の計算のバグを修正。 
+ ~2018/07/16
  + ver0.1.00 直接入力のモーメントに対して計算するコンポーネント追加。
  リリース用に中身を若干整理し、masterにマージ
+ ~2018/07/15
  + ver0.0.04 モーメント図出力コンポーネント実装とアイコンのリソースデータ化
+ ~2018/07/11
  + ver0.0.03 角型断面の計算に対応とそれに伴うパラメータの整理
+ ~2018/07/10
  + ver0.0.02 コンポーネント名の整理と台形分布荷重での計算に対応しました。
+ ~2018/07/07
  + ver0.0.01 アイコンの設定と、許容曲げ応力度を告示の式で計算するようにしました。  
+ ~2018/03/29   
  + 中央集中荷重の単純梁の応力とたわみの計算するコンポーネントと  
H型断面を入れると断面性能を計算して出力するコンポーネントの
二つが含まれています。  
developでは、UIの改善（コンテキストメニューやラジオボタンの追加）のための試行錯誤中
