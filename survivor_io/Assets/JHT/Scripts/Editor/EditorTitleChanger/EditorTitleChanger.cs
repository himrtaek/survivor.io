using UnityEditor;
using UnityEngine;

namespace JHT.Scripts.Editor.EditorTitleChanger
{
    [InitializeOnLoad]
    public class EditorTitleChanger
    {
        static EditorTitleChanger()
        {
            SetEvent();
        }

        private static void SetEvent()
        {
            const string assetPath = "/Assets";
            var assetPathFull = Application.dataPath;
            var workspace = assetPathFull.Remove(assetPathFull.Length - assetPath.Length);

            void Cb(ApplicationTitleDescriptor x) =>
	            x.title += $" ({workspace})";
            
            EditorApplication.updateMainWindowTitle += Cb;
            EditorApplication.UpdateMainWindowTitle();
            
            /*Debug.Log("updateMainWindowTitle");*/
        }
    }
}
