# GlazeWM 自动添加进程黑名单 

> 为 GlazeWM 自动添加进程黑名单，解放双手 \^o^/

---

## 📦 使用方法

1. **放置程序** — 将 `add-blacklist.exe` 放入 GlazeWM 配置文件根目录
2. **绑定快捷键** — 在 `config.yaml` 中添加如下指令：

```yaml
- commands: ['shell-exec "%USERPROFILE%\.glzr\add-blacklist.exe"']
  bindings: ['alt+shift+b']
```

3. **刷新配置** — 按下 `alt+shift+r` 刷新 GlazeWM 配置
4. **快捷添加黑名单** — 之后按 `alt+shift+tab` 即可快速添加黑名单 🚀

---

## ⌨️ 快捷键一览

| 快捷键          | 功能               |
| --------------- | ------------------ |
| `alt+shift+b` | 执行添加黑名单程序 |
| `alt+shift+r` | 刷新 GlazeWM 配置  |

---

## 📂 文件结构

```
%USERPROFILE%\.glzr\
├── add-blacklist.exe        # 黑名单添加工具
├── config.yaml              # GlazeWM 配置文件
└── ...                      # 其他配置文件
```
