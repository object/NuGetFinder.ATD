using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Simple.OData.Client;
using Xamarin.Forms;

namespace NuGetFinder
{
    public class SearchPage : ContentPage
    {
        private readonly Entry _searchText;
        private readonly Picker _sortPicker;
        private readonly Picker _resultSizePicker;

        public SearchPage()
        {
            Title = "NuGet Finder";

            NavigationPage.SetHasNavigationBar(this, true);

            var stackLayout = new StackLayout();

            if (Device.OS == TargetPlatform.WinPhone)
            {
                // WinPhone doesn't have the title showing
                stackLayout.Children.Add(new Label { Text = Title, Font = Font.SystemFontOfSize(50) });

                // Fill up extra space
                stackLayout.Children.Add(new Label()
                {
                    Text = "NuGet Finder is a cross-platform mobile application built using Xamarin Forms and Simple.OData.Client. " +
                           "It uses OData protocol to query NuGet package collection at nuget.org over HTTP."
                });
            }

            var searchLabel = new Label() { Text = "Find packages with titles containing text:" };
            stackLayout.Children.Add(searchLabel);

            _searchText = new Entry();
            stackLayout.Children.Add(_searchText);

            var sortLabel = new Label() { Text = "Sort results by:" };
            stackLayout.Children.Add(sortLabel);

            _sortPicker = new Picker() { Title = "Sort results by:" };
            _sortPicker.Items.Add("Package popularity");
            _sortPicker.Items.Add("Package title");
            _sortPicker.Items.Add("Update time");
            _sortPicker.SelectedIndex = 0;
            stackLayout.Children.Add(_sortPicker);

            var resultSizeLabel = new Label() { Text = "Limit result collection with:" };
            stackLayout.Children.Add(resultSizeLabel);

            _resultSizePicker = new Picker() { Title = "Limit result collection size with:" };
            _resultSizePicker.Items.Add("10 results");
            _resultSizePicker.Items.Add("25 results");
            _resultSizePicker.Items.Add("100 results");
            _resultSizePicker.SelectedIndex = 0;
            stackLayout.Children.Add(_resultSizePicker);

            var searchButton = new Button() { Text = "Search" };
            searchButton.Clicked += async (sender, e) =>
            {
                var packages = await FindPackagesAsync();
                var resultsPage = new ResultsPage(packages);
                await this.Navigation.PushAsync(resultsPage);
            };
            stackLayout.Children.Add(searchButton);

            stackLayout.VerticalOptions = LayoutOptions.FillAndExpand;

            this.Padding = new Thickness(10, Device.OnPlatform(20, 0, 0), 10, 5);
            this.Content = stackLayout;
        }

        private async Task<IEnumerable<Package>> FindPackagesAsync()
        {
            int count = 0;
            switch (_resultSizePicker.SelectedIndex)
            {
                case 0:
                    count = 10;
                    break;
                case 1:
                    count = 25;
                    break;
                case 2:
                    count = 100;
                    break;
            }

            Expression<Func<Package, object>> sortFunc;
            bool descending;
            switch (_sortPicker.SelectedIndex)
            {
                case 0:
                default:
                    sortFunc = x => x.DownloadCount;
                    descending = true;
                    break;
                case 1:
                    sortFunc = x => x.Id;
                    descending = false;
                    break;
                case 2:
                    sortFunc = x => x.LastUpdated;
                    descending = true;
                    break;
            }

            return await FindPackagesAsync(_searchText.Text, sortFunc, descending, count);
        }

        public async Task<IEnumerable<Package>> FindPackagesAsync(string searchText, Expression<Func<Package, object>> sortField, bool descending, int count)
        {
            var odataClient = new ODataClient("https://nuget.org/api/v1");

            var command = odataClient.For<Package>("Packages");

            if (descending)
            {
                command.OrderByDescending(sortField);
            }
            else
            {
                command.OrderBy(sortField);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                command.Filter(x => x.Title.Contains(searchText) && x.IsLatestVersion);
            }
            else
            {
                command.Filter(x => x.IsLatestVersion);
            }

            command.Top(count);

            command.Select(x => new { x.Id, x.Title, x.Version, x.LastUpdated, x.DownloadCount, x.VersionDownloadCount, x.PackageSize, x.Authors, x.Dependencies });

            return await command.FindEntriesAsync();
        }
    }
}
