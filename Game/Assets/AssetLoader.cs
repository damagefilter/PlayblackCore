using System;
using System.Collections;
using UnityEngine;
using Playblack.EventSystem;
using Playblack.EventSystem.Events;

namespace Playblack.Assets {
    public class AssetLoader : MonoBehaviour {
        /// <summary>
        /// Used as callback for asynchronously loading assets.
        /// This will NOT instantiate the object!
        /// </summary>
        public delegate void AssetLoaded(UnityEngine.Object loadedObj);

        private AssetManager assetManager;

        public AssetLoader() {
            AssetManager = new AssetManager();
        }

        void Awake() {
            EventDispatcher.Instance.Register<RequestAssetEvent>(OnAssetRequest);
        }

        void OnDestroy() {
            EventDispatcher.Instance.Unregister<RequestAssetEvent>(OnAssetRequest);
        }

        private void OnAssetRequest(RequestAssetEvent hook) {
            StartCoroutine(LoadAssetBundle(hook.AssetPath, hook.AssetBundle, hook.Callback));
        }

        private IEnumerator LoadAssetBundle(string assetPath, string assetBundle, AssetLoaded callback) {
            if (assetManager.HasAssetBundle(assetBundle)) {
                StartCoroutine(LoadAssetFromBundle(assetPath, assetBundle, callback));
                return null; // End this coroutine
            }

            WWW www = new WWW("file://" + Application.streamingAssetsPath + "/" + assetBundle + ".bundle");
            yield return www;

            assetManager.AddAssetBundle(assetBundle, www.assetBundle);
            StartCoroutine(LoadAssetFromBundle(assetPath, assetBundle, callback));
            return null; // End this coroutine
        }

        private IEnumerator LoadAssetFromBundle(string assetPath, string assetBundle, AssetLoaded callback) {
            var request = assetManager.GetAssetBundle(assetBundle).LoadAssetAsync(assetPath);
            yield return request;
            callback(request.asset);
        }
    }
}

