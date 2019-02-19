using System.Collections.Generic;
using UnityEngine;

namespace Pencil_4
{
#if UNITY_EDITOR
    public static class Utilities
    {
        /// <summary>
        /// プロジェクトから条件に一致するアセットを検索し、検索結果の先頭を返す。
        /// </summary>
        /// <typeparam name="T">検索するオブジェクトの型</typeparam>
        /// <param name="filter">検索条件</param>
        /// <param name="name">アセットの名前</param>
        /// <returns>検索結果の先頭（見つからなかった場合はnull）</returns>
        public static UnityEngine.Object FindAssetInProjectOnEditor(string filter, string name)
        {
            // TODO: プレイ中の動作の妥当性を検討する
            UnityEngine.Object ret = null;

            if (!string.IsNullOrEmpty(name))
            {
                var materialList = UnityEditor.AssetDatabase.FindAssets(string.Format("{0} {1}", filter, name));
                if (materialList.Length > 0)
                {
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(materialList[0]);
                    ret = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                }
            }

            return ret;
        }
    }
#endif
}
