using UnityEngine;

namespace HunterGoodin.SceneBridge
{
	public class SceneBridgeLoader : MonoBehaviour
	{
		// Singleton Stuff 
		public static SceneBridgeLoader Instance => instance;
		private static SceneBridgeLoader instance;
		[SerializeField] private bool addToDontDestroyOnLoad = false;

		// Scene Loader Stuff 
		[SerializeField] private GameObject fadeCanvas; 

		private void Awake()
		{
			// Singleton Stuff 
			if (instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				instance = this;
			}

			// Scene loader stuff 

		}

		public void LoadSceneAsynchronously(string sceneName)
		{

		}

		public void LoadSceneDirectly(string sceneName)
		{

		}
	}
}