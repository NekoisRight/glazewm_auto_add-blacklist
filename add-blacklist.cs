using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

class App {
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll", CharSet = CharSet.Auto)] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint p);
    [DllImport("user32.dll")] static extern IntPtr GetAncestor(IntPtr h, uint f);
    [DllImport("user32.dll")] static extern IntPtr WindowFromPoint(int x, int y);
    const uint GA_ROOT = 2;

    [STAThread]
    static void Main() {
        try {
            // 1. 取窗口
            IntPtr hWnd = GetForegroundWindow();
            string title = GetText(hWnd);

            if (string.IsNullOrWhiteSpace(title) || title == "Program Manager") {
                IntPtr r = GetAncestor(hWnd, GA_ROOT);
                if (r != hWnd && r != IntPtr.Zero) { hWnd = r; title = GetText(hWnd); }
            }
            if (string.IsNullOrWhiteSpace(title) || title == "Program Manager") {
                var pos = Cursor.Position;
                hWnd = WindowFromPoint(pos.X, pos.Y);
                title = GetText(hWnd);
                if (string.IsNullOrWhiteSpace(title) && hWnd != IntPtr.Zero) {
                    IntPtr r = GetAncestor(hWnd, GA_ROOT);
                    if (r != hWnd && r != IntPtr.Zero) { hWnd = r; title = GetText(hWnd); }
                }
            }

            if (title == "Program Manager") { Err("无法识别目标窗口\n请先点击目标窗口确保获得焦点, 再按快捷键"); return; }
            if (hWnd == IntPtr.Zero || string.IsNullOrWhiteSpace(title)) { Err("未获取到有效窗口"); return; }

            // 2. 进程名
            uint pid = 0;
            GetWindowThreadProcessId(hWnd, out pid);
            if (pid == 0) { Err("无法获取进程信息"); return; }
            string procName;
            try { procName = Process.GetProcessById((int)pid).ProcessName; }
            catch { Err("无法获取进程名称"); return; }

            // 3. 确认
            if (Msg("添加以下程序到 GlazeWM 黑名单? (匹配所有窗口/标签页)\n\n进程: " + procName + "\n\n选择【是】确认添加, 【否】取消。", 4 + 0x20) != 6) {
                Info("未添加: " + procName); return;
            }

            // 4. 写配置
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string cfg = Path.Combine(dir, "glazewm", "config.yaml");
            if (!File.Exists(cfg)) cfg = Path.Combine(dir, "config.yaml");
            if (!File.Exists(cfg)) { Err("找不到 config.yaml\n路径: " + dir); return; }

            string safe = procName.Replace("'", "''");
            string newLine = "      - window_process: { equals: '" + safe + "' }";

            string[] all = File.ReadAllLines(cfg, Encoding.UTF8);

            foreach (string ln in all)
                if (ln.Contains("equals: '" + safe + "'")) { Info("进程 '" + procName + "' 已在黑名单中"); return; }

            int idx = -1;
            for (int i = 0; i < all.Length; i++)
                if (all[i].TrimStart() == "binding_modes:") { idx = i; break; }
            if (idx == -1) { Err("config.yaml 找不到 binding_modes:"); return; }

            int last = idx - 1;
            while (last >= 0 && string.IsNullOrWhiteSpace(all[last])) last--;

            var list = new System.Collections.Generic.List<string>();
            for (int i = 0; i <= last; i++) list.Add(all[i]);
            list.Add("");
            list.Add(newLine);
            for (int i = idx; i < all.Length; i++) list.Add(all[i]);

            File.WriteAllText(cfg, string.Join("\r\n", list), new UTF8Encoding(false));
            Info(procName + " 已加入黑名单\n按 Alt+Shift+R 重载");

        } catch (Exception ex) { Err("错误: " + ex.Message); }
    }

    static string GetText(IntPtr h) { var sb = new StringBuilder(512); GetWindowText(h, sb, sb.Capacity); return sb.ToString().Trim(); }

    static int Msg(string text, int type) {
        MessageBoxButtons b = (type & 0x0F) == 4 ? MessageBoxButtons.YesNo : MessageBoxButtons.OK;
        MessageBoxIcon i = MessageBoxIcon.None;
        int iconBits = type & 0xF0;
        if (iconBits == 0x10) i = MessageBoxIcon.Error;
        else if (iconBits == 0x20) i = MessageBoxIcon.Question;
        else if (iconBits == 0x40) i = MessageBoxIcon.Information;
        return (int)MessageBox.Show(text, "GlazeWM", b, i);
    }

    static void Err(string t) { Msg(t, 0x10); }
    static void Info(string t) { Msg(t, 0x40); }
}
