using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HunterGoodin.SceneBridge
{
	public class InputSystemGatedLoadingScreen : LoadingScreen
	{
		private PlayerInput input;
		private InputAction readyAction;

		[Header("Scene References")]
		[SerializeField] private GameObject readyTMPObj;

		[Header("Color Coordination")]
		[SerializeField] private bool correlateReadyColorWithBackgoundImg;

		private void Awake()
		{
			input = new PlayerInput();
		}

		private void Update()
		{
			if (readyAction.IsPressed())
			{
				readyTMPObj.SetActive(false);
				SceneBridgeLoader.Instance.ContinueToNewScene();
			}
		}

		internal new void OnEnable()
		{
			base.OnEnable();

			if (correlateReadyColorWithBackgoundImg)
			{
				readyTMPObj.GetComponent<TextMeshProUGUI>().color = colors[bgRand];
			}

			readyAction = input.LoadingScreen.Progression;
			readyAction.Enable();
		}

		private void OnDisable()
		{
			readyAction.Disable();
		}

		public override void ReadyToLoadNewScene()
		{
			readyTMPObj.SetActive(true); 
		}
	}
}
