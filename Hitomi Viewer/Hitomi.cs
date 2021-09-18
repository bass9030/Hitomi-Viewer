using Microsoft.ClearScript.V8;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using NSoup;
using NSoup.Nodes;
using System.Web;


namespace Hitomi_Core
{
    public class Hitomi
    {

        private string code = Hitomi_Viewer.Properties.Resources.Load_Link_Code;
        private string gallery_html;
        public JObject gallery_info
        {
            get;
            private set;
        }

        public string gallery_id
        {
            get;
            private set;
        }

        public Hitomi(string _gallery_id)
        {
            WebClient web = new WebClient();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            web.Encoding = Encoding.UTF8;
            gallery_info = JObject.Parse(web.DownloadString("https://ltn.hitomi.la/galleries/" + _gallery_id + ".js").Split(new string[] { "var galleryinfo = " }, StringSplitOptions.None)[1]);
            Console.WriteLine($"https://hitomi.la/{gallery_info["type"]}/{HttpUtility.UrlEncode(gallery_info["title"].ToString().Replace(' ', '-').ToLower(new System.Globalization.CultureInfo("en-US"))) + "-" + gallery_info["language_localname"].ToString().ToLower() + "-" + _gallery_id}.html");
            gallery_html = web.DownloadString($"https://hitomi.la/{gallery_info["type"]}/{HttpUtility.UrlEncode(gallery_info["title"].ToString().Replace(' ', '-').ToLower(new System.Globalization.CultureInfo("en-US"))) + "-" + gallery_info["language_localname"].ToString().ToLower() + "-" + _gallery_id}.html");
            gallery_id = _gallery_id;
        }

        public string Group
        {
            get
            {
                Document doc = NSoupClient.Parse(gallery_html);
                string group = doc.Select("div.gallery-info").Select("tr")[0].Select("td")[1].Text();
                return group;
            }
        }

        public string Artist
        {
            get
            {
                Document doc = NSoupClient.Parse(gallery_html);
                string Artist = doc.Select("h2").Text;
                return Artist;
            }
        }

        public string Series
        {
            get
            {
                Document doc = NSoupClient.Parse(gallery_html);
                string series = doc.Select("div.gallery-info").Select("tr")[3].Select("td")[1].Text();
                return (series != "") ? series : "N/A";
            }
        }

        public string[] images
        {
            get
            {
                string[] result = new string[gallery_info.Value<JArray>("files").Count];
                var engin = new V8ScriptEngine();
                for (int i = 0; i<result.Length; i++)
                {
                    if (gallery_info.Value<JArray>("files")[i].Value<int>("hasavif") == 1)
                        result[i] = engin.Evaluate(code + "\n" + "var tmp = " + gallery_info.ToString() + "\n" + "url_from_url_from_hash(" + gallery_id + ", tmp[\"files\"][" + i + "], \"avif\", undefined, \"a\");").ToString();
                    else if (gallery_info.Value<JArray>("files")[i].Value<int>("haswebp") == 1)
                        result[i] = engin.Evaluate(code + "\n" + "var tmp = " + gallery_info.ToString() + "\n" + "url_from_url_from_hash(" + gallery_id + ", tmp[\"files\"][" + i + "], \"webp\", undefined, \"a\");").ToString();
                    else
                        result[i] = engin.Evaluate(code + "\n" + "var tmp = " + gallery_info.ToString() + "\n" + "url_from_url_from_hash(" + gallery_id + ", tmp[\"files\"][" + i + "]);").ToString();
                }
                return result;
            }
        }

        public string title
        {
            get
            {
                return gallery_info.Value<string>("title");
            }
        }

        public string language
        {
            get
            {
                return gallery_info.Value<string>("language_localname");
            }
        }

        public string upload_date
        {
            get
            {
                return gallery_info.Value<string>("date");
            }
        }

        public string type
        {
            get
            {
                return gallery_info.Value<string>("type");
            }
        }

        public string[] tags
        {
            get
            {
                int i = 0;
                string[] result = new string[gallery_info.Value<JArray>("tags").Count];
                foreach (JObject arr in gallery_info.Value<JArray>("tags"))
                {
                    string gender;
                    string tag_name;
                    if (arr.Value<string>("male") == "1")
                    {
                        gender = "male";
                    }
                    else if(arr.Value<string>("female") == "1")
                    {
                        gender = "female";
                    }
                    else
                    {
                        gender = "tag";
                    }
                    tag_name = arr.Value<string>("tag");
                    result[i] = gender + ":" + tag_name;
                    i++;
                }
                return result;
            }
        }
    }
}
