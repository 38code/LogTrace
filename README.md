# LogTrace 安装步骤

## 1. 安装步骤

### *命令行: nuget install logtrace*

### *使用vs自带的nuget管理工具*

## 2. web.config 配置

```xml
<system.diagnostics>
  <trace autoflush="false" useGlobalLock="false">
    <listeners>
      <clear />
      <add name="logs" type="LogServiceTraceListener,LogTrace">
    </listeners>
  </trace>
</system.diagnostics>
```

## 3. Global.asax文件配置

### *web工程*

[Global.asax.cs](https://github.com/davidmaster/LogTracer/blob/master/Demo/LogTrace.SampleWeb/Global.asax.cs)

### *api工程*

[Global.asax.cs](https://github.com/davidmaster/LogTracer/blob/master/Demo/LogTrace.SampleApi/Global.asax.cs)

## 4.下载日志查看工具到本地

[日志查看工具](https://logcdn.oss-cn-hangzhou.aliyuncs.com/LogView.html)