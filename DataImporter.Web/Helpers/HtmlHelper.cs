namespace DataImporter.Web.Helpers
{
    public class HtmlHelper
    {
        public static string ToHtml(string plainString)
        {
            return plainString
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />")
                .Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
        }
    }
}
