using UnityEngine;

namespace SK
{
    public class SKActionPrepareBossBattle : MonoBehaviour
    {
        void OnEnable()
        {
	        SKGameManager.Instance.ShowBossBattleWarningUI();
        }
    }
}
