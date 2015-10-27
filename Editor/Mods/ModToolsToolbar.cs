using System;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace PlayBlack.Editor.Mods {
    public static class ModToolsToolbar {
        #region Config
        [MenuItem("Mod Tools/Config/Init Config")]
        public static void InitModConfig() {
            var unused = ModConfig.Instance;
        }
        #endregion
        #region Build
        [MenuItem("Mod Tools/Build/Build AssetBundles")]
        public static void BuildAssetBundles() {
            // TODO: Should probably open some dialogue with configuration options
            // from which the build can be triggered.
            var cfg = ModConfig.Instance;
            string exportPath = (cfg.IsMod ? "ModExport/" : Application.streamingAssetsPath + "/") + cfg.Name;
            if (!Directory.Exists(exportPath)) {
                Directory.CreateDirectory(exportPath);
            }
            BuildPipeline.BuildAssetBundles(exportPath);
        }

        /// <summary>
        /// This actually copies the Assembly-Csharp dll to the mod folder.
        /// That has only an effect if the ModConfig says this ia a Mod, not a full game.
        /// </summary>
        [MenuItem("Mod Tools/Build/Build DLL")]
        public static void BuildDll() {
            
            // TODO: Should probably open some dialogue with configuration options
            // from which the build can be triggered.
            var cfg = ModConfig.Instance;
            if (cfg == null) {
                Debug.LogError("For unkown reasons I cannot load the mod configuration...");
                return;
            }
            if (!cfg.IsMod) {
                return;
            }
            BuildTools.BuildDll(cfg);
        }
        #endregion
    }
}

