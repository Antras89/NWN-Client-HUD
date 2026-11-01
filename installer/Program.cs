using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Forms;

namespace NWNClientHUDInstaller;

internal static class Program
{
    private const string InstallRelativePath = "Programs\\NWN Client HUD";
    private const string Repo = "Antras89/NWN-Client-HUD";
    private static readonly string[] Files =
    [
        "NWN.ClientHUD.exe", "NWN.ClientHUD.dll", "NWN.ClientHUD.deps.json",
        "NWN.ClientHUD.runtimeconfig.json", "hud-icon.ico", "icons.json",
        "buff-durations.json", "learned-defaults.json"
    ];
    private static readonly string[] PreservedFiles = ["settings.json", "learned.json"];

    [STAThread]
    private static async Task Main()
    {
        try
        {
            var target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), InstallRelativePath);
#if UPDATE_MODE
            if (!Directory.Exists(target) || !File.Exists(Path.Combine(target, "NWN.ClientHUD.exe")))
                throw new InvalidOperationException("NWN Client HUD is not installed. Download and run Setup.exe first.");
            var archive = await DownloadLatestPackageAsync();
            ApplyPackage(archive, target, installDefaults: false);
            MessageBox.Show("NWN Client HUD is up to date.", "NWN Client HUD Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
#else
            using var payload = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip")
                ?? throw new InvalidOperationException("Installer payload is missing.");
            using var archive = new MemoryStream();
            await payload.CopyToAsync(archive);
            archive.Position = 0;
            ApplyPackage(archive, target, installDefaults: true);
            MessageBox.Show("NWN Client HUD has been installed or updated. A desktop shortcut is ready.", "NWN Client HUD Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
            Process.Start(new ProcessStartInfo(Path.Combine(target, "NWN.ClientHUD.exe")) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "NWN Client HUD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.ExitCode = 1;
        }
    }

#if UPDATE_MODE
    private static async Task<MemoryStream> DownloadLatestPackageAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("NWNClientHUDUpdater", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        using var release = await client.GetStreamAsync($"https://api.github.com/repos/{Repo}/releases/latest");
        using var json = await JsonDocument.ParseAsync(release);
        var asset = json.RootElement.GetProperty("assets").EnumerateArray()
            .FirstOrDefault(x => x.GetProperty("name").GetString() == "NWN-HUD-Defensive-installer.zip");
        if (asset.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException("The latest release does not contain the HUD update package yet.");
        var url = asset.GetProperty("browser_download_url").GetString()
            ?? throw new InvalidOperationException("The update package URL is missing.");
        var bytes = await client.GetByteArrayAsync(url);
        return new MemoryStream(bytes, writable: false);
    }
#endif

    private static void ApplyPackage(Stream package, string target, bool installDefaults)
    {
        using var zip = new ZipArchive(package, ZipArchiveMode.Read, leaveOpen: true);
        var incoming = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in zip.Entries)
        {
            var relative = entry.FullName.Replace('/', '\\');
            if (Files.Contains(relative, StringComparer.OrdinalIgnoreCase) || PreservedFiles.Contains(relative, StringComparer.OrdinalIgnoreCase) || relative.StartsWith("icons\\", StringComparison.OrdinalIgnoreCase))
            {
                if (relative.Split('\\').Any(part => part == "..")) throw new InvalidDataException("Unsafe path in installer package.");
                incoming[relative] = entry;
            }
        }
        foreach (var name in Files)
            if (!incoming.ContainsKey(name)) throw new InvalidDataException($"Installer package is incomplete: {name}");
        if (!incoming.Keys.Any(name => name.StartsWith("icons\\", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidDataException("Installer package does not contain the HUD icon directory.");

        Directory.CreateDirectory(target);
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var backup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NWN Client HUD Backups", stamp);
        var exitExe = Path.Combine(target, "NWN.ClientHUD.exe");
        if (File.Exists(exitExe))
        {
            using var exit = Process.Start(new ProcessStartInfo(exitExe, "--exit") { UseShellExecute = false, CreateNoWindow = true });
            if (exit is not null && !exit.WaitForExit(10000)) throw new IOException("HUD did not close. No files were replaced.");
            foreach (var running in Process.GetProcessesByName("NWN.ClientHUD"))
            {
                using (running)
                    if (!running.WaitForExit(10000)) throw new IOException("HUD is still running. No files were replaced.");
            }
        }

        var changes = new List<(string path, string? backup, bool created)>();
        try
        {
            foreach (var (relative, entry) in incoming)
            {
                if (PreservedFiles.Contains(relative, StringComparer.OrdinalIgnoreCase)) continue;
                var destination = Path.GetFullPath(Path.Combine(target, relative));
                if (!destination.StartsWith(Path.GetFullPath(target) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Package path escapes the install folder.");
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                using var source = entry.Open();
                using var buffer = new MemoryStream();
                source.CopyTo(buffer);
                var bytes = buffer.ToArray();
                if (File.Exists(destination) && HashMatches(destination, bytes)) continue;
                string? backupPath = null;
                if (File.Exists(destination))
                {
                    backupPath = Path.Combine(backup, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                    File.Copy(destination, backupPath, overwrite: true);
                }
                changes.Add((destination, backupPath, backupPath is null));
                File.WriteAllBytes(destination, bytes);
            }

            if (installDefaults)
                foreach (var name in PreservedFiles)
                {
                    var destination = Path.Combine(target, name);
                    if (!File.Exists(destination) && incoming.TryGetValue(name, out var entry))
                    {
                        using var source = entry.Open();
                        using var file = File.Create(destination);
                        source.CopyTo(file);
                        changes.Add((destination, null, true));
                    }
                }
            CreateDesktopShortcut(exitExe, target);
        }
        catch
        {
            foreach (var change in changes.AsEnumerable().Reverse())
            {
                if (change.backup is not null) File.Copy(change.backup, change.path, overwrite: true);
                else if (change.created && File.Exists(change.path)) File.Delete(change.path);
            }
            throw;
        }
    }

    private static bool HashMatches(string existing, byte[] bytes)
    {
        using var stream = File.OpenRead(existing);
        return CryptographicOperations.FixedTimeEquals(SHA256.HashData(stream), SHA256.HashData(bytes));
    }

    private static void CreateDesktopShortcut(string exe, string target)
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        Directory.CreateDirectory(desktop);
        var shortcutPath = Path.Combine(desktop, "NWN Client HUD.lnk");
        var type = Type.GetTypeFromProgID("WScript.Shell") ?? throw new InvalidOperationException("Windows Script Host is unavailable.");
        dynamic shell = Activator.CreateInstance(type)!;
        try
        {
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            try
            {
                shortcut.TargetPath = exe;
                shortcut.WorkingDirectory = target;
                shortcut.IconLocation = Path.Combine(target, "hud-icon.ico");
                shortcut.Description = "Launch NWN Client HUD";
                shortcut.Save();
            }
            finally { Marshal.FinalReleaseComObject(shortcut); }
        }
        finally { Marshal.FinalReleaseComObject(shell); }
    }
}
