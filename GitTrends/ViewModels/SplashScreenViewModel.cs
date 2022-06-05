using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public class SplashScreenViewModel : BaseViewModel
	{
		public SplashScreenViewModel(IMainThread mainThread, IAnalyticsService analyticsService)
			: base(analyticsService, mainThread)
		{
		}
	}
}

