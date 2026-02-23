using UnityEngine;

namespace HunterGoodin.SceneBridge
{
	public class InputManagerGatedLoadingScreen : LoadingScreen
	{
		[SerializeField] private GameObject progressionTMPObj;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SceneBridgeLoader.Instance.ContinueToNewScene();
			}
		}

		public override void ReadyToLoadNewScene()
		{
			progressionTMPObj.SetActive(true);
		}
	}
}
