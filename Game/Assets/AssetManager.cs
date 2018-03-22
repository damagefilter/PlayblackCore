using System.Collections.Generic;
using UnityEngine;

// Code derivated from https://gist.github.com/asus4/2691788
namespace Playblack.Assets {

    public class AssetManager {
        private readonly Dictionary<string, AssetReference> bundles = new Dictionary<string, AssetReference>();

        public void AddAssetBundle(string bundleName, AssetBundle bundle) {
            if (bundles.ContainsKey(bundleName)) {
                bundles[bundleName].Increase();
            }
            else {
                bundles.Add(bundleName, new AssetReference(bundle));
            }
        }

        public void DeleteAssetBundle(string bundle) {
            if (bundles.ContainsKey(bundle)) {
                if (bundles[bundle].Decrease() <= 0) {
                    bundles.Remove(bundle);
                }
            }
        }

        public AssetBundle GetAssetBundle(string bundle) {
            return bundles.ContainsKey(bundle) ? bundles[bundle].bundle : null;
        }

        public bool HasAssetBundle(string bundle) {
            return bundles.ContainsKey(bundle);
        }
    }
}
