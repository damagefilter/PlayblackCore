using System;
using System.Collections.Generic;

namespace PlayBlack.Sequencer {
    /// <summary>
    /// Contains a number of ICustomFieldRenderer instances that are cached.
    /// Use this to get and manage these renderers.
    /// </summary>
    public class FieldRendererManager {

        private static FieldRendererManager instance;

        public static FieldRendererManager Instance {
            get {
                if (instance == null) {
                    instance = new FieldRendererManager();
                }

                return instance;
            }
        }

        private Dictionary<Type, ICustomFieldRenderer> resolvedRenderers;

        private FieldRendererManager() {
            resolvedRenderers = new Dictionary<Type, ICustomFieldRenderer>();
        }

        public ICustomFieldRenderer GetRenderer(Type type) {
            ICustomFieldRenderer r;
            if (resolvedRenderers.TryGetValue(type, out r)) {
                return r;
            }

            r = (ICustomFieldRenderer)Activator.CreateInstance(type);
            resolvedRenderers.Add(type, r);
            return r;
        }
    }
}