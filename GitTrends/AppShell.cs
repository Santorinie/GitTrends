using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace GitTrends
{
	class AppShell : Shell
	{
		readonly static IReadOnlyDictionary<Type, string> pageRouteMappingDictionary = new Dictionary<Type, string>(new[]
		{
			CreateRoutePageMapping<AboutPage, AboutViewModel>(),
			CreateRoutePageMapping<WelcomePage, WelcomeViewModel>(),
			CreateRoutePageMapping<SettingsPage, SettingsViewModel>(),
			CreateRoutePageMapping<RepositoryPage, RepositoryViewModel>(),
			CreateRoutePageMapping<SplashScreenPage, SplashScreenViewModel>(),
			CreateRoutePageMapping<ReferringSitesPage, ReferringSitesViewModel>(),
			CreateRouteCarouselPageMapping<TrendsCarouselPage, TrendsViewModel>(),
			CreateRouteCarouselPageMapping<OnboardingCarouselPage, OnboardingViewModel>(),
		});

		public AppShell(SplashScreenPage splashScreenPage)
		{
			Items.Add(splashScreenPage);
		}

		public static string GetRoute<TPage, TViewModel>() where TPage : BaseContentPage<TViewModel>
															where TViewModel : BaseViewModel
		{
			if (!pageRouteMappingDictionary.TryGetValue(typeof(TPage), out var route))
			{
				throw new KeyNotFoundException($"No map for ${typeof(TPage)} was found on navigation mappings. Please register your ViewModel in {nameof(AppShell)}.{nameof(pageRouteMappingDictionary)}");
			}

			return route;
		}

		static KeyValuePair<Type, string> CreateRoutePageMapping<TPage, TViewModel>() where TPage : BaseContentPage<TViewModel>
																						where TViewModel : BaseViewModel
		{
			var route = CreateRoute<TPage>();
			Routing.RegisterRoute(route, typeof(TPage));

			return new KeyValuePair<Type, string>(typeof(TPage), route);
		}

		static KeyValuePair<Type, string> CreateRouteCarouselPageMapping<TCarouselPage, TViewModel>() where TCarouselPage : BaseCarouselPage<TViewModel>
																										where TViewModel : BaseViewModel
		{
			var route = CreateRoute<TCarouselPage>();
			Routing.RegisterRoute(route, typeof(TCarouselPage));

			return new KeyValuePair<Type, string>(typeof(TCarouselPage), route);
		}

		static string CreateRoute<TPage>() where TPage : Page
		{
			if (typeof(TPage) == typeof(SplashScreenPage))
				return $"//{nameof(SplashScreenPage)}";

			if (typeof(TPage) == typeof(RepositoryPage))
				return $"//{nameof(RepositoryPage)}";

			if (typeof(TPage) == typeof(SettingsPage))
				return $"//{nameof(RepositoryPage)}/{nameof(SettingsPage)}";

			if (typeof(TPage) == typeof(AboutPage))
				return $"//{nameof(RepositoryPage)}/{nameof(SettingsPage)}/{nameof(AboutPage)}";

			if (typeof(TPage) == typeof(WelcomePage))
				return $"//{nameof(RepositoryPage)}/{nameof(WelcomePage)}";

			if (typeof(TPage) == typeof(OnboardingCarouselPage))
				return $"//{nameof(RepositoryPage)}/{nameof(OnboardingCarouselPage)}";

			if (typeof(TPage) == typeof(TrendsCarouselPage))
				return $"//{nameof(RepositoryPage)}/{nameof(TrendsCarouselPage)}";

			if (typeof(TPage) == typeof(ReferringSitesPage))
				return $"//{nameof(RepositoryPage)}/{nameof(TrendsCarouselPage)}/{nameof(ReferringSitesPage)}";

			throw new NotSupportedException($"{typeof(TPage)} Not Implemented in {nameof(pageRouteMappingDictionary)}");
		}
	}
}