using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : MonoBehaviour
{
    [SerializeField] private SKPlayerInput playerInput;
    
    public void Awake()
    {
        DataTableManager.AddDataTableType(typeof(SupportItemDataTable));
        DataTableManager.Load();
        SKGameManager.Instance.Init(1);
        playerInput.Init(SKGameManager.Instance.ObjectManager.ObjectPlayer);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
