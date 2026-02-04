using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using UnityEditor;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Editor
{
    public class EnemyEditorWindow : EditorWindow
    {
        private List<EnemyCharacterData> _allEnemies = new List<EnemyCharacterData>();
        private EnemyCharacterData _selectedEnemy;
        private SerializedObject _serializedEnemy;
        private Vector2 _scrollPositionLeft;
        private Vector2 _scrollPositionRight;
        private string _searchFilter = "";
        private int _selectedActTab = 0;
        private bool _showAbilities = true;
        private bool _showStats = true;
        private bool _showStatuses = true;
        
        [MenuItem("NueDeck/Enemy Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<EnemyEditorWindow>("Enemy Editor");
            window.minSize = new Vector2(800, 600);
        }
        
        private void OnEnable()
        {
            RefreshEnemyList();
        }
        
        private void RefreshEnemyList()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyCharacterData");
            _allEnemies = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyCharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(enemy => enemy != null)
                .OrderBy(enemy => enemy.name)
                .ToList();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left Panel - Enemy List
            DrawLeftPanel();
            
            // Right Panel - Enemy Details
            DrawRightPanel();
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            
            // Header
            GUILayout.Label("Enemy List", EditorStyles.boldLabel);
            
            // Search bar
            EditorGUILayout.BeginHorizontal();
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                RefreshEnemyList();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Enemy list
            _scrollPositionLeft = EditorGUILayout.BeginScrollView(_scrollPositionLeft);
            
            var filteredEnemies = string.IsNullOrEmpty(_searchFilter)
                ? _allEnemies
                : _allEnemies.Where(e => e.name.ToLower().Contains(_searchFilter.ToLower())).ToList();
            
            foreach (var enemy in filteredEnemies)
            {
                bool isSelected = _selectedEnemy == enemy;
                
                if (isSelected)
                {
                    GUI.backgroundColor = new Color(0.4f, 0.6f, 1f);
                }
                
                if (GUILayout.Button(enemy.name, GUILayout.Height(25)))
                {
                    _selectedEnemy = enemy;
                    _serializedEnemy = new SerializedObject(_selectedEnemy);
                    _selectedActTab = 0;
                }
                
                if (isSelected)
                {
                    GUI.backgroundColor = Color.white;
                }
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(5);
            GUILayout.Label($"Total Enemies: {_allEnemies.Count}", EditorStyles.miniLabel);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            
            if (_selectedEnemy == null || _serializedEnemy == null)
            {
                EditorGUILayout.HelpBox("Select an enemy from the left panel to view and edit details.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            _serializedEnemy.Update();
            
            _scrollPositionRight = EditorGUILayout.BeginScrollView(_scrollPositionRight);
            
            // Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_selectedEnemy.name, EditorStyles.largeLabel);
            if (GUILayout.Button("Ping Asset", GUILayout.Width(100)))
            {
                Selection.activeObject = _selectedEnemy;
                EditorGUIUtility.PingObject(_selectedEnemy);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // Act-Based Scaling Toggle
            SerializedProperty useActScalingProp = _serializedEnemy.FindProperty("useActBasedScaling");
            EditorGUILayout.PropertyField(useActScalingProp, new GUIContent("Use Act-Based Scaling"));
            
            // Selection Mode Settings
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Ability Selection Mode", EditorStyles.boldLabel);
            SerializedProperty followPatternProp = _serializedEnemy.FindProperty("followAbilityPattern");
            SerializedProperty useWeightedProp = _serializedEnemy.FindProperty("useWeightedSelection");
            SerializedProperty preventRepeatProp = _serializedEnemy.FindProperty("preventRepeatAbility");
            
            EditorGUILayout.PropertyField(followPatternProp, new GUIContent("Follow Ability Pattern"));
            EditorGUILayout.PropertyField(useWeightedProp, new GUIContent("Use Weighted Selection"));
            EditorGUILayout.PropertyField(preventRepeatProp, new GUIContent("Prevent Repeat Ability"));
            
            EditorGUILayout.Space(10);
            
            if (useActScalingProp.boolValue)
            {
                DrawActBasedView();
            }
            else
            {
                DrawDefaultView();
            }
            
            EditorGUILayout.EndScrollView();
            
            // Apply changes
            if (_serializedEnemy.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_selectedEnemy);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActBasedView()
        {
            SerializedProperty actConfigsProp = _serializedEnemy.FindProperty("actConfigurations");
            
            if (actConfigsProp == null || actConfigsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No act-specific configurations found.", MessageType.Warning);
                if (GUILayout.Button("Add Act Configuration"))
                {
                    actConfigsProp.InsertArrayElementAtIndex(actConfigsProp.arraySize);
                }
                return;
            }
            
            // Tab buttons
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < actConfigsProp.arraySize; i++)
            {
                SerializedProperty actProp = actConfigsProp.GetArrayElementAtIndex(i);
                SerializedProperty actNumberProp = actProp.FindPropertyRelative("actNumber");
                
                if (GUILayout.Toggle(_selectedActTab == i, $"Act {actNumberProp.intValue}", EditorStyles.toolbarButton))
                {
                    _selectedActTab = i;
                }
            }
            
            // Add/Remove buttons
            if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                actConfigsProp.InsertArrayElementAtIndex(actConfigsProp.arraySize);
                _selectedActTab = actConfigsProp.arraySize - 1;
            }
            if (actConfigsProp.arraySize > 0 && GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                actConfigsProp.DeleteArrayElementAtIndex(_selectedActTab);
                _selectedActTab = Mathf.Max(0, _selectedActTab - 1);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            if (_selectedActTab >= 0 && _selectedActTab < actConfigsProp.arraySize)
            {
                DrawActData(actConfigsProp.GetArrayElementAtIndex(_selectedActTab));
            }
        }
        
        private void DrawActData(SerializedProperty actProp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Act Number
            SerializedProperty actNumberProp = actProp.FindPropertyRelative("actNumber");
            EditorGUILayout.PropertyField(actNumberProp, new GUIContent("Act Number"));
            
            EditorGUILayout.Space(5);
            
            // Stats
            _showStats = EditorGUILayout.Foldout(_showStats, "Stats", true, EditorStyles.foldoutHeader);
            if (_showStats)
            {
                EditorGUI.indentLevel++;
                SerializedProperty maxHealthProp = actProp.FindPropertyRelative("maxHealth");
                EditorGUILayout.PropertyField(maxHealthProp, new GUIContent("Max Health"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Starting Statuses
            _showStatuses = EditorGUILayout.Foldout(_showStatuses, "Starting Statuses", true, EditorStyles.foldoutHeader);
            if (_showStatuses)
            {
                EditorGUI.indentLevel++;
                SerializedProperty statusesProp = actProp.FindPropertyRelative("startingStatuses");
                EditorGUILayout.PropertyField(statusesProp, new GUIContent("Starting Statuses"), true);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Abilities
            _showAbilities = EditorGUILayout.Foldout(_showAbilities, "Abilities", true, EditorStyles.foldoutHeader);
            if (_showAbilities)
            {
                EditorGUI.indentLevel++;
                SerializedProperty abilitiesProp = actProp.FindPropertyRelative("enemyAbilityList");
                EditorGUILayout.PropertyField(abilitiesProp, new GUIContent("Enemy Ability List"), true);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDefaultView()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Stats
            _showStats = EditorGUILayout.Foldout(_showStats, "Stats", true, EditorStyles.foldoutHeader);
            if (_showStats)
            {
                EditorGUI.indentLevel++;
                SerializedProperty maxHealthProp = _serializedEnemy.FindProperty("maxHealth");
                if (maxHealthProp != null)
                {
                    EditorGUILayout.PropertyField(maxHealthProp, new GUIContent("Max Health"));
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Starting Statuses
            _showStatuses = EditorGUILayout.Foldout(_showStatuses, "Starting Statuses", true, EditorStyles.foldoutHeader);
            if (_showStatuses)
            {
                EditorGUI.indentLevel++;
                SerializedProperty statusesProp = _serializedEnemy.FindProperty("startingStatuses");
                if (statusesProp != null)
                {
                    EditorGUILayout.PropertyField(statusesProp, new GUIContent("Starting Statuses"), true);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            
            // Abilities
            _showAbilities = EditorGUILayout.Foldout(_showAbilities, "Abilities", true, EditorStyles.foldoutHeader);
            if (_showAbilities)
            {
                EditorGUI.indentLevel++;
                SerializedProperty abilitiesProp = _serializedEnemy.FindProperty("enemyAbilityList");
                if (abilitiesProp != null)
                {
                    EditorGUILayout.PropertyField(abilitiesProp, new GUIContent("Enemy Ability List"), true);
                }
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}
