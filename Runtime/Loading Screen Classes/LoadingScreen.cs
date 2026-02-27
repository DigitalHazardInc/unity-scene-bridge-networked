using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunterGoodin.SceneBridge
{
	public class LoadingScreen : MonoBehaviour
	{
		[Header("Scene References")]
		[SerializeField] private TextMeshProUGUI headerTMP; 
		[SerializeField] private Image backgroundImg;
		[SerializeField] private TextMeshProUGUI tipTmp;
		[SerializeField] private TextMeshProUGUI progressMessageTMP;
		[SerializeField] private Image progressBar;

		[Header("Color Coordination")]
		[SerializeField] private bool correlateTipColorWithBackgoundImg;
		[SerializeField] private bool correlateProgColorWithBackgoundImg;
		[SerializeField] private bool correlateHeaderColorWithBackgoundImg;
		[SerializeField] private bool correlateloadingBarColorWithBackgoundImg;
		[SerializeField] internal Color[] colors;
		internal int bgRand; 

		[Header("Values to set")]
		[SerializeField] private Sprite[] backgroundSprites;
		[SerializeField] private string[] tips;

		internal void OnEnable()
		{
			bgRand = Random.Range(0, backgroundSprites.Length);
			backgroundImg.sprite = backgroundSprites[bgRand];
			tipTmp.text = tips[Random.Range(0, tips.Length)];

			if (correlateTipColorWithBackgoundImg)
			{
				tipTmp.color = colors[bgRand];
			}

			if (correlateProgColorWithBackgoundImg)
			{
				progressMessageTMP.color = colors[bgRand];
			}

			if (correlateHeaderColorWithBackgoundImg)
			{
				headerTMP.color = colors[bgRand];
			}

			if (correlateloadingBarColorWithBackgoundImg)
			{
				progressBar.color = colors[bgRand];
			}
		}

		public virtual void ReadyToLoadNewScene() { } 

		public void SetLoadingBarAmount(float amount)
		{
			progressBar.fillAmount = amount;
		}

		public void UpdateProgressMessage(string msg)
		{
			progressMessageTMP.text = msg; 
		}
	}
}
