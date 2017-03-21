# DumbQQ [![Build status]( 	https://img.shields.io/appveyor/ci/TJYSunset/DumbQQ.svg?style=flat)](https://ci.appveyor.com/project/TJYSunset/DumbQQ) [![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://raw.githubusercontent.com/TJYSunset/DumbQQ/master/LICENSE) [![NuGet]( 	https://img.shields.io/nuget/v/Sunsetware.DumbQQ.svg)](https://www.nuget.org/packages/Sunsetware.DumbQQ/)

本项目是对[@ScienJus](https://github.com/scienjus/)的[SmartQQ](https://github.com/scienjus/smartqq)的C#迁移（以及更多）。感谢对原项目做出贡献的各位的付出。

**⚠️由于个人智商有限，无法做到良好的版本管理，本项目结构随时可能发生变动，无法接受的话请务必不要手贱乱升级NuGet包**

## 功能

DumbQQ可以：

+ 收发文字消息
+ 获取好友、群、讨论组、好友分组和最近会话的列表

DumbQQ不可以：

+ 包括但不限于收发图片、结构化消息、特殊消息
+ 上传/下载/发送文件
+ 所有想想就不可能的事

以下功能在日程上但是暂未实现：

+ 收发系统表情（例：/微笑）

特色功能：

+ 尽量隐藏了底层API的杂乱架构
+ 可调节的缓存时间
+ 导出cookie便于下次快速登录
+ 便捷的检测消息是否提到我
+ 完善的XML注释
+ 掉线检测
+ 以字节数组形式接收二维码

## 文档

[项目wiki](https://github.com/TJYSunset/DumbQQ/wiki)

## FAQ

### 为啥突然消息发不出去了？

请至[SmartQQ](http://w.qq.com)检查消息是否可以发出，或者至[QQ安全中心](http://aq.qq.com/007)检查账户是否被冻结。

### 总是被冻结，怎么办？

请避免：

+ 短时间内大量发送消息
+ 在非常用网络环境下使用
+ 使用新号
+ 违反ToS的行为
+ 作死

如果有主要功能发生异常（如发出消息别人收不到但可正常接收消息）但别人无此问题，请自认倒霉并修养三日再开。

## Issues

本人并没有全面地测试该项目。如果使用中发现问题欢迎提Issue打脸。

确定是SmartQQ接口自身的问题请提到[原项目](https://github.com/scienjus/smartqq)，谢谢合作。

### 已知问题

~~所有问题都是已知问题~~

## Forks

[DumbQQ-Core](https://github.com/rmbadmin/DumbQQ-Core) maintained by [@rmbadmin](https://github.com/rmbadmin)