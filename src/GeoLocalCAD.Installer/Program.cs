using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Forms;

namespace GeoLocalCAD.Installer;

internal static class Program
{
    private const string ProductName = "GeoLocal CAD";
    private const string BundleName = "GeoLocalCAD.bundle";
    private const string BundleRoot = @"C:\ProgramData\Autodesk\ApplicationPlugins\GeoLocalCAD.bundle";
    private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\GeoLocalCAD";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        if (args.Any(a => string.Equals(a, "/uninstall", StringComparison.OrdinalIgnoreCase)))
        {
            Uninstall();
            return;
        }
        using var form = new InstallerForm();
        Application.Run(form);
    }

    private static void Install()
    {
        EnsureAutoCADClosed();
        var temp = Path.Combine(Path.GetTempPath(), "GeoLocalCAD-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(temp);
            using var stream = FindBundleResource();
            if (stream == null) throw new InvalidOperationException("GeoLocalCAD.bundle.zip was not embedded in this installer.");
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read)) zip.ExtractToDirectory(temp);
            var extracted = Directory.GetDirectories(temp, BundleName, SearchOption.AllDirectories).FirstOrDefault() ?? temp;
            if (!File.Exists(Path.Combine(extracted, "PackageContents.xml"))) throw new InvalidDataException("Invalid GeoLocalCAD bundle: PackageContents.xml is missing.");
            if (Directory.Exists(BundleRoot)) Directory.Delete(BundleRoot, true);
            Directory.CreateDirectory(Path.GetDirectoryName(BundleRoot)!);
            CopyDirectory(extracted, BundleRoot);
            using var key = Registry.LocalMachine.CreateSubKey(UninstallKey);
            key?.SetValue("DisplayName", ProductName);
            key?.SetValue("DisplayVersion", "0.6.0");
            key?.SetValue("Publisher", "GeoLocal CAD");
            key?.SetValue("InstallLocation", BundleRoot);
            key?.SetValue("UninstallString", '"' + Application.ExecutablePath + '" /uninstall');
            key?.SetValue("QuietUninstallString", '"' + Application.ExecutablePath + '" /uninstall');
            MessageBox.Show("تم تثبيت GeoLocal CAD بنجاح. أعد تشغيل AutoCAD/Civil 3D.", ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally { if (Directory.Exists(temp)) Directory.Delete(temp, true); }
    }

    private static void Uninstall()
    {
        EnsureAutoCADClosed();
        if (Directory.Exists(BundleRoot)) Directory.Delete(BundleRoot, true);
        Registry.LocalMachine.DeleteSubKeyTree(UninstallKey, false);
        MessageBox.Show("تمت إزالة GeoLocal CAD بنجاح.", ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static Stream? FindBundleResource()
    {
        var name = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("GeoLocalCAD.bundle.zip", StringComparison.OrdinalIgnoreCase));
        return name == null ? null : Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
    }

    private static void EnsureAutoCADClosed()
    {
        var running = new[] { "acad", "accoreconsole" }.Any(name => Process.GetProcessesByName(name).Length > 0);
        if (running) throw new InvalidOperationException("أغلق AutoCAD وCivil 3D بالكامل ثم أعد تشغيل المثبت.");
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        foreach (var dir in Directory.GetDirectories(source)) CopyDirectory(dir, Path.Combine(destination, Path.GetFileName(dir)));
    }

    private sealed class InstallerForm : Form
    {
        private readonly Label status = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Text = "GeoLocal CAD\nمُثبّت الإضافة لـ AutoCAD / Civil 3D", AutoSize = false };
        public InstallerForm()
        {
            Text = ProductName + " Installer"; Width = 520; Height = 260; StartPosition = FormStartPosition.CenterScreen; FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
            var install = new Button { Text = "تثبيت الإضافة", Width = 150, Height = 38, Left = 95, Top = 145 };
            var cancel = new Button { Text = "إلغاء", Width = 100, Height = 38, Left = 280, Top = 145 };
            install.Click += (_, _) => { try { Install(); Close(); } catch (Exception ex) { MessageBox.Show(ex.Message, ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error); } };
            cancel.Click += (_, _) => Close();
            Controls.Add(status); Controls.Add(install); Controls.Add(cancel);
        }
    }
}
