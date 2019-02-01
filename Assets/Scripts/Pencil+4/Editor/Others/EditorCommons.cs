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

    public static class EditorCommons
    {

        /// <summary>
        /// Hierarcy上にあるすべてのGameObjectを取得
        /// </summary>
        /// <param name="componentFilter">コンポーネントフィルタ</param>
        /// <returns> gameObjects : Hierarcy上の全てのGameObject </returns>
        public static List<GameObject> GetAllGameObject(String componentFilter = null)
        {
            // FIXME: 非アクティブなGameObjectが含まれない問題あり
            return GameObject.FindObjectsOfType<GameObject>()
                             .Where(x => x.activeInHierarchy)
                             .Where(x => String.IsNullOrEmpty(componentFilter) ||
                                         x.GetComponent(componentFilter) != null)
                             .ToList<GameObject>();
        }


        /// <summary>
        /// 編集中のシーンにGameObjectをスポーンする
        /// </summary>
        /// <param name="parent">追加したGameObjectの親に設定するGameObject</param>
        /// <param name="allGameObjs">hierarchy内のすべてのGameObject</param>
        /// <param name="prefab">作成する基となるGameObject</param>
        /// <param name="suffixName">作成したGameObjectのサフィックス名</param>
        /// <param name="propObject">作成したGameObjectを入れるSerializeProperty型の変数</param>
        /// <returns>作成したGameObject</returns>
        public static GameObject CreateObject(GameObject parent,
                                              List<GameObject> allGameObjs,
                                              GameObject prefab,
                                              String suffixName = " ",
                                              SerializedProperty propObject = null)
        {
            if (propObject != null && propObject.objectReferenceValue != null)
            {
                return null;
            }
            GameObject newObj = GameObject.Instantiate(prefab);

            newObj.name = allGameObjs.GetUniqueName(prefab, suffixName);

            if (propObject != null)
            {
                propObject.objectReferenceValue = newObj;
            }
            newObj.transform.parent = parent.transform;
            return newObj;
        }


        /// <summary>
        /// ダブルクリックが成功かチェックを行う
        /// </summary>
        /// <param name="diffTime">ダブルクリックの間隔</param>
        /// <param name="index">ダブルクリックのインデックス</param>
        /// <returns>ダブルクリックが成功したか</returns>
        static int beforeTime = -1;
        static int beforeIndex = -1;
        public static bool IsDoubleClickSucceed(int diffTime, int index = -1)
        {
            // 24h == 86400000ms
            int time24h = 86400000;

            if (index == -1)
            {
                beforeIndex = -1;
            }

            int time = GetCurrentTime();

            if (beforeTime > time)
            {
                time += time24h;
            }

            // 前回の選択インデックスと一致、前回クリックからdiffTimeミリ秒経っていなければ成功
            bool isSuccess = beforeTime + diffTime > time && index == beforeIndex;

            beforeTime = time % time24h;
            beforeIndex = index;

            return isSuccess;
        }


        /// <summary>
        /// 現在のミリ秒を取得
        /// </summary>
        /// <returns>現在のミリ秒</returns>
        public static int GetCurrentTime()
        {
            // TODO: ダブルクリックの間隔を測るためにしか使われていないので、後で削除する
            return DateTime.Now.Hour * 60 * 60 * 1000 +
                   DateTime.Now.Minute * 60 * 1000 +
                   DateTime.Now.Second * 1000 +
                   DateTime.Now.Millisecond;
        }


        static List<GameObject> delayRemoveList = new List<GameObject>();

        /// <summary>
        /// ノードリストを作成する
        /// </summary>
        /// <param name="serializedObj">シリアライズオブジェクト</param>
        /// <param name="propList">シリアライズプロパティ</param>
        /// <param name="currentNode">現在のノード</param>
        /// <param name="prefab">オブジェクト作成の基になるプレハブ</param>
        /// <param name="doubleClickDiffTime">ダブルクリックの間隔(ms)</param>
        /// <returns>作成されたリスト</returns>
        public static ReorderableList CreateReorderableNodeList(SerializedObject serializedObj,
                                                                SerializedProperty propList,
                                                                NodeBase currentNode, GameObject prefab,
                                                                int doubleClickDiffTime = 500)
        {
            ReorderableList newReorderList =
                new ReorderableList(serializedObj, propList);

            newReorderList.headerHeight = 2;

            newReorderList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (propList.arraySize <= index)
                {
                    return;
                }

                var element = propList.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                if (element.objectReferenceValue != null)
                {
                    EditorGUI.LabelField(rect, element.objectReferenceValue.name);
                }
                else
                {
                    propList.DeleteArrayElementAtIndex(index);
                }
            };

            // 要素の追加
            newReorderList.onAddCallback = (list) =>
            {
                list.index = propList.arraySize++;

                var element = propList.GetArrayElementAtIndex(list.index);
                List<GameObject> allGameObjs = GetAllGameObject();
                GameObject newNode = GameObject.Instantiate(prefab);
                Undo.RegisterCreatedObjectUndo(newNode, "Create Node");
                newNode.name = allGameObjs.GetUniqueName(prefab);

                newNode.transform.parent = currentNode.transform;
                element.objectReferenceValue = newNode;

            };

            // 要素の削除
            newReorderList.onRemoveCallback = (list) =>
            {
                var element = propList.GetArrayElementAtIndex(list.index);
                GameObject obj = element.objectReferenceValue as GameObject;
                if (obj != null)
                {
                    propList.DeleteArrayElementAtIndex(list.index);
                }
                propList.DeleteArrayElementAtIndex(list.index);

                if (list.index >= propList.arraySize)
                {
                    list.index = propList.arraySize - 1;
                }

                // デストロイ可能なタイミングで処理を行う
                if (obj != null)
                {
                    // MEMO: 普通に削除すると例外が発生する?
                    delayRemoveList.Add(obj);
                    EditorApplication.delayCall += () =>
                    {
                        delayRemoveList.Remove(obj);
                        Undo.DestroyObjectImmediate(obj);
                    };
                }
            };

            //// Elementの入れ替えを行った際に呼ばれるコールバック
            //newReorderList.onReorderCallback = (list) =>
            //{
            //    int count = propList.arraySize;
            //    List<int> siblingIndics = new List<int>();
            //    for (int i = 0; i < count; i++)
            //    {
            //        SerializedProperty propObj =
            //            propList.GetArrayElementAtIndex(i);
            //        GameObject obj = propObj.objectReferenceValue as GameObject;

            //        siblingIndics.Add(obj.transform.GetSiblingIndex());
            //    }

            //    siblingIndics.Sort();

            //    for (int i = 0; i < count; i++)
            //    {
            //        SerializedProperty propObj =
            //            propList.GetArrayElementAtIndex(i);
            //        GameObject obj = propObj.objectReferenceValue as GameObject;

            //        Undo.SetTransformParent(obj.transform,
            //                                currentNode.transform,
            //                                "Change Priority");

            //        obj.transform.SetSiblingIndex(siblingIndics[i]);
            //        propObj.objectReferenceValue = obj;
            //    }
            //};

            // ダブルクリック
            newReorderList.onSelectCallback = (list) =>
            {
                if (list.index < 0 ||
                   list.index >= propList.arraySize)
                {
                    return;
                }

                if (!IsDoubleClickSucceed(doubleClickDiffTime, list.index))
                {
                    return;
                }

                SerializedProperty propObj =
                    propList.GetArrayElementAtIndex(list.index);

                Selection.activeObject = propObj.objectReferenceValue;
            };

            return newReorderList;
        }

        /// <summary>
        /// 削除予定のノードのリストを返す
        /// </summary>
        /// <returns>削除予定のノードリスト</returns>
        public static List<GameObject> GetDelayRemoveList()
        {
            return delayRemoveList;
        }


        public static ReorderableList CreateObjectList(
            SerializedObject serializedObject,
            SerializedProperty serializedProperty,
            IEnumerable<GameObject> objectsToExclude,
            Action<List<GameObject>> objectsWillAdd)
        {
            return CreateReorderableList<GameObject>(serializedObject, serializedProperty,
                () => 
                {
                    ObjectPickerWindow.Open(objectsToExclude, x => objectsWillAdd(x));
                });
        }


        public static ReorderableList CreateMaterialList(
            SerializedObject serializedObject,
            SerializedProperty serializedProperty,
            IEnumerable<Material> materialsToExclude,
            Action<List<Material>> materialsWillAdd)
        {
            return CreateReorderableList<Material>(serializedObject, serializedProperty,
                () =>
                {
                    MaterialPickerWindow.Open(materialsToExclude, x => materialsWillAdd(x));
                });
        }



        /// <summary>
        /// ObjectList, MaterialListを作成する
        /// (CreateObjectList, CreateMaterialListから呼ばれる事を想定している)
        /// </summary>
        /// <typeparam name="T">列挙するオブジェクトの型</typeparam>
        /// <param name="serializedObj">表示対象のSerializedObject</param>
        /// <param name="propList">表示対象のSerializedProperty</param>
        /// <param name="pickerWillShow">ピッカーが表示される時に呼ばれるデリゲート</param>
        /// <param name="doubleClickDiffTimeMs">ダブルクリックの間隔</param>
        /// <returns></returns>
        public static ReorderableList CreateReorderableList<T>(
            SerializedObject serializedObj,
            SerializedProperty propList,
            Action pickerWillShow,
            int doubleClickDiffTimeMs = 500)
            where T : UnityEngine.Object
        {
            ReorderableList newReorderList =
                new ReorderableList(serializedObj, propList);

            newReorderList.headerHeight = 2;

            newReorderList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if(propList.arraySize <= index)
                {
                    return;
                }

                var element = propList.GetArrayElementAtIndex(index);
                rect.height -= 4;
                rect.y += 2;
                if (element.objectReferenceValue != null)
                {
                    EditorGUI.LabelField(rect, element.objectReferenceValue.name);
                }
                else
                {
                    propList.DeleteArrayElementAtIndex(index);
                }
            };

            // 要素の追加
            newReorderList.onAddCallback = (list) =>
            {
                pickerWillShow();
            };

            // 要素の削除
            newReorderList.onRemoveCallback = (list) =>
            {
                var element = propList.GetArrayElementAtIndex(list.index);
                T obj = element.objectReferenceValue as T;
                if (obj != null)
                {
                    propList.DeleteArrayElementAtIndex(list.index);
                }
                propList.DeleteArrayElementAtIndex(list.index);

                if (list.index >= propList.arraySize)
                {
                    list.index = propList.arraySize - 1;
                }
            };

            // ダブルクリック
            newReorderList.onSelectCallback = (list) =>
            {
                if (list.index < 0 ||
                   list.index >= propList.arraySize)
                {
                    return;
                }

                if (!IsDoubleClickSucceed(doubleClickDiffTimeMs, list.index))
                {
                    return;
                }

                SerializedProperty propObj =
                    propList.GetArrayElementAtIndex(list.index);

                Selection.activeObject = propObj.objectReferenceValue;
            };

            return newReorderList;
        }


        /// <summary>
        /// ノードリストにドラッグアンドドロップ機能を追加
        /// </summary>
        /// <typeparam name="T">ノードリストのタイプ</typeparam>
        /// <param name="parentNode">親ノード</param>
        /// <param name="propList">ノードリスト</param>
        /// <param name="rect">ドラッグアンドドロップ可能範囲</param>
        public static void DragAndDropNode<T>(NodeBase parentNode,
                                              SerializedProperty propList,
                                              Rect rect)
            where T : NodeBase
        {

            int id = GUIUtility.GetControlID(FocusType.Passive);
            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.activeControlID = id;

                    if (evt.type != EventType.DragPerform)
                    {
                        break;
                    }

                    DragAndDrop.AcceptDrag();

                    List<UnityEngine.Object> draggedList =
                        DragAndDrop.objectReferences.ToList();

                    // 条件に合わないノード
                    List<UnityEngine.Object> exclusionList =
                        draggedList.Where(x => x.GetType() != typeof(GameObject) ||
                                          (x as GameObject).GetComponent<T>() == null)
                                   .ToList();

                    foreach (var item in exclusionList)
                    {
                        Debug.LogWarning("\"" + item.name + "\" is not " + typeof(T).Name);
                    }

                    // 条件に合うノード
                    var addItemList =
                        draggedList.OfType<GameObject>()
                                   .Where(x => x.GetComponent<T>());

                    // 条件に合ったノードを持つオブジェクトを移動
                    foreach (var item in addItemList)
                    {
                        item.transform.parent = parentNode.transform;
                    }

                    DragAndDrop.activeControlID = 0;

                    Event.current.Use();
                    break;
            }
        }



        public static void DragAndDropObject<T, U>(NodeBase parentNode,
                                                List<GameObject> parentList,
                                                SerializedProperty propList,
                                                Rect rect,
                                                GetObjectList<T> getListDelegate,
                                                bool checkOtherNode)
            where T : UnityEngine.Object
            where U : NodeBase
        {
            int id = GUIUtility.GetControlID(FocusType.Passive);
            var evt = Event.current;
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!rect.Contains(evt.mousePosition)) break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    DragAndDrop.activeControlID = id;

                    if (evt.type != EventType.DragPerform)
                    {
                        break;
                    }

                    DragAndDrop.AcceptDrag();

                    // 追加処理

                    // propertyからlistにコピー
                    List<T> list = new List<T>();
                    for (int i = 0; i < propList.arraySize; i++)
                    {
                        SerializedProperty propObj = propList.GetArrayElementAtIndex(i);
                        list.Add(propObj.objectReferenceValue as T);
                    }

                    // GameObjectかそれ以外で処理を変更する
                    var draggedList = typeof(T) == typeof(GameObject) ?
                                          new List<T>() :
                                          DragAndDrop.objectReferences.OfType<T>().ToList();

                    List<GameObject> draggedObjList =
                        DragAndDrop.objectReferences.OfType<GameObject>().ToList();

                    List<T> draggedAllList = new List<T>();
                    foreach (var item in draggedObjList)
                    {
                        List<T> objs = new List<T>();

                        if(typeof(T) == typeof(GameObject))
                        {
                            objs = item.GetMeshObjectList() as List<T>;
                        }
                        else if(typeof(T) == typeof(Material))  // FIXME: TがMaterialを継承したクラスでも大丈夫?
                        {
                            objs = item.GetMeshMaterialList() as List<T>;
                        }
                        draggedAllList.AddRange(objs.ToList());
                    }

                    draggedList.AddRange(draggedAllList.Distinct().ToList());

                    // 他ノードとの重複チェック
                    if (checkOtherNode)
                    {
                        draggedList = CheckOtherNode<T, U>(parentList,
                                                           draggedList.Distinct().ToList(),
                                                           propList.name,
                                                           getListDelegate);
                    }

                    // 追加
                    list.AddRange(draggedList);

                    if (!checkOtherNode)
                    {
                        // 現在のリスト内で被ったら警告表示
                        List<T> overlappedList = list.CheckOverlapped();
                        foreach (var item in overlappedList)
                        {
                            Debug.LogWarning("\"" + item.name +
                                             "\" is already added in \"" +
                                             parentNode.name + "\"");
                        }
                        list = list.Distinct().ToList();
                    }

                    // Null除去
                    list = list.Where(x => x != null).ToList();

                    // propertyで反映
                    propList.arraySize = list.Count;
                    for (int i = 0; i < list.Count; i++)
                    {
                        SerializedProperty propObj = propList.GetArrayElementAtIndex(i);
                        propObj.objectReferenceValue = list[i];
                    }

                    DragAndDrop.activeControlID = 0;

                    Event.current.Use();
                    break;
            }
        }


        public delegate List<T> GetObjectList<T>(NodeBase node, String varName);

        public static List<T> CheckOtherNode<T, U>(List<GameObject> ObjList,
                                                   List<T> checkList,
                                                   String varName,
                                                   GetObjectList<T> getListDelegate)
            where T : UnityEngine.Object
            where U : NodeBase
        {
            foreach (var obj in ObjList)
            {
                var node = obj.GetComponent<U>();
                if (node == null)
                {
                    continue;
                }

                List<T> currentList = getListDelegate(node, varName);

                for (int i = 0; i < checkList.Count; i++)
                {
                    if (checkList[i] == default(T))
                    {
                        continue;
                    }

                    List<T> list = new List<T>(currentList);

                    list = list.Where(x => x == checkList[i]).ToList();
                    if (list.Count != 1)
                    {
                        continue;
                    }

                    Debug.LogWarning("\"" + checkList[i].name +
                                        "\" is already added in \"" +
                                        obj.name + "\"");

                    checkList[i] = default(T);
                }

            }

            return checkList.Where(x => x != null).ToList();
        }

    }
}
