using HtmlTags;

namespace StorytellerProject.Endpoints
{
    public class NavigationGenericEndpoint
    {
        public HtmlDocument Index(NavigationGenericInputModel model)
        {
            var document = new HtmlDocument();
            document.Title = "Navigation Generic Endpoint";
            document.Add("h1").Text("You navigated to an endpoint using a generic!");
            return document;
        }
    }
}