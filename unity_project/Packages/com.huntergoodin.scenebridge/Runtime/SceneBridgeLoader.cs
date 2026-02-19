using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HunterGoodin.SceneBridge
{
	public class SceneBridgeLoader : MonoBehaviour
	{
		// Singleton Stuff 
		public static SceneBridgeLoader Instance => instance;
		private static SceneBridgeLoader instance;

		// Scene Loader Stuff 
		[SerializeField] private GameObject transitionCanvas;
		[SerializeField] private GameObject loadingScreenCanvas;
		[SerializeField] private Image progressBar;
		private AsyncOperation newScene = null;
		private string newSceneName = null;
		[SerializeField] private float animationDuration; 
		[SerializeField] private float transitionMidPointDuration;

		private void Awake()
		{
			// Singleton Stuff 
			if (instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				instance = this;
			}

			DontDestroyOnLoad(gameObject); 
		}

		public void LoadSceneAsynchronously(string sceneName)
		{
			newSceneName = sceneName; 
			StartCoroutine(LoadSceneAsyncCo()); 
		}

		private IEnumerator LoadSceneAsyncCo()
		{
			// Start loading the scene 
			newScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
			newScene.allowSceneActivation = false;

			// Play transition in animation 
			transitionCanvas.GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvas.GetComponent<Animator>().speed = (1.0f / animationDuration); 
			yield return new WaitForSeconds(animationDuration);

			// Loading Screen / wait for transition out animation 
			loadingScreenCanvas.SetActive(true);
			progressBar.fillAmount = 0.0f; 
			yield return new WaitForSeconds(transitionMidPointDuration); 

			// Play transition out animation 
			transitionCanvas.GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvas.GetComponent<Animator>().speed = (1.0f / animationDuration);
			yield return new WaitForSeconds(animationDuration);

			// Loading screen display 
			do
			{
				progressBar.fillAmount = newScene.progress;
			}
			while (newScene.progress < 0.9f);

			yield return new WaitForEndOfFrame(); 
			progressBar.fillAmount = 1.0f; 

			// Allow scene switching 
			loadingScreenCanvas.GetComponent<LoadingScreen>().ReadyToLoadNewScene(); 
			Debug.Log("New Scene ready!");

			//StartCoroutine(UnloadPreviousScene()); 
		}

		private IEnumerator LoadIntoNewScene()
		{
			// Play transition in animation 
			transitionCanvas.GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvas.GetComponent<Animator>().speed = (1.0f / animationDuration);
			yield return new WaitForSeconds(animationDuration + 0.1f);

			// Let new scene go 
			newScene.allowSceneActivation = true;

			// Wait until scene is finished 
			yield return !newScene.isDone;

			// Unload old scene 
			Scene curScene = SceneManager.GetActiveScene();
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));
			SceneManager.UnloadSceneAsync(curScene);

			// Loading Screen / wait for transition out animation 
			loadingScreenCanvas.SetActive(false);
			yield return new WaitForSeconds(transitionMidPointDuration);

			// Play transition out animation 
			transitionCanvas.GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvas.GetComponent<Animator>().speed = (1.0f / animationDuration);
			yield return new WaitForSeconds(animationDuration);
		}

		public void ContinueToNewScene()
		{
			StartCoroutine(LoadIntoNewScene()); 
		}

		public void LoadSceneDirectly(string sceneName)
		{
			SceneManager.LoadScene(sceneName, LoadSceneMode.Single); 
		}
	}
}