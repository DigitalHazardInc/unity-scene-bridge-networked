# SceneBridge

A simple additive scene loading system with a built-in loading screen workflow.

Loads scenes asynchronously in the background, keeps them inactive until fully ready, displays a loading canvas during the transition, and then cleanly activates the new scene while unloading the old one. Designed to provide smooth, controlled scene transitions with minimal setup.

Check out [Releases](https://github.com/Persomatey/unity-asynchronous-scene-loading-system/releases) tab to see a history of all versions of this package. 

## Installation 
### Install via Package Manager

<details>
<summary>Select `Install package from git URL...` in the Package Manager</summary>
	<img src="https://raw.githubusercontent.com/Persomatey/unity-package-ci-cd-system-template/refs/heads/main/images/git-url-installation-example.png">
</details>

For latest release:</br>
`https://github.com/DigitalHazardInc/unity-scene-bridge-networked.git#upm`</br>
Or install specific releases:</br>
`https://github.com/DigitalHazardInc/unity-scene-bridge-networked.git#v1.0.1`

### Download the tarball directly from release
<details>
<summary>Go to Release and download directly from Assets</summary>
	  <img src="https://raw.githubusercontent.com/Persomatey/unity-package-ci-cd-system-template/refs/heads/main/images/release-tab-tarball-circled.png">
</details>

`com.huntergoodin.scenebridge-v#.#.#.tgz`

`com.huntergoodin.scenebridge-v#.#.#.zip`

<details>
<summary>Select `Install package from tarball...` in the Package Manager</summary>
	  <img src="https://raw.githubusercontent.com/Persomatey/unity-package-ci-cd-system-template/refs/heads/main/images/tarball-installation-example.png">
</details>

## Set Up 
1. [Install the package](https://github.com/Persomatey/unity-scene-bridge/#installation)
2. Drag the `SceneBridge Loader` prefab into your scene
3. Feel free to create your own transition canvases

### Samples 
1. Import the sample 
2. Add the sample scenes to the Build Profile 

## Features 
- A singleton-based Scene Loader
	- Centralized loader accessible from anywhere in your project
	- Easy `SceneBridge Loader` prefab provided to drag and drop into your project 
- Custom loading screens 
	- Loading screen can display the current progress of the `AsyncOperation`
	- Loading screen can display various backgrounds with a random selection from an array in the `SceneBridge Loader` prefab
 		- (Optional) Text and image color synchronization accross background images
			- `LoadingScreen.correlateTipColorWithBackgoundImg`
			- `LoadingScreen.correlateHeaderColorWithBackgoundImg`
			- `LoadingScreen.correlateloadingBarColorWithBackgoundImg`
			- `InputSystemGatedLoadingScreen.correlateProgColorWithBackgoundImg`
			- `InputManagerGatedLoadingScreen.correlateProgColorWithBackgoundImg`
	- Loading screen can display various text snippets (for tips, lore tidbits, etc.) with a random selection from an array in the `SceneBridge Loader` prefab
 	- Five types of loading screens (more details in the [Loading Screens section](https://github.com/Persomatey/unity-scene-bridge/#loading-screens)):
		```
		└── LoadingScreen
  			├── AutomaticLoadingScreen
    		├── UIGatedLoadingScreen
    		├── InputManagerGatedLoadingScreen
    		└── InputSystemGatedLoadingScreen
		```
- The loading screen you want can be set in `SceneBridgeLoader.chosenLoadingScreen`
	- <img src="https://github.com/Persomatey/unity-scene-bridge/blob/main/images/PrefabInspectorScreenshot-ChosenLoadingScreenHighlighted.png?raw=true">
- Scene transition animation support
	- Transition animations can play into (and out of) screens
		- Ex: Play a transition animation into the loading screen, then a transition animation into the new scene.
  		- Ex: Play a transition animation when loading into a new scene directly 
	- Duration control for the animations
 		- `SceneBridgeLoader.AnimationDuration` 
		- Included animations are exactly one second to make it easy to calculate the speed of the animation given a duration
	- Mid-point duration control
 		- `SceneBridgeLoader.TransitionMidPointDuration` 
		- If you want there to be a slight pause in between the "transition in" and "transition out" animations (to hang on a black screen for a sec or something)
	- Transitions included in the `SceneBridge Loader` prefab by default: 
		- Fade
			- Fade to black / fade out of black
			- Included in the nested `Fade Transition Canvas` canvas
		- Swipe
			- Swipe to black / swipe out of black
			- Included in the nested `swipe Transition Canvas` canvas
	- It's easy to swap in/out which animations you want into the `SceneBridgeLoader.transitionCanvases` array
		- Pass which transition index you want to play by passing it as a variable to the functions outlined below 
	- Feel free to create your own transition animations!
 		- Remember to add them to the `SceneBridgeLoader.transitionCanvases` array 
		- <img src="https://github.com/Persomatey/unity-scene-bridge/blob/main/images/PrefabInspectorScreenshot-TransitionArrHighlighted.png?raw=true">
- Several scene loading functions:
	- Outlined in the [Loading Functions](https://github.com/Persomatey/unity-scene-bridge/#loading-functions) section
 	- Maybe sometimes you want the loading screen to appear, sometimes you don't 
- Ability to change loading screen shown
	- In case there are times when gating progression makes sense, and others where it doesn't
- Scene Cleanup
	– Automatically unload previous scenes after switching to the new one
- (Optional) DontDestroyOnLoad support for persistence across scenes
	- Not sure why you wouldn't want this enabled for a tool like this, but it's an option just in case 

### Loading Screens 
#### Base Classes (not meant to actually be used): 
- Loading Screen
	- `LoadingScreen.cs`
 	- Base class for all loading screens 
- Gated Loading Screen
	- `GatedLoadingScreen.cs`
 	- Base class for gated loading screens 
#### Derived classes (meant to be used): 
- Automatic Loading Screen
	- `AutomaticLoadingScreen.cs`
 	- Loads directly into the next scene automatically when the scene is ready  
	- Example Canvas provided in the `SceneBridge Loader` prefab 
- UI Gated Loading Screen
	- `UIGatedLoadingScreen.cs`
 	- Progression blocked behind a button press
    - Example Canvas provided in the `SceneBridge Loader` prefab 
- Input Manager Gated Loading Screen
	- `InputManagerGatedLoadingScreen.cs`
 	- Uses Unity's old [Input Manager](https://docs.unity3d.com/6000.3/Documentation/Manual/class-InputManager.html)
	- Progression blocked behind a `if (Input.GetKeyDown())` logic gate 
	- Example Canvas provided in the `SceneBridge Loader` prefab 
- Input System Gated Loading Screen
	- `InputSystemGatedLoadingScreen.cs`
	- Uses Unity's new [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest/)
 	- Progression blocked behind a `if (anyButton.IsPressed())` logic gate 
	- Example Canvas provided in the `SceneBridge Loader` prefab

### Loading Functions 
- Load Scene Asynchronously With Loading Screen And Transitions
	- Called using `SceneBridgeLoader.Instance.LoadSceneAsynchronouslyWithLoadingScreenAndTransition("scene_name", transitionInIndexFirst, transitionOutIndexFirst, transitionInIndexSecond, transitionOutIndexSecond)` 
	- Flow: 
		1. Play transition animation at `transitionInIndexFirst`
		2. Enable loading screen
		3. Play transition animation at `transitionOutIndexFirst`
		4. Load scene/gate logic (if any) 
		5. Play transition animation at `transitionInIndexSecond`
 		6. Disable/reset loading screen
		7. Enable new scene
 		8. Unload old scene 
		9. Play transition animation at `transitionOutIndexSecond`
	- Takes a string (`sceneName`) for the scenes name you're loading into
	- Takes an int (`transitionInIndexFirst`) for the "transition in" animation (for anim in the index of `SceneBridgeLoader.transitionCanvases`)
	- Takes an int (`transitionOutIndexFirst`) for the "transition out" animation (for anim in the index of `SceneBridgeLoader.transitionCanvases`)
	- Takes an int (`transitionInIndexSecond`) for the "transition in" animation (for anim in the index of `SceneBridgeLoader.transitionCanvases`)
	- Takes an int (`transitionOutIndexSecond`) for the "transition out" animation (for anim in the index of `SceneBridgeLoader.transitionCanvases`)
- Load Scene Asynchronously With Transitions
	- Called using `SceneBridgeLoader.Instance.LoadSceneAsynchronouslyWithTransition("scene_name", transitionInIndex, transitionOutIndex)` 
	- Flow: 
		1. Play transition animation at `transitionInIndex`
		2. Load new scene
  		3. Enable new scene
    	4. Unload old scene
		5. Play transition animation at `transitionOutIndex`
	- Takes a string (`sceneName`) for the scenes name you're loading into
	- Takes an int (`transitionInIndex`) for the "transition in" animation (for anim in the index of `SceneBridgeLoader.transitionCanvases`)
	- Takes an int (`transitionOutIndex`) for the "transition out" animation (for anim in the index of `SceneBridgeLoader.transitionCanvases`)
 - Load Scene Asynchronously With Loading Screen
	- Called using `SceneBridgeLoader.Instance.LoadSceneAsynchronouslyWithTransition("scene_name")` 
	- Flow: 
		1. Enable loading screen
  		2. Load scene/gate logic (if any)
    	3. Enable new scene
     	4. Unload old scene
		5. Disable/reset loading screen 
	- Takes a string (`sceneName`) for the scenes name you're loading into

## Future Plans 
*No plans on when I'd release these features, would likely depend on my needs for a specific project/boredom/random interest in moving this project along.*
- Minimum loading screen duration
	- Add a variable to all loading screens that has X seconds (tunable) alive to prevent loading screens from flashing too quickly on fast loads 
- Timer Conditional Loading Screen 
	- A type of derived loading screen (one for each type of screen) that stays on the mid-transition animation until X seconds (tunable) before it decides to show the loading screen.
		- Maybe inheritance isn't the best call here... maybe make this a toggleable option in all loading screen classes? Or use a design pattern like an Interface or something?
	- Another solution to the "loading screens from flashing too quickly on fast loads" problem but also one that some games might just feel better with 
