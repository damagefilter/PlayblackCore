using System;
using Playblack.EventSystem.Events;

namespace Playblack.Mods {
    /// <summary>
    /// Defines the communication channel between the core game and modded / extra content.
    /// Implement this to hook your customisations into the game.
    /// A mod should reside as DLL in the persistentDataPath of the game.
    /// Use Application.persistentDataPath.
    /// </summary>
    public interface IMod {
        #region Descriptions
        /// <summary>
        /// Return the mods name.
        /// </summary>
        /// <value>The name.</value>
        string Name {
            get;
        }

        /// <summary>
        /// Return a list of relative paths to more external DLLs that
        /// need to be loaded additionally.
        /// Return null if nothing new needs adding.
        /// </summary>
        /// <value>The external dlls.</value>
        string[] ExternalDlls {
            get;
        }

        /// <summary>
        /// Return a list of paths to asset bundles that should be made available
        /// to the asset management.
        /// It's best to return absolute paths via Application.persistentDataPath.
        /// </summary>
        /// <value>The asset bundles.</value>
        string[] AssetBundles {
            get;
        }

        /// <summary>
        /// Provides a list with AssetBundles that specifically contain scenes.
        /// Every asset inside a Scene AssetBundle is expected to be an actual scene.
        /// 
        /// </summary>
        /// <value>The scenes.</value>
        string[] ScenesBundles {
            get;
        }
        #endregion

        #region Hooks

        /// <summary>
        /// Callback raised after the Mod is loaded but before asset bundles and such are registered.
        /// </summary>
        void OnLoad();

        /// <summary>
        /// Called when this mod is enabled.
        /// That is after all asset bundles and external DLLs were loaded.
        /// </summary>
        void OnEnable();

        /// <summary>
        /// Called when this mod is disabled.
        /// </summary>
        void OnDisable();

        #endregion
    }
}