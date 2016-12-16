# DevExpress 起始點重合線圖
畫出起始點重合的折線圖
![snap photo](https://github.com/DaHao/devexpress_overlapping_startValue_chart/blob/master/snap.png)

## 環境
  + DevExpress 15.1

## 說明
畫股市線圖時，常會需要比較兩條線的走勢，但是如果以DevExpress直接畫的話，由於DevExpress並不提供將多條折線的原點接在一起的功能，所以會看不出哪條線的幅度是比較高的。

最初的想法是將兩條線取最高及最低值，然後直接設定軸線，後來發現這樣不行，因為兩條線分別屬於不同Y軸，取同一最高最低值並沒有意義。

重點在於兩條線的Y軸刻度所代表的比例不一。

要解決這個問題，可以由漲跌幅的公式來推導：
漲跌幅 = (成交價 - 參考價) / 參考價

參考價在這裡就是以原點(startValue)來代表。

要求漲跌幅 == 1，因為漲跌幅以百分比表示，所以代0.01

因為成交價一定是基於startValue之上或之下，所以成交價可表示成(startValue ± N)

0.01 = (startValue ± N - startValue) / startValue

0.01 = ±N / startValue

±N = startValue * 0.01

所以可以得出漲跌幅要上升1，需要多少Y軸值
```C#
unitValue = startValue * 0.01
```
---
如果只是要讓原點在中間，可以單純抓一個比例，直接設定上下限
```C#
// 比例抓15
double s1_axisH = startValue + unitValue * 15;
double s1_axisL = startValue + unitValue * 15;

AxisY.WholeRange.SetMinMaxValues(s1_axisL, s1_axisH);
```
但是這樣呈現並不是很理想，有時候會有過多的留白。

最好是可以根據原點對於最高最低值，自動算出比例來。

可以用(最高值or最低值 - 原點值) / 單位值來算出這個比例。
```C#
double s1_proportionH = Math.Round((s1_HighValue - startValue) / unitValue, 0, MidpointRounding.AwayFromZero);
double s1_proportionL = Math.Round((s1_startValue - s1_LowValue) / unitValue, 0, MidpointRounding.AwayFromZero);
```

可適用於兩條以上的Series，算出所有線的比例之後挑最高值設定上下限即可。
