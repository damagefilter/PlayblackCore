using System;
using System.IO;
using System.Diagnostics;
using UnityEditor;

namespace PlayBlack.Editor.Mods {
    /// <summary>
    /// A set of methods that help with building a mod bundle.
    /// </summary>
    public static class BuildTools {
        public static void BuildDll(ModConfig cfg) {
            if (!Directory.Exists(cfg.CodePath)) {
                throw new ArgumentException(cfg.CodePath + " is not a directory!");
            }

            var files = string.Join(" ", Directory.GetFiles(cfg.CodePath));
            string options = " -target:library -out:ModExport/" + cfg.Name + "/" + cfg.Name + ".dll";

            if (cfg.ReferencedAssemblies != null && cfg.ReferencedAssemblies.Length > 0) {
                options += string.Join(" -t:", cfg.ReferencedAssemblies);
            }
            // unities mono path
            var compiler = "sh " + EditorApplication.applicationContentsPath + "/Mono/bin/mcs";
            Process proc = new Process();
            var startInfo = new ProcessStartInfo(compiler, options + " " + files);
            startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            proc.StartInfo = startInfo;
            proc.ErrorDataReceived += BuildTools.OnError;
            proc.Start();
        }

        private static void OnError(object sender, DataReceivedEventArgs args) {
            UnityEngine.Debug.LogError(args.Data);
        }
    }
}

