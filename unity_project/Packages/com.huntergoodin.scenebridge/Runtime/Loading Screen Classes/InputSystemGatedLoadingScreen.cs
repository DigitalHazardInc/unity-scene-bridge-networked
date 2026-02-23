using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HunterGoodin.SceneBridge
{
	public class InputSystemGatedLoadingScreen : LoadingScreen
	{
		private PlayerInput input;
		private InputAction progressionAction;

		[Header("Scene References")]
		[SerializeField] private GameObject progressionTMPObj;

		[Header("Color Coordination")]
		[SerializeField] private bool correlateProgColorWithBackgoundImg;

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

		internal new void OnEnable()
		{
			base.OnEnable();
			progressionTMPObj.GetComponent<TextMeshProUGUI>().color = colors[bgRand];

			progressionAction = input.LoadingScreen.Progression;
			progressionAction.Enable();
		}

		private void OnDisable()
		{
			progressionAction.Disable();
		}

		public override void ReadyToLoadNewScene()
		{
			progressionTMPObj.SetActive(true); 
		}
	}
}
