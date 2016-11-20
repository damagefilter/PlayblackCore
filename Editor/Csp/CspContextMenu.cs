using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Playblack.Editor.Csp {
    public class CspContextMenu {
        /// <summary>
        /// Reads a json file containing definitions for CSP objects
        /// and auto-generates a class with annotations and methods to handle these.
        /// </summary>
        [MenuItem("Playblack/CSP/Generate CSP Context Menus")]
        public static void GenerateContextMenus() {
            var txt = File.ReadAllText("Assets/Playblack/csp_definitions.fgd");
            Debug.Assert(txt != null);
            var cspList = JsonConvert.DeserializeObject<List<CspTypeDescription2>>(txt); // yeeah, fgd. Good times!
            var generator = new ClassAutoGenerator();
            foreach (var csp in cspList) {
                generator.AddMenuEntry(csp);
            }
            generator.WriteMenuFile();
        }
    }

    public class CspTypeDescription2 {

        public string Category {
            get;
            set;
        }

        public string NameInMenu {
            get;
            set;
        }

        public string TypeName {
            get;
            set;
        }

        public string[] DependentComponents {
            get;
            set;
        }
    }

    public class ClassAutoGenerator {
        private static string classTemplate = @"
using UnityEditor;
using UnityEngine;
using System;
public class CspContextMenuEntries {{
    {0}
}}
";
        private static string methodTemplate =
    "   [MenuItem(\"GameObject/CSP/{0}\", false, 10)]\n" +
    "   public static void {1}() {{\n{2}\n    }}\n\n";

        private StringBuilder generatedMethods;

        private int methodCount;
        public ClassAutoGenerator() {
            generatedMethods = new StringBuilder();
            methodCount = 0;
        }

        public void AddMenuEntry(CspTypeDescription2 csp) {
            var methodBody = @"
        var goPos = SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
        var go = new GameObject();
        go.name = ""{0}"";
        go.transform.position = goPos;
        {1}
        go.AddComponent<{2}>();
        go.AddComponent<Playblack.Csp.SignalProcessor>();
";
            StringBuilder dependentComponents = new StringBuilder();
            if (csp.DependentComponents != null) {
                foreach (var typeName in csp.DependentComponents) {
                    dependentComponents.AppendLine(string.Format("go.AddComponent<{0}>();", typeName));
                }
            }
            methodBody = string.Format(methodBody, csp.NameInMenu, dependentComponents.ToString(), csp.TypeName);


            var method = string.Format(
                methodTemplate,
                string.Join("/", new string[] { csp.Category, csp.NameInMenu }),// 0
                "AddCspObject" + methodCount++, // 1
                methodBody // 2
            );
            generatedMethods.Append(method);
        }

        public void WriteMenuFile() {
            Debug.Log("Writing file.");
            var assetPath = "Assets/Playblack/Editor/CspContextMenuEntries.cs";
            File.WriteAllText(assetPath, string.Format(classTemplate, generatedMethods.ToString()));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Debug.Log("Done. Triggering import.");
        }
    }
}
