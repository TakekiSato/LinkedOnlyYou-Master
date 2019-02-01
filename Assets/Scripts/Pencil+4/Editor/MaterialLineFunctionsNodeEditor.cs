﻿using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(MaterialLineFunctionsNode))]
    public class MaterialLineFunctionsNodeEditor : Editor
    {
        SerializedProperty propTargetMaterials;
        SerializedProperty propOutlineOn;
        SerializedProperty propOutlineColor;
        SerializedProperty propOutlineAmount;
        SerializedProperty propObjectOn;
        SerializedProperty propObjectColor;
        SerializedProperty propObjectAmount;
        SerializedProperty propIntersectionOn;
        SerializedProperty propIntersectionColor;
        SerializedProperty propIntersectionAmount;
        SerializedProperty propSmoothOn;
        SerializedProperty propSmoothColor;
        SerializedProperty propSmoothAmount;
        SerializedProperty propMaterialOn;
        SerializedProperty propMaterialColor;
        SerializedProperty propMaterialAmount;
        SerializedProperty propNormalAngleOn;
        SerializedProperty propNormalAngleColor;
        SerializedProperty propNormalAngleAmount;
        SerializedProperty propWireframeOn;
        SerializedProperty propWireframeColor;
        SerializedProperty propWireframeAmount;
        SerializedProperty propDisableIntersection;
        SerializedProperty propDrawHiddenLines;
        SerializedProperty propDrawHiddenLinesOfTarget;
        SerializedProperty propDrawObjects;
        SerializedProperty propDrawMaterials;
        SerializedProperty propMaskHiddenLinesOfTarget;
        SerializedProperty propMaskObjects;
        SerializedProperty propMaskMaterials;

        bool foldoutReplaceLineColor = true;
        bool foldoutEdgeDetection = true;
        static GUIStyle indentStyle;

        ReorderableList reorderableTargetMaterialList;
        ReorderableList reorderableDrawObjectList;
        ReorderableList reorderableDrawMaterialList;
        ReorderableList reorderableMaskObjectList;
        ReorderableList reorderableMaskMaterialList;

        const int doubleClickDiff = 500;

        void ChangeIndent()
        {
            int indent = EditorGUI.indentLevel > 0 ? EditorGUI.indentLevel : 0;
            indentStyle.margin = new RectOffset(indent * 20, 0, 0, 0);
        }

        /// <summary>
        /// delegate用リスト取得関数
        /// </summary>
        /// <typeparam name="T">リストのタイプ</typeparam>
        /// <param name="node">取得を行うノード</param>
        /// <param name="varName">取得する変数名</param>
        /// <returns>取得リスト</returns>
        List<T> GetList<T>(NodeBase node, String varName)
            where T : UnityEngine.Object
        {
            MaterialLineFunctionsNode lineFunctionsNode =
                node as MaterialLineFunctionsNode;

            List<T> newList = new List<T>();
            switch (varName)
            {
                case "TargetMaterials":
                    newList = lineFunctionsNode.TargetMaterials
                                                .Select(x => x as T)
                                                .ToList();
                    break;

                case "DrawObjects":
                    newList = lineFunctionsNode.DrawObjects
                                                .Select(x => x as T)
                                                .ToList();
                    break;

                case "DrawMaterials":
                    newList = lineFunctionsNode.DrawMaterials
                                                .Select(x => x as T)
                                                .ToList();
                    break;

                case "MaskObjects":
                    newList = lineFunctionsNode.MaskObjects
                                                .Select(x => x as T)
                                                .ToList();
                    break;

                case "MaskMaterials":
                    newList = lineFunctionsNode.MaskMaterials
                                                .Select(x => x as T)
                                                .ToList();
                    break;

                default:
                    break;
            }

            return newList;
        }



        /// <summary>
        /// Objectを追加、削除を行うリストを作成
        /// </summary>
        /// <typeparam name="T">リストの型</typeparam>
        /// <param name="prop">リストのプロパティ</param>
        /// <param name="reorderList">リスト</param>
        /// <param name="style">リストのスタイル</param>
        void MakeList<T>(SerializedProperty prop,
                         ReorderableList reorderList,
                         GUIStyle style,
                         String label,
                         bool checkOtherNode = false)
            where T : UnityEngine.Object
        {
            List<GameObject> lineFunctionsList = new List<GameObject>();

            MaterialLineFunctionsNode currentLineFunctions = target as MaterialLineFunctionsNode;

            LineListNode lineListNode =
                         currentLineFunctions.transform.parent != null ?
                         currentLineFunctions.transform.parent.GetComponent<LineListNode>() :
                         null;

            List<GameObject> parentList;

            if (lineListNode != null)
            {
                lineFunctionsList.AddRange(lineListNode.LineFunctionsList);
                parentList = lineListNode.LineFunctionsList;
            }
            else
            {
                checkOtherNode = false;
                parentList = null;
            }

            EditorGUILayout.LabelField(label);


            EditorGUILayout.VerticalScope verticalLayout
                = new EditorGUILayout.VerticalScope(style);

            // ドラッグアンドドロップ処理
            Common.DragAndDropObject<T, MaterialLineFunctionsNode>(
                                        lineListNode,
                                        parentList,
                                        prop,
                                        verticalLayout.rect,
                                        GetList<T>,
                                        checkOtherNode);

            // 表示
            using (verticalLayout)
            {
                reorderList.DoLayoutList();
            }

        }


        void MakeTargetMaterials(MaterialLineFunctionsNode MaterialLineFunctionsNode)
        {

            GUIStyle style = new GUIStyle();
            style.margin = new RectOffset(4, 8, 0, 4);

            MakeList<Material>(propTargetMaterials,
                               reorderableTargetMaterialList,
                               style,
                               "Target Materials",
                               true);

            EditorGUILayout.Separator();
        }


        /// <summary>
        /// ReplaceLineColor項目のGUIパーツ
        /// </summary>
        /// <param name="label">表示するラベル</param>
        /// <param name="propOn">Enable</param>
        /// <param name="propColor">色</param>
        /// <param name="propAmount">量</param>
        void MakeReplaceLineColorParts(String label,
                                       SerializedProperty propOn,
                                       SerializedProperty propColor,
                                       SerializedProperty propAmount)
        {
            propOn.boolValue = EditorGUILayout.Toggle(label, propOn.boolValue);

            ++EditorGUI.indentLevel;

            EditorGUI.BeginDisabledGroup(!propOn.boolValue); // Replace Line Color Disable

            propColor.colorValue = EditorGUILayout.ColorField("Replace Color", propColor.colorValue);
            propAmount.floatValue = EditorGUILayout.Slider("Replace Amount",
                                                           propAmount.floatValue,
                                                           0.0f, 1.0f);

            EditorGUI.EndDisabledGroup(); // End of Replace Line Color Disable

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// ReplaceLineColor項目のGUIの追加
        /// </summary>
        void MakeReplaceLineColor()
        {
            foldoutReplaceLineColor =
                EditorGUILayout.Foldout(foldoutReplaceLineColor, "Replace Line Color");
            if (!foldoutReplaceLineColor)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Outline
            MakeReplaceLineColorParts("Outline",
                                      propOutlineOn,
                                      propOutlineColor,
                                      propOutlineAmount);

            // Object
            MakeReplaceLineColorParts("Object",
                                      propObjectOn,
                                      propObjectColor,
                                      propObjectAmount);

            // Intersection
            MakeReplaceLineColorParts("Intersection",
                                      propIntersectionOn,
                                      propIntersectionColor,
                                      propIntersectionAmount);

            // Smooth
            MakeReplaceLineColorParts("Smoothing Boundary",
                                      propSmoothOn,
                                      propSmoothColor,
                                      propSmoothAmount);

            // Material
            MakeReplaceLineColorParts("Material Boundary",
                                      propMaterialOn,
                                      propMaterialColor,
                                      propMaterialAmount);

            // Normal Angle
            MakeReplaceLineColorParts("Normal Angle",
                                      propNormalAngleOn,
                                      propNormalAngleColor,
                                      propNormalAngleAmount);

            // Wireframe
            MakeReplaceLineColorParts("Wireframe",
                                      propWireframeOn,
                                      propWireframeColor,
                                      propWireframeAmount);

            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// EdgeDetection項目のGUIを追加
        /// </summary>
        void MakeEdgeDetection(MaterialLineFunctionsNode node)
        {
            foldoutEdgeDetection =
                EditorGUILayout.Foldout(foldoutEdgeDetection, "EdgeDetection");
            if (!foldoutEdgeDetection)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Disable Intersection
            propDisableIntersection.boolValue =
                EditorGUILayout.Toggle("Disable Intersection",
                                       propDisableIntersection.boolValue);

            // Draw Hidden Lines
            propDrawHiddenLines.boolValue =
                EditorGUILayout.Toggle("Draw Hidden Lines",
                                        propDrawHiddenLines.boolValue);


            using (new EditorGUI.DisabledGroupScope(propDrawHiddenLines.boolValue))
            {
                // Draw Hidden Lines of Target
                EditorGUILayout.LabelField("Draw Hidden Lines of Target");

                ++EditorGUI.indentLevel;

                propDrawHiddenLinesOfTarget.boolValue =
                    EditorGUILayout.Toggle("On",
                                            propDrawHiddenLinesOfTarget.boolValue);

                GUIStyle style = new GUIStyle();
                style.margin = new RectOffset(60, 8, 0, 4);

                using (new EditorGUI.DisabledGroupScope(!propDrawHiddenLinesOfTarget.boolValue))
                {
                    // Draw Objects
                    MakeList<GameObject>(propDrawObjects,
                                         reorderableDrawObjectList,
                                         style,
                                         "Object List");

                    // Draw Materials
                    MakeList<Material>(propDrawMaterials,
                                       reorderableDrawMaterialList,
                                       style,
                                       "Material List");

                }

                --EditorGUI.indentLevel;

                // Mask Hidden Lines of Target
                EditorGUILayout.LabelField("Mask Hidden Lines of Target");

                ++EditorGUI.indentLevel;

                propMaskHiddenLinesOfTarget.boolValue =
                    EditorGUILayout.Toggle("On",
                                            propMaskHiddenLinesOfTarget.boolValue);


                using (new EditorGUI.DisabledGroupScope(!propMaskHiddenLinesOfTarget.boolValue))
                {
                    // Mask Objects
                    MakeList<GameObject>(propMaskObjects,
                                         reorderableMaskObjectList,
                                         style,
                                         "Object List");

                    // Mask Materials
                    MakeList<Material>(propMaskMaterials,
                                       reorderableMaskMaterialList,
                                       style,
                                       "Material List");
                }

                --EditorGUI.indentLevel;
            }


            --EditorGUI.indentLevel;

        }

        void OnEnable()
        {
            if (indentStyle == null)
            {
                indentStyle = new GUIStyle();
                indentStyle.border = new RectOffset(1, 1, 1, 1);
                indentStyle.padding = new RectOffset(0, 0, 0, 0);
                indentStyle.margin = new RectOffset(60, 0, 0, 0);
            }

            propTargetMaterials = serializedObject.FindProperty("TargetMaterials");
            propOutlineOn = serializedObject.FindProperty("OutlineOn");
            propOutlineColor = serializedObject.FindProperty("OutlineColor");
            propOutlineAmount = serializedObject.FindProperty("OutlineAmount");
            propObjectOn = serializedObject.FindProperty("ObjectOn");
            propObjectColor = serializedObject.FindProperty("ObjectColor");
            propObjectAmount = serializedObject.FindProperty("ObjectAmount");
            propIntersectionOn = serializedObject.FindProperty("IntersectionOn");
            propIntersectionColor = serializedObject.FindProperty("IntersectionColor");
            propIntersectionAmount = serializedObject.FindProperty("IntersectionAmount");
            propSmoothOn = serializedObject.FindProperty("SmoothOn");
            propSmoothColor = serializedObject.FindProperty("SmoothColor");
            propSmoothAmount = serializedObject.FindProperty("SmoothAmount");
            propMaterialOn = serializedObject.FindProperty("MaterialOn");
            propMaterialColor = serializedObject.FindProperty("MaterialColor");
            propMaterialAmount = serializedObject.FindProperty("MaterialAmount");
            propNormalAngleOn = serializedObject.FindProperty("NormalAngleOn");
            propNormalAngleColor = serializedObject.FindProperty("NormalAngleColor");
            propNormalAngleAmount = serializedObject.FindProperty("NormalAngleAmount");
            propWireframeOn = serializedObject.FindProperty("WireframeOn");
            propWireframeColor = serializedObject.FindProperty("WireframeColor");
            propWireframeAmount = serializedObject.FindProperty("WireframeAmount");
            propDisableIntersection = serializedObject.FindProperty("DisableIntersection");
            propDrawHiddenLines = serializedObject.FindProperty("DrawHiddenLines");
            propDrawHiddenLinesOfTarget = serializedObject.FindProperty("DrawHiddenLinesOfTarget");
            propDrawObjects = serializedObject.FindProperty("DrawObjects");
            propDrawMaterials = serializedObject.FindProperty("DrawMaterials");
            propMaskHiddenLinesOfTarget = serializedObject.FindProperty("MaskHiddenLinesOfTarget");
            propMaskObjects = serializedObject.FindProperty("MaskObjects");
            propMaskMaterials = serializedObject.FindProperty("MaskMaterials");


            reorderableTargetMaterialList = Common.CreateMaterialList(
                serializedObject,
                propTargetMaterials,
                Resources.FindObjectsOfTypeAll<MaterialLineFunctionsNode>().SelectMany(x => x.TargetMaterials),
                selectedMaterials => 
                {
                    serializedObject.Update();
                    propTargetMaterials.AppendObjects(selectedMaterials);
                    serializedObject.ApplyModifiedProperties();
                    reorderableTargetMaterialList.index = reorderableTargetMaterialList.count - 1;
                });

            reorderableDrawObjectList = Common.CreateObjectList(
                serializedObject,
                propDrawObjects,
                (target as MaterialLineFunctionsNode).DrawObjects,
                selectedObjects => 
                {
                    serializedObject.Update();
                    propDrawObjects.AppendObjects(selectedObjects);
                    serializedObject.ApplyModifiedProperties();
                    reorderableDrawObjectList.index = reorderableDrawObjectList.count - 1;
                });


            reorderableDrawMaterialList = Common.CreateMaterialList(
                serializedObject,
                propDrawMaterials,
                (target as MaterialLineFunctionsNode).DrawMaterials,
                selectedMaterials => 
                {
                    serializedObject.Update();
                    propDrawMaterials.AppendObjects(selectedMaterials);
                    serializedObject.ApplyModifiedProperties();
                    reorderableDrawMaterialList.index = reorderableDrawMaterialList.count - 1;
                });


            reorderableMaskObjectList = Common.CreateObjectList(
                serializedObject,
                propMaskObjects,
                (target as MaterialLineFunctionsNode).MaskObjects,
                selectedObjects => 
                {
                    serializedObject.Update();
                    propMaskObjects.AppendObjects(selectedObjects);
                    serializedObject.ApplyModifiedProperties();
                    reorderableMaskObjectList.index = reorderableMaskObjectList.count - 1;
                });


            reorderableMaskMaterialList = Common.CreateMaterialList(
                serializedObject,
                propMaskMaterials,
                (target as MaterialLineFunctionsNode).MaskMaterials,
                selectedMaterials => 
                {
                    serializedObject.Update();
                    propMaskMaterials.AppendObjects(selectedMaterials);
                    serializedObject.ApplyModifiedProperties();
                    reorderableMaskMaterialList.index = reorderableMaskMaterialList.count - 1;
                });

        }

        public override void OnInspectorGUI()
        {
            var node = target as MaterialLineFunctionsNode;
            serializedObject.Update();

            MakeTargetMaterials(node);
            MakeReplaceLineColor();
            MakeEdgeDetection(node);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにLineFunctionsノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Material Line Functions Node", false)]
        public static void OpenMaterialLineFunctionsNode()
        {
            List<GameObject> allGameObjs = Common.GetAllGameObject();
            GameObject newLineFunctions = Instantiate(Prefabs.LineFunctions);
            Undo.RegisterCreatedObjectUndo(newLineFunctions, "Create Material Line Functions Node");
            newLineFunctions.name = allGameObjs.GetUniqueName(Prefabs.LineFunctions);

            // 選択しているものにLineListNodeがあれば優先的に子に設定する
            List<GameObject> selectList = Selection.gameObjects.ToList();
            List<GameObject> lineList = selectList.SearchComponent<LineListNode>();
            if (lineList.Count > 0)
            {
                newLineFunctions.transform.parent = lineList[0].transform;
                return;
            }

            // LineListがHierarchy内にあれば子に設定する
            lineList = allGameObjs.SearchComponent<LineListNode>();
            if (lineList.Count > 0)
            {
                newLineFunctions.transform.parent = lineList[0].transform;
            }

        }
    }
}
