#pragma warning disable 0414
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pencil_4;

namespace Pcl4Editor
{

    using Common = EditorCommons;
    using LineType = LineSetNode.LineType;

    [CustomEditor(typeof(LineSetNode))]
    public class LineSetNodeEditor : Editor
    {
        public bool foldoutBrush = true;
        public bool foldoutEdge = true;
        public bool foldoutReduction = true;

        // Maya版のタブの代わりにセット
        [SerializeField]
        public LineType currentLineType =
            LineType.Visible;

        private SerializedProperty propId;

        private SerializedProperty propObjects;
        private SerializedProperty propMaterials;

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

        private SerializedObject serializedBrushSettingsVisibleParams;
        private SerializedObject serializedBrushSettingsHiddenParams;
        private SerializedObject serializedBrushDetailVisibleParams;
        private SerializedObject serializedBrushDetailHiddenParams;

        private ReorderableList reorderableObjectsList;
        private ReorderableList reorderableMaterialsList;

        /// <summary>
        /// 現在のLineTypeに応じた設定に再設定を行うメソッド
        /// </summary>
        /// <param name="lineSetNode">LineSetNode</param>
        /// <param name="lineType">現在のLineType</param>
        void ChangedLineTypeSettings(LineType lineType)
        {
            switch (lineType)
            {
                case LineType.Visible:
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

                case LineType.Hidden:
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
                        serializedObject,
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
                        serializedObject,
                        propReductionSettings,
                        (nodeObject) => { });
            }

            --EditorGUI.indentLevel;

        }

        /// <summary>
        /// ObjectsListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        void MakeObjectsList(GUIStyle style)
        {
            var lineSetNode = target as LineSetNode;

            EditorGUILayout.LabelField("Objects");

            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<GameObject, LineSetNode>(lineSetNode,
                                                               null,
                                                               propObjects,
                                                               verticalLayout.rect,
                                                               GetObjectsList<GameObject>,
                                                               false);

            using (verticalLayout)
            {
                reorderableObjectsList.DoLayoutList();
            }
        }

        /// <summary>
        /// delegate用のObjectsリスト取得関数
        /// </summary>
        /// <typeparam name="T">取得するリストの型</typeparam>
        /// <param name="node">リストの存在するノード</param>
        /// <param name="varName">名前</param>
        /// <returns></returns>
        List<T> GetObjectsList<T>(NodeBase node, String varName)
             where T : UnityEngine.Object
        {
            return (node as LineSetNode).Objects
                                         .Select(x => x as T)
                                         .ToList();
        }

        /// <summary>
        /// MaterialsListを作成する
        /// </summary>
        /// <param name="style">リストのスタイル</param>
        void MakeMaterialsList(GUIStyle style)
        {
            var lineSetNode = target as LineSetNode;

            EditorGUILayout.LabelField("Materials");


            EditorGUILayout.VerticalScope verticalLayout =
                new EditorGUILayout.VerticalScope(style);

            Common.DragAndDropObject<Material, LineSetNode>(lineSetNode,
                                                             null,
                                                             propMaterials,
                                                             verticalLayout.rect,
                                                             GetMateiralsList<Material>,
                                                             false);

            using (verticalLayout)
            {
                reorderableMaterialsList.DoLayoutList();
            }
        }

        /// <summary>
        /// delegate用のMaterialsリスト取得関数
        /// </summary>
        /// <typeparam name="T">取得するリストの型</typeparam>
        /// <param name="node">リストの存在するノード</param>
        /// <param name="varName">名前</param>
        /// <returns></returns>
        List<T> GetMateiralsList<T>(NodeBase node, String varName)
             where T : UnityEngine.Object
        {
            return (node as LineSetNode).Materials
                                           .Select(x => x as T)
                                           .ToList();
        }

        /// <summary>
        /// LineSetのGUIを作成
        /// </summary>
        void MakeLineSet()
        {
            // Lists
            GUIStyle style = new GUIStyle();
            style.margin = new RectOffset(4, 8, 0, 4);

            MakeObjectsList(style);
            MakeMaterialsList(style);

            // ID
            propId.intValue = EditorGUILayout.IntSlider("ID", propId.intValue, 1, 8);

            // LineType (Visible or Hidden)
            var lineType = (LineType)Enum
                           .GetValues(typeof(LineType))
                           .GetValue((int)currentLineType);

            // Tabの代わりにボタンを置く
            EditorGUILayout.BeginHorizontal();
            {
                bool btnVisible = (lineType == LineType.Visible);
                bool btnHidden = !btnVisible;
                bool beforeVisible = btnVisible;
                bool beforeHidden = btnHidden;

                btnVisible = GUILayout.Toggle(btnVisible, "Visible", (GUIStyle)"OL Titleleft");
                btnHidden = GUILayout.Toggle(btnHidden, "Hidden", (GUIStyle)"OL Titleright");

                if (btnVisible != beforeVisible)
                {
                    currentLineType = LineType.Visible;
                }
                if (btnHidden != beforeHidden)
                {
                    currentLineType = LineType.Hidden;
                }
            }
            EditorGUILayout.EndHorizontal();

            ChangedLineTypeSettings(lineType);
        }

        /// <summary>
        /// Brush項目のGUIを作成
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
                serializedObject,
                currentParams.propBrushSettings,
                (nodeObject) => 
                {
                    ChangedBrushSettings();
                });


            var bsObj = currentParams.propBrushSettings.objectReferenceValue;
            using (new EditorGUI.DisabledGroupScope(bsObj == null))
            {
                MakeBrushSettings((GameObject)currentParams.propBrushSettings.objectReferenceValue);
            }

            EditorGUILayout.Separator();
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
        /// Edge項目のGUIを作成
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

            bool before = false;
            GameObject beforeObj;

            List<GameObject> allGameObjs = Common.GetAllGameObject();

            Action<String, EdgeParams> EdgeGroup =
                (label, param) =>
            {
                before = param.propSpecificOn.boolValue;
                beforeObj = param.propBrushSettings.objectReferenceValue as GameObject;

                MakeEdgeParts(lineSetNode, label,
                                param.propOn,
                                param.propSpecificOn,
                                param.propBrushSettings,
                                param.propNormalAngleMin,
                                param.propNormalAngleMax);

                if (before != param.propSpecificOn.boolValue &&
                    before == false &&
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
        /// Reduction項目のGUIを追加
        /// </summary>
        /// <param name="lineSetNode">LineSetNode</param>
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
        /// BrushSettings部分のGUIの作成
        /// </summary>
        /// <param name="linSetObject"></param>
        void MakeBrushSettings(GameObject linSetObject)
        {
            Action NoBrushSettings = () =>
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
            };


            if (linSetObject == null)
            {
                NoBrushSettings();
                return;
            }

            BrushSettingsNode brushSettingsNode = linSetObject.GetComponent<BrushSettingsNode>();
            if (brushSettingsNode == null)
            {
                NoBrushSettings();
                return;
            }


            if (currentLineType == LineSetNode.LineType.Visible)
            {
                serializedBrushSettingsVisibleParams.Update();
            }
            else
            {
                serializedBrushSettingsHiddenParams.Update();
            }

            // Brush Detail

            EditorGUICustomLayout.PencilNodeField(
                        "Brush Detail",
                        typeof(BrushDetailNode),
                        (currentLineType == LineSetNode.LineType.Visible) ?
                            serializedBrushSettingsVisibleParams :
                            serializedBrushSettingsHiddenParams,
                        currentParams.propBrushDetail,
                        (nodeObject) =>
                        {
                            // LineSetNodeEditorが参照しているBrushDetailsのプロパティを再設定
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


            // BlendMode
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
            currentParams.propMapOpacity.floatValue = EditorGUILayout.Slider("Map Opacity",
                                                               currentParams.propMapOpacity.floatValue,
                                                               0.0f, 1.0f);

            // Size
            currentParams.propSize.floatValue = EditorGUILayout.Slider("Size",
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
        /// BrushDetail部分のGUIの作成
        /// </summary>
        /// <param name="brushDetailObject">BrushDetailコンポーネントを持っているGameObject</param>
        void MakeBrushDetail(GameObject brushDetailObject)
        {

            if (brushDetailObject == null
                || brushDetailObject.GetComponent<BrushDetailNode>() == null)
            {
                EditorGUILayout.Slider("Stretch", 0, -1.0f, 1.0f);
                EditorGUILayout.Slider("Angle", 0, -360.0f, 360.0f);
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
        /// BrushSettingsのプロパティの再取得を行うメソッド
        /// </summary>
        void ChangedBrushSettings()
        {
            // Set Brush Settings Properties
            GameObject brushSettings;

            brushSettings = visibleParams.propBrushSettings.objectReferenceValue as GameObject;

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


            brushSettings = hiddenParams.propBrushSettings.objectReferenceValue as GameObject;

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

        void OnEnable()
        {
            propId = serializedObject.FindProperty("Id");

            // ---------- Objects List ----------
            propObjects =
                serializedObject.FindProperty("Objects");

            reorderableObjectsList =
                Common.CreateObjectList(
                    serializedObject,
                    propObjects,
                    (target as LineSetNode).Objects,
                    selectedObjects =>
                    {
                        serializedObject.Update();
                        propObjects.AppendObjects(selectedObjects);
                        serializedObject.ApplyModifiedProperties();
                        reorderableObjectsList.index = reorderableObjectsList.count - 1;
                    });

            // ---------- Materials List ----------
            propMaterials =
                serializedObject.FindProperty("Materials");

            reorderableMaterialsList =
                Common.CreateMaterialList(
                    serializedObject,
                    propMaterials,
                    (target as LineSetNode).Materials,
                    selectedMaterials =>
                    {
                        serializedObject.Update();
                        propMaterials.AppendObjects(selectedMaterials);
                        serializedObject.ApplyModifiedProperties();
                        reorderableMaterialsList.index = reorderableMaterialsList.count - 1;
                    });

            // --------------------
            visibleParams.propBrushSettings = 
                serializedObject.FindProperty("VBrushSettings");
            visibleParams.propEdgeOutlineOn = 
                serializedObject.FindProperty("VOutlineOn");
            visibleParams.propEdgeOutlineS = 
                serializedObject.FindProperty("VOutlineSpecificOn");
            visibleParams.propEdgeOutline = 
                serializedObject.FindProperty("VOutline");
            visibleParams.propEdgeObjectOn = 
                serializedObject.FindProperty("VObjectOn");
            visibleParams.propEdgeObjectS = 
                serializedObject.FindProperty("VObjectSpecificOn");
            visibleParams.propEdgeObject = 
                serializedObject.FindProperty("VObject");
            visibleParams.propEdgeIntersectionOn = 
                serializedObject.FindProperty("VIntersectionOn");
            visibleParams.propEdgeIntersectionS = 
                serializedObject.FindProperty("VIntersectionSpecificOn");
            visibleParams.propEdgeIntersection = 
                serializedObject.FindProperty("VIntersection");
            visibleParams.propEdgeSmoothOn = 
                serializedObject.FindProperty("VSmoothOn");
            visibleParams.propEdgeSmoothS = 
                serializedObject.FindProperty("VSmoothSpecificOn");
            visibleParams.propEdgeSmooth = 
                serializedObject.FindProperty("VSmooth");
            visibleParams.propEdgeMaterialOn = 
                serializedObject.FindProperty("VMaterialOn");
            visibleParams.propEdgeMaterialS = 
                serializedObject.FindProperty("VMaterialSpecificOn");
            visibleParams.propEdgeMaterial = 
                serializedObject.FindProperty("VMaterial");
            visibleParams.propEdgeNormalAngleOn = 
                serializedObject.FindProperty("VNormalAngleOn");
            visibleParams.propEdgeNormalAngleS = 
                serializedObject.FindProperty("VNormalAngleSpecificOn");
            visibleParams.propEdgeNormalAngle = 
                serializedObject.FindProperty("VNormalAngle");
            visibleParams.propEdgeNormalAngleMin = 
                serializedObject.FindProperty("VNormalAngleMin");
            visibleParams.propEdgeNormalAngleMax = 
                serializedObject.FindProperty("VNormalAngleMax");
            visibleParams.propEdgeWireframeOn = 
                serializedObject.FindProperty("VWireframeOn");
            visibleParams.propEdgeWireframeS = 
                serializedObject.FindProperty("VWireframeSpecificOn");
            visibleParams.propEdgeWireframe = 
                serializedObject.FindProperty("VWireframe");
            visibleParams.propSizeReductionOn = 
                serializedObject.FindProperty("VSizeReductionOn");
            visibleParams.propSizeReduction = 
                serializedObject.FindProperty("VSizeReduction");
            visibleParams.propAlphaReductionOn = 
                serializedObject.FindProperty("VAlphaReductionOn");
            visibleParams.propAlphaReduction = 
                serializedObject.FindProperty("VAlphaReduction");

            hiddenParams.propBrushSettings = 
                serializedObject.FindProperty("HBrushSettings");
            hiddenParams.propEdgeOutlineOn = 
                serializedObject.FindProperty("HOutlineOn");
            hiddenParams.propEdgeOutlineS = 
                serializedObject.FindProperty("HOutlineSpecificOn");
            hiddenParams.propEdgeOutline = 
                serializedObject.FindProperty("HOutline");
            hiddenParams.propEdgeObjectOn = 
                serializedObject.FindProperty("HObjectOn");
            hiddenParams.propEdgeObjectS = 
                serializedObject.FindProperty("HObjectSpecificOn");
            hiddenParams.propEdgeObject = 
                serializedObject.FindProperty("HObject");
            hiddenParams.propEdgeIntersectionOn =
                serializedObject.FindProperty("HIntersectionOn");
            hiddenParams.propEdgeIntersectionS =
                serializedObject.FindProperty("HIntersectionSpecificOn");
            hiddenParams.propEdgeIntersection =
                serializedObject.FindProperty("HIntersection");
            hiddenParams.propEdgeSmoothOn =
                serializedObject.FindProperty("HSmoothOn");
            hiddenParams.propEdgeSmoothS =
                serializedObject.FindProperty("HSmoothSpecificOn");
            hiddenParams.propEdgeSmooth = 
                serializedObject.FindProperty("HSmooth");
            hiddenParams.propEdgeMaterialOn =
                serializedObject.FindProperty("HMaterialOn");
            hiddenParams.propEdgeMaterialS =
                serializedObject.FindProperty("HMaterialSpecificOn");
            hiddenParams.propEdgeMaterial =
                serializedObject.FindProperty("HMaterial");
            hiddenParams.propEdgeNormalAngleOn =
                serializedObject.FindProperty("HNormalAngleOn");
            hiddenParams.propEdgeNormalAngleS =
                serializedObject.FindProperty("HNormalAngleSpecificOn");
            hiddenParams.propEdgeNormalAngle = 
                serializedObject.FindProperty("HNormalAngle");
            hiddenParams.propEdgeNormalAngleMin =
                serializedObject.FindProperty("HNormalAngleMin");
            hiddenParams.propEdgeNormalAngleMax =
                serializedObject.FindProperty("HNormalAngleMax");
            hiddenParams.propEdgeWireframeOn = 
                serializedObject.FindProperty("HWireframeOn");
            hiddenParams.propEdgeWireframeS =
                serializedObject.FindProperty("HWireframeSpecificOn");
            hiddenParams.propEdgeWireframe =
                serializedObject.FindProperty("HWireframe");
            hiddenParams.propSizeReductionOn = 
                serializedObject.FindProperty("HSizeReductionOn");
            hiddenParams.propSizeReduction = 
                serializedObject.FindProperty("HSizeReduction");
            hiddenParams.propAlphaReductionOn = serializedObject.FindProperty("HAlphaReductionOn");
            hiddenParams.propAlphaReduction = 
                serializedObject.FindProperty("HAlphaReduction");

            propWeldsEdges = serializedObject.FindProperty("WeldsEdges");
            propMaskHiddenLines = serializedObject.FindProperty("MaskHiddenLines");


            ChangedBrushSettings();

        }

        public override void OnInspectorGUI()
        {
            LineSetNode pclLineSetNode = target as LineSetNode;
            serializedObject.Update();

            MakeLineSet();

            MakeBrush(pclLineSetNode);
            MakeEdge(pclLineSetNode);
            MakeReduction(pclLineSetNode);

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// MenuにLineSetノードを追加する項目を追加
        /// </summary>
        [MenuItem("GameObject/Pencil+ 4/Line Set Node", false)]
        public static void OpenLineSetNode()
        {
            GameObject newLineSet = Instantiate(Prefabs.LineSet);
            Undo.RegisterCreatedObjectUndo(newLineSet, "Create Line Set Node");
            List<GameObject> allGameObjs = Common.GetAllGameObject();
            newLineSet.name = allGameObjs.GetUniqueName(Prefabs.LineSet);

            // BrushSettingsの追加
            LineSetNode lineSetNode = newLineSet.GetComponent<LineSetNode>();

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

        }
    }

}