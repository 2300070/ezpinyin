# EzPinyin

#### 介绍

适用于.Net Framework及.Net Standard的汉字转拼音组件。主要的设计目是实现对字符串进行快速的拼音转换，从而帮助开发人员在做数据开发时可以简单快速的建立拼音索引。

#### 特性

- 极快的处理速度，具体请参考性能章节。
- 小巧简洁，包含字典、词典内容在内，整个文件大小约1M左右，无其他项目依赖，不会造成项目臃肿。
- 涵盖Unicode平面的基本汉字及补充汉字、汉字扩展区A的全部汉字，涵盖扩展区B、C、D、E、F、G的大部分汉字，总计约覆盖九万汉字、五十万词汇。
- 更好的字典，通过重新对字典中的多音字进行分析优化，使得此项目对于名称的处理具备更好的效果。
- 支持在运行时重定义拼音之外，还可以用文件的方式自定义汉字或者词汇的拼音，从而达到修正或者补充的效果。
- 支持简体与繁体。

#### 性能

对常见字符进行处理时，此项目性能如下：

| 项目    | 性能(字/秒) |
|-------|----------|
| 检索拼音字符串 | 3000万      |
| 检索拼音首字母 | 5000万      |
| 检索拼音数组  | 6000万     |


#### 兼容性

.Net Framework 2.0及以上 或者.Net Standard 2.0及以上。

#### 线程安全性

如注释文档中没有额外说明，此项目中公开的方法都是线程安全的。

#### 引用本项目

除了直接下载项目之外，还可以通过nuget来添加并使用本项目。本项目的nuget地址为 https://www.nuget.org/packages/EzPinyin/ 。
可以通过visual studio的界面来搜索“ezpinyin”来引用本项目，也可以在nuget管理控制台输入命令来添加项目引用：“Install-Package EzPinyin”。



#### 示例


```csharp
//获得拼音完整字符串
PinyinHelper.GetPinyin("重庆银行川藏大区成都分行朝阳区长厦路支行重工大厦办事处九楼董事长办公室");//chong qing yin hang chuan zang da qu cheng du fen hang chao yang qu chang xia lu zhi hang zhong gong da sha ban shi chu jiu lou dong shi zhang ban gong shi

//获得拼音的首字母
PinyinHelper.GetInitial("㐀㲒䔤䶵𠀀𠧄𡎈𡵌𢜐𣃔𣪘𤑜𤸠𥟤𦆨𦭬𧔰𧻴𨢸𩉼𩱀𪜀𪻐𫜴𫝀𫠝𫠠𫿰𬟀𬺰𭡫𮈦𰀀𱍊");//qbpchgfcwdgzmlldjlkspdc𫜴wbyst𬺰𭡫lzc

//获得拼音信息的数组
PinyinHelper.GetArray(text);//string[]

//重新定义拼音信息
PinyinHelper.Define("𫜴", "lun");//拼音是我胡诌的
PinyinHelper.Define("𫜴吧", new[]{"lun", "biu"});//拼音是我胡诌的

//加载自定义的拼音配置文件
PinyinHelper.LoadFrom("custom_file.txt");//文件内容格式请参考自定义配置章节。

//跳过文件名称，直接加载自定义配置文件的内容
PinyinHelper.Load("𫜴 liu\n𫜴吧 lun biu");//内容格式请参考自定义配置章节。
```


更多请参考EzPinyin.UnitTest项目中的代码。

#### 自定义配置

你可以自定义一个或者数个文件名称包含“dict”关键字的文本文件作为自定义字典文件，扩展名不一定必须是“.txt”，将这些文件置于应用程序集相同的目录即可发挥作用，具体的定义方法非常简单，请参考dictionary.txt模板。

#### 免责声明

如你所见，这个项目不是为了纯粹的拼音转换而开发的。由于多音字在汉字的不同语义下使用十分复杂，因而这个项目在处理多音字时仍然会出现错误，尤其不适用于以阅读为目的的拼音转换。任何使用本项目的开发人员应该理解这个问题并谨慎使用，此项目的开发者不对此项目的任何功能或者问题（或损失）提供任何（直接或间接）担保与承诺。

#### 特别致谢

本项目的开发者仅贡献了核心的查询与优化算法，主要的汉字数据则自互联网收集，在此要特别感谢此项目的直接数据来源：

- 百度汉语 http://hanyu.baidu.com
- 汉典 http://zdic.net
- 叶典 http://yedict.com

此外，此项目也吸收了下列站点的部分内容作为参考，一并致谢。
- 词典网 http://cd.cidianwang.com
- 汉文学网 http://cd.hwxnet.com
- 必应词典 https://cn.bing.com/dict/
- 百度百科 http://baike.baidu.com
- 国学大师 http://www.guoxuedashi.net/

#### 支持

此项目当前没有任何盈利计划，若你认为她对你有所帮助，可以考虑支持叶典(http://yedict.com/zhichi.htm) 或其他相关网站。
