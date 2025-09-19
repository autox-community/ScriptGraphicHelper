# ScriptGraphicHelper

**一款简单好用的图色助手, 快速生成多种脚本开发工具的图色格式代码**

<br/>

## 功能

- 模拟器模式: 调用模拟器命令行进行截图, 无需手动连接adb(适用于雷电、夜神、逍遥)
- AJ连接模式: 调用aj的tcp调试端口进行截图(需要安装autojs.pro 8, 并开启调试服务和悬浮窗)
- AT连接模式: 调用astator的tcp调试端口进行截图(需要安装[astator](https://gitee.com/astator/astator), 并开启调试服务和悬浮窗)
- ADB连接模式: 与设备通过adb进行截图(usb/wifi)
- 句柄模式: 调用大漠进行前后台截图
- 支持大漠、按键、触动、autojs、easyclick、astator以及自定义的格式代码生成
- 多分辨率适配的测试和代码生成(锚点格式)

<br/>

## 支持平台

- win：  所有功能

- mac： aj连接模式, tcp模式(mac上需要把scriptGraphichelper改为可执行文件, chmod +x filename)

