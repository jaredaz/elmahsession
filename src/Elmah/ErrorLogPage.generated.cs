﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Elmah
{
    using System;
    using System.Collections.Generic;
    
    #line 2 "..\..\ErrorLogPage.cshtml"
    using System.Globalization;
    
    #line default
    #line hidden
    
    #line 3 "..\..\ErrorLogPage.cshtml"
    using System.IO;
    
    #line default
    #line hidden
    using System.Linq;
    using System.Text;
    
    #line 4 "..\..\ErrorLogPage.cshtml"
    using System.Web;
    
    #line default
    #line hidden
    
    #line 5 "..\..\ErrorLogPage.cshtml"
    using Elmah;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "1.2.0.0")]
    internal partial class ErrorLogPage : WebTemplateBase
    {
#line hidden

        #line 241 "..\..\ErrorLogPage.cshtml"

    IHtmlString LinkHere(string basePageName, string type, string text, int pageIndex, int pageSize)
    {
        return new HtmlString(
            "<a"
            + new HtmlString(!string.IsNullOrEmpty(type) ? " rel=\"" + type + "\"" : null)
            + " href=\""
            + Encode(
                string.Format(CultureInfo.InvariantCulture, 
                    @"{0}?page={1}&size={2}",
                    basePageName,
                    pageIndex + 1,
                    pageSize))
            + "\">"
            + Encode(text)
            + "</a>"
        );
    } 

        #line default
        #line hidden

        public override void Execute()
        {


WriteLiteral("\r\n");







            
            #line 7 "..\..\ErrorLogPage.cshtml"
  
    const int defaultPageSize = 15;
    const int maximumPageSize = 100;

    var basePageName = Request.ServerVariables["URL"];
    
    //
    // Get the page index and size parameters within their bounds.
    //

    var pageSize = Convert.ToInt32(Request.QueryString["size"], CultureInfo.InvariantCulture);
    pageSize = Math.Min(maximumPageSize, Math.Max(0, pageSize));

    if (pageSize == 0)
    {
        pageSize = defaultPageSize;
    }

    var pageIndex = Convert.ToInt32(Request.QueryString["page"], CultureInfo.InvariantCulture);
    pageIndex = Math.Max(1, pageIndex) - 1;

    //
    // Read the error records.
    //

    var log = this.ErrorLog ?? ErrorLog.GetDefault(Context);
    var errorEntryList = new List<ErrorLogEntry>(pageSize);
    var totalCount = log.GetErrors(pageIndex, pageSize, errorEntryList);

    //
    // Set the title of the page.
    //

    var hostName = Elmah.Environment.TryGetMachineName(Context);
    var title = string.Format(
        hostName.Length > 0
        ? "Error log for {0} on {2} (Page #{1})"
        : "Error log for {0} (Page #{1})",
        log.ApplicationName, (pageIndex + 1).ToString("N0"), hostName);    


            
            #line default
            #line hidden
WriteLiteral(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
    <head>
        <meta http-equiv=""X-UA-Compatible"" content=""IE=EmulateIE7"" />
        <title>");


            
            #line 51 "..\..\ErrorLogPage.cshtml"
          Write(title);

            
            #line default
            #line hidden
WriteLiteral("</title>\r\n        <link rel=\"stylesheet\" type=\"text/css\" href=\"");


            
            #line 52 "..\..\ErrorLogPage.cshtml"
                                                Write(basePageName);

            
            #line default
            #line hidden
WriteLiteral("/stylesheet\" />\r\n        <link rel=\"alternate\" type=\"application/rss+xml\" title=\"" +
"RSS\" href=\"");


            
            #line 53 "..\..\ErrorLogPage.cshtml"
                                                                      Write(basePageName);

            
            #line default
            #line hidden
WriteLiteral("/rss\" />\r\n");


            
            #line 54 "..\..\ErrorLogPage.cshtml"
         if (pageIndex == 0)
        {
            // If on the first page, then enable auto-refresh every minute

            
            #line default
            #line hidden
WriteLiteral("            <meta http-equiv=\"refresh\" content=\"60\" />\r\n");


            
            #line 58 "..\..\ErrorLogPage.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </head>\r\n    <body>\r\n");


            
            #line 61 "..\..\ErrorLogPage.cshtml"
          
            // If the application name matches the APPL_MD_PATH then its
            // of the form /LM/W3SVC/.../<name>. In this case, use only the 
            // <name> part to reduce the noise. The full application name is 
            // still made available through a tooltip.

            string simpleName = log.ApplicationName;

            if (string.Compare(simpleName, Request.ServerVariables["APPL_MD_PATH"],
                true, CultureInfo.InvariantCulture) == 0)
            {
                var lastSlashIndex = simpleName.LastIndexOf('/');

                if (lastSlashIndex > 0)
                {
                    simpleName = simpleName.Substring(lastSlashIndex + 1);
                }
            }
                

            
            #line default
            #line hidden
WriteLiteral("        <h1 id=\"PageTitle\">\r\n            Error Log for <span id=\"ApplicationName\"" +
" title=\"");


            
            #line 81 "..\..\ErrorLogPage.cshtml"
                                                       Write(log.ApplicationName);

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 81 "..\..\ErrorLogPage.cshtml"
                                                                             Write(simpleName);

            
            #line default
            #line hidden
WriteLiteral(" \r\n");


            
            #line 82 "..\..\ErrorLogPage.cshtml"
             if (!string.IsNullOrEmpty(hostName))
            {

            
            #line default
            #line hidden
WriteLiteral("                ");

WriteLiteral(" on ");


            
            #line 84 "..\..\ErrorLogPage.cshtml"
                     Write(hostName);

            
            #line default
            #line hidden
WriteLiteral(" \r\n");


            
            #line 85 "..\..\ErrorLogPage.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("            </span>\r\n        </h1>\r\n        \r\n");


            
            #line 89 "..\..\ErrorLogPage.cshtml"
          
            IHtmlString navbar;
            using (var sw = new StringWriter())
            {
                using (var writer = Request.Browser.CreateHtmlTextWriter(sw))
                {
                    SpeedBar.Render(writer,
                        SpeedBar.RssFeed.Format(basePageName),
                        SpeedBar.RssDigestFeed.Format(basePageName),
                        SpeedBar.DownloadLog.Format(basePageName),
                        SpeedBar.Help,
                        SpeedBar.About.Format(basePageName));
                    writer.Flush();
                    navbar = Html(sw.ToString());
                }
            }
        

            
            #line default
            #line hidden
WriteLiteral("        ");


            
            #line 106 "..\..\ErrorLogPage.cshtml"
   Write(navbar);

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");


            
            #line 108 "..\..\ErrorLogPage.cshtml"
         if (errorEntryList.Count > 0)
        {
            // Write error number range displayed on this page and the
            // total available in the log, followed by stock
            // page sizes.

            var firstErrorNumber = pageIndex * pageSize + 1;
            var lastErrorNumber = firstErrorNumber + errorEntryList.Count - 1;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Write out a set of stock page size choices. Note that
            // selecting a stock page size re-starts the log 
            // display from the first page to get the right paging.

            var stockSizes =
                from sizes in new[]
                {
                    new[] { 10, 15, 20, 25, 30, 50, 100, },
                }
                from size in sizes.Index()
                let separator = 
                    size.Key + 1 == sizes.Length 
                    ? null 
                    : size.Key + 2 == sizes.Length 
                    ? " or " 
                    : ", "
                select LinkHere(basePageName, HtmlLinkType.Start, size.Value.ToString("N0"), 0, size.Value)
                     + separator;
            

            
            #line default
            #line hidden
WriteLiteral("            <p>Errors ");


            
            #line 137 "..\..\ErrorLogPage.cshtml"
                 Write(firstErrorNumber.ToString("N0"));

            
            #line default
            #line hidden
WriteLiteral(" to ");


            
            #line 137 "..\..\ErrorLogPage.cshtml"
                                                     Write(lastErrorNumber.ToString("N0"));

            
            #line default
            #line hidden
WriteLiteral(" \r\n                of total ");


            
            #line 138 "..\..\ErrorLogPage.cshtml"
                    Write(totalCount.ToString("N0"));

            
            #line default
            #line hidden
WriteLiteral(" \r\n                (page ");


            
            #line 139 "..\..\ErrorLogPage.cshtml"
                  Write((pageIndex + 1).ToString("N0"));

            
            #line default
            #line hidden
WriteLiteral(" of ");


            
            #line 139 "..\..\ErrorLogPage.cshtml"
                                                      Write(totalPages.ToString("N0"));

            
            #line default
            #line hidden
WriteLiteral(". \r\n                Start with ");


            
            #line 140 "..\..\ErrorLogPage.cshtml"
                      Write(Html(string.Join(null, stockSizes.ToArray())));

            
            #line default
            #line hidden
WriteLiteral(" errors per page.</p>\r\n");


            
            #line 141 "..\..\ErrorLogPage.cshtml"

            // Write out the main table to display the errors.


            
            #line default
            #line hidden
WriteLiteral(@"            <table id=""ErrorLog"" cellspacing=""0"" style=""border-collapse:collapse;"">
                <tr>
                    <th class=""host-col"" style=""white-space:nowrap;"">Host</th>
                    <th class=""code-col"" style=""white-space:nowrap;"">Code</th>
                    <th class=""type-col"" style=""white-space:nowrap;"">Type</th>
                    <th class=""error-col"" style=""white-space:nowrap;"">Error</th>
                    <th class=""user-col"" style=""white-space:nowrap;"">User</th>
                    <th class=""date-col"" style=""white-space:nowrap;"">Date</th>
                    <th class=""time-col"" style=""white-space:nowrap;"">Time</th>
                </tr>

");


            
            #line 155 "..\..\ErrorLogPage.cshtml"
             foreach (var item in errorEntryList.Select((e, i) => new { Index = i, Entry = e, }))
            {
                var errorIndex = item.Index;
                var errorEntry = item.Entry;
                var error = errorEntry.Error;


            
            #line default
            #line hidden
WriteLiteral("                <tr class=\"");


            
            #line 161 "..\..\ErrorLogPage.cshtml"
                       Write(errorIndex % 2 == 0 ? "even-row" : "odd-row");

            
            #line default
            #line hidden
WriteLiteral("\">\r\n                    \r\n                    <td class=\"host-col\" style=\"white-s" +
"pace:nowrap;\">");


            
            #line 163 "..\..\ErrorLogPage.cshtml"
                                                                Write(error.HostName);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                    <td class=\"code-col\" style=\"white-space:nowrap;\"><span" +
" title=\"");


            
            #line 164 "..\..\ErrorLogPage.cshtml"
                                                                             Write(HttpWorkerRequest.GetStatusDescription(error.StatusCode));

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 164 "..\..\ErrorLogPage.cshtml"
                                                                                                                                        Write(error.StatusCode);

            
            #line default
            #line hidden
WriteLiteral("</span></td>\r\n                    <td class=\"type-col\" style=\"white-space:nowrap;" +
"\"><span title=\"");


            
            #line 165 "..\..\ErrorLogPage.cshtml"
                                                                             Write(error.Type);

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 165 "..\..\ErrorLogPage.cshtml"
                                                                                          Write(ErrorDisplay.HumaneExceptionErrorType(error));

            
            #line default
            #line hidden
WriteLiteral("</span></td>\r\n                    \r\n                    <td class=\"error-col\"><sp" +
"an>");


            
            #line 167 "..\..\ErrorLogPage.cshtml"
                                           Write(error.Message);

            
            #line default
            #line hidden
WriteLiteral("</span> \r\n                        <a href=\"");


            
            #line 168 "..\..\ErrorLogPage.cshtml"
                            Write(basePageName);

            
            #line default
            #line hidden
WriteLiteral("/detail?id=");


            
            #line 168 "..\..\ErrorLogPage.cshtml"
                                                    Write(errorEntry.Id);

            
            #line default
            #line hidden
WriteLiteral("\">Details&hellip;</a></td>\r\n                    \r\n                    <td class=\"" +
"user-col\" style=\"white-space:nowrap;\">");


            
            #line 170 "..\..\ErrorLogPage.cshtml"
                                                                Write(error.User);

            
            #line default
            #line hidden
WriteLiteral("</td>\r\n                    <td class=\"date-col\" style=\"white-space:nowrap;\"><span" +
" title=\"");


            
            #line 171 "..\..\ErrorLogPage.cshtml"
                                                                             Write(error.Time.ToLongDateString());

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 171 "..\..\ErrorLogPage.cshtml"
                                                                                                             Write(error.Time.ToShortDateString());

            
            #line default
            #line hidden
WriteLiteral("</span></td>\r\n                    <td class=\"time-col\" style=\"white-space:nowrap;" +
"\"><span title=\"");


            
            #line 172 "..\..\ErrorLogPage.cshtml"
                                                                             Write(error.Time.ToLongTimeString());

            
            #line default
            #line hidden
WriteLiteral("\">");


            
            #line 172 "..\..\ErrorLogPage.cshtml"
                                                                                                             Write(error.Time.ToShortTimeString());

            
            #line default
            #line hidden
WriteLiteral("</span></td>\r\n                </tr>\r\n");


            
            #line 174 "..\..\ErrorLogPage.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("            </table>\r\n");


            
            #line 176 "..\..\ErrorLogPage.cshtml"

            // Write out page navigation links.

            //
            // If not on the last page then render a link to the next page.
            //

            var nextPageIndex = pageIndex + 1;
            var moreErrors = nextPageIndex * pageSize < totalCount;
            

            
            #line default
            #line hidden
WriteLiteral("            <p>\r\n\r\n");


            
            #line 188 "..\..\ErrorLogPage.cshtml"
                 if (moreErrors)
                {
                    
            
            #line default
            #line hidden
            
            #line 190 "..\..\ErrorLogPage.cshtml"
               Write(LinkHere(basePageName, HtmlLinkType.Next, "Next errors", nextPageIndex, pageSize));

            
            #line default
            #line hidden
            
            #line 190 "..\..\ErrorLogPage.cshtml"
                                                                                                      
                }

            
            #line default
            #line hidden

            
            #line 192 "..\..\ErrorLogPage.cshtml"
                 if (pageIndex > 0 && totalCount > 0)
                {
                    if (moreErrors) {
                        Write("; ");
                    }
                    
            
            #line default
            #line hidden
            
            #line 197 "..\..\ErrorLogPage.cshtml"
               Write(LinkHere(basePageName, HtmlLinkType.Start, "Back to first page", 0, pageSize));

            
            #line default
            #line hidden
            
            #line 197 "..\..\ErrorLogPage.cshtml"
                                                                                                  
                }

            
            #line default
            #line hidden
WriteLiteral("\r\n            </p>\r\n");


            
            #line 201 "..\..\ErrorLogPage.cshtml"
        }
        else
        {
            // No errors found in the log, so display a corresponding
            // message.

            // It is possible that there are no error at the requested 
            // page in the log (especially if it is not the first page).
            // However, if there are error in the log

            if (pageIndex > 0 && totalCount > 0)
            {

            
            #line default
            #line hidden
WriteLiteral("                <p>");


            
            #line 213 "..\..\ErrorLogPage.cshtml"
              Write(LinkHere(basePageName, HtmlLinkType.Start, "Go to first page", 0, pageSize));

            
            #line default
            #line hidden
WriteLiteral(".</p>\r\n");


            
            #line 214 "..\..\ErrorLogPage.cshtml"
            }
            else
            {

            
            #line default
            #line hidden
WriteLiteral("                <p>No errors found.</p>\r\n");


            
            #line 218 "..\..\ErrorLogPage.cshtml"
            }
        }

            
            #line default
            #line hidden
WriteLiteral("\r\n");


            
            #line 221 "..\..\ErrorLogPage.cshtml"
           
            var now = DateTime.Now;
            var tz = TimeZone.CurrentTimeZone;
        

            
            #line default
            #line hidden
WriteLiteral("\r\n        <p id=\"Footer\">\r\n            <span>Powered by <a href=\"http://elmah.goo" +
"glecode.com/\">ELMAH</a>, \r\n                  version ");



WriteLiteral(" ?.?.?.?. \r\n                  Copyright (c) 2004, Atif Aziz. All rights reserved." +
" \r\n                  Licensed under <a href=\"http://www.apache.org/licenses/LICE" +
"NSE-2.0\">Apache License, Version 2.0</a>. \r\n            </span>\r\n            Ser" +
"ver date is ");


            
            #line 232 "..\..\ErrorLogPage.cshtml"
                      Write(now.ToString("D", CultureInfo.InvariantCulture));

            
            #line default
            #line hidden
WriteLiteral(". \r\n            Server time is ");


            
            #line 233 "..\..\ErrorLogPage.cshtml"
                      Write(now.ToString("T", CultureInfo.InvariantCulture));

            
            #line default
            #line hidden
WriteLiteral(". \r\n            All dates and times displayed are in the \r\n            ");


            
            #line 235 "..\..\ErrorLogPage.cshtml"
        Write(tz.IsDaylightSavingTime(now) ? tz.DaylightName : tz.StandardName);

            
            #line default
            #line hidden
WriteLiteral(" zone. \r\n            This log is provided by the ");


            
            #line 236 "..\..\ErrorLogPage.cshtml"
                                   Write(log.Name);

            
            #line default
            #line hidden
WriteLiteral(".\r\n        </p>\r\n    </body>\r\n</html>\r\n");


WriteLiteral("\r\n");


        }
    }
}
#pragma warning restore 1591
