using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Net;
using System.IO;

public class WebScraper
{
    public WebScraper()
    {
        SetUserAgent(UserAgent.Chrome81_Win10); //default agent
    }

#region Cookies
    private CookieContainer Cookies = new CookieContainer();

    public void AddCookie(string Name, string data, string path = "", string domain = "")
    {
        var ck = new Cookie(Name, data, path, domain);
        AddCookie(ck);
    }

    public void AddCookie(Cookie cookie)
    {
        Cookies.Add(cookie);
    }

    public void ResetSession()
    {
        Cookies = new CookieContainer();
        //TODO: Add other session reset code here
    }

    public CookieCollection GetCookies(System.Uri uri)
    {
        return Cookies.GetCookies(uri);
    }

    public CookieCollection GetCookies(string url)
    {
        return Cookies.GetCookies(new Uri(url));
    }
#endregion

    public int TimeOut { get; set; } = 100000;//100000 matches default used by httprequest if none is specified

    public Encoding PageEncoding { get; set; } = Encoding.UTF8;

#region User Agents
    // TODO: Update this... it's VERY stale
    // TODO: Move to separate class with distinct sub-types (eg: UserAgents.IE.6XP or UserAgents.FF.2XP, classes that overload .ToString())
    public enum UserAgent {
            IE6SP1,
            IE7_XP,
            IE7_Vista,
            FF2_XP,
            FF2_Vista,
            FF2_Mac,
            FF2_Linux,
            Safari,
            Chrome81_Win10
    }

    public void SetUserAgent(UserAgent agent)
    {
        switch (agent)
        {
            case WebScraper.UserAgent.FF2_Linux:
                Agent = "Mozilla/5.0 (X11; U; Linux i686; en-US; rv:1.8.1.4) Gecko/20070713 Firefox/2.0.0.5";
                break;
            case WebScraper.UserAgent.FF2_Mac:
                Agent = "Mozilla/5.0 (Macintosh; U; Intel Mac OS X; en-US; rv:1.8.1) Gecko/20070713 Firefox/2.0.0.5";
                break;
            case WebScraper.UserAgent.FF2_Vista:
                Agent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.8.1.3) Gecko/20070713 Firefox/2.0.0.5";
                break;
            case WebScraper.UserAgent.FF2_XP:
                Agent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.1.3) Gecko/20070713 Firefox/2.0.0.5";
                break;
            case WebScraper.UserAgent.IE6SP1:
                Agent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                break;
            case WebScraper.UserAgent.IE7_Vista:
                Agent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                break;
            case WebScraper.UserAgent.IE7_XP:
                Agent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                break;
            case WebScraper.UserAgent.Safari:
                Agent = "Mozilla/5.0 (Macintosh; U; Intel Mac OS X; en) AppleWebKit/522.12.1 (KHTML, like Gecko) Safari/522.12.1";
                break;
            case WebScraper.UserAgent.Chrome81_Win10:
                Agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36";
                break;
        }
    }

    public void SetUserAgent(string agent)
    {
        Agent = agent;
    }

    private string Agent;
#endregion

#region GetPage()
    public string GetPage(Uri URL, string PostData = "")
    {
        using (var reader = new StreamReader(SendRequest(URL, PostData).GetResponseStream(), PageEncoding))
        {
            return reader.ReadToEnd();
        }
    }

    public string GetPage(string URL, string PostData = "")
    {
        return GetPage(new Uri(URL), PostData);
    }

    public string GetPage(string URL, IEnumerable<KeyValuePair<string, string>> PostData)
    {
        return GetPage(URL, PrepPostData(PostData));
    }

    public string GetPage(Uri URL, IEnumerable<KeyValuePair<string, string>> PostData)
    {
        return GetPage(URL, PrepPostData(PostData));
    }
#endregion

#region GetResponse()
    public object GetResponse(Uri URL, string Postdata = "")
    {
        var x = SendRequest(URL, Postdata);

        if (x.ContentType.Contains("text"))
        {
            using (var reader = new StreamReader(x.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
        else if (x.ContentType.Contains("image"))
        {
            return System.Drawing.Image.FromStream(x.GetResponseStream());
        }
        else
        {
            return x.GetResponseStream();
        }
    }

    public object GetResponse(Uri URL, IEnumerable<KeyValuePair<string, string>> PostData)
    {
        return GetResponse(URL, PrepPostData(PostData));
    }

    public object GetResponse(string URL, IEnumerable<KeyValuePair<string, string>> PostData)
    {
        return GetResponse(URL, PrepPostData(PostData));
    }

    public object GetResponse(string URL, string PostData = "")
    {
        return GetResponse(new Uri(URL), PostData);
    }
#endregion

#region SaveResponseToFile()
    public void SaveResponseToFile(string FullFileName, Uri URL, string PostData = "")
    {
        using (var x = SendRequest(URL, PostData).GetResponseStream())
        using (var y = new FileStream(FullFileName, FileMode.Create))
        {
            x.CopyTo(y);
        }
    }

    public void SaveResponseToFile(string FullFileName, string URL, string PostData = "")
    {
        SaveResponseToFile(FullFileName, new Uri(URL), PostData);
    }

    public void SaveResponseToFile(string FullFileName, string URL, IEnumerable<KeyValuePair<string, string>> PostData)
    {
        SaveResponseToFile(FullFileName, URL, PrepPostData(PostData));
    }

    public void SaveResponseToFile(string FullFileName, Uri URL, IEnumerable<KeyValuePair<string, string>> PostData)
    {
        SaveResponseToFile(FullFileName, URL, PrepPostData(PostData));
    }
#endregion

#region GetImage()
    public Image GetImage(string URL)
    {
        return Image.FromStream(SendRequest(URL).GetResponseStream());
    }

    public Image GetImage(Uri URL)
    {
        return Image.FromStream(SendRequest(URL).GetResponseStream());
    }
#endregion
                
#region PostToURL()
    public void PostToURL(string URL, string PostData = "")
    {
        SendRequest(URL, PostData);
    }

    public void PostToURL(Uri URL, string PostData = "")
    {
        SendRequest(URL, PostData);
    }

    public void PostToURL(string URL, Dictionary<string, string> PostData)
    {
        PostToURL(URL, PrepPostData(PostData));
    }

    public void PostToURL(Uri URL, Dictionary<string, string> PostData)
    {
        PostToURL(URL, PrepPostData(PostData));
    }
#endregion
        
#region "Private Methods"
    private string PrepPostData(IEnumerable<KeyValuePair<string, string>> PostData)
    {
        var result = ""; // TODO: properly encode post data
        var delimiter = "";
        foreach (var pair in PostData)
        {
            result += string.Format("{0}{1}={2}", delimiter, pair.Key, pair.Value);
            delimiter = "&";
        }
        return result;
    }


    private HttpWebResponse SendRequest(string URL, string PostData = "")
    {
        return SendRequest(new Uri(URL), PostData);
    }

    private HttpWebResponse SendRequest(Uri URL, string PostData = "")
    {
        var request = HttpWebRequest.Create(URL) as HttpWebRequest;
        request.CookieContainer = Cookies;
        request.Timeout = TimeOut;
        request.UserAgent = Agent;

        if (PostData.Length > 0)
        {
            request.Method = "POST"; // TODO: allow explicitly setting METHOD and Content-type for request via properties
            request.ContentType = "application/x-www-form-urlencoded";

            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(PostData);
            }
        }
        try
        {
            return request.GetResponse() as HttpWebResponse;
        }
        catch(WebException ex)
        {
            //for debugging
            throw;
        }            
    }
#endregion

}
