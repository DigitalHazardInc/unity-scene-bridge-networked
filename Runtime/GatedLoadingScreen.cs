using UnityEngine;
using UnityEngine.UI;

namespace HunterGoodin.SceneBridge
{
	public class GatedLoadingScreen : LoadingScreen
	{
		[SerializeField] private Button progressbutton; 

		// If we want to use an input progress instead of a UI button (I just didn't want to have to set up the Input System but you get the point) 
		//private void Update()
		//{
		//	// Old Input Manager 
		//	//if (Input.GetKeyDown(KeyCode.Space))
		//	//{
		//	//	LoadNewScene(); 
		//	//}
		//	// New Input System 
		//	//if (anyButton.IsPressed())
		//	//{
		//	//	LoadNewScene();
		//	//}
		//}

		public override void ReadyToLoadNewScene()
		{
			progressbutton.interactable = true; 
		}

		public void LoadNewScene()
		{
			progressbutton.interactable = false;
			SceneBridgeLoader.Instance.ContinueToNewScene();
		}
	}
}
