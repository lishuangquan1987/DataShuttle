# DataShuttle.WpfSample 完善计划

> 文档日期：2026-04-15  
> 基于对现有代码的完整阅读，梳理当前状态与待完善项。

---

## 一、现状分析

### 已完成的部分

| 模块 | 状态 | 说明 |
|------|------|------|
| 主窗口布局 | ✅ 完成 | 左侧列表 + 右侧状态/脚本双栏，GridSplitter 可拖拽 |
| 穿梭线列表 | ✅ 完成 | 支持增删改，持久化到 JSON 配置文件 |
| 配置弹窗 (SetupView) | ✅ 完成 | 串口/TCP客户端/TCP服务端三种类型切换 |
| 启动/停止控制 | ✅ 完成 | 绑定 ShuttleLine.Run/Stop，状态指示灯联动 |
| Lua 脚本拦截 | ✅ 完成 | NLua 驱动，支持左→右、右→左两路独立脚本 |
| 脚本编辑器 | ✅ 完成 | 内嵌 TextBox，支持测试运行（HEX 输入/输出） |
| 配置持久化 | ✅ 完成 | DefaultConfig<T> 单例 + JSON 文件，启动自动恢复 |
| 退出时停止所有线路 | ✅ 完成 | App.OnExit 调用 StopAll |

### 已知问题 / 明显缺陷

1. **PluginHelper 会崩溃**：`Activator.CreateInstance(x)` 对需要参数的构造函数会抛异常，导致插件名称列表加载失败，SetupView 的下拉框为空。
2. **脚本编辑器无语法高亮**：纯 TextBox，编写 Lua 体验差。
3. **数据流量监控缺失**：运行中看不到实时收发字节数/速率。
4. **ItemConfig 名称缺失**：列表项只显示传输类型+地址，无法自定义名称，多条线路时难以区分。
5. **脚本修改后未自动保存**：脚本内容修改后需要手动触发保存（目前只有增删改配置时才 SaveConfig）。
6. **错误信息只显示最后一条**：ErrorText 被覆盖，历史错误丢失。
7. **SetupView 串口波特率为自由输入**：应改为常用值下拉（9600/19200/38400/57600/115200 等）。
8. **TcpServerName 常量与实际 ITransport.Name 不一致**：`TransportFactory.TcpServerName = "TCP服务端"`，但 `TcpServerTransport.Name` 实际值需核实，可能导致 PluginHelper 返回的名称与 TransportFactory.Create 的 switch 分支不匹配。
9. **窗口标题栏无最小化到托盘**：工具类应用通常需要后台运行。
10. **无日志/历史记录面板**：无法回溯数据传输历史。

---

## 二、完善计划

### Task 1：修复 PluginHelper 崩溃问题（必须先做）

**问题**：`Activator.CreateInstance(x)` 对 `SerialPortTransport`、`TcpClientTransport`、`TcpServerTransport` 这类需要工厂方法创建的类会抛异常。

**方案**：改为硬编码或基于 `ITransport.Name` 静态属性/特性的方式注册传输类型名称，不再反射实例化。

```csharp
// 方案：在 TransportFactory 中直接维护名称列表
public static IReadOnlyList<string> GetTransportNames() =>
    new[] { SerialPortName, TcpClientName, TcpServerName };
```

`PluginHelper.GetPluginNames()` 改为调用 `TransportFactory.GetTransportNames()`，或保留反射但加 try-catch 跳过无法实例化的类型。

---

### Task 2：ItemConfig 增加自定义名称字段

**目的**：列表中每条穿梭线可以有一个用户自定义的备注名称，方便多线路管理。

**改动点**：
- `ItemConfig` 增加 `string Name` 属性（默认值可自动生成，如"线路 1"）
- `ShuttleLineItemViewModel` 暴露 `DisplayName` 属性（优先显示 Name，否则 fallback 到 FromSummary）
- `SetupView` 增加名称输入框
- 列表 ItemTemplate 顶部显示名称

---

### Task 3：脚本修改实时自动保存

**问题**：`ScriptEditorViewModel.FromScript/ToScript` 的 setter 只写入 `_config`，不触发 `App.Config.SaveConfig()`。

**方案**：在 setter 中调用延迟保存（`isSaveImmediately = false`），避免每次按键都写磁盘。

```csharp
set
{
    if (_config != null) { _config.FromInterceptorScript = value; App.Config.SaveConfig(false); }
    OnPropertyChanged();
}
```

---

### Task 4：修复串口波特率为下拉选择

**改动点**：
- `SetupViewModel` 增加 `BaudRates` 属性（常用值列表）
- `SetupView` 串口配置区的波特率改为 `ComboBox`，`IsEditable="True"` 保留自定义输入能力

---

### Task 5：运行状态面板增加流量统计

**目的**：显示左→右、右→左的实时字节数和包数。

**方案**：
- `ShuttleLineItemViewModel` 增加 `FromByteCount`、`ToByteCount`、`FromPacketCount`、`ToPacketCount` 属性
- 在 `Start()` 中，通过 `AddFromIntercept` / `AddToIntercept` 注入统计 Action（在 Lua 拦截之后叠加一个计数器）
- 或在 `ShuttleLine` 层面扩展统计事件（需改核心库，侵入性较大，建议在 WpfSample 层叠加）
- UI 在状态面板中增加两行计数显示

---

### Task 6：错误信息改为日志列表

**目的**：保留历史错误，不被新错误覆盖。

**方案**：
- `ShuttleLineItemViewModel` 将 `ErrorText` 改为 `ObservableCollection<string> Logs`
- 状态面板下方增加一个只读 `ListBox` 显示日志，最多保留 200 条，自动滚动到底部
- 日志条目带时间戳前缀

---

### Task 7：脚本编辑器增加语法高亮（可选，优先级低）

**方案**：引入 `AvalonEdit`（ICSharpCode.AvalonEdit NuGet 包），替换 `TextBox`，配置 Lua 语法定义文件（`.xshd`）。

**改动点**：
- `DataShuttle.WpfSample.csproj` 添加 `ICSharpCode.AvalonEdit` 包引用
- `ScriptEditorView.xaml` 替换两个 `TextBox` 为 `avalonedit:TextEditor`
- 添加 Lua.xshd 语法高亮定义文件（作为嵌入资源）
- `ScriptEditorViewModel` 适配（AvalonEdit 不直接支持 MVVM 绑定，需要附加行为或 code-behind 辅助）

---

### Task 8：最小化到系统托盘（可选，优先级低）

**目的**：关闭窗口时不退出程序，保持后台数据转发。

**方案**：
- 引入 `Hardcodet.NotifyIcon.Wpf` 包
- `App.xaml` 中添加 `TaskbarIcon` 资源
- `MainWindow` 重写 `OnClosing`，改为隐藏窗口而非退出
- 托盘菜单提供"显示"/"退出"选项

---

## 三、优先级排序

| 优先级 | Task | 理由 |
|--------|------|------|
| P0（阻塞性 Bug） | Task 1：修复 PluginHelper | 下拉框为空，无法正常使用 |
| P1（核心体验） | Task 2：自定义名称 | 多线路管理必需 |
| P1（核心体验） | Task 3：脚本自动保存 | 防止数据丢失 |
| P1（核心体验） | Task 4：波特率下拉 | 减少配置错误 |
| P2（功能增强） | Task 5：流量统计 | 运行监控 |
| P2（功能增强） | Task 6：日志列表 | 问题排查 |
| P3（体验优化） | Task 7：语法高亮 | 脚本编写体验 |
| P3（体验优化） | Task 8：托盘 | 后台运行 |

---

## 四、不在本次计划内的事项

- 核心库（DataShuttle / DataShuttle.Core / DataShuttle.Transports）的改动
- 单元测试
- 多语言/国际化
- 安装包打包

---

## 五、文件改动预览

```
DataShuttle.WpfSample/
├── Helpers/
│   ├── PluginHelper.cs          ← Task 1：改为静态列表
│   └── TransportFactory.cs      ← Task 1：增加 GetTransportNames()
├── Configs/
│   └── ItemConfig.cs            ← Task 2：增加 Name 属性
├── ViewModels/
│   ├── ShuttleLineItemViewModel.cs  ← Task 2/5/6：DisplayName、流量统计、日志
│   ├── MainWindowViewModel.cs       ← Task 2：列表显示 DisplayName
│   ├── SetupViewModel.cs            ← Task 2/4：名称字段、BaudRates
│   └── ScriptEditorViewModel.cs     ← Task 3：自动保存
├── Views/
│   ├── MainWindow.xaml              ← Task 2/5/6：UI 调整
│   ├── SetupView.xaml               ← Task 2/4：名称输入、波特率下拉
│   └── ScriptEditorView.xaml        ← Task 7（可选）：AvalonEdit
└── DataShuttle.WpfSample.csproj     ← Task 7/8（可选）：新增包引用
```
