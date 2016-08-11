using Playblack.Assets;
using UnityEngine;

namespace Playblack.EventSystem.Events {

    public class RequestAssetEvent : Event<RequestAssetEvent> {

        public string AssetBundle {
            get;
            private set;
        }

        public string AssetPath {
            get;
            private set;
        }

        public AsyncAssetLoader.AssetLoaded Callback {
            get;
            private set;
        }

        /// <summary>
        /// The Coroutine that is used to load the asset.
        /// You can use that to yield return on a higher level
        /// and get the "everything is finished" time in a more reliable way.
        ///
        /// The asset loader listening to this event can (and should) pass in its loading coroutine here.
        /// If it is not async this may be null.
        /// You can access this property after the event call returned.
        /// </summary>
        /// <value>The asset loading process.</value>
        public Coroutine AssetLoadingProcess {
            get;
            set;
        }

        public RequestAssetEvent(string bundle, string assetPath, AsyncAssetLoader.AssetLoaded callback) {
            AssetBundle = bundle;
            AssetPath = assetPath;
            Callback = callback;
        }
    }
}