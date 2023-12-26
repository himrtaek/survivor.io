using UnityEditor;
using Object = UnityEngine.Object;

namespace SK
{
	internal static class SKObjectCreateMenu
	{
		[MenuItem("CONTEXT/SKObject/Replace with SKObjectPlayerWeaponSpawner")]
		private static void SKObjectReplaceSKObjectPlayerWeaponSpawner(MenuCommand menuCommand)
		{
			var skObject = (SKObject)menuCommand.context;
			var obj = skObject.gameObject;
			Object.DestroyImmediate(skObject);
			var skObjectPlayerWeaponSpawner = obj.AddComponent<SKObjectPlayerWeaponSpawner>();

			skObject.SerializeFieldCopyTo(skObjectPlayerWeaponSpawner);
		}
	}
}
