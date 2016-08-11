using System;
using UnityEngine;

namespace Playblack {

    internal class AssetReference : IDisposable {
        private int count;
        public AssetBundle bundle;

        public AssetReference(AssetBundle bundle) {
            this.bundle = bundle;
            this.count = 0;
        }

        public int Increase() {
            return ++count;
        }

        public int Decrease() {
            --count;
            if (count <= 0) {
                this.Dispose();
            }
            return count;
        }

        public void Dispose() {
            if (bundle != null) {
                bundle.Unload(false);
                bundle = null;
            }
        }
    }
}