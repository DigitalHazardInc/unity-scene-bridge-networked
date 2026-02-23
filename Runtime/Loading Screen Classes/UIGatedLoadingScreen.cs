using UnityEngine;
using UnityEngine.UI;

namespace HunterGoodin.SceneBridge
{
	public class UIGatedLoadingScreen : LoadingScreen
	{
		[SerializeField] private Button progressButton;

		public override void ReadyToLoadNewScene()
		{
			progressButton.interactable = true;
		}

		public void LoadNewScene()
		{
			progressButton.interactable = false;
			SceneBridgeLoader.Instance.ContinueToNewScene();
		}
	}
}
