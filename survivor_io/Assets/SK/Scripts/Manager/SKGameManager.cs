using System.Collections;
using System.Collections.Generic;
using JHT.Scripts.Common;
using UnityEngine;

public class SKGameManager : MonoSingleton<SKGameManager>
{
    public SKObjectManager ObjectManager { get; } = new();
    public uint StageId { get; private set; }

    public void Init(uint stageId)
    {
        ObjectManager.Init();
        StageId = stageId;
    }

    public void Update()
    {
        GameUpdate(Time.deltaTime);
    }

    public void GameUpdate(float deltaTime)
    {
        ObjectManager.GameUpdate(deltaTime);
    }
}
