using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HunterGoodin.SceneBridge
{
	public class SceneBridgeLoader : MonoBehaviour
	{
		// Singleton Stuff 
		public static SceneBridgeLoader Instance => instance;
		private static SceneBridgeLoader instance;
		[SerializeField] private bool addToDontDestroyOnLoad = true;

		// Scene Loader Stuff 
		public enum LoadingScreenType
		{
			Automatic = 0, 
			UIGated = 1, 
			InputSystemGated = 2, 
			InputManagerGated = 3
		}

		[SerializeField] private GameObject[] transitionCanvases;
		[SerializeField] private GameObject[] loadingScreenCanvases;
		[SerializeField] private GameObject chosenLoadingScreenCanvas;
		private AsyncOperation newScene = null;
		private string newSceneName = null;
		[SerializeField] private float animationDuration; 
		[SerializeField] private float transitionMidPointDuration;
		private bool loadIntoNewSceenAllowed = false; 

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

			if (addToDontDestroyOnLoad)
			{
				DontDestroyOnLoad(gameObject);
			}
		}

		public void ChangeLoadingScreenType(LoadingScreenType loadingScreenType)
		{
			chosenLoadingScreenCanvas = loadingScreenCanvases[(int)loadingScreenType];
		}

		public void ChangeTransitionAnimationDuration(float duration)
		{
			animationDuration = duration; 
		}

		public void LoadSceneAsynchronouslyWithLoadingScreenAndTransition(string sceneName, int transitionInIndexFirst, int transitionOutIndexFirst, int transitionInIndexSecond, int transitionOutIndexSecond)
		{
			newSceneName = sceneName;
			StartCoroutine(LoadSceneAsynchronouslyWithLoadingScreenAndTransitionCo(transitionInIndexFirst, transitionOutIndexFirst, transitionInIndexSecond, transitionOutIndexSecond)); 
		}

		private IEnumerator LoadSceneAsynchronouslyWithLoadingScreenAndTransitionCo(int transitionInIndexFirst, int transitionOutIndexFirst, int transitionInIndexSecond, int transitionOutIndexSecond)
		{
			// Start loading the scene 
			newScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
			newScene.allowSceneActivation = false;

			// Play transition in animation 
			transitionCanvases[transitionInIndexFirst].GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvases[transitionInIndexFirst].GetComponent<Animator>().speed = (1.0f / animationDuration);
			yield return new WaitForSeconds(animationDuration);

			// Loading Screen / wait for transition out animation 
			chosenLoadingScreenCanvas.SetActive(true);
			yield return new WaitForSeconds(transitionMidPointDuration);

			// Play transition out animation 
			if (transitionOutIndexFirst != transitionInIndexFirst)
			{
				transitionCanvases[transitionInIndexFirst].GetComponent<Animator>().SetTrigger("reset");
			}

			transitionCanvases[transitionOutIndexFirst].GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvases[transitionOutIndexFirst].GetComponent<Animator>().speed = (1.0f / animationDuration);
			yield return new WaitForSeconds(animationDuration);

			// Loading screen display 
			LoadingScreen loadingScreen = chosenLoadingScreenCanvas.GetComponent<LoadingScreen>(); 
			do
			{
				loadingScreen.SetLoadingBarAmount(newScene.progress); 
			}
			while (newScene.progress < 0.9f);

			yield return new WaitForEndOfFrame();
			loadingScreen.SetLoadingBarAmount(1.0f);

			// Allow scene switching 
			loadingScreen.ReadyToLoadNewScene();
			Debug.Log("New Scene ready!");

			// Wait until the loading screen says we can progress 
			while (!loadIntoNewSceenAllowed)
			{
				yield return new WaitForEndOfFrame(); 
			}

			// Play transition in animation 
			transitionCanvases[transitionInIndexSecond].GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvases[transitionInIndexSecond].GetComponent<Animator>().speed = (1.0f / animationDuration);
			yield return new WaitForSeconds(animationDuration + 0.1f);

			// Let new scene go 
			newScene.allowSceneActivation = true;

			// Wait until scene is finished 
			while (!newScene.isDone)
			{
				yield return new WaitForEndOfFrame();
			}

			// Unload old scene 
			Scene curScene = SceneManager.GetActiveScene();
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));
			SceneManager.UnloadSceneAsync(curScene);

			// Loading Screen / wait for transition out animation 
			loadingScreen.SetLoadingBarAmount(0.0f);
			chosenLoadingScreenCanvas.SetActive(false);
			yield return new WaitForSeconds(transitionMidPointDuration);

			// Play transition out animation 
			if (transitionOutIndexSecond != transitionInIndexSecond)
			{
				transitionCanvases[transitionInIndexSecond].GetComponent<Animator>().SetTrigger("reset");
			}

			transitionCanvases[transitionOutIndexSecond].GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvases[transitionOutIndexSecond].GetComponent<Animator>().speed = (1.0f / animationDuration);
			yield return new WaitForSeconds(animationDuration);

			// Reset 
			loadIntoNewSceenAllowed = false; 
		}

		public void LoadSceneAsynchronouslyWithTransitionOnly(string sceneName, int transitionInIndex, int transitionOutIndex)
		{
			newSceneName = sceneName;
			StartCoroutine(LoadSceneAsynchronouslyWithTransitionCo(transitionInIndex, transitionOutIndex));
		}

		private IEnumerator LoadSceneAsynchronouslyWithTransitionCo(int transitionInIndex, int transitionOutIndex)
		{
			// Start loading the scene 
			newScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
			newScene.allowSceneActivation = false;
			
			// Play transition in animation 
			transitionCanvases[transitionInIndex].GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvases[transitionInIndex].GetComponent<Animator>().speed = (1.0f / animationDuration);
			
			yield return new WaitForSeconds(animationDuration);
			
			// Let new scene go 
			newScene.allowSceneActivation = true;
			
			// Wait until scene is finished 
			while (!newScene.isDone)
			{
				yield return new WaitForEndOfFrame();
			}
			
			// Unload old scene 
			Scene curScene = SceneManager.GetActiveScene();
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));
			SceneManager.UnloadSceneAsync(curScene);
			
			// Wait for transition out animation 
			yield return new WaitForSeconds(transitionMidPointDuration);
			
			// Play transition out animation 
			transitionCanvases[transitionOutIndex].GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvases[transitionOutIndex].GetComponent<Animator>().speed = (1.0f / animationDuration);
			
			yield return new WaitForSeconds(animationDuration);
			
			// Reset 
			loadIntoNewSceenAllowed = false;
		}

		public void LoadSceneAsynchronouslyWithLoadingScreenOnly(string sceneName)
		{
			newSceneName = sceneName;
			StartCoroutine(LoadSceneAsynchronouslyWithLoadingScreenCo()); 
		}

		private IEnumerator LoadSceneAsynchronouslyWithLoadingScreenCo()
		{
			// Activate loading screen 
			chosenLoadingScreenCanvas.SetActive(true);

			// Start loading the scene 
			newScene = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
			newScene.allowSceneActivation = false;

			// Loading screen display 
			LoadingScreen loadingScreen = chosenLoadingScreenCanvas.GetComponent<LoadingScreen>();
			do
			{
				loadingScreen.SetLoadingBarAmount(newScene.progress);
			}
			while (newScene.progress < 0.9f);

			yield return new WaitForEndOfFrame();
			loadingScreen.SetLoadingBarAmount(1.0f);

			// Allow scene switching 
			loadingScreen.ReadyToLoadNewScene();
			Debug.Log("New Scene ready!");

			// Wait until the loading screen says we can progress 
			while (!loadIntoNewSceenAllowed)
			{
				yield return new WaitForEndOfFrame();
			}

			// Let new scene go 
			newScene.allowSceneActivation = true;

			// Wait until scene is finished 
			while (!newScene.isDone)
			{
				yield return new WaitForEndOfFrame();
			}

			// Unload old scene 
			Scene curScene = SceneManager.GetActiveScene();
			SceneManager.SetActiveScene(SceneManager.GetSceneByName(newSceneName));
			SceneManager.UnloadSceneAsync(curScene);

			// Deactivate loading screen 
			loadingScreen.SetLoadingBarAmount(0.0f);
			chosenLoadingScreenCanvas.SetActive(false);

			// Reset 
			loadIntoNewSceenAllowed = false;
		}

		public void ContinueToNewScene()
		{
			loadIntoNewSceenAllowed = true;
		}
	}
}