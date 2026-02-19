using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HunterGoodin.SceneBridge
{
	public class LoadingScreen : MonoBehaviour
	{
		[Header("Scene References")]
		[SerializeField] private Image backgroundImg;
		[SerializeField] private TextMeshProUGUI tmp;

		[Header("Values to set")]
		[SerializeField] private Sprite[] backgroundSprites;
		[SerializeField] private string[] tips;

		private void OnEnable()
		{
			backgroundImg.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Length)];
			tmp.text = tips[Random.Range(0, tips.Length)]; 
		}

		public virtual void ReadyToLoadNewScene()
		{
			SceneBridgeLoader.Instance.ContinueToNewScene(); 
		}
	}
}
