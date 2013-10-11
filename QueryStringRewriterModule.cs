using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FortyFingers.NbStoreMenuManipulator
{
    public class NbStoreMenuManipulatorModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
        }

        private void BeginRequest(object sender, EventArgs e)
        {
            // if there's a tabid in querystring: rewrite the url to contain the tabid in a DNN6 way
            // also, if the requested page is not Default.aspx, correct it to be default.aspx

            var app = (HttpApplication)sender;
            var server = app.Server;
            var request = app.Request;

            // So, if there's no tabid: this module doesn't have a function:
            if (!request.QueryString.AllKeys.Contains("tabid")) return;

            // save the url to be able to set it back later on
            // app.Context.Items.Add("QueryStringRewrite:OriginalUrl", app.Request.Url.AbsoluteUri);

            //We want a URL that looks like this to be handled nicely:
            //http://www.domain.com/blah/blah/something.aspx?tabid=###&mid=###&ctl=xxx
            //DNN needs Friendly URLs to be using the following format
            //http://www.domain.com/tabid/###/mid/###/ctl/xxx/default.aspx

            string sendTo = "";

            //save and remove the querystring as it gets added back on later
            //path parameter specifications will take precedence over querystring parameters
            string requestedPath = request.AppRelativeCurrentExecutionFilePath.ToString();
            if ((!String.IsNullOrEmpty(app.Request.Url.Query)))
            {
                // make sendTo contain the full url, except for the page
                // it ends with /
                sendTo = requestedPath.Substring(0, requestedPath.LastIndexOf("/") + 1);
            }

            foreach (var key in request.QueryString.AllKeys)
            {
                // move the parameter from the qstring to the path
                sendTo += String.Format("{0}/{1}/", key, request[key]);
            }

            // now we've moved all qstring params to the path
            // finally append the page name
            sendTo += "Default.aspx";
            //Rewrite it
            app.Context.RewritePath(sendTo);
        }

        public void Dispose()
        {

        }


    }
}