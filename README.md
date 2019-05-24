# LogTrace 安装步骤

## 1. 安装步骤

* 命令行: nuget install logtrace

* 使用vs自带的nuget管理工具

## 2. web.config 配置

```xml
<system.diagnostics>
  <trace autoflush="false" useGlobalLock="false">
    <listeners>
      <clear />
      <add name="logs" type="LogServiceTraceListener,LogTrace"/>
    </listeners>
  </trace>
</system.diagnostics>
```

## 3. Global.asax文件配置

### 新版本不需要配置，已移除

## 4. 使用方法

* System.Diagnostics.Trace.TraceInformation 提示信息
* System.Diagnostics.Trace.TraceWarning 警告信息
* System.Diagnostics.Trace.TraceError 错误信息
* System.Diagnostics.Trace.WriteLine 调试信息

### *例如我想打印每个方法的执行时间*

* System.Diagnostics.Trace.WriteLine(1000,"方法名称")

## 5.下载日志查看工具到本地

[日志查看工具](https://logcdn.oss-cn-hangzhou.aliyuncs.com/LogView.html)

## 6. 关于阿里云采集配置

阿里云内网，同一区域直接安装LogTail组件

其他云厂商或者自建IDC，需要到目录C:\LogtailData\users创建UID文件

[阿里云LogTail部署文档](https://help.aliyun.com/document_detail/49006.html?spm=a2c4g.11186623.6.601.22272f6aX5lhvB)

[UID文件创建文档](https://help.aliyun.com/document_detail/49007.html?spm=a2c4g.11186623.2.20.6b715332tVE8tR)
