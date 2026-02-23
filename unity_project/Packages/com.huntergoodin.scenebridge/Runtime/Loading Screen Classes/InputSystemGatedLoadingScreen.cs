using UnityEngine;
using UnityEngine.InputSystem;

namespace HunterGoodin.SceneBridge
{
	public class InputSystemGatedLoadingScreen : LoadingScreen
	{
		private PlayerInput input;
		private InputAction progressionAction;
		[SerializeField] private GameObject progressionTMPObj;

		private void Awake()
		{
			input = new PlayerInput();
		}

		private void Update()
		{
			if (progressionAction.IsPressed())
			{
				progressionTMPObj.SetActive(false);
				SceneBridgeLoader.Instance.ContinueToNewScene();
			}
		}

		void OnEnable()
		{
			progressionAction = input.LoadingScreen.Progression;

			progressionAction.Enable();
		}

		void OnDisable()
		{
			progressionAction.Disable();
		}

		public override void ReadyToLoadNewScene()
		{
			progressionTMPObj.SetActive(true); 
		}
	}
}
