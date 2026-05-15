using UnityEngine;
using UnityEditor;
using NueGames.NueDeck.Scripts.Data.Containers;
using System.Reflection;

namespace NueGames.NueDeck.Scripts.Data.Containers.Editor
{
    /// <summary>
    /// Utility to copy keyword entries from SpecialKeywordData to CardKeywordData.
    /// </summary>
    public class KeywordDataConverter : EditorWindow
    {
        private SpecialKeywordData sourceData;
        private CardKeywordData targetData;

        [MenuItem("NueDeck/Tools/Copy Keywords to Card Keywords")]
        public static void ShowWindow()
        {
            GetWindow<KeywordDataConverter>("Keyword Converter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Copy Keywords Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool copies all keyword entries from SpecialKeywordData (for status tooltips) " +
                "to CardKeywordData (for card tooltips). Both will have the same text initially, " +
                "then you can customize the card versions.",
                MessageType.Info);

            GUILayout.Space(10);

            sourceData = (SpecialKeywordData)EditorGUILayout.ObjectField(
                "Source (SpecialKeywordData)", 
                sourceData, 
                typeof(SpecialKeywordData), 
                false);

            targetData = (CardKeywordData)EditorGUILayout.ObjectField(
                "Target (CardKeywordData)", 
                targetData, 
                typeof(CardKeywordData), 
                false);

            GUILayout.Space(10);

            GUI.enabled = sourceData != null && targetData != null;
            
            if (GUILayout.Button("Copy All Entries", GUILayout.Height(40)))
            {
                CopyKeywords();
            }

            GUI.enabled = true;

            GUILayout.Space(10);

            if (GUILayout.Button("Create New CardKeywordData Asset"))
            {
                CreateNewCardKeywordData();
            }
        }

        private void CopyKeywords()
        {
            if (sourceData == null || targetData == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both source and target data.", "OK");
                return;
            }

            Undo.RecordObject(targetData, "Copy Keywords");

            // Get the private list field using reflection
            var sourceListField = typeof(SpecialKeywordData).GetField("specialKeywordBaseList", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var targetListField = typeof(CardKeywordData).GetField("cardKeywordBaseList", 
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (sourceListField == null || targetListField == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not access keyword lists. Check field names.", "OK");
                return;
            }

            var sourceList = sourceListField.GetValue(sourceData) as System.Collections.Generic.List<SpecialKeywordBase>;
            var targetList = new System.Collections.Generic.List<CardKeywordBase>();

            if (sourceList == null)
            {
                EditorUtility.DisplayDialog("Error", "Source list is null or empty.", "OK");
                return;
            }

            // Copy each entry
            foreach (var sourceEntry in sourceList)
            {
                var newEntry = new CardKeywordBase();
                
                // Copy fields using reflection
                var sourceKeywordField = typeof(SpecialKeywordBase).GetField("specialKeyword", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var sourceHeaderField = typeof(SpecialKeywordBase).GetField("header", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var sourceContentField = typeof(SpecialKeywordBase).GetField("contentText", 
                    BindingFlags.NonPublic | BindingFlags.Instance);

                var targetKeywordField = typeof(CardKeywordBase).GetField("specialKeyword", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var targetHeaderField = typeof(CardKeywordBase).GetField("header", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var targetContentField = typeof(CardKeywordBase).GetField("contentText", 
                    BindingFlags.NonPublic | BindingFlags.Instance);

                targetKeywordField.SetValue(newEntry, sourceKeywordField.GetValue(sourceEntry));
                targetHeaderField.SetValue(newEntry, sourceHeaderField.GetValue(sourceEntry));
                targetContentField.SetValue(newEntry, sourceContentField.GetValue(sourceEntry));

                targetList.Add(newEntry);
            }

            targetListField.SetValue(targetData, targetList);

            EditorUtility.SetDirty(targetData);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Success", 
                $"Copied {targetList.Count} keyword entries from SpecialKeywordData to CardKeywordData!", 
                "OK");
        }

        private void CreateNewCardKeywordData()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create CardKeywordData",
                "CardKeywords",
                "asset",
                "Choose location for new CardKeywordData asset");

            if (!string.IsNullOrEmpty(path))
            {
                var newAsset = CreateInstance<CardKeywordData>();
                AssetDatabase.CreateAsset(newAsset, path);
                AssetDatabase.SaveAssets();
                
                targetData = newAsset;
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newAsset;

                EditorUtility.DisplayDialog("Success", 
                    "Created new CardKeywordData asset. Now assign your SpecialKeywordData and click 'Copy All Entries'.", 
                    "OK");
            }
        }
    }
}
