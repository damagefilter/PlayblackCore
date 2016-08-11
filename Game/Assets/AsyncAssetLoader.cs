using Playblack.EventSystem;
using Playblack.EventSystem.Events;
using System.Collections;
using UnityEngine;

namespace Playblack.Assets {

    public class AsyncAssetLoader : MonoBehaviour {

        /// <summary>
        /// Used as callback for asynchronously loading assets.
        /// This will NOT instantiate the object!
        /// </summary>
        public delegate void AssetLoaded(UnityEngine.Object loadedObj);

        private AssetManager assetManager;

        public AsyncAssetLoader() {
            assetManager = new AssetManager();
        }

        private void Awake() {
            EventDispatcher.Instance.Register<RequestAssetEvent>(OnAssetRequest);
        }

        private void OnDestroy() {
            EventDispatcher.Instance.Unregister<RequestAssetEvent>(OnAssetRequest);
        }

        private void OnAssetRequest(RequestAssetEvent hook) {
            var c = StartCoroutine(LoadAssetBundle(hook.AssetPath, hook.AssetBundle, hook.Callback));
            hook.AssetLoadingProcess = c;
        }

        private IEnumerator LoadAssetBundle(string assetPath, string assetBundle, AssetLoaded callback) {
            if (assetManager.HasAssetBundle(assetBundle)) {
                yield return StartCoroutine(LoadAssetFromBundle(assetPath, assetBundle, callback));
                //                yield break; // End this coroutine
            }

            WWW www = new WWW("file://" + Application.streamingAssetsPath + "/" + assetBundle + ".bundle");
            yield return www;

            assetManager.AddAssetBundle(assetBundle, www.assetBundle);
            yield return StartCoroutine(LoadAssetFromBundle(assetPath, assetBundle, callback));
        }

        private IEnumerator LoadAssetFromBundle(string assetPath, string assetBundle, AssetLoaded callback) {
            var request = assetManager.GetAssetBundle(assetBundle).LoadAssetAsync(assetPath);
            yield return request;
            callback(request.asset);
        }
    }
}