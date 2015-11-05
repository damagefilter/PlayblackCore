using System;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Playblack.Editor.Mods {
    /// <summary>
    /// Editor Configuration for the mod in development.
    /// This describes how your mod is built against the core code
    /// and how it is put together.
    /// It is entirely not relevant for the final build of the game or mod.
    /// </summary>
    public class ModConfig : ScriptableObject {
        #region Static
        private static ModConfig _instance;
        public static ModConfig Instance {
            get {
                if (_instance == null) {
                    var cfg = AssetDatabase.LoadAssetAtPath<ModConfig>("Assets/ModConfig.asset");
                    if (cfg == null) {
                        cfg = ScriptableObject.CreateInstance<ModConfig>();
                        AssetDatabase.CreateAsset(cfg, "Assets/ModConfig.asset");
                    }
                    _instance = cfg;
                }
                return _instance;
            }
        }
        #endregion
        /// <summary>
        /// Is the configuration a mod configuration?
        /// If not, it is assumed that this is a core-game configuration.
        /// The difference lies in what is built and where.
        /// </summary>
        [Tooltip("Set false if you are intending to do a core game. True if this is a mod to a core game.")]
        [SerializeField]private bool isMod;
        public bool IsMod {
            get {
                return isMod;
            }
            set {
                isMod = value;
            }
        }
        /// <summary>
        /// A directory that contains OTHER mods which you may use as dependencies.
        /// </summary>
        [Tooltip("A directory that contains OTHER mods which you may use as dependencies")]
        [SerializeField]private string modDirectory;
        public string ModDirectory {
            get {
                return modDirectory;
            }
            set {
                modDirectory = value;
            }
        }

        [Tooltip("The name of your game or mod.")]
        [SerializeField]private string modName;
        public string Name {
            get {
                return modName;
            }
            set {
                modName = value;
            }
        }

        /// <summary>
        /// As a convenience, you can just write and manage code in unity, as usual.
        /// This path information helps in the build process to generate a dll that can be used as a mod dll.
        /// </summary>
        [Tooltip("If you wish to compile your project code rather than external DLLS into a mod DLL, specify here where your code is.")]
        [SerializeField]private string codePath;
        public string CodePath {
            get {
                return codePath;
            }
            set {
                codePath = value;
            }
        }

        [Tooltip("If you reference external assemblies they'll be taken along when generating the mod dll.")]
        [SerializeField]private string[] referencedAssemblies;
        public string[] ReferencedAssemblies {
            get {
                return referencedAssemblies;
            }
            set {
                referencedAssemblies = value;
            }
        }
    }
}

