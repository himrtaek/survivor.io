using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Text;
using JHT.Scripts.Common;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace N2.MOMA2
{
	public class SpineGenerator
	{
		private static readonly string[] BindDirectories =
		{
			"Survivor_io_assets/TextAsset",
		};
		
		[MenuItem("SK/탕탕 스파인 리소스 생성")]
		public static void SetAttribute()
		{
			// 바인딩
			var assetPath = Application.dataPath;
			foreach (var bindPath in BindDirectories)
			{
				ParsingSpineFromDirectoryPath(Path.Combine(assetPath, bindPath));
			}
			
			// 리프레쉬
			AssetDatabase.Refresh();
		}

		private static Utf8ValueStringBuilder _zStringBuilder = ZString.CreateUtf8StringBuilder();

		static void ParsingSpineFromDirectoryPath(string rootDirectoryPath)
		{
			if (false == Directory.Exists(rootDirectoryPath))
			{
				return;
			}
			
			var filePaths = Directory.GetFiles(rootDirectoryPath, "*.atlas.txt");
			foreach (var filePath in filePaths)
			{
				ParsingSpineFromFilePath(filePath);
			}
		}
		
		static void ParsingSpineFromFilePath(string atlasTextPath)
		{
			if (false == File.Exists(atlasTextPath).IsFalse(false))
			{
				Copy(atlasTextPath, atlasTextPath.Replace("/TextAsset\\", "/Spine\\"));
			}

			var atlasAssetPath = atlasTextPath.Replace(".atlas.txt", "_Atlas.asset");
			if (false == File.Exists(atlasAssetPath).IsFalse(false))
			{
				Copy(atlasAssetPath, atlasAssetPath.Replace("/TextAsset\\", "/Spine\\"));
			}
			
			var jsonPath = atlasTextPath.Replace(".atlas.txt", ".txt");
			if (false == File.Exists(jsonPath).IsFalse(false))
			{
				Copy(jsonPath, jsonPath.Replace("/TextAsset\\", "/Spine\\").Replace(".txt", ".json"));
			}
			
			var texturePath = jsonPath.Replace("/TextAsset\\", "/Texture2D\\").Replace(".txt", ".png");
			if (false == File.Exists(texturePath).IsFalse(false))
			{
				Copy(texturePath, texturePath.Replace("/Texture2D\\", "/Spine\\"));
			}
		}

		static void Copy(string srcFilePath, string dstFilePath)
		{
			var directoryName = Path.GetDirectoryName(dstFilePath);
			if (false == Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}

			File.Move(srcFilePath, dstFilePath);
		}
	}
}
