namespace HunterGoodin.SceneBridge
{
	public class AutomaticLoadingScreen : LoadingScreen
	{
		public override void ReadyToLoadNewScene()
		{
			SceneBridgeLoader.Instance.ContinueToNewScene();
		}
	}
}
