using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Itan.EditorTools
{
    /// <summary>
    /// Third-party CC0 assets are not kept in git. They are listed in
    /// Assets/_Project/ThirdPartyAssets.txt and fetched into Assets/_ThirdParty
    /// with Itan > Assets > Download Third-Party Assets.
    ///
    /// Manifest lines (tab separated): url, target path, licence, author, title.
    /// A target ending in "/" means the download is a zip to unpack there.
    /// Blank lines and lines starting with # are ignored.
    /// </summary>
    public static class ThirdPartyDownloader
    {
        public const string Manifest = "Assets/_Project/ThirdPartyAssets.txt";
        const string Report = "Logs/itan_downloads.log";

        [MenuItem("Itan/Assets/Download Third-Party Assets")]
        public static void DownloadMissing() => Run(false);

        [MenuItem("Itan/Assets/Re-download All Third-Party Assets")]
        static void DownloadAll() => Run(true);

        static void Run(bool force)
        {
            var log = new StringBuilder();
            int got = 0, skipped = 0, failed = 0;
            foreach (var entry in Read())
            {
                bool isZip = entry.target.EndsWith("/");
                bool exists = isZip ? Directory.Exists(entry.target) && Directory.GetFiles(entry.target, "*", SearchOption.AllDirectories).Length > 0
                                    : File.Exists(entry.target);
                if (exists && !force) { skipped++; continue; }

                EditorUtility.DisplayProgressBar("Ìtàn assets", entry.title, 0.5f);
                var bytes = Fetch(entry.url, out string error);
                if (bytes == null) { failed++; log.AppendLine($"FAIL {entry.url} -> {error}"); continue; }

                if (isZip) Unzip(bytes, entry.target);
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(entry.target));
                    File.WriteAllBytes(entry.target, bytes);
                }
                got++;
                log.AppendLine($"OK   {entry.target} ({bytes.Length / 1024} KB)");
            }
            EditorUtility.ClearProgressBar();
            log.AppendLine($"Done: {got} downloaded, {skipped} already there, {failed} failed.");
            File.WriteAllText(Report, log.ToString());
            Debug.Log("[Assets] " + log.ToString().Trim());
            AssetDatabase.Refresh();
        }

        static byte[] Fetch(string url, out string error)
        {
            using var req = UnityWebRequest.Get(url);
            req.timeout = 180;
            var op = req.SendWebRequest();
            while (!op.isDone) System.Threading.Thread.Sleep(50);
            error = req.result == UnityWebRequest.Result.Success ? null : $"{req.responseCode} {req.error}";
            return error == null ? req.downloadHandler.data : null;
        }

        static void Unzip(byte[] bytes, string folder)
        {
            Directory.CreateDirectory(folder);
            using var zip = new ZipArchive(new MemoryStream(bytes));
            foreach (var e in zip.Entries)
            {
                if (string.IsNullOrEmpty(e.Name)) continue;
                var dest = Path.GetFullPath(Path.Combine(folder, e.FullName));
                if (!dest.StartsWith(Path.GetFullPath(folder))) continue; // ignore paths that escape the folder
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                e.ExtractToFile(dest, true);
            }
        }

        public struct Entry { public string url, target, licence, author, title; }

        public static List<Entry> Read()
        {
            var list = new List<Entry>();
            if (!File.Exists(Manifest)) return list;
            foreach (var raw in File.ReadAllLines(Manifest))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var p = line.Split('\t');
                if (p.Length < 2) continue;
                list.Add(new Entry
                {
                    url = p[0].Trim(), target = p[1].Trim(),
                    licence = p.Length > 2 ? p[2].Trim() : "", author = p.Length > 3 ? p[3].Trim() : "",
                    title = p.Length > 4 ? p[4].Trim() : Path.GetFileName(p[1]),
                });
            }
            return list;
        }

        /// <summary>Quick probe used while picking assets: tries each URL in Logs/itan_probe.txt and reports status and size.</summary>
        [MenuItem("Itan/Assets/Probe URLs")]
        static void Probe()
        {
            var sb = new StringBuilder();
            foreach (var url in File.ReadAllLines("Logs/itan_probe.txt"))
            {
                if (string.IsNullOrWhiteSpace(url)) continue;
                var bytes = Fetch(url.Trim(), out string err);
                sb.AppendLine(bytes != null ? $"OK {bytes.Length} {url}" : $"FAIL {err} {url}");
            }
            File.WriteAllText("Logs/itan_probe_out.txt", sb.ToString());
        }
    }
}
