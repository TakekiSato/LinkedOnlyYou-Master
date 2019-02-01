using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Pcl4Editor
{
    public class VersionWindow : EditorWindow
    {
        static VersionWindow versionWindow;

        const int windowWidth = 400;
        const int windowHeight = 180;

        Texture pencilLogoTexture;

        [MenuItem("Pencil+ 4/About", false, 2)]
        static void Open()
        {

            if (!versionWindow)
            {
                versionWindow = CreateInstance<VersionWindow>();
            }

            versionWindow.pencilLogoTexture = Resources.Load("Pencil+4/Textures/PencilLogo") as Texture;
            versionWindow.titleContent = new GUIContent("About");
            versionWindow.maxSize = versionWindow.minSize = new Vector2(windowWidth, windowHeight);
            versionWindow.ShowUtility();

            versionWindow.position = new Rect(
                (Screen.currentResolution.width / 2) - (windowWidth / 2),
                (Screen.currentResolution.height / 2) - (windowHeight / 2),
                windowWidth,
                windowHeight);
        }




        void OnGUI()
        {
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Box(pencilLogoTexture, GUIStyle.none);
            GUILayout.Space(15);
            EditorGUILayout.EndHorizontal();

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Version " + Pencil_4.VersionInformation.VersionString, labelStyle);

            GUILayout.Space(10);
            EditorGUILayout.LabelField(Pencil_4.VersionInformation.CopyrightString, labelStyle);
        }
    }
}
