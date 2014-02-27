using Bottles.Configuration;
using Serenity.Fixtures;
using StoryTeller;
using StoryTeller.Assertions;
using StoryTeller.Engine;
using StorytellerProject.Endpoints;

namespace StorytellerProject.Fixtures
{
    public class NavigationGenericFixture : ScreenFixture
    {
        [FormatAs("Navigate to Input Model with Generic")]
        public void NavigateToInputModel()
        {
            Navigation.NavigateTo<NavigationGenericInputModel>();
        }

        [FormatAs("Is on NavigationGenericEndpoint")]
        public void IsOnNavigationGenericEndpoint()
        {
            StoryTellerAssert.Fail(Navigation.GetCurrentUrl() != Application.Urls.UrlFor<NavigationGenericInputModel>(), "Navigation to input model failed.");
        }
    }
}