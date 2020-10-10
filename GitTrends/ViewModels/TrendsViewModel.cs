﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Mobile.Common;
using GitTrends.Mobile.Common.Constants;
using GitTrends.Shared;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace GitTrends
{
    public class TrendsViewModel : BaseViewModel
    {
        public const int MinumumChartHeight = 20;

        readonly GitHubApiV3Service _gitHubApiV3Service;
        readonly GitHubGraphQLApiService _gitHubGraphQLApiService;

        bool _isFetchingData = true;
        bool _isViewsSeriesVisible, _isUniqueViewsSeriesVisible, _isClonesSeriesVisible, _isUniqueClonesSeriesVisible;

        string _emptyDataViewText = string.Empty;
        string _starsStatisticsText = string.Empty;
        string _viewsStatisticsText = string.Empty;
        string _clonesStatisticsText = string.Empty;
        string _uniqueViewsStatisticsText = string.Empty;
        string _uniqueClonesStatisticsText = string.Empty;

        IReadOnlyList<DailyStarsModel>? _dailyStarsList;
        IReadOnlyList<DailyViewsModel>? _dailyViewsList;
        IReadOnlyList<DailyClonesModel>? _dailyClonesList;

        public TrendsViewModel(IMainThread mainThread,
                                IAnalyticsService analyticsService,
                                GitHubApiV3Service gitHubApiV3Service,
                                GitHubGraphQLApiService gitHubGraphQLApiService,
                                TrendsChartSettingsService trendsChartSettingsService) : base(analyticsService, mainThread)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            _gitHubGraphQLApiService = gitHubGraphQLApiService;

            IsViewsSeriesVisible = trendsChartSettingsService.ShouldShowViewsByDefault;
            IsUniqueViewsSeriesVisible = trendsChartSettingsService.ShouldShowUniqueViewsByDefault;
            IsClonesSeriesVisible = trendsChartSettingsService.ShouldShowClonesByDefault;
            IsUniqueClonesSeriesVisible = trendsChartSettingsService.ShouldShowUniqueClonesByDefault;

            ViewsCardTappedCommand = new Command(() => IsViewsSeriesVisible = !IsViewsSeriesVisible);
            UniqueViewsCardTappedCommand = new Command(() => IsUniqueViewsSeriesVisible = !IsUniqueViewsSeriesVisible);
            ClonesCardTappedCommand = new Command(() => IsClonesSeriesVisible = !IsClonesSeriesVisible);
            UniqueClonesCardTappedCommand = new Command(() => IsUniqueClonesSeriesVisible = !IsUniqueClonesSeriesVisible);

            FetchDataCommand = new AsyncCommand<(Repository Repository, CancellationToken CancellationToken)>(tuple => ExecuteFetchDataCommand(tuple.Repository, tuple.CancellationToken));
        }

        public ICommand ViewsCardTappedCommand { get; }
        public ICommand UniqueViewsCardTappedCommand { get; }
        public ICommand ClonesCardTappedCommand { get; }
        public ICommand UniqueClonesCardTappedCommand { get; }

        public IAsyncCommand<(Repository Repository, CancellationToken CancellationToken)> FetchDataCommand { get; }

        public double DailyViewsClonesMinValue { get; } = 0;
        public double DailyStarsMinValue { get; } = 0;

        public bool IsEmptyDataViewVisible => !IsChartVisible && !IsFetchingData;
        public bool IsChartVisible => !IsFetchingData && DailyViewsList.Sum(x => x.TotalViews + x.TotalUniqueViews) + DailyClonesList.Sum(x => x.TotalClones + x.TotalUniqueClones) > 0;

        public DateTime MinViewClonesDate => DateTimeService.GetMinimumLocalDateTime(DailyViewsList, DailyClonesList);
        public DateTime MaxViewClonesDate => DateTimeService.GetMaximumLocalDateTime(DailyViewsList, DailyClonesList);

        public DateTime MaxDailyStarsDate => DailyStarsList.Any() ? DailyStarsList.Max(x => x.Day).DateTime : DateTime.Today;
        public DateTime MinDailyStarsDate => DailyStarsList.Any() ? DailyStarsList.Min(x => x.Day).DateTime : DateTime.Today.Subtract(TimeSpan.FromDays(7));

        public double DailyStarsMaxValue => DailyStarsList.Any() ? DailyStarsList.Max(x => x.TotalStars) : 0;

        public double DailyViewsClonesMaxValue
        {
            get
            {
                var dailyViewMaxValue = DailyViewsList.Any() ? DailyViewsList.Max(x => x.TotalViews) : 0;
                var dailyClonesMaxValue = DailyClonesList.Any() ? DailyClonesList.Max(x => x.TotalClones) : 0;

                return Math.Max(Math.Max(dailyViewMaxValue, dailyClonesMaxValue), MinumumChartHeight);
            }
        }

        public string EmptyDataViewTitle
        {
            get => _emptyDataViewText;
            set => SetProperty(ref _emptyDataViewText, value);
        }

        public string StarsStatisticsText
        {
            get => _starsStatisticsText;
            set => SetProperty(ref _starsStatisticsText, value);
        }

        public string ViewsStatisticsText
        {
            get => _viewsStatisticsText;
            set => SetProperty(ref _viewsStatisticsText, value);
        }

        public string UniqueViewsStatisticsText
        {
            get => _uniqueViewsStatisticsText;
            set => SetProperty(ref _uniqueViewsStatisticsText, value);
        }

        public string ClonesStatisticsText
        {
            get => _clonesStatisticsText;
            set => SetProperty(ref _clonesStatisticsText, value);
        }

        public string UniqueClonesStatisticsText
        {
            get => _uniqueClonesStatisticsText;
            set => SetProperty(ref _uniqueClonesStatisticsText, value);
        }

        public bool IsViewsSeriesVisible
        {
            get => _isViewsSeriesVisible;
            set => SetProperty(ref _isViewsSeriesVisible, value);
        }

        public bool IsUniqueViewsSeriesVisible
        {
            get => _isUniqueViewsSeriesVisible;
            set => SetProperty(ref _isUniqueViewsSeriesVisible, value);
        }

        public bool IsClonesSeriesVisible
        {
            get => _isClonesSeriesVisible;
            set => SetProperty(ref _isClonesSeriesVisible, value);
        }

        public bool IsUniqueClonesSeriesVisible
        {
            get => _isUniqueClonesSeriesVisible;
            set => SetProperty(ref _isUniqueClonesSeriesVisible, value);
        }

        public bool IsFetchingData
        {
            get => _isFetchingData;
            set => SetProperty(ref _isFetchingData, value, () =>
            {
                OnPropertyChanged(nameof(IsChartVisible));
                OnPropertyChanged(nameof(IsEmptyDataViewVisible));
            });
        }

        public IReadOnlyList<DailyViewsModel> DailyViewsList
        {
            get => _dailyViewsList ??= Array.Empty<DailyViewsModel>();
            set => SetProperty(ref _dailyViewsList, value, UpdateDailyViewsListPropertiesChanged);
        }

        public IReadOnlyList<DailyClonesModel> DailyClonesList
        {
            get => _dailyClonesList ??= Array.Empty<DailyClonesModel>();
            set => SetProperty(ref _dailyClonesList, value, UpdateDailyClonesListPropertiesChanged);
        }

        public IReadOnlyList<DailyStarsModel> DailyStarsList
        {
            get => _dailyStarsList ??= Array.Empty<DailyStarsModel>();
            set => SetProperty(ref _dailyStarsList, value, UpdateDailyStarsListPropertiesChanged);
        }

        async Task ExecuteFetchDataCommand(Repository repository, CancellationToken cancellationToken)
        {
            IReadOnlyList<DateTimeOffset> repositoryStars = Array.Empty<DateTimeOffset>();
            IReadOnlyList<DailyViewsModel> repositoryViews = Array.Empty<DailyViewsModel>();
            IReadOnlyList<DailyClonesModel> repositoryClones = Array.Empty<DailyClonesModel>();

            var minimumTimeTask = Task.Delay(TimeSpan.FromSeconds(1));

            try
            {
                if (repository.DailyClonesList.Any() && repository.DailyViewsList.Any())
                {
                    repositoryStars = repository.StarredAt;
                    repositoryViews = repository.DailyViewsList;
                    repositoryClones = repository.DailyClonesList;
                }
                else
                {
                    IsFetchingData = true;

                    var getStarGazersTask = _gitHubGraphQLApiService.GetStarGazers(repository.Name, repository.OwnerLogin, cancellationToken);
                    var getRepositoryViewStatisticsTask = _gitHubApiV3Service.GetRepositoryViewStatistics(repository.OwnerLogin, repository.Name, cancellationToken);
                    var getRepositoryCloneStatisticsTask = _gitHubApiV3Service.GetRepositoryCloneStatistics(repository.OwnerLogin, repository.Name, cancellationToken);

                    await Task.WhenAll(getRepositoryViewStatisticsTask, getRepositoryCloneStatisticsTask, getStarGazersTask).ConfigureAwait(false);

                    var starGazersResponse = await getStarGazersTask.ConfigureAwait(false);
                    var repositoryViewsResponse = await getRepositoryViewStatisticsTask.ConfigureAwait(false);
                    var repositoryClonesResponse = await getRepositoryCloneStatisticsTask.ConfigureAwait(false);

                    repositoryStars = starGazersResponse.StarredAt.Select(x => x.StarredAt).ToList();
                    repositoryViews = repositoryViewsResponse.DailyViewsList;
                    repositoryClones = repositoryClonesResponse.DailyClonesList;
                }

                EmptyDataViewTitle = EmptyDataViewConstants.NoTrafficYet;
            }
            catch (Exception e)
            {
                repositoryStars = Array.Empty<DateTimeOffset>();
                repositoryViews = Array.Empty<DailyViewsModel>();
                repositoryClones = Array.Empty<DailyClonesModel>();

                EmptyDataViewTitle = EmptyDataViewConstants.UnableToRetrieveData;

                AnalyticsService.Report(e);
            }
            finally
            {
                DailyStarsList = GetDailyStarsList(repositoryStars).OrderBy(x => x.Day).ToList();
                DailyViewsList = repositoryViews.OrderBy(x => x.Day).ToList();
                DailyClonesList = repositoryClones.OrderBy(x => x.Day).ToList();

                StarsStatisticsText = repositoryStars.Count.ToAbbreviatedText();

                ViewsStatisticsText = repositoryViews.Sum(x => x.TotalViews).ToAbbreviatedText();
                UniqueViewsStatisticsText = repositoryViews.Sum(x => x.TotalUniqueViews).ToAbbreviatedText();

                ClonesStatisticsText = repositoryClones.Sum(x => x.TotalClones).ToAbbreviatedText();
                UniqueClonesStatisticsText = repositoryClones.Sum(x => x.TotalUniqueClones).ToAbbreviatedText();

                //Display the Activity Indicator for a minimum time to ensure consistant UX
                await minimumTimeTask.ConfigureAwait(false);
                IsFetchingData = false;
            }

            PrintDays();
        }

        IEnumerable<DailyStarsModel> GetDailyStarsList(IReadOnlyList<DateTimeOffset> starredAtDates)
        {
            int totalStars = 0;

            foreach (var starDate in starredAtDates)
                yield return new DailyStarsModel(++totalStars, starDate);
        }

        void UpdateDailyStarsListPropertiesChanged()
        {
            OnPropertyChanged(nameof(IsChartVisible));
            OnPropertyChanged(nameof(IsEmptyDataViewVisible));

            OnPropertyChanged(nameof(DailyStarsMaxValue));
            OnPropertyChanged(nameof(DailyStarsMinValue));

            OnPropertyChanged(nameof(MaxDailyStarsDate));
            OnPropertyChanged(nameof(MinDailyStarsDate));
        }

        void UpdateDailyClonesListPropertiesChanged()
        {
            OnPropertyChanged(nameof(IsChartVisible));
            OnPropertyChanged(nameof(IsEmptyDataViewVisible));

            OnPropertyChanged(nameof(DailyViewsClonesMaxValue));
            OnPropertyChanged(nameof(DailyViewsClonesMinValue));

            OnPropertyChanged(nameof(MinViewClonesDate));
            OnPropertyChanged(nameof(MaxViewClonesDate));
        }

        void UpdateDailyViewsListPropertiesChanged()
        {
            OnPropertyChanged(nameof(IsChartVisible));
            OnPropertyChanged(nameof(IsEmptyDataViewVisible));

            OnPropertyChanged(nameof(DailyViewsClonesMaxValue));
            OnPropertyChanged(nameof(DailyViewsClonesMaxValue));

            OnPropertyChanged(nameof(MinViewClonesDate));
            OnPropertyChanged(nameof(MaxViewClonesDate));
        }

        [Conditional("DEBUG")]
        void PrintDays()
        {
            Debug.WriteLine("Clones");
            foreach (var cloneDay in DailyClonesList.Select(x => x.Day))
                Debug.WriteLine(cloneDay);

            Debug.WriteLine("");

            Debug.WriteLine("Views");
            foreach (var viewDay in DailyViewsList.Select(x => x.Day))
                Debug.WriteLine(viewDay);
        }
    }
}
