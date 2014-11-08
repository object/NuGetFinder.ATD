using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace NuGetFinder.Specs
{
    [Binding]
    public class DefinitionSteps
    {
        private SearchPage _searchPage;
        private IEnumerable<Package> _result;

        [Given(@"I am on the Search page")]
        public void GivenIAmOnTheSearchPage()
        {
            _searchPage = new SearchPage();
        }
        
        [When(@"search packages by criteria")]
        public void WhenSearchPackagesByCriteria(Table table)
        {
            var searchText = table.Rows[0]["Search text"];
            var numberOfResults = int.Parse(table.Rows[0]["Number of results"]);
            Expression<Func<Package, object>> sortField;
            bool descending;
            switch (table.Rows[0]["Sort by"])
            {
                case "Popularity":
                default:
                    sortField = x => x.DownloadCount;
                    descending = true;
                    break;
                case "Package title":
                    sortField = x => x.Id;
                    descending = false;
                    break;
                case "Update time":
                    sortField = x => x.LastUpdated;
                    descending = true;
                    break;
            }

            _result = _searchPage.FindPackagesAsync(searchText, sortField, descending, numberOfResults).Result;
        }

        [Then(@"I should get (.*) results")]
        public void ThenIShouldGetResults(int count)
        {
            Assert.AreEqual(count, _result.Count());
        }

        [Then(@"the top result should be ""(.*)""")]
        public void ThenTheTopResultShouldBe(string title)
        {
            Assert.AreEqual(title, _result.First().Id);
        }

        [Then(@"the top result should have update time later than yesterday")]
        public void ThenTheTopResultShouldHaveUpdateTimeLaterThanYesterday()
        {
            Assert.Less(DateTime.Now.AddDays(-1), _result.First().LastUpdated);
        }

        [Then(@"all results should contain text ""(.*)"" in the package title")]
        public void ThenAllResultsShouldContainTextInThePackageTitle(string text)
        {
            Assert.IsTrue(_result.All(x => x.Title.ToUpper().Contains(text.ToUpper())));
        }
    }
}