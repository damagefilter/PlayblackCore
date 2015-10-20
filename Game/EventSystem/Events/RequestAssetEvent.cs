using Playblack.Assets;

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

        public AssetLoader.AssetLoaded Callback {
            get;
            private set;
        }

        public RequestAssetEvent(string bundle, string assetPath, AssetLoader.AssetLoaded callback) {
            AssetBundle = bundle;
            AssetPath = assetPath;
            Callback = callback;
        }
    }
}
