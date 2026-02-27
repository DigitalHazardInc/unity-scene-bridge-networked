using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace HunterGoodin.SceneBridge
{
	public class SceneBridgeLoader : NetworkBehaviour
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

		private bool transitionInProgress = false;
		private bool useLoadingScreen = false;
		private bool useTransition = false;
		private LoadingScreen loadingScreenRef = null;

		private int transInFirst;
		private int transOutFirst;
		private int transInSecond;
		private int transOutSecond;
		private string oldSceneName;

		private void Awake()
		{
			// Singleton Stuff 
			/*
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
			}*/
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (instance != null)
			{
				Destroy(gameObject);
			}
			else
			{
				instance = this;
				NetworkManager.SceneManager.OnSceneEvent += HandleSceneEvent;

				if (addToDontDestroyOnLoad)
				{
					DontDestroyOnLoad(gameObject);
				}
			}
		}

		private void SetBridgeParams(bool loadingScreen, bool transition, int transitionInIndexFirst, int transitionOutIndexFirst, int transitionInIndexSecond, int transitionOutIndexSecond)
		{
			useLoadingScreen = loadingScreen;
			useTransition = transition;

			transInFirst = transitionInIndexFirst;
			transOutFirst = transitionOutIndexFirst;
			transInSecond = transitionInIndexSecond;
			transOutSecond = transitionOutIndexSecond;

			if (chosenLoadingScreenCanvas != null)
			{
				loadingScreenRef = chosenLoadingScreenCanvas.GetComponent<LoadingScreen>();
			}

			oldSceneName = SceneManager.GetActiveScene().name;
		}

		[ServerRpc]
		public async void StartSceneChangeWithTransitionServerRpc(string sceneName, int transitionInIndex, int transitionOutIndex)
		{

			SetBridgeParams(false, true, transitionInIndex, transitionOutIndex, -1, -1);

			await StartSceneTransitionClientRpc(false, true, transitionInIndex, transitionOutIndex, -1, -1);

			NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
		}

		[ServerRpc]
		public async void StartSceneChangeWithLoadingScreenServerRpc(string sceneName)
		{

			SetBridgeParams(true, false, -1, -1, -1, -1);

			await StartSceneTransitionClientRpc(true, false, -1, -1, -1, -1);

		}

		[ServerRpc]
		public async void StartSceneChangeWithLoadingScreenAndTransitionServerRpc(string sceneName, int transitionInIndexFirst, int transitionOutIndexFirst, int transitionInIndexSecond, int transitionOutIndexSecond)
		{

			SetBridgeParams(true, true, transitionInIndexFirst, transitionOutIndexFirst, transitionInIndexSecond, transitionOutIndexSecond);

			await StartSceneTransitionClientRpc(true, true, transitionInIndexFirst, transitionOutIndexFirst, transitionInIndexSecond, transitionOutIndexSecond);
		}

		[ClientRpc]
		public async Awaitable StartSceneTransitionClientRpc(bool loadingScreen, bool transition, int transitionInIndexFirst, int transitionOutIndexFirst, int transitionInIndexSecond, int transitionOutIndexSecond)
		{
			if (!NetworkManager.Singleton.IsHost)
			{
				SetBridgeParams(loadingScreen, transition, transitionInIndexFirst, transitionOutIndexFirst, transitionInIndexSecond, transitionOutIndexSecond);
			}

			if (useTransition)
			{
				transitionCanvases[transInFirst].GetComponent<Animator>().SetTrigger("playTransitionIn");
				transitionCanvases[transInFirst].GetComponent<Animator>().speed = (1.0f / animationDuration);
				transitionCanvases[transInFirst].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
				await Awaitable.WaitForSecondsAsync(animationDuration + 0.1f);

				if (useLoadingScreen)
				{
					await Awaitable.WaitForSecondsAsync(transitionMidPointMinDuration);
				}

				if (logSceneAsyncOperations)
				{
					Debug.Log("Loading new scene...");
				}
			}

			if (useLoadingScreen)
			{
				loadingScreenRef.UpdateProgressMessage(loadingNewSceneStr);
				chosenLoadingScreenCanvas.SetActive(true);
				loadingScreenRef.SetLoadingBarAmount(0f);

				if(useTransition)
				{
					if (transOutFirst != transInFirst)
					{
						transitionCanvases[transInFirst].GetComponent<Animator>().SetTrigger("reset");
					}

					transitionCanvases[transOutFirst].GetComponent<Animator>().SetTrigger("playTransitionOut");
					transitionCanvases[transOutFirst].GetComponent<Animator>().speed = (1.0f / animationDuration);
					transitionCanvases[transOutFirst].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
					await Awaitable.WaitForSecondsAsync(animationDuration + 0.1f);
				}
			}

		}

		public async void FinishSceneLoadTransition()
		{
			await RunGarbageCollection();

			if (useLoadingScreen)
			{
				// Allow scene switching 
				loadingScreenRef.UpdateProgressMessage("");
				loadingScreenRef.SetLoadingBarAmount(1.0f);
				loadingScreenRef.ReadyToLoadNewScene();

				// Wait until the loading screen says we can progress 
				do
				{
					await Awaitable.NextFrameAsync();
				}
				while (!loadIntoNewSceenAllowed);

				if(useTransition)
				{
					// Play transition in animation 
					transitionCanvases[transInSecond].GetComponent<Animator>().SetTrigger("playTransitionIn");
					transitionCanvases[transInSecond].GetComponent<Animator>().speed = (1.0f / animationDuration);
					transitionCanvases[transInSecond].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
					await Awaitable.WaitForSecondsAsync(animationDuration + 0.1f);

					await Awaitable.WaitForSecondsAsync(transitionMidPointMinDuration);

					// Deactivate loading screen 
					loadingScreenRef.SetLoadingBarAmount(0.0f);
					chosenLoadingScreenCanvas.SetActive(false);

					// Play transition out animation 
					if (transOutSecond != transInSecond)
					{
						transitionCanvases[transInSecond].GetComponent<Animator>().SetTrigger("reset");
					}

					transitionCanvases[transOutSecond].GetComponent<Animator>().SetTrigger("playTransitionOut");
					transitionCanvases[transOutSecond].GetComponent<Animator>().speed = (1.0f / animationDuration);
					transitionCanvases[transOutSecond].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
					Time.timeScale = 1f;

					await Awaitable.WaitForSecondsAsync(animationDuration + 0.1f);
				}

			}
			else
			{
				// Let's still to the mid point wait 
				await Awaitable.WaitForSecondsAsync(transitionMidPointMinDuration);

				// Play transition out animation 
				if (transOutFirst != transInFirst)
				{
					transitionCanvases[transInFirst].GetComponent<Animator>().SetTrigger("reset");
				}

				transitionCanvases[transOutFirst].GetComponent<Animator>().SetTrigger("playTransitionOut");
				transitionCanvases[transOutFirst].GetComponent<Animator>().speed = (1.0f / animationDuration);
				transitionCanvases[transOutFirst].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
				Time.timeScale = 1f;

				await Awaitable.WaitForSecondsAsync(animationDuration + 0.1f);
			}

			loadIntoNewSceenAllowed = false;
			newSceneName = null;
			isLoading = false;
		}


		public async void HandleSceneEvent(SceneEvent sceneEvent)
		{
			if (NetworkManager.Singleton.LocalClientId != sceneEvent.ClientId)
				return;

			switch (sceneEvent.SceneEventType)
			{
				case SceneEventType.Load:
					sceneEvent.AsyncOperation.priority = (int)ThreadPriority.High;
					if (useLoadingScreen)
					{
						float displayed = 0f;
						do
						{
							float target = sceneEvent.AsyncOperation.progress * 0.333f;
							displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
							loadingScreenRef.SetLoadingBarAmount(displayed);
							await Awaitable.NextFrameAsync();

						} while (!sceneEvent.AsyncOperation.isDone);
					}
					break;
				case SceneEventType.LoadComplete:
					if (logSceneAsyncOperations)
					{
						Debug.Log("...new scene loaded");
					}

					Time.timeScale = 0f;


					if (NetworkManager.Singleton.IsHost)
					{
						Scene newScene = SceneManager.GetSceneByName(newSceneName);

						if (!newScene.IsValid() || !newScene.isLoaded)
						{
							Debug.LogError("Scene failed to load before activation.");
							return;
						}

						NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(oldSceneName));

					}
					break;
				case SceneEventType.Unload:
					if (useLoadingScreen)
					{
						loadingScreenRef.UpdateProgressMessage(unloadingOldSceneStr);
					}

					if (logSceneAsyncOperations)
					{
						Debug.Log("Unloading old scene...");
					}

					if (useLoadingScreen)
					{
						float displayed = 0.333f;
						do
						{
							float target = 0.333f + (sceneEvent.AsyncOperation.progress * 0.333f);
							displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
							loadingScreenRef.SetLoadingBarAmount(displayed);
							await Awaitable.NextFrameAsync();

						} while (!sceneEvent.AsyncOperation.isDone);
					}

					break;
				case SceneEventType.UnloadComplete:
					if (logSceneAsyncOperations)
					{
						Debug.Log("...old scene unloaded");
					}
					FinishSceneLoadTransition();
					break;
			}
		}

		public async Awaitable RunGarbageCollection()
		{
			if (logSceneAsyncOperations)
			{
				Debug.Log("Garbage collection...");
			}

			long memoryBefore = System.GC.GetTotalMemory(false);

			if (logCleanup)
			{
				Debug.Log($"Memory before cleanup: {memoryBefore / (1024f * 1024f):0.00} MB");
			}

			loadingScreenRef.UpdateProgressMessage(garbageCollectionStr);
			AsyncOperation gcOp = Resources.UnloadUnusedAssets();
			gcOp.priority = (int)ThreadPriority.High;
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			System.GC.Collect();

			float displayed = 0.666f;
			do
			{
				if (useLoadingScreen)
				{
					float target = 0.666f + (gcOp.progress * 0.333f);
					displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime);
					loadingScreenRef.SetLoadingBarAmount(displayed);
				}
				await Awaitable.NextFrameAsync();
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