using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace PlayBlack.Editor.Windows {

    /// <summary>
    /// Used for easily creating popup / popout windows that can also be docked.
    /// </summary>
    public abstract class GenericPopupWindow : EditorWindow {
        public static TWindow Popup<TWindow>() where TWindow : GenericPopupWindow {
            TWindow window = EditorWindow.GetWindow<TWindow>();
            window.titleContent = new GUIContent(window.GetTitle());
            window.InternalInit();
            //window.skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/materials/CodeEditorSkin.guiskin");
            return window;
        }

        public abstract string GetTitle();

        public abstract void InternalInit();
    }
}
