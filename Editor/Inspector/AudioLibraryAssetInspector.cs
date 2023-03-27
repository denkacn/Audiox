using Audiox.Runtime.Assets;
using UnityEditor;
using UnityEngine;

namespace Audiox.Editor.Inspector
{
    [CustomEditor(typeof(AudioxSampleLibraryAsset))]
    public class AudioLibraryAssetInspector : UnityEditor.Editor
    {
        private SerializedProperty _clip;
        private SerializedProperty _libraryName;
        private SerializedProperty _samples;

        void OnEnable()
        {
            _clip = serializedObject.FindProperty("Clip");
            _libraryName = serializedObject.FindProperty("LibraryName");
            _samples = serializedObject.FindProperty("Samples");
        }

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var assetPath = AssetDatabase.GetAssetPath(instanceId);
            var libraryAsset = AssetDatabase.LoadAssetAtPath<AudioxSampleLibraryAsset>(assetPath);
            if (libraryAsset != null)
            {
                OpenEditorWindow(libraryAsset);
                return true;
            }
            return false;
        }

        private static void OpenEditorWindow(AudioxSampleLibraryAsset libraryAsset)
        {
            if (AudioxEditorWindow.Instance != null)
            {
                AudioxEditorWindow.Instance.Init(true, AssetDatabase.GetAssetPath(libraryAsset));
                AudioxEditorWindow.Instance.Repaint();
            }
            else
            {
                var editorWindow = (AudioxEditorWindow) EditorWindow.GetWindow(typeof(AudioxEditorWindow), false, AudioxEditorWindow.Title);
                editorWindow.Init(true, AssetDatabase.GetAssetPath(libraryAsset));
                editorWindow.Show();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var libraryAsset = (AudioxSampleLibraryAsset) target;
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.PropertyField(_clip, new GUIContent("Clip", "Audio Clip"));
            EditorGUILayout.PropertyField(_libraryName, new GUIContent("LibraryName", "Library Name"));
            EditorGUILayout.PropertyField(_samples, new GUIContent("Samples", "Samples list"));
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(libraryAsset);
                AssetDatabase.SaveAssets();
                Repaint();
            }
            
            if (GUILayout.Button("Open in Audiox Editor"))
            {
                OpenEditorWindow(libraryAsset);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}