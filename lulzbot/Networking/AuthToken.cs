using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace lulzbot.Networking
{
    public class AuthToken
    {
        private const String _users_uri = @"https://www.deviantart.com/users/rockedout";
        private const String _login_uri = @"https://www.deviantart.com/users/login";
        private const String _chat_uri  = @"http://chat.deviantart.com/chat/datashare";
        private const String _regex     = "dAmn_Login\\( \"[^\"]*\", \"([^\"]*)\" \\);";
        private const String _useragent = @"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.61 Safari/537.36";

        /// <summary>
        /// Grabs the authtoken for the username and password.
        /// </summary>
        /// <param name="username">dA username</param>
        /// <param name="password">dA password</param>
        /// <returns>authtoken</returns>
        public static String Grab (String username, String password)
        {
            // This should really be replaced with an OAuth method, or the likes.
        
            // Make sure we can bypass certificate checks on Linux machines.
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

            // Initialize the request and variables.
            String page_content         = String.Empty;
            CookieContainer cookie_jar  = new CookieContainer();
            HttpWebRequest request      = (HttpWebRequest)HttpWebRequest.Create(_users_uri);

            // Set a few request parameters
            request.KeepAlive = false;
            request.Proxy = null;
            request.CookieContainer = cookie_jar;
            request.Host = "www.deviantart.com";
            request.UserAgent = _useragent;
            request.Accept = "text/html";
            request.Method = "GET";

            try
            {
                // Create a temporary stream reader
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    // Grab the entire page contents
                    page_content = reader.ReadToEnd();
                }
            }
            catch (Exception Ex)
            {
                ConIO.Warning("AuthToken", "AT.Grab[R1]: " + Ex.Message);
            }

            if (page_content == null || !page_content.Contains("validate_token") || !page_content.Contains("validate_key")) return null;

            var token_loc = page_content.LastIndexOf("validate_token");
            var key_loc = page_content.LastIndexOf("validate_key");

            if (token_loc == -1 || key_loc == -1) return null;

            token_loc += 23;
            key_loc += 21;

            // Create our POST data string
            String post_data = String.Format("&username={0}&password={1}&remember_me=1&validate_token={2}&validate_key={3}", Uri.EscapeUriString(username), Uri.EscapeUriString(password), page_content.Substring(token_loc, 20), page_content.Substring(key_loc, 10));

            request = (HttpWebRequest)HttpWebRequest.Create(_login_uri);

            // Set a few request parameters
            request.KeepAlive = false;
            request.Proxy = null;
            request.CookieContainer = cookie_jar;
            request.Host = "www.deviantart.com";
            request.Referer = _users_uri;
            request.UserAgent = _useragent;
            request.Accept = "text/html";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = post_data.Length;

            try
            {
                // Create a temporary stream writer
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    // Write the post data to the request, and POST the request.
                    writer.Write(post_data);
                    writer.Flush();
                    request.GetResponse();
                }
            }
            catch (Exception Ex)
            {
                ConIO.Warning("AuthToken", "AT.Grab[R2]: " + Ex.Message);
            }

            // Now we make a request to the chat page to get the real authtoken
            HttpWebRequest page_request = (HttpWebRequest)HttpWebRequest.Create(_chat_uri);

            // Request parameters
            page_request.Method = "GET";
            page_request.KeepAlive = false;
            page_request.Proxy = null;
            page_request.Host = "chat.deviantart.com";
            page_request.CookieContainer = cookie_jar;
            page_request.Referer = @"http://chat.deviantart.com/";
            page_request.UserAgent = _useragent;
            page_request.Accept = "text/html";

            try
            {
                // Create a temporary stream reader
                using (StreamReader reader = new StreamReader(page_request.GetResponse().GetResponseStream()))
                {
                    // Grab the entire page contents
                    page_content = reader.ReadToEnd();
                }
            }
            catch (Exception Ex)
            {
                ConIO.Warning("AuthToken", "AT.Grab[R3]: " + Ex.Message);
            }

            // If the page contains the dAmn_Login function
            if (page_content.Contains("dAmn_Login"))
            {
                // Grab and return the authtoken
                Match match = Regex.Match(page_content, _regex);
                return Regex.Replace(match.Value, _regex, "$1");
            } // Otherwise, return null
            else return null;
        }

        /// <summary>
        /// Bypass certificate checks on Linux.
        /// </summary>
        private static bool ValidateRemoteCertificate (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
