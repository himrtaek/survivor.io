using System.IO;
using System.Reflection;
using JHT.Scripts.Common;
using MemoryPack;
using MemoryPack.Compression;
using UnityEngine;
using UnityEngine.Profiling;

namespace JHT.Scripts.MemoryPack
{
	public static class MemoryPackUtil
	{
		public static void MemoryPackSerializeWithCompressor<T>(T data, string path)
		{
			using (new UnityStopwatchScope($"MemoryPackSerializeWithCompressor.{path}"))
			{
				BrotliCompressor compressor = new();
				MemoryPackSerializer.Serialize(compressor, data);
				var bytes = compressor.ToArray();	
				File.WriteAllBytes(path, bytes);
			}
		}

		public static bool TryMemoryPackDeserializeWithDecompressor<T>(string msgPath, out T output, bool showLog = true)
		{
			using (new UnityStopwatchScope($"TryMemoryPackDeserializeWithDecompressor.{msgPath}"))
			{
				var bytes = GetAssetToBytes(msgPath, showLog);
				if (null == bytes)
				{
					output = default;
					return false;
				}
				
				BrotliDecompressor decompressor = new();
				var decompress = decompressor.Decompress(bytes);
				output = MemoryPackSerializer.Deserialize<T>(decompress);
				return true;
			}
		}

		public static byte[] GetAssetToBytes(string path, bool showLog = true)
		{
			using (new UnityStopwatchScope($"GetAssetToBytes.{path}"))
			{
				Profiler.BeginSample($"{MethodBase.GetCurrentMethod()?.Name}.{path}");
				var textAsset = ResourceManager.ResourceManager.Instance.LoadOriginalAsset<TextAsset>(path, showLog:showLog);
				Profiler.EndSample();
			
				if (textAsset.IsNull())
				{
					return null;
				}
			
				return textAsset.bytes;
			}
		}
    }
}
