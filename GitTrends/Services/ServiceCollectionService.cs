using System;
using System.Net.Http;
using GitHubApiStatus;
using GitTrends.Mobile.Common;
using GitTrends.Shared;
using Microsoft.Extensions.DependencyInjection;
using Plugin.StoreReview;
using Shiny;
using Shiny.Jobs;
using Shiny.Notifications;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;

namespace GitTrends
{
	public static class ServiceCollectionService
	{
		readonly static Lazy<IServiceProvider> _containerHolder = new(CreateContainer);

		public static IServiceProvider Container => _containerHolder.Value;

		static IServiceProvider CreateContainer()
		{
			var services = new ServiceCollection();

			//Register App
			services.AddSingleton<App>();
			services.AddSingleton<AppShell>();

			//Register Xamarin.Essentials
			services.AddSingleton<IAppInfo, AppInfoImplementation>();
			services.AddSingleton<IBrowser, BrowserImplementation>();
			services.AddSingleton<IDeviceInfo, DeviceInfoImplementation>();
			services.AddSingleton<IEmail, EmailImplementation>();
			services.AddSingleton<IFileSystem, FileSystemImplementation>();
			services.AddSingleton<ILauncher, LauncherImplementation>();
			services.AddSingleton<IMainThread, MainThreadImplementation>();
			services.AddSingleton<IPreferences, PreferencesImplementation>();
			services.AddSingleton<ISecureStorage, SecureStorageImplementation>();
			services.AddSingleton<IVersionTracking, VersionTrackingImplementation>();

			//Register Services
			services.AddSingleton<IAnalyticsService, AnalyticsService>();
			services.AddSingleton<AnalyticsInitializationService>();
			services.AddSingleton<AppInitializationService>();
			services.AddSingleton<AzureFunctionsApiService>();
			services.AddSingleton<BackgroundFetchService>();
			services.AddSingleton<DeepLinkingService>();
			services.AddSingleton<FavIconService>();
			services.AddSingleton<FirstRunService>();
			services.AddSingleton<GitHubApiStatusService>();
			services.AddSingleton<GitHubApiRepositoriesService>();
			services.AddSingleton<GitHubApiV3Service>();
			services.AddSingleton<GitHubAuthenticationService>();
			services.AddSingleton<GitHubUserService>();
			services.AddSingleton<GitHubGraphQLApiService>();
			services.AddSingleton<GitTrendsStatisticsService>();
			services.AddSingleton<ImageCachingService>();
			services.AddSingleton<LanguageService>();
			services.AddSingleton<LibrariesService>();
			services.AddSingleton<MediaElementService>();
			services.AddSingleton<NotificationService>();
			services.AddSingleton<ReferringSitesDatabase>();
			services.AddSingleton<RepositoryDatabase>();
			services.AddSingleton<ReviewService>();
			services.AddSingleton<MobileSortingService>();
			services.AddSingleton<SyncfusionService>();
			services.AddSingleton<ThemeService>();
			services.AddSingleton<TrendsChartSettingsService>();
			services.AddSingleton(CrossStoreReview.Current);
			services.AddSingleton(ShinyHost.Resolve<IJobManager>());
			services.AddSingleton(ShinyHost.Resolve<INotificationManager>());
			services.AddSingleton(ShinyHost.Resolve<IDeviceNotificationsService>());
#if !AppStore
			services.AddSingleton<UITestsBackdoorService>();
#endif

			//Register ViewModels
			services.AddTransient<AboutViewModel>();
			services.AddTransient<OnboardingViewModel>();
			services.AddTransient<ReferringSitesViewModel>();
			services.AddTransient<RepositoryViewModel>();
			services.AddTransient<SettingsViewModel>();
			services.AddTransient<TrendsViewModel>();
			services.AddTransient<WelcomeViewModel>();

			//Register Pages
			services.AddTransient<AboutPage>();
			services.AddTransient<ChartOnboardingPage>();
			services.AddTransient<ConnectToGitHubOnboardingPage>();
			services.AddTransient<GitTrendsOnboardingPage>();
			services.AddTransient<NotificationsOnboardingPage>();
			services.AddTransient<OnboardingCarouselPage>();
			services.AddTransient<ReferringSitesPage>();
			services.AddTransient<RepositoryPage>();
			services.AddTransient<SettingsPage>();
			services.AddTransient<SplashScreenPage>();
			services.AddTransient<StarsTrendsPage>();
			services.AddTransient<TrendsCarouselPage>();
			services.AddTransient<ViewsClonesTrendsPage>();
			services.AddTransient<WelcomePage>();

			//Register Refit Services
			IGitHubApiV3 gitHubV3ApiClient = RefitExtensions.For<IGitHubApiV3>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubRestApiUrl, new HttpClient()));
			IGitHubGraphQLApi gitHubGraphQLApiClient = RefitExtensions.For<IGitHubGraphQLApi>(BaseApiService.CreateHttpClient(GitHubConstants.GitHubGraphQLApi, new HttpClient()));
			IAzureFunctionsApi azureFunctionsApiClient = RefitExtensions.For<IAzureFunctionsApi>(BaseApiService.CreateHttpClient(AzureConstants.AzureFunctionsApiUrl, new HttpClient()));

			services.AddSingleton(gitHubV3ApiClient);
			services.AddSingleton(gitHubGraphQLApiClient);
			services.AddSingleton(azureFunctionsApiClient);

			return services.BuildServiceProvider();
		}
	}
}