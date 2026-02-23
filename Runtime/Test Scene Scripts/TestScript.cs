using UnityEngine;

namespace HunterGoodin.SceneBridge
{
	public class TestScript : MonoBehaviour
	{
		public void DoTheSceneLoad(string sceneName)
		{
			SceneBridgeLoader.Instance.LoadSceneAsynchronously(sceneName); 
		}
	}
}
