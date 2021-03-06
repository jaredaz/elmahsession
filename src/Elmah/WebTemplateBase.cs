namespace Elmah
{
    #region Imports

    using System;
    using System.Web;

    #endregion

    class WebTemplateBase : RazorTemplateBase
    {
        public HttpContextBase Context { get; set; }
        public HttpResponseBase Response { get { return Context.Response; } }
        public HttpRequestBase Request { get { return Context.Request; } }
        public HttpServerUtilityBase Server { get { return Context.Server; } }

        public IHtmlString Html(string html)
        {
            return new HtmlString(html);
        }

        public string AttributeEncode(string text)
        {
            return string.IsNullOrEmpty(text)
                 ? string.Empty
                 : HttpUtility.HtmlAttributeEncode(text);
        }

        public string Encode(string text)
        {
            return string.IsNullOrEmpty(text) 
                 ? string.Empty 
                 : HttpUtility.HtmlEncode(text);
        }

        public override void Write(object value)
        {
            if (value == null)
                return;
            var html = value as IHtmlString;
            base.Write(html != null ? html.ToHtmlString() : Encode(value.ToString()));
        }

        public override object RenderBody()
        {
            return new HtmlString(base.RenderBody().ToString());
        }

        public override string TransformText()
        {
            if (Context == null)
                throw new InvalidOperationException("The Context property has not been initialzed with an instance.");
            return base.TransformText();
        }
    }
}