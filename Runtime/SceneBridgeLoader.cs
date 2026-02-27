using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HunterGoodin.SceneBridge
{
	public class SceneBridgeLoader : MonoBehaviour
	{
		public enum LoadingScreenType
		{
			Automatic = 0,
			UIGated = 1,
			InputSystemGated = 2,
			InputManagerGated = 3
		}

		[Header("Singleton Stuff")]
		public static SceneBridgeLoader Instance => instance;
		private static SceneBridgeLoader instance;
		[SerializeField] private bool addToDontDestroyOnLoad = true;

		[Header("Scene Loader Stuff")]
		private bool isLoading = false; 
		// Scene references 
		[SerializeField] private GameObject[] transitionCanvases;
		[SerializeField] private GameObject[] loadingScreenCanvases;
		[SerializeField] private GameObject chosenLoadingScreenCanvas;
		[SerializeField] private float animationDuration; 
		[SerializeField] private float transitionMidPointMinDuration;
		private string newSceneName = null;
		private bool loadIntoNewSceenAllowed = false;
		// Progress messages 
		[SerializeField] private string loadingNewSceneStr; 
		[SerializeField] private string unloadingOldSceneStr;
		[SerializeField] private string garbageCollectionStr;
		// Logging 
		[SerializeField] private bool logSceneAsyncOperations; 
		[SerializeField] private bool logCleanup;

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
			if (isLoading)
			{
				Debug.LogError($"Scene load ({newSceneName}) already in progress.");
				return;
			}

			isLoading = true;

			newSceneName = sceneName;
			StartCoroutine(LoadSceneAsynchronouslyWithLoadingScreenAndTransitionCo(transitionInIndexFirst, transitionOutIndexFirst, transitionInIndexSecond, transitionOutIndexSecond)); 
		}

		private IEnumerator LoadSceneAsynchronouslyWithLoadingScreenAndTransitionCo(int transitionInIndexFirst, int transitionOutIndexFirst, int transitionInIndexSecond, int transitionOutIndexSecond)
		{
			LoadingScreen loadingScreen = chosenLoadingScreenCanvas.GetComponent<LoadingScreen>();

			// Play transition in animation 
			transitionCanvases[transitionInIndexFirst].GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvases[transitionInIndexFirst].GetComponent<Animator>().speed = (1.0f / animationDuration);
			transitionCanvases[transitionInIndexFirst].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
			yield return new WaitForSecondsRealtime(animationDuration + 0.1f);

			yield return new WaitForSecondsRealtime(transitionMidPointMinDuration);

			// Activate loading screen 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Loading new scene...");
			}

			loadingScreen.UpdateProgressMessage(loadingNewSceneStr);
			chosenLoadingScreenCanvas.SetActive(true);
			loadingScreen.SetLoadingBarAmount(0f);

			// Play transition out animation 
			if (transitionOutIndexFirst != transitionInIndexFirst)
			{
				transitionCanvases[transitionInIndexFirst].GetComponent<Animator>().SetTrigger("reset");
			}

			transitionCanvases[transitionOutIndexFirst].GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvases[transitionOutIndexFirst].GetComponent<Animator>().speed = (1.0f / animationDuration);
			transitionCanvases[transitionOutIndexFirst].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
			yield return new WaitForSecondsRealtime(animationDuration + 0.1f);

			// Load new scene 
			AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
			loadOp.priority = (int)ThreadPriority.High;
			loadOp.allowSceneActivation = true;

			float displayed = 0f;
			do
			{
				float target = loadOp.progress * 0.333f;
				displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
				loadingScreen.SetLoadingBarAmount(displayed);
				yield return null;
			}
			while (!loadOp.isDone);

			if (logSceneAsyncOperations)
			{
				Debug.Log("...new scene loaded");
			}

			// Unload old scene 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Unloading old scene...");
			}

			Time.timeScale = 0f;
			loadingScreen.UpdateProgressMessage(unloadingOldSceneStr);
			Scene oldScene = SceneManager.GetActiveScene();
			Scene newScene = SceneManager.GetSceneByName(newSceneName);

			if (!newScene.IsValid() || !newScene.isLoaded)
			{
				Debug.LogError("Scene failed to load before activation.");
				yield break;
			}

			SceneManager.SetActiveScene(newScene);
			AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldScene);
			unloadOp.priority = (int)ThreadPriority.High;

			displayed = 0.333f;
			do
			{
				float target = 0.333f + (unloadOp.progress * 0.333f);
				displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
				loadingScreen.SetLoadingBarAmount(displayed);
				yield return null;
			}
			while (!unloadOp.isDone);

			if (logSceneAsyncOperations)
			{
				Debug.Log("...old scene unloaded");
			}

			// Garbage collection 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Garbage collection...");
			}

			long memoryBefore = System.GC.GetTotalMemory(false);

			if (logCleanup)
			{
				Debug.Log($"Memory before cleanup: {memoryBefore / (1024f * 1024f):0.00} MB");
			}
			loadingScreen.UpdateProgressMessage(garbageCollectionStr);
			AsyncOperation gcOp = Resources.UnloadUnusedAssets();
			gcOp.priority = (int)ThreadPriority.High;
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			System.GC.Collect();

			displayed = 0.666f;
			do
			{
				float target = 0.666f + (gcOp.progress * 0.333f);
				displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
				loadingScreen.SetLoadingBarAmount(displayed);
				yield return null;
			}
			while (!gcOp.isDone);

			if (logCleanup)
			{
				long memoryAfter = System.GC.GetTotalMemory(true);
				Debug.Log($"Memory after cleanup: {memoryAfter / (1024f * 1024f):0.00} MB");
				Debug.Log($"Memory freed: {(memoryBefore - memoryAfter) / (1024f * 1024f):0.00} MB");
			}

			if (logSceneAsyncOperations)
			{
				Debug.Log("...garbage collection");
			}

			// Allow scene switching 
			loadingScreen.UpdateProgressMessage("");
			loadingScreen.SetLoadingBarAmount(1.0f);
			loadingScreen.ReadyToLoadNewScene();

			// Wait until the loading screen says we can progress 
			do
			{
				yield return null;
			}
			while (!loadIntoNewSceenAllowed);

			// Play transition in animation 
			transitionCanvases[transitionInIndexSecond].GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvases[transitionInIndexSecond].GetComponent<Animator>().speed = (1.0f / animationDuration);
			transitionCanvases[transitionInIndexSecond].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
			yield return new WaitForSecondsRealtime(animationDuration + 0.1f);

			yield return new WaitForSecondsRealtime(transitionMidPointMinDuration);

			// Deactivate loading screen 
			loadingScreen.SetLoadingBarAmount(0.0f);
			chosenLoadingScreenCanvas.SetActive(false);

			// Play transition out animation 
			if (transitionOutIndexSecond != transitionInIndexSecond)
			{
				transitionCanvases[transitionInIndexSecond].GetComponent<Animator>().SetTrigger("reset");
			}

			transitionCanvases[transitionOutIndexSecond].GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvases[transitionOutIndexSecond].GetComponent<Animator>().speed = (1.0f / animationDuration);
			transitionCanvases[transitionOutIndexSecond].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
			Time.timeScale = 1f;

			yield return new WaitForSecondsRealtime(animationDuration + 0.1f);

			// Reset 
			loadIntoNewSceenAllowed = false;
			newSceneName = null;
			isLoading = false;
		}

		public void LoadSceneAsynchronouslyWithTransitionOnly(string sceneName, int transitionInIndex, int transitionOutIndex)
		{
			if (isLoading)
			{
				Debug.LogError($"Scene load ({newSceneName}) already in progress.");
				return;
			}

			isLoading = true;

			newSceneName = sceneName;
			StartCoroutine(LoadSceneAsynchronouslyWithTransitionCo(transitionInIndex, transitionOutIndex));
		}

		private IEnumerator LoadSceneAsynchronouslyWithTransitionCo(int transitionInIndex, int transitionOutIndex)
		{
			// Play transition in animation 
			transitionCanvases[transitionInIndex].GetComponent<Animator>().SetTrigger("playTransitionIn");
			transitionCanvases[transitionInIndex].GetComponent<Animator>().speed = (1.0f / animationDuration);
			transitionCanvases[transitionInIndex].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
			yield return new WaitForSecondsRealtime(animationDuration + 0.1f);

			// Load new scene 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Loading new scene...");
			}

			AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
			loadOp.priority = (int)ThreadPriority.High;
			loadOp.allowSceneActivation = true;

			do
			{
				yield return null;
			}
			while (!loadOp.isDone);

			if (logSceneAsyncOperations)
			{
				Debug.Log("...new scene loaded");
			}

			// Unload old scene 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Unloading old scene...");
			}

			Time.timeScale = 0f;
			Scene oldScene = SceneManager.GetActiveScene();
			Scene newScene = SceneManager.GetSceneByName(newSceneName);

			if (!newScene.IsValid() || !newScene.isLoaded)
			{
				Debug.LogError("Scene failed to load before activation.");
				yield break;
			}

			SceneManager.SetActiveScene(newScene);
			AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldScene);
			unloadOp.priority = (int)ThreadPriority.High;

			if (logSceneAsyncOperations)
			{
				Debug.Log("...old scene unloaded");
			}

			// Garbage collection 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Garbage collection...");
			}

			long memoryBefore = System.GC.GetTotalMemory(false);

			if (logCleanup)
			{
				Debug.Log($"Memory before cleanup: {memoryBefore / (1024f * 1024f):0.00} MB");
			}

			AsyncOperation gcOp = Resources.UnloadUnusedAssets();
			gcOp.priority = (int)ThreadPriority.High;
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			System.GC.Collect();

			do
			{
				yield return null;
			}
			while (!gcOp.isDone);

			if (logCleanup)
			{
				long memoryAfter = System.GC.GetTotalMemory(true);
				Debug.Log($"Memory after cleanup: {memoryAfter / (1024f * 1024f):0.00} MB");
				Debug.Log($"Memory freed: {(memoryBefore - memoryAfter) / (1024f * 1024f):0.00} MB");
			}

			if (logSceneAsyncOperations)
			{
				Debug.Log("...garbage collected");
			}

			// Let's still to the mid point wait 
			yield return new WaitForSecondsRealtime(transitionMidPointMinDuration);

			// Play transition out animation 
			if (transitionOutIndex != transitionInIndex)
			{
				transitionCanvases[transitionInIndex].GetComponent<Animator>().SetTrigger("reset");
			}

			transitionCanvases[transitionOutIndex].GetComponent<Animator>().SetTrigger("playTransitionOut");
			transitionCanvases[transitionOutIndex].GetComponent<Animator>().speed = (1.0f / animationDuration);
			transitionCanvases[transitionOutIndex].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
			Time.timeScale = 1f;

			yield return new WaitForSecondsRealtime(animationDuration + 0.1f);

			// Reset 
			loadIntoNewSceenAllowed = false;
			newSceneName = null;
			isLoading = false;
		}

		public void LoadSceneAsynchronouslyWithLoadingScreenOnly(string sceneName)
		{
			if (isLoading)
			{
				Debug.LogError($"Scene load ({newSceneName}) already in progress."); 
				return; 
			}

			isLoading = true; 

			newSceneName = sceneName;
			StartCoroutine(LoadSceneAsynchronouslyWithLoadingScreenCo()); 
		}

		private IEnumerator LoadSceneAsynchronouslyWithLoadingScreenCo()
		{
			LoadingScreen loadingScreen = chosenLoadingScreenCanvas.GetComponent<LoadingScreen>();

			// Activate loading screen 
			loadingScreen.UpdateProgressMessage(loadingNewSceneStr); 
			chosenLoadingScreenCanvas.SetActive(true);
			loadingScreen.SetLoadingBarAmount(0f);

			// Load new scene 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Loading new scene...");
			}

			AsyncOperation loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
			loadOp.priority = (int)ThreadPriority.High; 
			loadOp.allowSceneActivation = true;

			float displayed = 0f;
			do
			{
				float target = loadOp.progress * 0.333f;
				displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
				loadingScreen.SetLoadingBarAmount(displayed);
				yield return null;
			}
			while (!loadOp.isDone);

			if (logSceneAsyncOperations)
			{
				Debug.Log("...new scene loaded");
			}

			// Unload old scene 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Unloading old scene...");
			}

			Time.timeScale = 0f;
			loadingScreen.UpdateProgressMessage(unloadingOldSceneStr);
			Scene oldScene = SceneManager.GetActiveScene();
			Scene newScene = SceneManager.GetSceneByName(newSceneName);

			if (!newScene.IsValid() || !newScene.isLoaded)
			{
				Debug.LogError("Scene failed to load before activation.");
				yield break; 
			}

			SceneManager.SetActiveScene(newScene);
			AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(oldScene);
			unloadOp.priority = (int)ThreadPriority.High;

			displayed = 0.333f;
			do
			{
				float target = 0.333f + (unloadOp.progress * 0.333f);
				displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
				loadingScreen.SetLoadingBarAmount(displayed);
				yield return null;
			}
			while (!unloadOp.isDone);

			if (logSceneAsyncOperations)
			{
				Debug.Log("...old scene unloaded");
			}

			// Garbage collection 
			if (logSceneAsyncOperations)
			{
				Debug.Log("Garbage collection...");
			}

			long memoryBefore = System.GC.GetTotalMemory(false);

			if (logCleanup)
			{
				Debug.Log($"Memory before cleanup: {memoryBefore / (1024f * 1024f):0.00} MB");
			}
			loadingScreen.UpdateProgressMessage(garbageCollectionStr);
			AsyncOperation gcOp = Resources.UnloadUnusedAssets();
			gcOp.priority = (int)ThreadPriority.High;
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			System.GC.Collect();

			displayed = 0.666f;
			do
			{
				float target = 0.666f + (gcOp.progress * 0.333f);
				displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
				loadingScreen.SetLoadingBarAmount(displayed);
				yield return null;
			}
			while (!gcOp.isDone);

			if (logCleanup)
			{
				long memoryAfter = System.GC.GetTotalMemory(true);
				Debug.Log($"Memory after cleanup: {memoryAfter / (1024f * 1024f):0.00} MB");
				Debug.Log($"Memory freed: {(memoryBefore - memoryAfter) / (1024f * 1024f):0.00} MB");
			}

			if (logSceneAsyncOperations)
			{
				Debug.Log("...garbage collected");
			}

			// Allow scene switching 
			loadingScreen.UpdateProgressMessage("");
			loadingScreen.SetLoadingBarAmount(1.0f);
			loadingScreen.ReadyToLoadNewScene();

			// Wait until the loading screen says we can progress 
			do
			{
				yield return null; 
			}
			while (!loadIntoNewSceenAllowed);

			// Deactivate loading screen 
			loadingScreen.SetLoadingBarAmount(0.0f);
			chosenLoadingScreenCanvas.SetActive(false);
			Time.timeScale = 1f;

			// Reset 
			loadIntoNewSceenAllowed = false;
			newSceneName = null;
			isLoading = false;
		}

		public void ContinueToNewScene()
		{
			loadIntoNewSceenAllowed = true;
		}
	}
}