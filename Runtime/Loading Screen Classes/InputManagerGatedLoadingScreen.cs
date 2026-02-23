using TMPro;
using UnityEngine;

namespace HunterGoodin.SceneBridge
{
	public class InputManagerGatedLoadingScreen : LoadingScreen
	{
		[Header("Scene References")]
		[SerializeField] private GameObject progressionTMPObj;

		[Header("Color Coordination")]
		[SerializeField] private bool coorelateProgColorWithBackgoundImg;

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
			progressionTMPObj.GetComponent<TextMeshProUGUI>().color = colors[bgRand];
		}

		public override void ReadyToLoadNewScene()
		{
			progressionTMPObj.SetActive(true);
		}
	}
}
