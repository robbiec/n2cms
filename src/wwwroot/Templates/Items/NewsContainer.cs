using N2.Integrity;

namespace N2.Templates.Items
{
    [Definition("News Container", "NewsContainer", "A list of news. News items can be added to this page.", "", 150)]
    [RestrictParents(typeof (IStructuralPage))]
	[DefaultTemplate("NewsList")]
    public class NewsContainer : AbstractContentPage
    {
        protected override string IconName
        {
            get { return "newspaper_link"; }
        }
    }
}