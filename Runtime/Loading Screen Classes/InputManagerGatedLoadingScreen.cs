using TMPro;
using UnityEngine;

namespace HunterGoodin.SceneBridge
{
	public class InputManagerGatedLoadingScreen : LoadingScreen
	{
		[Header("Scene References")]
		[SerializeField] private GameObject readyTMPObj;

		[Header("Color Coordination")]
		[SerializeField] private bool correlateReadyColorWithBackgoundImg;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
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
		}

		public override void ReadyToLoadNewScene()
		{
			readyTMPObj.SetActive(true);
		}
	}
}
