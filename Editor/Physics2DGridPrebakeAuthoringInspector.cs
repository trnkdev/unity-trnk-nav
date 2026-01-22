#if UNITY_EDITOR
using NekoNav.Physics2D;
using UnityEditor;
using UnityEngine;

namespace NekoNav
{
    [CustomEditor(typeof(Physics2DGridPrebakeAuthoring))]
    public sealed class Physics2DGridPrebakeAuthoringInspector : Editor
    {
        private const string DefaultAssetName = "GridBakeData";
        private const string DefaultFolder = "Assets";
        private const float RightButtonWidth = 64f;
        private const float Space = 6f;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var navigable = serializedObject.FindProperty("_navigableTilemap");
            var obstacle = serializedObject.FindProperty("_obstacleTilemap");
            var bakeAsset = serializedObject.FindProperty("_bakeAsset");

            EditorGUILayout.PropertyField(navigable, new GUIContent("Navigable Tilemap"));
            EditorGUILayout.PropertyField(obstacle, new GUIContent("Obstacle Tilemap"));

            EditorGUILayout.Space(Space);
            DrawBakeAssetRow(bakeAsset);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBakeAssetRow(SerializedProperty bakeAssetProp)
        {
            var a = (Physics2DGridPrebakeAuthoring)target;

            bool canOperate = a != null && a.CanOperate && !EditorApplication.isPlaying;
            bool hasAsset = a != null && a.BakeAsset != null;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(bakeAssetProp, new GUIContent("Bake Asset"));

            using (new EditorGUI.DisabledScope(!canOperate))
            {
                string label = hasAsset ? "Bake" : "Create";
                if (GUILayout.Button(label, GUILayout.Width(RightButtonWidth)))
                {
                    if (!hasAsset)
                    {
                        GridBakeAsset created = CreateBakeAsset();
                        if (created != null)
                        {
                            bakeAssetProp.objectReferenceValue = created;
                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        Undo.RecordObject(a.BakeAsset, "Bake Grid");
                        a.BakeToAsset();
                        MarkDirty(a.BakeAsset);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (EditorApplication.isPlaying)
                EditorGUILayout.HelpBox("Bake is disabled in Play Mode.", MessageType.Info);
        }

        private static GridBakeAsset CreateBakeAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Grid Bake Asset",
                DefaultAssetName,
                "asset",
                "Choose location for the bake asset.",
                DefaultFolder);

            if (string.IsNullOrEmpty(path))
                return null;

            var asset = ScriptableObject.CreateInstance<GridBakeAsset>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        private static void MarkDirty(Object obj)
        {
            if (obj == null) return;
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
