using UnityEngine;

using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
public static class GTextEmojiAtlasBaker  {

    [MenuItem("Emoji/Bake Atlas")]
    private static void BakeEmojiAtlas()
    {
	    var pathRoot = Application.dataPath + "/Externals/GText";
        var pathToEmojis = pathRoot + "/Textures/Emojis";
        var pathToBakedAtlas = pathRoot + "/Textures/Baked/Resources/EmojiInfo/BakedEmojis.png";
        var pathToEmojiInfo = pathRoot + "/Textures/Baked/Resources/EmojiInfo/BakedEmojisInfo.txt";

        var emojiFileNames = Directory.GetFiles(pathToEmojis, "*.png");       

        var emojiTextures = new Texture2D[emojiFileNames.Length];

        for (var i = 0; i < emojiFileNames.Length; i++)
        {
            Texture2D tex = new Texture2D(2, 2);
            if (!tex.LoadImage(File.ReadAllBytes(emojiFileNames[i])))
            {
                Debug.LogError("Cannot load file " + emojiFileNames[i] + " via tex.LoadImage!!!");
                return;
            }
            emojiTextures[i] = tex;       
        }

        var atlas = new Texture2D(2048, 2048);
        var rects = atlas.PackTextures(emojiTextures, 1, 2048);

        var atlasBytes = atlas.EncodeToPNG();
        File.WriteAllBytes(pathToBakedAtlas, atlasBytes);

        var sb = new StringBuilder();
        for (var i = 0; i < emojiFileNames.Length; i++)
        {
	        sb.AppendLine(Path.GetFileNameWithoutExtension(emojiFileNames[i]) + " " + rects[i].x + " " + rects[i].y +
	                      " " + rects[i].width + " " + rects[i].height);
        }
        
        File.WriteAllText(pathToEmojiInfo, sb.ToString());

        Debug.Log("Baked " + emojiFileNames.Length + " emojis into " + pathToBakedAtlas);
        
        AssetDatabase.Refresh();
    }
}
#endif
