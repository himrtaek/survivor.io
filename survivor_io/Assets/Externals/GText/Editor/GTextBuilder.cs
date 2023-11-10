#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class GTextBuilder
{
    [MenuItem("GameObject/UI/GText")]
    static void Create()
    {
        GameObject select = Selection.activeGameObject;
        if (select == null)
            return;
        RectTransform transform = select.GetComponent<RectTransform>();
        if(transform == null)
            return;

        GameObject obj = new GameObject("GText");
        obj.transform.SetParent(transform);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(160, 30);

        obj.AddComponent<CanvasRenderer>();
        GText text = obj.AddComponent<GText>();
        text.text = "New GText";

        Selection.activeGameObject = obj;
    }
}


#endif
