//#define SORT_LIST

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(LineListNode))]
    public class LineListNodeEditor : Editor
    {
        SerializedProperty propLineList;
        SerializedProperty propLineFunctionsList;
        SerializedObject serializedLineFunctionsObject;
        SerializedProperty propTargetMaterials;
        SerializedProperty propDoubleSidedMaterials;
        SerializedProperty propIgnoreObjectList;

        ReorderableList reorderableLineList;
        ReorderableList reorderableFunctionsList;
        ReorderableList reorderableTargetMaterialList;
        ReorderableList reorderableDoubleSidedMaterials;
        ReorderableList reorderableIgnoreObjectList;

        // Material Line FunctionsのMaterialsリストを更新するため、定期的な再描画を行う
        override public bool RequiresConstantRepaint() { return true; }

        const int doubleClickDiff = 500;

#if SORT_LIST
        /// <summary>
        /// hierarchyのLineListNodeの子のソートを行うコールバック
        /// </summary>
        static void SortLineListNodeCallback()
        {
            List<GameObject> lineListNodeList = Common.GetAllGameObject("LineListNode");

            // Hierarchyにない場合はコールバックから外す
            if (lineListNodeList.Count == 0)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorApplication.hierarchyWindowChanged -= SortLineListNodeCallback;
                };
                return;
            }

            foreach (var lineListNode in lineListNodeList)
            {
                List<GameObject> sortedList = new List<GameObject>();
                List<GameObject> children = new List<GameObject>();
                int count = lineListNode.transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    children.Add(lineListNode.transform.GetChild(i).gameObject);
                }

                sortedList.AddRange(children.SearchComponent<LineNode>());
                sortedList.AddRange(children.SearchComponent<MaterialLineFunctionsNode>());

                count = sortedList.Count;
                for (int i = 0; i < count; i++)
                {
                    sortedList[i].transform.SetSiblingIndex(i);
                }
            }
        }
#endif

        void OnEnable()
        {
            var currentNode = target as LineListNode;

            // ---------- Line List ----------

            propLineList =
                serializedObject.FindProperty("LineList");


            reorderableLineList =
                Common.CreateReorderableNodeList(serializedObject,
                                                 propLineList,
                                                 currentNode,
                                                 Prefabs.Line);


            // ---------- Material Line Functions List ----------

            propLineFunctionsList =
                serializedObject.FindProperty("LineFunctionsList");

            reorderableFunctionsList =
                Common.CreateReorderableNodeList(serializedObject,
                                                 propLineFunctionsList,
                                                 currentNode,
                                                 Prefabs.LineFunctions);

            // Elementを追加するコールバック
            reorderableFunctionsList.onAddCallback += (list) =>
            {
                ChangedLineFunctionsIndex();
            };

            // Elementを削除するコールバック
            reorderableFunctionsList.onRemoveCallback += (list) =>
            {
                ChangedLineFunctionsIndex();
            };

            // Elementの入れ替えを行った際に呼ばれるコールバック
            reorderableFunctionsList.onReorderCallback += (list) =>
            {
                ChangedLineFunctionsIndex();
            };

            // 選択状態変更
            reorderableFunctionsList.onSelectCallback += (list) =>
            {
                ChangedLineFunctionsIndex();
            };


            // ---------- Double Sided Materials ----------

            propDoubleSidedMaterials =
                serializedObject.FindProperty("DoubleSidedMaterials");

            reorderableDoubleSidedMaterials =
                Common.CreateMaterialList(
                    serializedObject,
                    propDoubleSidedMaterials,
                    (target as LineListNode).DoubleSidedMaterials,
                    selectedMaterials => 
                    {
                        serializedObject.Update();
                        propDoubleSidedMaterials.AppendObjects(selectedMaterials);
                        serializedObject.ApplyModifiedProperties();
                        reorderableDoubleSidedMaterials.index = reorderableDoubleSidedMaterials.count - 1;
                    });


            // ---------- Ignore Object List ----------

            propIgnoreObjectList =
                serializedObject.FindProperty("IgnoreObjectList");

            reorderableIgnoreObjectList =
                Common.CreateObjectList(
                    serializedObject,
                    propIgnoreObjectList,
                    (target as LineListNode).IgnoreObjectList,
                    selectedObjects => 
                    {
                        serializedObject.Update();
                        propIgnoreObjectList.AppendObjects(selectedObjects);
                        serializedObject.ApplyModifiedProperties();
                        reorderableIgnoreObjectList.index = reorderableIgnoreObjectList.count - 1;
                    });


#if SORT_LIST
            // Sortコールバックが登録されていない場合は登録
            if (EditorApplication.hierarchyWindowChanged == null)
            {
                EditorApplication.hierarchyWindowChanged += SortLineListNodeCallback;
                return;
            }
            Delegate[] del = EditorApplication.hierarchyWindowChanged.GetInvocationList();
            if (!del.Any(x => x.Method.Name == "SortLineListNodeCallback"))
            {
                EditorApplication.hierarchyWindowChanged += SortLineListNodeCallback;
            }
#endif
        }



        /// <summary>
        /// LineListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        void MakeLineList(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            EditorGUILayout.LabelField("Line List");

            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropNode<LineNode>(lineListNode,
                                             propLineList,
                                             verticalLayout.rect);

            using (verticalLayout)
            {
                reorderableLineList.DoLayoutList();
            }
        }

        /// <summary>
        /// LineFunctionsListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        void MakeLineFunctionsList(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            EditorGUILayout.LabelField("Material Line Functions List");

            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropNode<MaterialLineFunctionsNode>(lineListNode,
                                                              propLineFunctionsList,
                                                              verticalLayout.rect);

            using (verticalLayout)
            {
                reorderableFunctionsList.DoLayoutList();
            }
        }

        /// <summary>
        /// DoubleSidedMaterialListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        void MakeDoubleSidedMaterialList(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            EditorGUILayout.LabelField("Double Sided Material List");


            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<Material, LineListNode>(lineListNode,
                                                             null,
                                                             propDoubleSidedMaterials,
                                                             verticalLayout.rect,
                                                             GetDoubleSidedMateiralList<Material>,
                                                             false);

            using (verticalLayout)
            {
                reorderableDoubleSidedMaterials.DoLayoutList();
            }
        }

        /// <summary>
        /// IgnoreObjectListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        void MakeIgnoreObjectList(GUIStyle style)
        {
            var lineListNode = target as LineListNode;

            EditorGUILayout.LabelField("Ignore Object List");

            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<GameObject, LineListNode>(lineListNode,
                                                               null,
                                                               propIgnoreObjectList,
                                                               verticalLayout.rect,
                                                               GetIgnoreObjectList<GameObject>,
                                                               false);

            using (verticalLayout)
            {
                reorderableIgnoreObjectList.DoLayoutList();
            }
        }

        /// <summary>
        /// delegate用のDoubleSidedMaterialリスト取得関数
        /// </summary>
        /// <typeparam name="T">取得するリストの型</typeparam>
        /// <param name="node">リストの存在するノード</param>
        /// <param name="varName">名前</param>
        /// <returns></returns>
        List<T> GetDoubleSidedMateiralList<T>(NodeBase node, String varName)
             where T : UnityEngine.Object
        {
            return (node as LineListNode).DoubleSidedMaterials
                                           .Select(x => x as T)
                                           .ToList();
        }

        /// <summary>
        /// delegate用のIgnoreObjectリスト取得関数
        /// </summary>
        /// <typeparam name="T">取得するリストの型</typeparam>
        /// <param name="node">リストの存在するノード</param>
        /// <param name="varName">名前</param>
        /// <returns></returns>
        List<T> GetIgnoreObjectList<T>(NodeBase node, String varName)
             where T : UnityEngine.Object
        {
            return (node as LineListNode).IgnoreObjectList
                                         .Select(x => x as T)
                                         .ToList();
        }

        /// <summary>
        /// delegate用のTargetMaterialリスト取得関数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="varName"></param>
        /// <returns></returns>
        List<T> GetLineFunctionsList<T>(NodeBase node, String varName)
             where T : UnityEngine.Object
        {
            MaterialLineFunctionsNode lineFunctionsNode =
                node as MaterialLineFunctionsNode;

            List<T> newList = lineFunctionsNode.TargetMaterials
                                         .Select(x => x as T)
                                         .ToList();

            return newList;
        }

        /// <summary>
        /// TargetMaterialListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        void MakeTargetMaterialList(GUIStyle style)
        {
            if (serializedLineFunctionsObject == null ||
                serializedLineFunctionsObject.targetObject == null)
            {
                return;
            }

            int index = reorderableFunctionsList.index;

            if (index == -1)
            {
                return;
            }

            var lineListNode = target as LineListNode;
            SerializedProperty propFunctions = propLineFunctionsList.GetArrayElementAtIndex(index);

            GameObject obj = propFunctions.objectReferenceValue as GameObject;

            String label = obj.name + " -> Target Material";
            EditorGUILayout.LabelField(label);

            serializedLineFunctionsObject.Update();

            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<Material, MaterialLineFunctionsNode>(
                                                lineListNode,
                                                lineListNode.LineFunctionsList,
                                                propTargetMaterials,
                                                verticalLayout.rect,
                                                GetLineFunctionsList<Material>,
                                                true);


            // 表示
            using (verticalLayout)
            {
                reorderableTargetMaterialList.DoLayoutList();
            }
            serializedLineFunctionsObject.ApplyModifiedProperties();


        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            GUIStyle style = new GUIStyle();
            style.margin = new RectOffset(4, 8, 0, 4);

            MakeLineList(style);

            MakeLineFunctionsList(style);

            MakeTargetMaterialList(style);

            MakeDoubleSidedMaterialList(style);

            MakeIgnoreObjectList(style);

            serializedObject.ApplyModifiedProperties();


            //
            //EditorGUILayout.BeginHorizontal();
            //if (GUILayout.Button("Import"))
            //{
            //    ImportLineListSettings();
            //}
            //if (GUILayout.Button("Export"))
            //{
            //    ExportLineListSettings();
            //}
            //EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// LineFunctionIndexを変更したときに呼ぶ
        /// </summary>
        void ChangedLineFunctionsIndex()
        {
            int funcsIndex = reorderableFunctionsList.index;
            if (funcsIndex < 0 ||
               funcsIndex >= propLineFunctionsList.arraySize)
            {
                return;
            }

            SerializedProperty propFuncs =
                propLineFunctionsList.GetArrayElementAtIndex(funcsIndex);

            GameObject currentLineFuncs =
                propFuncs.objectReferenceValue as GameObject;

            serializedLineFunctionsObject =
                currentLineFuncs != null ?
                new SerializedObject(currentLineFuncs.GetComponent<MaterialLineFunctionsNode>()) :
                null;

            if (serializedLineFunctionsObject == null ||
                serializedLineFunctionsObject.targetObject == null)
            {
                return;
            }

            propTargetMaterials = serializedLineFunctionsObject.FindProperty("TargetMaterials");

            reorderableTargetMaterialList =
                Common.CreateMaterialList(
                    serializedLineFunctionsObject,
                    propTargetMaterials,
                    new List<Material>(),
                    selectedMaterials => 
                    {
                        serializedLineFunctionsObject.Update();
                        propTargetMaterials.AppendObjects(selectedMaterials);
                        serializedLineFunctionsObject.ApplyModifiedProperties();
                        reorderableTargetMaterialList.index = reorderableTargetMaterialList.count - 1;
                    });

        }

        /// <summary>
        /// MenuにLineListノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Line List Node", false)]
        public static void OpenLineListNode()
        {
            List<GameObject> lineLists = Common.GetAllGameObject("LineListNode");
            if (lineLists.Count != 0)
            {
                // 既にHierarchyに追加されている
                Debug.LogWarning("Line List Node is already added in Hierarchy");
                return;
            }

            GameObject lineList = Instantiate(Prefabs.LineList);
            Undo.RegisterCreatedObjectUndo(lineList, "Create Line List Node");
            lineList.name = Prefabs.LineList.name;

#if SORT_LIST
            // Sortコールバックが登録されていない場合は登録
            if (EditorApplication.hierarchyWindowChanged == null)
            {
                EditorApplication.hierarchyWindowChanged += SortLineListNodeCallback;
                return;
            }
            Delegate[] del = EditorApplication.hierarchyWindowChanged.GetInvocationList();
            if (!del.Any(x => x.Method.Name == "SortLineListNodeCallback"))
            {
                EditorApplication.hierarchyWindowChanged += SortLineListNodeCallback;
            }
#endif
        }

        /// <summary>
        /// xmlファイルからLineListNodeの設定を読み込む。
        /// </summary>
        private void ImportLineListSettings()
        {
            string openPath = EditorUtility.OpenFilePanel("Import Pencil Data", Application.dataPath, "xml");

            if (!string.IsNullOrEmpty(openPath))
            {
                var lineListNode = target as LineListNode;
                if(lineListNode != null)
                {
                    lineListNode.ImportFromXmlFile(
                        openPath,
                        (filter, name) =>  Utilities.FindAssetInProjectOnEditor(filter, name));
                }
            }
        }

        /// <summary>
        /// LineListNodeの設定をxml形式で出力する。
        /// </summary>
        private void ExportLineListSettings()
        {
            string savePath = EditorUtility.SaveFilePanel("Export Pencil Data", Application.dataPath, "pencilData", "xml");
            if(!string.IsNullOrEmpty(savePath))
            {
                var lineListNode = target as LineListNode;
                if(lineListNode != null)
                {
                    lineListNode.ExportToXmlFile(savePath);
                }

            }
        }

    }
}

