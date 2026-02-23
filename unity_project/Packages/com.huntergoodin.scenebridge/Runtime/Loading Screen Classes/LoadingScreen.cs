using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunterGoodin.SceneBridge
{
	public class LoadingScreen : MonoBehaviour
	{
		[Header("Scene References")]
		[SerializeField] private Image backgroundImg;
		[SerializeField] private TextMeshProUGUI tipTmp;
		[SerializeField] private Image progressBar;

		[Header("Values to set")]
		[SerializeField] private Sprite[] backgroundSprites;
		[SerializeField] private string[] tips;

		private void OnEnable()
		{
			backgroundImg.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Length)];
			tipTmp.text = tips[Random.Range(0, tips.Length)]; 
		}

		public virtual void ReadyToLoadNewScene() { } 

		public void SetLoadingBarAmount(float amount)
		{
			progressBar.fillAmount = amount;
		}
	}
}
