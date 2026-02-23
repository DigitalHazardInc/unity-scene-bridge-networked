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
		[SerializeField] private Image progressBar;

		[Header("Color Coordination")]
		[SerializeField] private bool coorelateTipColorWithBackgoundImg;
		[SerializeField] private bool coorelateHeaderColorWithBackgoundImg;
		[SerializeField] private bool coorelateloadingBarColorWithBackgoundImg;
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

			if (coorelateTipColorWithBackgoundImg)
			{
				tipTmp.color = colors[bgRand];
			}

			if (coorelateHeaderColorWithBackgoundImg)
			{
				headerTMP.color = colors[bgRand];
			}

			if (coorelateloadingBarColorWithBackgoundImg)
			{
				progressBar.color = colors[bgRand];
			}
		}

		public virtual void ReadyToLoadNewScene() { } 

		public void SetLoadingBarAmount(float amount)
		{
			progressBar.fillAmount = amount;
		}
	}
}
