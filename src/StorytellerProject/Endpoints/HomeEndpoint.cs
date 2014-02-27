using HtmlTags;

namespace StorytellerProject.Endpoints
{
    public class HomeEndpoint
    {
        public HtmlDocument Index()
        {
            var document = new HtmlDocument();
            document.Title = "Serenity/Storyteller Harness";
            document.Add("h1").Text("Serenity/Storyteller Harness");

            document.Add("input").Attr("name", "foo").Id("foo").Attr("value", "bar");

            return document;
        }
    }
}