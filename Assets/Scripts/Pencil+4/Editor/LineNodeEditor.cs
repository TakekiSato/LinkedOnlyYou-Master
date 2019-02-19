//#define SORT_LIST

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Pencil_4;

namespace Pcl4Editor
{
    using Common = EditorCommons;

    [CustomEditor(typeof(LineNode))]
    public class LineNodeEditor : Editor
    {

        // foldout
        public bool foldoutBasicParam = true;
        public bool foldoutLineSize = true;
        public bool foldoutOthers = true;
        public bool foldoutLineSet = true;
        public bool foldoutBrush = true;
        public bool foldoutEdge = true;
        public bool foldoutReduction = true;

        public LineSetNode.LineType currentLineType =
        LineSetNode.LineType.Visible;

        private SerializedProperty propLineSets;
        private SerializedProperty propObjects;
        private SerializedProperty propMaterials;
        private SerializedProperty propLineSize;
        private SerializedProperty propOverSampling;
        private SerializedProperty propAntialiasing;
        private SerializedProperty propOffScreenDistance;
        private SerializedProperty propRandomSeed;

        private SerializedProperty propId;

        ReorderableList reorderableSetList;

        struct PropertyLineType
        {
            public SerializedProperty propBrushSettings;
            public SerializedProperty propBrushDetail;
            public SerializedProperty propBlendMode;
            public SerializedProperty propBlendAmount;
            public SerializedProperty propColor;
            public SerializedProperty propColorMap;
            public SerializedProperty propMapOpacity;
            public SerializedProperty propSize;
            public SerializedProperty propSizeMap;
            public SerializedProperty propSizeMapAmount;
            public SerializedProperty propStretch;
            public SerializedProperty propAngle;
            public SerializedProperty propEdgeOutlineOn;
            public SerializedProperty propEdgeOutlineS;
            public SerializedProperty propEdgeOutline;
            public SerializedProperty propEdgeObjectOn;
            public SerializedProperty propEdgeObjectS;
            public SerializedProperty propEdgeObject;
            public SerializedProperty propEdgeIntersectionOn;
            public SerializedProperty propEdgeIntersectionS;
            public SerializedProperty propEdgeIntersection;
            public SerializedProperty propEdgeSmoothOn;
            public SerializedProperty propEdgeSmoothS;
            public SerializedProperty propEdgeSmooth;
            public SerializedProperty propEdgeMaterialOn;
            public SerializedProperty propEdgeMaterialS;
            public SerializedProperty propEdgeMaterial;
            public SerializedProperty propEdgeNormalAngleOn;
            public SerializedProperty propEdgeNormalAngleS;
            public SerializedProperty propEdgeNormalAngle;
            public SerializedProperty propEdgeNormalAngleMin;
            public SerializedProperty propEdgeNormalAngleMax;
            public SerializedProperty propEdgeWireframeOn;
            public SerializedProperty propEdgeWireframeS;
            public SerializedProperty propEdgeWireframe;
            public SerializedProperty propSizeReductionOn;
            public SerializedProperty propSizeReduction;
            public SerializedProperty propAlphaReductionOn;
            public SerializedProperty propAlphaReduction;
        }

        private PropertyLineType visibleParams;
        private PropertyLineType hiddenParams;
        private PropertyLineType currentParams;

        private SerializedProperty propWeldsEdges;
        private SerializedProperty propMaskHiddenLines;

        [SerializeField]
        private SerializedObject serializedLineSetParams;
        [SerializeField]
        private SerializedObject serializedBrushSettingsVisibleParams;
        [SerializeField]
        private SerializedObject serializedBrushSettingsHiddenParams;
        [SerializeField]
        private SerializedObject serializedBrushDetailVisibleParams;
        [SerializeField]
        private SerializedObject serializedBrushDetailHiddenParams;

        private GUIStyle listBoxStyle;
        private GUIStyle inListBoxStyle;
        private GUIStyle indent1Style;

        ReorderableList reorderableObjects = null;
        ReorderableList reorderableMaterials = null;
        GameObject SelectedLineSet = null;
        int objectPickerID = -1;
        int materialPickerID = -1;

        // Line SetのObjects / Materialsリストの更新するため、定期的な再描画を行う
        override public bool RequiresConstantRepaint() { return true;  }

#if SORT_LIST
        /// <summary>
        /// hierarchyが変更されたときにLineNodeの子を並び替えるCB
        /// </summary>
        static void SortLineSetNodeCallback()
        {
            List<GameObject> lineListNodeList = Common.GetAllGameObject("LineNode");

            // Hierarchyにない場合はコールバックから外す
            if (lineListNodeList.Count == 0)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorApplication.hierarchyWindowChanged -= SortLineSetNodeCallback;
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

                sortedList.AddRange(children.SearchComponent<LineSetNode>());

                count = sortedList.Count;
                for (int i = 0; i < count; i++)
                {
                    sortedList[i].transform.SetSiblingIndex(i);
                }
            }
        }
#endif

        List<T> GetList<T>(NodeBase node, String varName)
            where T : UnityEngine.Object
        {
            LineSetNode lineSetNode = node as LineSetNode;

            List<T> newList = new List<T>();

            switch (varName)
            {
                case "Objects":
                    newList = lineSetNode.Objects
                                         .Select(x => x as T)
                                         .ToList();
                    break;

                case "Materials":
                    newList = lineSetNode.Materials
                                         .Select(x => x as T)
                                         .ToList();
                    break;

                default:
                    break;
            }

            return newList;
        }


        /// <summary>
        /// ObjectまたはMaterialの追加、削除を行うリストを作成
        /// </summary>
        /// <typeparam name="T">リストの型</typeparam>
        /// <param name="prop">リストのプロパティ</param>
        /// <param name="reorderList">リスト</param>
        /// <param name="pickerId">オブジェクトピッカーのID</param>
        /// <param name="style">リストのスタイル</param>
        void MakeList<T>(SerializedProperty prop,
                         ReorderableList reorderList,
                         int pickerId,
                         GUIStyle style)
            where T : UnityEngine.Object
        {
            var lineNode = target as LineNode;

            List<GameObject> setList = new List<GameObject>();
            bool isGameObject = typeof(T) == typeof(GameObject);


            int index = reorderableSetList.index;

            if (index == -1)
            {
                return;
            }

            int size = propLineSets.arraySize;
            for (int i = 0; i < size; i++)
            {
                SerializedProperty propLineSet = propLineSets.GetArrayElementAtIndex(i);
                setList.Add(propLineSet.objectReferenceValue as GameObject);
            }

            UnityEngine.Object obj = setList[index];

            String label = obj.name;
            label += isGameObject ? " -> Object List" :
                                    " -> " + typeof(T).Name + " List";

            EditorGUILayout.LabelField(label);

            serializedLineSetParams.Update();


            EditorGUILayout.VerticalScope verticalLayout
                = new EditorGUILayout.VerticalScope(style);

            // D&D
            Common.DragAndDropObject<T, LineSetNode>(lineNode,
                                                     lineNode.LineSets,
                                                     prop,
                                                     verticalLayout.rect,
                                                     GetList<T>,
                                                     true);

            // 表示
            using (verticalLayout)
            {
                reorderList.DoLayoutList();
            }

            serializedLineSetParams.ApplyModifiedProperties();

        }

        /// <summary>
        /// BasicParametersのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        void MakeBasicParameters(LineNode lineNode)
        {
            foldoutBasicParam =
                EditorGUILayout.Foldout(foldoutBasicParam, "Basic Parameters");
            if (!foldoutBasicParam)
            {
                return;
            }

            ++EditorGUI.indentLevel;


            // Line Set List
            EditorGUILayout.LabelField("Line Set List");

            GUIStyle style = new GUIStyle();
            style.margin = new RectOffset(30, 8, 0, 4);

            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropNode<LineSetNode>(lineNode, propLineSets, verticalLayout.rect);

            using (verticalLayout)
            {
                reorderableSetList.DoLayoutList();
            }

            // LineSetが選択されていない場合は表示なし
            if (reorderableSetList.index < 0 ||
               reorderableSetList.index >= propLineSets.arraySize)
            {
                --EditorGUI.indentLevel;
                return;
            }

            SerializedProperty propLineSet = propLineSets.GetArrayElementAtIndex(reorderableSetList.index);
            if (propLineSet.objectReferenceValue == null)
            {
                --EditorGUI.indentLevel;
                return;
            }

            LineSetNode lineSetNode = (propLineSet.objectReferenceValue as GameObject)
                                                  .GetComponent<LineSetNode>();

            if (lineSetNode == null)
            {
                --EditorGUI.indentLevel;
                return;
            }

            if (serializedLineSetParams == null)
            {
                return;
            }

            if (serializedLineSetParams.targetObject == null)
            {
                ChangedLineSet();
            }
            serializedLineSetParams.Update();

            // Objects
            MakeList<GameObject>(propObjects,
                                 reorderableObjects,
                                 objectPickerID,
                                 style);


            // Materials
            MakeList<Material>(propMaterials,
                               reorderableMaterials,
                               materialPickerID,
                               style);



            serializedLineSetParams.ApplyModifiedProperties();

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// LineSizeのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        void MakeLineSize(LineNode lineNode)
        {
            foldoutLineSize =
                EditorGUILayout.Foldout(foldoutLineSize, "Line Size");
            if (!foldoutLineSize)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // enumの型を直す
            var lineSize = (LineNode.LineSize)Enum
                           .GetValues(typeof(LineNode.LineSize))
                           .GetValue(propLineSize.enumValueIndex);

            propLineSize.enumValueIndex = (int)(LineNode.LineSize)EditorGUILayout.EnumPopup("Line Size", lineSize);

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }


        /// <summary>
        /// OthersのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        void MakeOthers(LineNode lineNode)
        {
            foldoutOthers =
                EditorGUILayout.Foldout(foldoutOthers, "Others");
            if (!foldoutOthers)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            // Over Sampling
            propOverSampling.intValue =
                EditorGUILayout.IntSlider("Over Sampling",
                                          propOverSampling.intValue,
                                          1, 4);

            // Antialiasing
            propAntialiasing.floatValue =
                EditorGUILayout.Slider("Antialiasing",
                                       propAntialiasing.floatValue,
                                       0.0f, 2.0f);

            // Off Screen Distance
            propOffScreenDistance.floatValue =
                EditorGUILayout.Slider("Off Screen Distance",
                                       propOffScreenDistance.floatValue,
                                       0.0f, 1000.0f);

            // Ramdom Seed
            propRandomSeed.intValue =
                EditorGUILayout.IntSlider("Random Seed",
                                          propRandomSeed.intValue,
                                          0, 65535);

            EditorGUILayout.Separator();
            --EditorGUI.indentLevel;

        }


        /// <summary>
        /// LineSetのGUIを作成
        /// </summary>
        /// <param name="lineNode">LineNode</param>
        void MakeLineSet(LineNode lineNode)
        {
            if (propLineSets.arraySize < 0 ||
                propLineSets.arraySize - 1 < reorderableSetList.index ||
                reorderableSetList.index < 0)
            {
                return;
            }

            SerializedProperty propLineSet =
                propLineSets.GetArrayElementAtIndex(reorderableSetList.index);

            GameObject lineSet = propLineSet.objectReferenceValue as GameObject;

            if (lineSet == null)
            {
                return;
            }

            foldoutLineSet =
                EditorGUILayout.Foldout(foldoutLineSet, lineSet.name);
            if (!foldoutLineSet)
            {
                return;
            }


            LineSetNode lineSetNode = lineSet.GetComponent<LineSetNode>();

            if (lineSetNode == null)
            {
                return;
            }



            if (serializedLineSetParams == null)
            {
                return;
            }

            if (serializedLineSetParams.targetObject == null)
            {
                ChangedLineSet();
            }
            serializedLineSetParams.Update();

            ++EditorGUI.indentLevel;

            // ID
            propId.intValue = EditorGUILayout.IntSlider("ID", lineSetNode.Id, 1, 8);

            // LineType (Visible or Hidden)
            var lineType = (LineSetNode.LineType)Enum
                           .GetValues(typeof(LineSetNode.LineType))
                           .GetValue((int)currentLineType);

            // Tabの代わりにボタンを置く
            EditorGUILayout.BeginHorizontal(indent1Style);

            bool btnVisible = (lineType == LineSetNode.LineType.Visible);
            bool btnHidden = !btnVisible;
            bool beforeVisible = btnVisible;
            bool beforeHidden = btnHidden;

            btnVisible = GUILayout.Toggle(btnVisible, "Visible", (GUIStyle)"OL Titleleft");
            btnHidden = GUILayout.Toggle(btnHidden, "Hidden", (GUIStyle)"OL Titleright");

            if (btnVisible != beforeVisible)
            {
                currentLineType = LineSetNode.LineType.Visible;
            }
            if (btnHidden != beforeHidden)
            {
                currentLineType = LineSetNode.LineType.Hidden;
            }

            ChangedLineTypeSettings(lineSetNode, lineType);


            EditorGUILayout.EndHorizontal();

            lineSetNode.Id = propId.intValue;

            MakeBrush(lineSetNode);
            MakeEdge(lineSetNode);
            MakeReduction(lineSetNode);

            --EditorGUI.indentLevel;

            serializedLineSetParams.ApplyModifiedProperties();
        }

        /// <summary>
        /// LineSetのBrush項目のGUIを作成
        /// </summary>
        /// <param name="lineSetNode">選択中のLineSetNode</param>
        void MakeBrush(LineSetNode lineSetNode)
        {

            foldoutBrush =
                EditorGUILayout.Foldout(foldoutBrush, "Brush");
            if (!foldoutBrush)
            {
                return;
            }

            ++EditorGUI.indentLevel;

            EditorGUICustomLayout.PencilNodeField(
                "Brush Settings",
                typeof(BrushSettingsNode),
                serializedLineSetParams,
                currentParams.propBrushSettings,
                (nodeObject) =>
                {
                    ChangedLineSet();
                }
              );

            var bsObj = currentParams.propBrushSettings.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bsObj == null))
            {
                MakeBrushSettings((GameObject)currentParams.propBrushSettings.objectReferenceValue);
            }

            EditorGUILayout.Separator();
        }

        /// <summary>
        /// LineSetのBrushSettingsに関する部分のGUIの作成
        /// </summary>
        /// <param name="lineSetObject">LineSetGameObject</param>
        void MakeBrushSettings(GameObject lineSetObject)
        {
            var brushSettingsNode = lineSetObject.GetComponent<BrushSettingsNode>();

            if(lineSetObject == null
               || brushSettingsNode == null)
            {
                Color dummyColor = new Color();
                //BrushSettingsNode.BlendModeType dummyEnum =
                //    BrushSettingsNode.BlendModeType.Normal;

                EditorGUILayout.ObjectField("Brush Detail", null, typeof(GameObject), true);

                //EditorGUILayout.EnumPopup("Blend Mode", dummyEnum);
                EditorGUILayout.Slider("Blend Amount", 1.0f, 0.0f, 1.0f);
                EditorGUILayout.ColorField("Color", dummyColor);
                EditorGUILayout.ObjectField("ColorMap", null, typeof(Material), false);
                EditorGUILayout.Slider("Map Opacity", 1.0f, 0.0f, 1.0f);
                EditorGUILayout.Slider("Size", 1.0f, 0.1f, 20.0f);
                EditorGUILayout.ObjectField("Size Map", null, typeof(Material), false);
                EditorGUILayout.Slider("Size Map Amount", 1.0f, 0.0f, 1.0f);

                MakeBrushDetail(null);

                --EditorGUI.indentLevel;
                return;
            }

           
          
            if (currentLineType == LineSetNode.LineType.Visible
                && serializedBrushSettingsVisibleParams != null)
            {
                serializedBrushSettingsVisibleParams.Update();
            }
            else if (serializedBrushSettingsHiddenParams != null)
            {
                serializedBrushSettingsHiddenParams.Update();
            }
          

            // Brush Detail
            
            EditorGUICustomLayout.PencilNodeField(
                "Brush Detail",
                typeof(BrushDetailNode),
                currentLineType == LineSetNode.LineType.Visible ? 
                    serializedBrushSettingsVisibleParams : 
                    serializedBrushSettingsHiddenParams,
                currentParams.propBrushDetail,
                (nodeObject) =>
                {
                    // LineNodeEditorが参照しているBrushDetailsNodeの値を再設定
                    if (nodeObject == null)
                    {
                        return;
                    }

                    if (currentLineType == LineSetNode.LineType.Visible)
                    {
                        serializedBrushDetailVisibleParams =
                            new SerializedObject(nodeObject.GetComponent<BrushDetailNode>());

                        currentParams.propStretch = serializedBrushDetailVisibleParams.FindProperty("Stretch");
                        currentParams.propAngle = serializedBrushDetailVisibleParams.FindProperty("Angle");
                    }
                    else
                    {
                        serializedBrushDetailHiddenParams =
                            new SerializedObject(nodeObject.GetComponent<BrushDetailNode>());

                        currentParams.propStretch = serializedBrushDetailHiddenParams.FindProperty("Stretch");
                        currentParams.propAngle = serializedBrushDetailHiddenParams.FindProperty("Angle");

                    }
                });


            //// BlendMode
            //var blendMode = (BrushSettingsNode.BlendModeType)Enum
            //                .GetValues(typeof(BrushSettingsNode.BlendModeType))
            //                .GetValue(currentParams.propBlendMode.enumValueIndex);

            //currentParams.propBlendMode.enumValueIndex =
            //    (int)(LineSetNode.LineType)EditorGUILayout.EnumPopup("Blend Mode", blendMode);

            // BlendAmount
            currentParams.propBlendAmount.floatValue = EditorGUILayout.Slider("Blend Amount",
                                                                currentParams.propBlendAmount.floatValue,
                                                                0.0f, 1.0f);

            // Color
            currentParams.propColor.colorValue =
                EditorGUILayout.ColorField("Color", currentParams.propColor.colorValue);



            // ColorMap
            EditorGUICustomLayout.PencilNodeField(
                "Color Map",
                typeof(TextureMapNode),
                currentLineType == LineSetNode.LineType.Visible ?
                    serializedBrushSettingsVisibleParams :
                    serializedBrushSettingsHiddenParams,
                currentParams.propColorMap,
                nodeObject => { },
                () =>
                {
                    var textureMap = Instantiate(Prefabs.TextureMap);
                    textureMap.name = Common.GetAllGameObject().GetUniqueName(Prefabs.TextureMap);
                    textureMap.transform.parent = brushSettingsNode.transform;
                    currentParams.propColorMap.objectReferenceValue = textureMap;
                    Selection.activeObject = textureMap;
                    Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                });

            // MapOpacity
            currentParams.propMapOpacity.floatValue = EditorGUILayout.Slider(
                "Map Opacity",
                currentParams.propMapOpacity.floatValue,
                0.0f, 1.0f);

            // Size
            currentParams.propSize.floatValue = EditorGUILayout.Slider(
                "Size",
                currentParams.propSize.floatValue,
                0.1f, 20.0f);

            // SizeMap
            EditorGUICustomLayout.PencilNodeField(
                "Size Map",
                typeof(TextureMapNode),
                currentLineType == LineSetNode.LineType.Visible ?
                    serializedBrushSettingsVisibleParams :
                    serializedBrushSettingsHiddenParams,
                currentParams.propSizeMap,
                nodeObject => { },
                () =>
                {
                    var textureMap = Instantiate(Prefabs.TextureMap);
                    textureMap.name = Common.GetAllGameObject().GetUniqueName(Prefabs.TextureMap);
                    textureMap.transform.parent = brushSettingsNode.transform;
                    currentParams.propSizeMap.objectReferenceValue = textureMap;
                    Selection.activeObject = textureMap;
                    Undo.RegisterCreatedObjectUndo(textureMap, "Create Texture Map Node");
                });

            // SizeMapAmount
            currentParams.propSizeMapAmount.floatValue = EditorGUILayout.Slider("Size Map Amount",
                                                                  currentParams.propSizeMapAmount.floatValue,
                                                                  0.0f, 1.0f);

            if (currentLineType == LineSetNode.LineType.Visible)
            {
                serializedBrushSettingsVisibleParams.ApplyModifiedProperties();
            }
            else
            {
                serializedBrushSettingsHiddenParams.ApplyModifiedProperties();
            }

            // BrushDetailParams
            var bdObj = currentParams.propBrushDetail.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bdObj == null))
            {
                MakeBrushDetail((GameObject)currentParams.propBrushDetail.objectReferenceValue);
            }

            --EditorGUI.indentLevel;
        }

        /// <summary>
        /// LineSetのBrushDetailに関する部分のGUIの作成
        /// </summary>
        /// <param name="brushDetailObject">BrushDetailコンポーネントを持っているGameObject</param>
        void MakeBrushDetail(GameObject brushDetailObject)
        {
            Action NoBrushDetails = () =>
            {
                EditorGUILayout.Slider("Stretch", 0, -1.0f, 1.0f);
                EditorGUILayout.Slider("Angle", 0, -360.0f, 360.0f);
            };

            if (brushDetailObject == null)
            {
                NoBrushDetails();
                return;
            }

            BrushDetailNode brushDetailNode = brushDetailObject.GetComponent<BrushDetailNode>();
            if (brushDetailNode == null)
            {
                NoBrushDetails();
                return;
            }

            if (serializedBrushDetailVisibleParams != null)
            {
                serializedBrushDetailVisibleParams.Update();
            }
            if (serializedBrushDetailHiddenParams != null)
            {
                serializedBrushDetailHiddenParams.Update();
            }

            // Stretch
            currentParams.propStretch.floatValue = EditorGUILayout.Slider("Stretch",
                                                            currentParams.propStretch.floatValue,
                                                            -1.0f, 1.0f);

            // Angle
            currentParams.propAngle.floatValue = EditorGUILayout.Slider("Angle",
                                                          currentParams.propAngle.floatValue,
                                                          -360.0f, 360.0f);

            if (serializedBrushDetailVisibleParams != null)
            {
                serializedBrushDetailVisibleParams.ApplyModifiedProperties();
            }
            if (serializedBrushDetailHiddenParams != null)
            {
                serializedBrushDetailHiddenParams.ApplyModifiedProperties();
            }

        }

        /// <summary>
        /// Edgeのパラメータ
        /// </summary>
        struct EdgeParams
        {
            public SerializedProperty propOn;
            public SerializedProperty propSpecificOn;
            public SerializedProperty propBrushSettings;
            public SerializedProperty propNormalAngleMin;
            public SerializedProperty propNormalAngleMax;

            public EdgeParams(SerializedProperty on,
                              SerializedProperty specific,
                              SerializedProperty brushSettings,
                              SerializedProperty min = null,
                              SerializedProperty max = null)
            {
                propOn = on;
                propSpecificOn = specific;
                propBrushSettings = brushSettings;
                propNormalAngleMin = min;
                propNormalAngleMax = max;
            }
        };

        /// <summary>
        /// LineSetのEdge項目のGUIを作成
        /// </summary>
        /// <param name="lineSetNode">選択中のLineSetNode</param>
        void MakeEdge(LineSetNode lineSetNode)
        {
            foldoutEdge =
                EditorGUILayout.Foldout(foldoutEdge, "Edge");
            if (!foldoutEdge)
            {
                return;
            }

            String suffixName =
                currentLineType == LineSetNode.LineType.Visible ?
                " Visible " :
                " Hidden ";


            ++EditorGUI.indentLevel;

            bool beforeSpecificOn = false;
            GameObject beforeObj;

            List<GameObject> allGameObjs = Common.GetAllGameObject();

            Action<String, EdgeParams> EdgeGroup =
                (label, param) =>
                {
                    beforeSpecificOn = param.propSpecificOn.boolValue;
                    beforeObj = param.propBrushSettings.objectReferenceValue as GameObject;

                    MakeEdgeParts(lineSetNode, label,
                                    param.propOn,
                                    param.propSpecificOn,
                                    param.propBrushSettings,
                                    param.propNormalAngleMin,
                                    param.propNormalAngleMax);

                    if (beforeSpecificOn != param.propSpecificOn.boolValue &&
                        beforeSpecificOn == false &&
                        param.propBrushSettings.objectReferenceValue == null)
                    {

                        GameObject newBrushSettings =
                            Common.CreateObject(lineSetNode.gameObject,
                                            allGameObjs,
                                            Prefabs.BrushSettings,
                                            suffixName,
                                            param.propBrushSettings);

                        Undo.RegisterCreatedObjectUndo(param.propBrushSettings.objectReferenceValue,
                                                       "Create Brush Settings");

                        GameObject newBrushDetails =
                            Common.CreateObject(param.propBrushSettings.objectReferenceValue as GameObject,
                                            allGameObjs,
                                            Prefabs.BrushDetail,
                                            suffixName);

                        // BrushSettingsにBrushDetailを接続
                        var newBrushSettingsNode = newBrushSettings.GetComponent<BrushSettingsNode>();
                        newBrushSettingsNode.BrushDetail = newBrushDetails;
                    }

                    if (beforeObj != param.propBrushSettings.objectReferenceValue)
                    {
                        param.propBrushSettings =
                            param.propBrushSettings.UndoObject<BrushSettingsNode>(beforeObj);
                    }
                };



            // Outline
            EdgeGroup("Outline",
                      new EdgeParams(currentParams.propEdgeOutlineOn,
                                      currentParams.propEdgeOutlineS,
                                      currentParams.propEdgeOutline));

            // Object
            EdgeGroup("Object",
                      new EdgeParams(currentParams.propEdgeObjectOn,
                                     currentParams.propEdgeObjectS,
                                     currentParams.propEdgeObject));

            // Intersection
            EdgeGroup("Intersection",
                      new EdgeParams(currentParams.propEdgeIntersectionOn,
                                     currentParams.propEdgeIntersectionS,
                                     currentParams.propEdgeIntersection));

            // Smoothing Boundary
            EdgeGroup("Smoothing Boundary",
                      new EdgeParams(currentParams.propEdgeSmoothOn,
                                     currentParams.propEdgeSmoothS,
                                     currentParams.propEdgeSmooth));

            // Material Boundary
            EdgeGroup("Material Boundary",
                      new EdgeParams(currentParams.propEdgeMaterialOn,
                                     currentParams.propEdgeMaterialS,
                                     currentParams.propEdgeMaterial));


            // Normal Angle
            EdgeGroup("Normal Angle",
                      new EdgeParams(currentParams.propEdgeNormalAngleOn,
                                     currentParams.propEdgeNormalAngleS,
                                     currentParams.propEdgeNormalAngle,
                                     currentParams.propEdgeNormalAngleMin,
                                     currentParams.propEdgeNormalAngleMax));

            // Wireframe
            EdgeGroup("Wireframe",
                      new EdgeParams(currentParams.propEdgeWireframeOn,
                                     currentParams.propEdgeWireframeS,
                                     currentParams.propEdgeWireframe));

            // Welds Edge Between Object
            EditorGUILayout.LabelField("Welds Edges Between Objects");

            ++EditorGUI.indentLevel;

            propWeldsEdges.boolValue =
                EditorGUILayout.Toggle("On", propWeldsEdges.boolValue);

            --EditorGUI.indentLevel;

            // Mask Hidden Lines of Other Line Sets
            EditorGUILayout.LabelField("Mask Hidden Lines of Other Line Sets");

            ++EditorGUI.indentLevel;

            propMaskHiddenLines.boolValue =
                EditorGUILayout.Toggle("On",
                                       propMaskHiddenLines.boolValue);

            --EditorGUI.indentLevel;

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// LineSetのReduction項目のGUIを追加
        /// </summary>
        /// <param name="lineSetNode">選択中のLineSetNode</param>
        void MakeReduction(LineSetNode lineSetNode)
        {
            foldoutReduction =
                EditorGUILayout.Foldout(foldoutReduction, "Reduction");
            if (!foldoutReduction)
            {
                return;
            }

            String suffixName =
                currentLineType == LineSetNode.LineType.Visible ?
                " Visible " :
                " Hidden ";

            List<GameObject> allGameObj = Common.GetAllGameObject();

            ++EditorGUI.indentLevel;

            bool before = false;
            GameObject beforeObj;

            Action<String, SerializedProperty, SerializedProperty> ReductionGroup =
                (label, propOn, propReduction) =>
                {
                    before = propOn.boolValue;
                    beforeObj = propReduction.objectReferenceValue as GameObject;
                    MakeReductionParts(lineSetNode, label,
                                        propOn,
                                        propReduction);

                    if (before != propOn.boolValue &&
                        before == false)
                    {

                        Common.CreateObject(lineSetNode.gameObject,
                                        allGameObj,
                                        Prefabs.ReductionSettings,
                                        suffixName,
                                        propReduction);

                        Undo.RegisterCreatedObjectUndo(propReduction.objectReferenceValue,
                                                        "Create Reduction Settings");

                    }
                    if (beforeObj != propReduction.objectReferenceValue)
                    {
                        propReduction =
                            propReduction.UndoObject<ReductionSettingsNode>(beforeObj);
                    }
                };

            // Size
            ReductionGroup("Size Reduction",
                           currentParams.propSizeReductionOn,
                           currentParams.propSizeReduction);

            // Alpha
            ReductionGroup("Alpha Reduction",
                           currentParams.propAlphaReductionOn,
                           currentParams.propAlphaReduction);

            EditorGUILayout.Separator();

            --EditorGUI.indentLevel;
        }


        /// <summary>
        /// 現在のLineTypeに応じた設定に切替を行うメソッド
        /// </summary>
        /// <param name="lineSetNode">LineSetNode</param>
        /// <param name="lineType">現在のLineType</param>
        void ChangedLineTypeSettings(LineSetNode lineSetNode, LineSetNode.LineType lineType)
        {
            switch (lineType)
            {
                case LineSetNode.LineType.Visible:
                    currentParams.propBrushSettings = visibleParams.propBrushSettings;
                    currentParams.propBrushDetail = visibleParams.propBrushDetail;
                    currentParams.propBlendMode = visibleParams.propBlendMode;
                    currentParams.propBlendAmount = visibleParams.propBlendAmount;
                    currentParams.propColor = visibleParams.propColor;
                    currentParams.propColorMap = visibleParams.propColorMap;
                    currentParams.propMapOpacity = visibleParams.propMapOpacity;
                    currentParams.propSize = visibleParams.propSize;
                    currentParams.propSizeMap = visibleParams.propSizeMap;
                    currentParams.propSizeMapAmount = visibleParams.propSizeMapAmount;
                    currentParams.propStretch = visibleParams.propStretch;
                    currentParams.propAngle = visibleParams.propAngle;
                    currentParams.propEdgeOutlineOn = visibleParams.propEdgeOutlineOn;
                    currentParams.propEdgeOutlineS = visibleParams.propEdgeOutlineS;
                    currentParams.propEdgeOutline = visibleParams.propEdgeOutline;
                    currentParams.propEdgeObjectOn = visibleParams.propEdgeObjectOn;
                    currentParams.propEdgeObjectS = visibleParams.propEdgeObjectS;
                    currentParams.propEdgeObject = visibleParams.propEdgeObject;
                    currentParams.propEdgeIntersectionOn = visibleParams.propEdgeIntersectionOn;
                    currentParams.propEdgeIntersectionS = visibleParams.propEdgeIntersectionS;
                    currentParams.propEdgeIntersection = visibleParams.propEdgeIntersection;
                    currentParams.propEdgeSmoothOn = visibleParams.propEdgeSmoothOn;
                    currentParams.propEdgeSmoothS = visibleParams.propEdgeSmoothS;
                    currentParams.propEdgeSmooth = visibleParams.propEdgeSmooth;
                    currentParams.propEdgeMaterialOn = visibleParams.propEdgeMaterialOn;
                    currentParams.propEdgeMaterialS = visibleParams.propEdgeMaterialS;
                    currentParams.propEdgeMaterial = visibleParams.propEdgeMaterial;
                    currentParams.propEdgeNormalAngleOn = visibleParams.propEdgeNormalAngleOn;
                    currentParams.propEdgeNormalAngleS = visibleParams.propEdgeNormalAngleS;
                    currentParams.propEdgeNormalAngle = visibleParams.propEdgeNormalAngle;
                    currentParams.propEdgeNormalAngleMin = visibleParams.propEdgeNormalAngleMin;
                    currentParams.propEdgeNormalAngleMax = visibleParams.propEdgeNormalAngleMax;
                    currentParams.propEdgeWireframeOn = visibleParams.propEdgeWireframeOn;
                    currentParams.propEdgeWireframeS = visibleParams.propEdgeWireframeS;
                    currentParams.propEdgeWireframe = visibleParams.propEdgeWireframe;
                    currentParams.propSizeReductionOn = visibleParams.propSizeReductionOn;
                    currentParams.propSizeReduction = visibleParams.propSizeReduction;
                    currentParams.propAlphaReductionOn = visibleParams.propAlphaReductionOn;
                    currentParams.propAlphaReduction = visibleParams.propAlphaReduction;
                    break;

                case LineSetNode.LineType.Hidden:
                    currentParams.propBrushSettings = hiddenParams.propBrushSettings;
                    currentParams.propBrushDetail = hiddenParams.propBrushDetail;
                    currentParams.propBlendMode = hiddenParams.propBlendMode;
                    currentParams.propBlendAmount = hiddenParams.propBlendAmount;
                    currentParams.propColor = hiddenParams.propColor;
                    currentParams.propColorMap = hiddenParams.propColorMap;
                    currentParams.propMapOpacity = hiddenParams.propMapOpacity;
                    currentParams.propSize = hiddenParams.propSize;
                    currentParams.propSizeMap = hiddenParams.propSizeMap;
                    currentParams.propSizeMapAmount = hiddenParams.propSizeMapAmount;
                    currentParams.propStretch = hiddenParams.propStretch;
                    currentParams.propAngle = hiddenParams.propAngle;
                    currentParams.propEdgeOutlineOn = hiddenParams.propEdgeOutlineOn;
                    currentParams.propEdgeOutlineS = hiddenParams.propEdgeOutlineS;
                    currentParams.propEdgeOutline = hiddenParams.propEdgeOutline;
                    currentParams.propEdgeObjectOn = hiddenParams.propEdgeObjectOn;
                    currentParams.propEdgeObjectS = hiddenParams.propEdgeObjectS;
                    currentParams.propEdgeObject = hiddenParams.propEdgeObject;
                    currentParams.propEdgeIntersectionOn = hiddenParams.propEdgeIntersectionOn;
                    currentParams.propEdgeIntersectionS = hiddenParams.propEdgeIntersectionS;
                    currentParams.propEdgeIntersection = hiddenParams.propEdgeIntersection;
                    currentParams.propEdgeSmoothOn = hiddenParams.propEdgeSmoothOn;
                    currentParams.propEdgeSmoothS = hiddenParams.propEdgeSmoothS;
                    currentParams.propEdgeSmooth = hiddenParams.propEdgeSmooth;
                    currentParams.propEdgeMaterialOn = hiddenParams.propEdgeMaterialOn;
                    currentParams.propEdgeMaterialS = hiddenParams.propEdgeMaterialS;
                    currentParams.propEdgeMaterial = hiddenParams.propEdgeMaterial;
                    currentParams.propEdgeNormalAngleOn = hiddenParams.propEdgeNormalAngleOn;
                    currentParams.propEdgeNormalAngleS = hiddenParams.propEdgeNormalAngleS;
                    currentParams.propEdgeNormalAngle = hiddenParams.propEdgeNormalAngle;
                    currentParams.propEdgeNormalAngleMin = hiddenParams.propEdgeNormalAngleMin;
                    currentParams.propEdgeNormalAngleMax = hiddenParams.propEdgeNormalAngleMax;
                    currentParams.propEdgeWireframeOn = hiddenParams.propEdgeWireframeOn;
                    currentParams.propEdgeWireframeS = hiddenParams.propEdgeWireframeS;
                    currentParams.propEdgeWireframe = hiddenParams.propEdgeWireframe;
                    currentParams.propSizeReductionOn = hiddenParams.propSizeReductionOn;
                    currentParams.propSizeReduction = hiddenParams.propSizeReduction;
                    currentParams.propAlphaReductionOn = hiddenParams.propAlphaReductionOn;
                    currentParams.propAlphaReduction = hiddenParams.propAlphaReduction;
                    break;

            }

        }

        /// <summary>
        /// EdgeのGUIの作成
        /// </summary>
        /// <param name="lineSetNode">現在のLineSetNode</param>
        /// <param name="label">ラベル</param>
        /// <param name="propEdgeOn">On/Offの切り替えに使用するSerializeProperty型の変数</param>
        /// <param name="propEdgeSpecificOn">SpecificのOn/Offの切り替えに使用するSerializeProperty型の変数</param>
        /// <param name="propBrushSettings">BrushSettingsに使用するSerializeProperty型の変数</param>
        /// <param name="normalAngleMin">NormalAngleMinパラメータ用のSerializeProperty型の変数</param>
        /// <param name="normalAngleMax">NormalAngleMaxパラメータ用のSerializeProperty型の変数</param>
        void MakeEdgeParts(LineSetNode lineSetNode,
                        String label,
                        SerializedProperty propEdgeOn,
                        SerializedProperty propEdgeSpecificOn,
                        SerializedProperty propBrushSettings,
                        SerializedProperty normalAngleMin = null,
                        SerializedProperty normalAngleMax = null)
        {
            // Label
            EditorGUILayout.LabelField(label);

            ++EditorGUI.indentLevel;

            // On
            propEdgeOn.boolValue = EditorGUILayout.Toggle("On", propEdgeOn.boolValue);


            using (new EditorGUI.DisabledGroupScope(!propEdgeOn.boolValue))
            {
                // Specific On
                propEdgeSpecificOn.boolValue = EditorGUILayout.Toggle("Specific On", propEdgeSpecificOn.boolValue);


                using (new EditorGUI.DisabledGroupScope(!propEdgeSpecificOn.boolValue))
                {
                    // BrushSettings
                    EditorGUICustomLayout.PencilNodeField(
                        "",
                        typeof(BrushSettingsNode),
                        serializedLineSetParams,
                        propBrushSettings,
                        (nodeObject) => { });
                }

                // Normal Angle Params
                if (normalAngleMin != null && normalAngleMax != null)
                {
                    normalAngleMin.floatValue = EditorGUILayout.Slider("Min",
                                                              normalAngleMin.floatValue,
                                                              0, 180);

                    normalAngleMax.floatValue = EditorGUILayout.Slider("Max",
                                                              normalAngleMax.floatValue,
                                                              0, 180);
                }
            }


            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// ReductionのGUIの作成
        /// </summary>
        /// <param name="lineSetNode">現在のLineSetNode</param>
        /// <param name="label">ラベル</param>
        /// <param name="propReductionOn">On/Offの切り替えに使用するSerializeProperty型の変数</param>
        /// <param name="propReductionSettings">ReductionSettingsに使用するSerializeProperty型の変数</param>
        void MakeReductionParts(LineSetNode lineSetNode,
                                String label,
                                SerializedProperty propReductionOn,
                                SerializedProperty propReductionSettings)
        {
            EditorGUILayout.LabelField(label);

            ++EditorGUI.indentLevel;

            // On
            propReductionOn.boolValue = EditorGUILayout.Toggle("On", propReductionOn.boolValue);

            using (new EditorGUI.DisabledGroupScope(!propReductionOn.boolValue))
            {
                // ReductionSettings
                EditorGUICustomLayout.PencilNodeField(
                    "",
                    typeof(ReductionSettingsNode),
                    serializedLineSetParams,
                    propReductionSettings,
                    selectedObject => {});

            }

            --EditorGUI.indentLevel;

        }



        /// <summary>
        /// LineSetの選択が変更されたら行う処理
        /// </summary>
        void ChangedLineSet()
        {
            // Set Line Set Properties
            if (reorderableSetList.index < 0 ||
               reorderableSetList.index >= propLineSets.arraySize)
            {
                return;
            }

            SerializedProperty currentLineSet = propLineSets.GetArrayElementAtIndex(reorderableSetList.index);
            if (currentLineSet == null)
            {
                return;
            }

            GameObject lineSet = currentLineSet.objectReferenceValue as GameObject;

            serializedLineSetParams =
                lineSet ?
                new SerializedObject(lineSet.GetComponent<LineSetNode>()) :
                null;

            if (serializedLineSetParams != null)
            {
                propId = serializedLineSetParams.FindProperty("Id");
                propObjects = serializedLineSetParams.FindProperty("Objects");
                propMaterials = serializedLineSetParams.FindProperty("Materials");

                visibleParams.propBrushSettings =
                    serializedLineSetParams.FindProperty("VBrushSettings");
                visibleParams.propEdgeOutlineOn =
                    serializedLineSetParams.FindProperty("VOutlineOn");
                visibleParams.propEdgeOutlineS =
                    serializedLineSetParams.FindProperty("VOutlineSpecificOn");
                visibleParams.propEdgeOutline =
                    serializedLineSetParams.FindProperty("VOutline");
                visibleParams.propEdgeObjectOn =
                    serializedLineSetParams.FindProperty("VObjectOn");
                visibleParams.propEdgeObjectS =
                    serializedLineSetParams.FindProperty("VObjectSpecificOn");
                visibleParams.propEdgeObject =
                    serializedLineSetParams.FindProperty("VObject");
                visibleParams.propEdgeIntersectionOn =
                    serializedLineSetParams.FindProperty("VIntersectionOn");
                visibleParams.propEdgeIntersectionS =
                    serializedLineSetParams.FindProperty("VIntersectionSpecificOn");
                visibleParams.propEdgeIntersection =
                    serializedLineSetParams.FindProperty("VIntersection");
                visibleParams.propEdgeSmoothOn =
                    serializedLineSetParams.FindProperty("VSmoothOn");
                visibleParams.propEdgeSmoothS =
                    serializedLineSetParams.FindProperty("VSmoothSpecificOn");
                visibleParams.propEdgeSmooth =
                    serializedLineSetParams.FindProperty("VSmooth");
                visibleParams.propEdgeMaterialOn =
                    serializedLineSetParams.FindProperty("VMaterialOn");
                visibleParams.propEdgeMaterialS =
                    serializedLineSetParams.FindProperty("VMaterialSpecificOn");
                visibleParams.propEdgeMaterial =
                    serializedLineSetParams.FindProperty("VMaterial");
                visibleParams.propEdgeNormalAngleOn =
                    serializedLineSetParams.FindProperty("VNormalAngleOn");
                visibleParams.propEdgeNormalAngleS =
                    serializedLineSetParams.FindProperty("VNormalAngleSpecificOn");
                visibleParams.propEdgeNormalAngle =
                    serializedLineSetParams.FindProperty("VNormalAngle");
                visibleParams.propEdgeNormalAngleMin =
                    serializedLineSetParams.FindProperty("VNormalAngleMin");
                visibleParams.propEdgeNormalAngleMax =
                    serializedLineSetParams.FindProperty("VNormalAngleMax");
                visibleParams.propEdgeWireframeOn =
                    serializedLineSetParams.FindProperty("VWireframeOn");
                visibleParams.propEdgeWireframeS =
                    serializedLineSetParams.FindProperty("VWireframeSpecificOn");
                visibleParams.propEdgeWireframe =
                    serializedLineSetParams.FindProperty("VWireframe");
                visibleParams.propSizeReductionOn =
                    serializedLineSetParams.FindProperty("VSizeReductionOn");
                visibleParams.propSizeReduction =
                    serializedLineSetParams.FindProperty("VSizeReduction");
                visibleParams.propAlphaReductionOn =
                    serializedLineSetParams.FindProperty("VAlphaReductionOn");
                visibleParams.propAlphaReduction =
                    serializedLineSetParams.FindProperty("VAlphaReduction");

                hiddenParams.propBrushSettings =
                    serializedLineSetParams.FindProperty("HBrushSettings");
                hiddenParams.propEdgeOutlineOn =
                    serializedLineSetParams.FindProperty("HOutlineOn");
                hiddenParams.propEdgeOutlineS =
                    serializedLineSetParams.FindProperty("HOutlineSpecificOn");
                hiddenParams.propEdgeOutline =
                    serializedLineSetParams.FindProperty("HOutline");
                hiddenParams.propEdgeObjectOn =
                    serializedLineSetParams.FindProperty("HObjectOn");
                hiddenParams.propEdgeObjectS =
                    serializedLineSetParams.FindProperty("HObjectSpecificOn");
                hiddenParams.propEdgeObject =
                    serializedLineSetParams.FindProperty("HObject");
                hiddenParams.propEdgeIntersectionOn =
                    serializedLineSetParams.FindProperty("HIntersectionOn");
                hiddenParams.propEdgeIntersectionS =
                    serializedLineSetParams.FindProperty("HIntersectionSpecificOn");
                hiddenParams.propEdgeIntersection =
                    serializedLineSetParams.FindProperty("HIntersection");
                hiddenParams.propEdgeSmoothOn =
                    serializedLineSetParams.FindProperty("HSmoothOn");
                hiddenParams.propEdgeSmoothS =
                    serializedLineSetParams.FindProperty("HSmoothSpecificOn");
                hiddenParams.propEdgeSmooth =
                    serializedLineSetParams.FindProperty("HSmooth");
                hiddenParams.propEdgeMaterialOn =
                    serializedLineSetParams.FindProperty("HMaterialOn");
                hiddenParams.propEdgeMaterialS =
                    serializedLineSetParams.FindProperty("HMaterialSpecificOn");
                hiddenParams.propEdgeMaterial =
                    serializedLineSetParams.FindProperty("HMaterial");
                hiddenParams.propEdgeNormalAngleOn =
                    serializedLineSetParams.FindProperty("HNormalAngleOn");
                hiddenParams.propEdgeNormalAngleS =
                    serializedLineSetParams.FindProperty("HNormalAngleSpecificOn");
                hiddenParams.propEdgeNormalAngle =
                    serializedLineSetParams.FindProperty("HNormalAngle");
                hiddenParams.propEdgeNormalAngleMin =
                    serializedLineSetParams.FindProperty("HNormalAngleMin");
                hiddenParams.propEdgeNormalAngleMax =
                    serializedLineSetParams.FindProperty("HNormalAngleMax");
                hiddenParams.propEdgeWireframeOn =
                    serializedLineSetParams.FindProperty("HWireframeOn");
                hiddenParams.propEdgeWireframeS =
                    serializedLineSetParams.FindProperty("HWireframeSpecificOn");
                hiddenParams.propEdgeWireframe =
                    serializedLineSetParams.FindProperty("HWireframe");
                hiddenParams.propSizeReductionOn =
                    serializedLineSetParams.FindProperty("HSizeReductionOn");
                hiddenParams.propSizeReduction =
                    serializedLineSetParams.FindProperty("HSizeReduction");
                hiddenParams.propAlphaReductionOn =
                    serializedLineSetParams.FindProperty("HAlphaReductionOn");
                hiddenParams.propAlphaReduction =
                    serializedLineSetParams.FindProperty("HAlphaReduction");

                propWeldsEdges = serializedLineSetParams.FindProperty("WeldsEdges");
                propMaskHiddenLines = serializedLineSetParams.FindProperty("MaskHiddenLines");


                
                // ---------- Object List ----------
                reorderableObjects = Common.CreateObjectList(
                    serializedLineSetParams,
                    propObjects,
                    (target as LineNode).LineSets
                        .Select(x => x.GetComponent<LineSetNode>())
                        .Where(x => x != null)
                        .SelectMany(x => x.Objects),
                    selectedObjects => 
                    {
                        serializedLineSetParams.Update();
                        propObjects.AppendObjects(selectedObjects);
                        serializedLineSetParams.ApplyModifiedProperties();
                        reorderableObjects.index = reorderableObjects.count - 1;
                    });


                // ---------- Material List ----------
                reorderableMaterials = Common.CreateMaterialList(
                    serializedLineSetParams,
                    propMaterials,
                    (target as LineNode).LineSets
                        .Select(x => x.GetComponent<LineSetNode>())
                        .Where(x => x != null)
                        .SelectMany(x => x.Materials),
                    selectedMaterials => 
                    {
                        serializedLineSetParams.Update();
                        propMaterials.AppendObjects(selectedMaterials);
                        serializedLineSetParams.ApplyModifiedProperties();
                        reorderableMaterials.index = reorderableMaterials.count - 1;
                    });

            }

            // Set Brush Settings Properties
            GameObject brushSettings;

            brushSettings = visibleParams.propBrushSettings != null ?
                visibleParams.propBrushSettings.objectReferenceValue as GameObject :
                null;

            serializedBrushSettingsVisibleParams =
                brushSettings != null ?
                new SerializedObject(brushSettings.GetComponent<BrushSettingsNode>()) :
                null;

            if (serializedBrushSettingsVisibleParams != null)
            {
                visibleParams.propBrushDetail =
                    serializedBrushSettingsVisibleParams.FindProperty("BrushDetail");
                visibleParams.propBlendMode =
                    serializedBrushSettingsVisibleParams.FindProperty("BlendMode");
                visibleParams.propBlendAmount =
                    serializedBrushSettingsVisibleParams.FindProperty("BlendAmount");
                visibleParams.propColor =
                    serializedBrushSettingsVisibleParams.FindProperty("BrushColor");
                visibleParams.propColorMap =
                    serializedBrushSettingsVisibleParams.FindProperty("ColorMap");
                visibleParams.propMapOpacity =
                    serializedBrushSettingsVisibleParams.FindProperty("MapOpacity");
                visibleParams.propSize =
                    serializedBrushSettingsVisibleParams.FindProperty("Size");
                visibleParams.propSizeMap =
                    serializedBrushSettingsVisibleParams.FindProperty("SizeMap");
                visibleParams.propSizeMapAmount =
                    serializedBrushSettingsVisibleParams.FindProperty("SizeMapAmount");
            }


            brushSettings = hiddenParams.propBrushSettings != null ?
                            hiddenParams.propBrushSettings.objectReferenceValue as GameObject :
                            null;

            serializedBrushSettingsHiddenParams =
                brushSettings != null ?
                new SerializedObject(brushSettings.GetComponent<BrushSettingsNode>()) :
                null;

            if (serializedBrushSettingsHiddenParams != null)
            {
                hiddenParams.propBrushDetail =
                    serializedBrushSettingsHiddenParams.FindProperty("BrushDetail");
                hiddenParams.propBlendMode =
                    serializedBrushSettingsHiddenParams.FindProperty("BlendMode");
                hiddenParams.propBlendAmount =
                    serializedBrushSettingsHiddenParams.FindProperty("BlendAmount");
                hiddenParams.propColor =
                    serializedBrushSettingsHiddenParams.FindProperty("BrushColor");
                hiddenParams.propColorMap =
                    serializedBrushSettingsHiddenParams.FindProperty("ColorMap");
                hiddenParams.propMapOpacity =
                    serializedBrushSettingsHiddenParams.FindProperty("MapOpacity");
                hiddenParams.propSize =
                    serializedBrushSettingsHiddenParams.FindProperty("Size");
                hiddenParams.propSizeMap =
                    serializedBrushSettingsHiddenParams.FindProperty("SizeMap");
                hiddenParams.propSizeMapAmount =
                    serializedBrushSettingsHiddenParams.FindProperty("SizeMapAmount");
            }


            // Set Brush Detail Properties
            GameObject brushDetail;

            brushDetail = visibleParams.propBrushDetail != null ?
                          visibleParams.propBrushDetail.objectReferenceValue as GameObject :
                          null;

            serializedBrushDetailVisibleParams =
                brushDetail != null ?
                new SerializedObject(brushDetail.GetComponent<BrushDetailNode>()) :
                null;

            if (serializedBrushDetailVisibleParams != null)
            {
                visibleParams.propStretch = serializedBrushDetailVisibleParams.FindProperty("Stretch");
                visibleParams.propAngle = serializedBrushDetailVisibleParams.FindProperty("Angle");
            }

            brushDetail = hiddenParams.propBrushDetail != null ?
                          hiddenParams.propBrushDetail.objectReferenceValue as GameObject :
                          null;

            serializedBrushDetailHiddenParams =
                brushDetail != null ?
                new SerializedObject(brushDetail.GetComponent<BrushDetailNode>()) :
                null;

            if (serializedBrushDetailHiddenParams != null)
            {
                hiddenParams.propStretch = serializedBrushDetailHiddenParams.FindProperty("Stretch");
                hiddenParams.propAngle = serializedBrushDetailHiddenParams.FindProperty("Angle");
            }
        }

        void OnDisable()
        {
            var lineNode = target as LineNode;
            lineNode.SelectedLineSet = SelectedLineSet;
        }


        void OnEnable()
        {
            var lineNode = target as LineNode;

            SelectedLineSet = lineNode.SelectedLineSet;

            // Styleの作成
            if (indent1Style == null)
            {
                indent1Style = new GUIStyle();
                indent1Style.border = new RectOffset(1, 1, 1, 1);
                indent1Style.padding = new RectOffset(0, 0, 0, 0);
                indent1Style.margin = new RectOffset(30, 0, 0, 0);
            }

            if (listBoxStyle == null)
            {
                listBoxStyle = new GUIStyle("box");
                listBoxStyle.border = new RectOffset(1, 1, 1, 1);
                listBoxStyle.padding = new RectOffset(1, 1, 1, 1);
                listBoxStyle.margin = new RectOffset(30, 0, 0, 0);
                listBoxStyle.stretchWidth = true;
                listBoxStyle.stretchHeight = false;
                listBoxStyle.fixedHeight = 128;

            }

            if (inListBoxStyle == null)
            {
                inListBoxStyle = new GUIStyle();
                inListBoxStyle.border = new RectOffset(0, 0, 0, 0);
                inListBoxStyle.padding = new RectOffset(0, 0, 0, 0);
                inListBoxStyle.margin = new RectOffset(0, 0, 0, 0);
            }

            // SerializePropertyをセット
            propLineSets = serializedObject.FindProperty("LineSets");
            propLineSize = serializedObject.FindProperty("LineSizeType");
            propOverSampling = serializedObject.FindProperty("OverSampling");
            propAntialiasing = serializedObject.FindProperty("Antialiasing");
            propOffScreenDistance = serializedObject.FindProperty("OffscreenDistance");
            propRandomSeed = serializedObject.FindProperty("RandomSeed");


            // Line Set List
            reorderableSetList =
                Common.CreateReorderableNodeList(serializedObject,
                                                 propLineSets,
                                                 lineNode,
                                                 Prefabs.LineSet);

            // Elementを追加するコールバック
            reorderableSetList.onAddCallback = (list) =>
            {
                // LineSetの追加
                GameObject newLineSet = Instantiate(Prefabs.LineSet);
                Undo.RegisterCreatedObjectUndo(newLineSet, "Create Line Set Node");

                List<GameObject> allGameObjs = Common.GetAllGameObject();
                newLineSet.name = allGameObjs.GetUniqueName(Prefabs.LineSet);

                list.index = propLineSets.arraySize;

                bool isZeroArraySize = list.index <= 0;

                if (isZeroArraySize)
                {
                    propLineSets.arraySize = 1;
                }

                propLineSets.InsertArrayElementAtIndex(list.index);

                if (isZeroArraySize)
                {
                    propLineSets.arraySize = 1;
                }

                SerializedProperty propLineSet =
                    propLineSets.GetArrayElementAtIndex(reorderableSetList.index);
                propLineSet.objectReferenceValue = newLineSet;
                newLineSet.transform.parent = lineNode.gameObject.transform;


                // BrushSettingsの追加
                LineSetNode lineSetNode = newLineSet.GetComponent<LineSetNode>();

                allGameObjs = Common.GetAllGameObject();
                GameObject newVBrushSettings =
                    Common.CreateObject(newLineSet, allGameObjs, Prefabs.BrushSettings, " Visible ");
                GameObject newHBrushSettings =
                    Common.CreateObject(newLineSet, allGameObjs, Prefabs.BrushSettings, " Hidden ");

                lineSetNode.VBrushSettings = newVBrushSettings;
                lineSetNode.HBrushSettings = newHBrushSettings;


                // BrushDetailの追加
                BrushSettingsNode brushSettingsNodev = newVBrushSettings.GetComponent<BrushSettingsNode>();
                BrushSettingsNode brushSettingsNodeh = newHBrushSettings.GetComponent<BrushSettingsNode>();

                GameObject newVBrushDetail =
                    Common.CreateObject(newVBrushSettings, allGameObjs, Prefabs.BrushDetail, " Visible ");
                GameObject newHBrushDetail =
                    Common.CreateObject(newHBrushSettings, allGameObjs, Prefabs.BrushDetail, " Hidden ");

                brushSettingsNodev.BrushDetail = newVBrushDetail;
                brushSettingsNodeh.BrushDetail = newHBrushDetail;

                SelectedLineSet = propLineSet.objectReferenceValue as GameObject;
                ChangedLineSet();
            };

            Action<ReorderableList> ChangedSelectedLineSet = (list) =>
            {
                if (propLineSets.arraySize > 0)
                {
                    SerializedProperty propLineSet = propLineSets.GetArrayElementAtIndex(list.index);
                    SelectedLineSet = propLineSet.objectReferenceValue as GameObject;
                }
            };

            // Elementを削除するコールバック
            reorderableSetList.onRemoveCallback += (list) =>
            {
                ChangedSelectedLineSet(list);
                ChangedLineSet();
            };

            // Elementの入れ替えを行った際に呼ばれるコールバック
            reorderableSetList.onReorderCallback += (list) =>
            {
                ChangedSelectedLineSet(list);
                ChangedLineSet();
            };

            // 選択状態変更
            reorderableSetList.onSelectCallback += (list) =>
            {
                ChangedSelectedLineSet(list);
                ChangedLineSet();
            };


            if (propLineSets.arraySize == 0)
            {
                reorderableSetList.index = 0;
            }

            if (reorderableSetList.index >= propLineSets.arraySize)
            {
                reorderableSetList.index = propLineSets.arraySize - 1;
            }

#if SORT_LIST
            // Sortコールバックが登録されていない場合は登録
            if (EditorApplication.hierarchyWindowChanged == null)
            {
                EditorApplication.hierarchyWindowChanged += SortLineSetNodeCallback;
                return;
            }
            Delegate[] del = EditorApplication.hierarchyWindowChanged.GetInvocationList();
            if (!del.Any(x => x.Method.Name == "SortLineSetNodeCallback"))
            {
                EditorApplication.hierarchyWindowChanged += SortLineSetNodeCallback;
            }
#endif

            // 前回のLineSetの選択を読み込み
            int count = propLineSets.arraySize;
            bool isFound = false;
            for (int i = 0; i < count; ++i)
            {
                if (propLineSets.GetArrayElementAtIndex(i).objectReferenceValue == SelectedLineSet)
                {
                    isFound = true;
                    reorderableSetList.index = i;
                    break;
                }
            }
            if(count > 0 && !isFound)
            {
                reorderableSetList.index = 0;
            }

            // ObjectとMaterialで使用するControlIDを取得
            objectPickerID = EditorGUIUtility.GetControlID(FocusType.Passive);
            materialPickerID = EditorGUIUtility.GetControlID(FocusType.Passive);

            ChangedLineSet();
        }


        public override void OnInspectorGUI()
        {
            var lineNode = target as LineNode;
            serializedObject.Update();

            // Basic Parameters
            MakeBasicParameters(lineNode);

            // Line Size
            MakeLineSize(lineNode);

            // Others
            MakeOthers(lineNode);

            // Line Set
            MakeLineSet(lineNode);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにLineノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Line Node", false)]
        public static void OpenLineNode()
        {
            List<GameObject> allGameObjs = Common.GetAllGameObject();
            GameObject newLine = Instantiate(Prefabs.Line);
            Undo.RegisterCreatedObjectUndo(newLine, "Create Line Node");
            newLine.name = allGameObjs.GetUniqueName(Prefabs.Line);

            // 選択しているものにLineListNodeがあれば優先的に子に設定する
            List<GameObject> selectList = Selection.gameObjects.ToList();
            List<GameObject> lineList = selectList.SearchComponent<LineListNode>();
            if (lineList.Count > 0)
            {
                newLine.transform.parent = lineList[0].transform;
                return;
            }

            // LineListがHierarchy内にあれば子に設定する
            lineList = allGameObjs.SearchComponent<LineListNode>();
            if (lineList.Count > 0)
            {
                newLine.transform.parent = lineList[0].transform;
            }

#if SORT_LIST
            // Sortコールバックが登録されていない場合は登録
            if (EditorApplication.hierarchyWindowChanged == null)
            {
                EditorApplication.hierarchyWindowChanged += SortLineSetNodeCallback;
                return;
            }
            Delegate[] del = EditorApplication.hierarchyWindowChanged.GetInvocationList();
            if (!del.Any(x => x.Method.Name == "SortLineSetNodeCallback"))
            {
                EditorApplication.hierarchyWindowChanged += SortLineSetNodeCallback;
            }
#endif
        }

    }
}

