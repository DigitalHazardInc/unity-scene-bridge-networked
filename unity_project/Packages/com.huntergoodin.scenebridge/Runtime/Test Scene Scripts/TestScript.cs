using UnityEngine;
using UnityEngine.UI;

namespace HunterGoodin.SceneBridge
{
	public class TestScript : MonoBehaviour
	{
		[SerializeField] private string sceneName;

		[Header("Load into the new scene with transitions and a loading screen")]
		[SerializeField] private int transitionInIndexFirst;
		[SerializeField] private int transitionOutIndexFirst;
		[SerializeField] private int transitionInIndexSecond;
		[SerializeField] private int transitionOutIndexSecond;

		[Header("Load into the new scene with transitions (no loading screen)")]
		[SerializeField] private int transitionInIndex; 
		[SerializeField] private int transitionOutIndex;

		public void LoadWithLoadingScreenAndTransisions()
		{
			SceneBridgeLoader.Instance.LoadSceneAsynchronouslyWithLoadingScreenAndTransition(sceneName, transitionInIndexFirst, transitionOutIndexFirst, transitionInIndexSecond, transitionOutIndexSecond);
			DisableAllButtons(); 
		}

		public void LoadWithTransitionsOnly()
		{
			SceneBridgeLoader.Instance.LoadSceneAsynchronouslyWithTransitionOnly(sceneName, transitionInIndex, transitionOutIndex);
			DisableAllButtons(); 
		}

		public void LoadWithLoadingScreenOnly()
		{
			SceneBridgeLoader.Instance.LoadSceneAsynchronouslyWithLoadingScreenOnly(sceneName);
			DisableAllButtons(); 
		}

		public void ChangeScreen()
		{
			SceneBridgeLoader.Instance.ChangeLoadingScreenType(SceneBridgeLoader.LoadingScreenType.UIGated); 
		}

		private void DisableAllButtons()
		{
			Button[] btns = GetComponentsInChildren<Button>(); 

			for (int i = 0; i < btns.Length; i++)
			{
				btns[i].interactable = false; 
			}
		}
	}
}
