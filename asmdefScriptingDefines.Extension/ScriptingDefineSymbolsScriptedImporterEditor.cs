using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEditorInternal;
using UnityEngine;

namespace ForCuteIzmChan
{
    [CustomEditor(typeof(ScriptingDefineSymbolsScriptedImporter))]
    public class ScriptingDefineSymbolsScriptedImporterEditor : ScriptedImporterEditor
    {
        private static class Styles
        {
            public static readonly GUIContent apply = EditorGUIUtility.TrTextContent("Apply");
            public static readonly GUIContent revert = EditorGUIUtility.TrTextContent("Revert");
        }
        private ReorderableList list;
        private ScriptingDefineSymbolsScriptableObject obj;
        private bool modified;

        public override bool showImportedObject => false;

        public override void OnInspectorGUI()
        {
            if (obj == null)
            {
                InitializeFields();
            }
            if (list == null)
            {
                InitializeList();
            }

            list.DoLayoutList();
            
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(!modified))
            {
                if (GUILayout.Button(Styles.revert))
                {
                    InitializeFields();
                    InitializeList();
                }

                if (GUILayout.Button(Styles.apply))
                {
                    SaveAndInitializeFields();
                }
            }

            GUILayout.EndHorizontal();
        }

        public override void OnDisable()
        {
            if(!modified || obj == null) return;
            if (EditorUtility.DisplayDialog("Change Not Applied!", "Change has not been applied!", "Apply", "Revert"))
            {
                Save();
            }
        }

        private void SaveAndInitializeFields()
        {
            Save();
            InitializeFields();
            InitializeList();
        }

        private void Save()
        {
            var importer = (ScriptingDefineSymbolsScriptedImporter) this.target;
            File.WriteAllText(importer.assetPath, obj.ToString());
        }

        private void InitializeFields()
        {
            modified = false;
            var importer = (ScriptingDefineSymbolsScriptedImporter) this.target;
            obj = CreateInstance<ScriptingDefineSymbolsScriptableObject>();
            obj.Parse(File.ReadAllText(importer.assetPath));
        }

        private void InitializeList()
        {
            list = new ReorderableList(obj.Symbols, typeof(ScriptingDefineSymbol), true, false, true, true)
            {
                drawElementCallback = DrawElement,
                onAddCallback = OnAdd,
                onRemoveCallback = OnRemove,
                onReorderCallback = OnReorder,
                elementHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2,
                headerHeight = 3,
            };
        }

        private void DrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            var symbol = obj.Symbols[index];
            
            var scriptingDefineSymbolType = symbol.Type;
            var width = rect.width;
            var start = rect.x;
            var enumWidth = width / 3;
            var textStart = start + enumWidth;
            var textWidth = width - enumWidth;
            symbol.Type = (ScriptingDefineSymbolType)EditorGUI.EnumPopup(new Rect(rect.x, rect.y, enumWidth, rect.height), scriptingDefineSymbolType);
            if (symbol.Type != scriptingDefineSymbolType)
                modified = true;
            var oldSymbol = symbol.Symbol;
            symbol.Symbol = EditorGUI.TextField(new Rect(textStart, rect.y, textWidth, rect.height), symbol.Symbol);
            if (oldSymbol != symbol.Symbol)
                modified = true;

            list.list[index] = symbol;
        }

        private void OnReorder(ReorderableList reorderableList)
        {
            modified = true;
        }

        private void OnRemove(ReorderableList reorderableList)
        {
            modified = true;
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }

        private void OnAdd(ReorderableList reorderableList)
        {
            modified = true;
            obj.Symbols.Add(new ScriptingDefineSymbol(ScriptingDefineSymbolType.PlaceHolder, "PLACE_HOLDER"));
        }
    }
}