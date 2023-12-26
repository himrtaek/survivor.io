using UnityEngine;

namespace SK
{
	[RequireComponent(typeof(SKComponentSpawnerBase))]
    public class SKObjectSpawner : SKObject
    {
	    public override SKObjectType ObjectType { get; } = SKObjectType.Spawner;
    }
}
