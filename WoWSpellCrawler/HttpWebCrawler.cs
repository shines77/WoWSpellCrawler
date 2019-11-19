using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace WoWSpellCrawler
{
    public enum SecurityProtocolType2
    {
        Ssl3 = 0x30,
        Tls = 0xc0,
        Tls11 = 0x300,
        Tls12 = 0xc00
    }

    //
    // C# HttpWebRequest 未能为 SSL/TLS 安全通道建立信任的终极解决办法.
    //
    // See: https://blog.csdn.net/yunwu009/article/details/80768860
    //
    class HttpWebCrawler
    {
        public HttpWebCrawler()
        {
            //
        }

        public static string GetParamList(Dictionary<string, string> dictionary)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach (var item in dictionary)
            {
                if (i > 0)
                {
                    builder.Append("&");
                }
                builder.AppendFormat("{0}={1}", item.Key, item.Value);
                i++;
            }

            return builder.ToString();
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // 总是接受
            return true;
        }

        public string HttpGet(string url, string paramList = "", string token = "")
        {
            string html = string.Empty;

            if (!string.IsNullOrEmpty(paramList))
            {
                url = url + "?" + paramList;
            }

            try
            {
                HttpWebRequest request = null;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.ProtocolVersion = HttpVersion.Version10;

                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                    // SecurityProtocolType.Tls1.2;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Ssl3;
                    ServicePointManager.CheckCertificateRevocationList = false;
                    ServicePointManager.DefaultConnectionLimit = 1024;
                    ServicePointManager.Expect100Continue = true;

                    //
                    // See: https://www.cnblogs.com/ccsharp/p/3270344.html
                    //
                    Uri uri = new Uri(url);
                    ServicePoint svc = ServicePointManager.FindServicePoint(uri);

                    X509Store certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    certStore.Open(OpenFlags.ReadOnly);

                    // 根据名称查找匹配的证书集合，这里注意最后一个参数，传true的话会找不到
                    X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, "PowerShell User", false);

                    if (certCollection.Count > 0)
                    {
                        foreach (var cert in certCollection)
                        {
                            //svc.ClientCertificate.Import(cert.RawData);
                        }
                    }

                    //
                    // See: https://blog.csdn.net/yunwu009/article/details/80768860
                    //

                    // 开始载入网站的证书
                    string filePath = System.IO.Directory.GetCurrentDirectory() + "\\wowhead_com.cer";
                    if (System.IO.File.Exists(filePath))
                    {
                        X509Certificate certSite = new X509Certificate(filePath);
                        if (certSite.Handle != null)
                        {
                            // 将证书添加客户端证书集合
                            request.Credentials = CredentialCache.DefaultCredentials;
                            request.ClientCertificates.Add(certSite);
                        }
                    }
                }
                else
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    request.ProtocolVersion = HttpVersion.Version11;
                }

                request.CookieContainer = new CookieContainer();
                request.Timeout = 45000;
                request.ReadWriteTimeout = 45000;

                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";
                request.KeepAlive = true;
                request.Referer = null;
                request.AllowAutoRedirect = true;
                // request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                // request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
                request.Accept = "*/*";

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("authority", token);
                    request.Headers.Add("Authorization", token);
                }

                /*
                if (this.Header != null && this.Header.Count > 0)
                {
                    foreach (var item in this.Header)
                    {
                        SetHeaderValue(request.Headers, item.key, item.value);
                    }
                }
                //*/

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Stream stream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                {
                    html = reader.ReadToEnd();
                    reader.Close();
                }

                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                html = ex.ToString();
            }

            return html;
        }


        private string HttpPost(string url, string postData, string paramList = "", string token = "")
        {
            string html = string.Empty;

            if (!string.IsNullOrEmpty(paramList))
            {
                url = url + "?" + paramList;
            }

            try
            {
                HttpWebRequest request = null;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                    // SecurityProtocolType.Tls1.2;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Ssl3;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 1024;
                    ServicePointManager.Expect100Continue = true;
                }
                else
                {
                    request = WebRequest.Create(url) as HttpWebRequest; ;
                }

                request.ProtocolVersion = HttpVersion.Version11;
                request.CookieContainer = new CookieContainer();
                request.Timeout = 45000;
                request.ReadWriteTimeout = 45000;

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.KeepAlive = true;
                request.Referer = null;
                request.AllowAutoRedirect = true;
                // request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
                request.Accept = "*/*";

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("authority", token);
                    request.Headers.Add("Authorization", token);
                }

                byte[] data = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = data.Length;

                Stream reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                Stream stream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("utf-8")))
                {
                    html = reader.ReadToEnd();
                    reader.Close();
                }

                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return html;
        }

        private static string parseJsonValue(string json, int startPos)
        {
            string value = string.Empty;
            int pos = startPos;
            while (Chars.isWhiteSpaces(json[pos]))
            {
                pos++;
            }

            int firstPos = pos, lastPos;
            char firstChar = json[pos];
            pos++;
            if (pos >= json.Length)
            {
                return value;
            }

            if (firstChar == '[')
            {
                // Array
                firstPos++;
                while (json[pos] != ']')
                {
                    pos++;
                }

                lastPos = pos;
                value = json.Substring(firstPos, lastPos - firstPos);
            }
            else if (firstChar == '\"')
            {
                // String
                firstPos++;
                while (json[pos] != '\"')
                {
                    pos++;
                }

                lastPos = pos;
                value = json.Substring(firstPos, lastPos - firstPos);
            }
            else if (Chars.isDigital(firstChar))
            {
                // Digitals
                while (Chars.isDigital(json[pos]))
                {
                    pos++;
                }

                lastPos = pos;
                value = json.Substring(firstPos, lastPos - firstPos);
            }
            else if (firstChar == '{')
            {
                // Object
                firstPos++;
                while (json[pos] != '}')
                {
                    pos++;
                }

                lastPos = pos;
                value = json.Substring(firstPos, lastPos - firstPos);
            }
            else
            {
                // Error
                value = "{Error}";
            }
            return value;
        }

        private int parseSpellInfo(string html, out List<SpellInfo> spellInfoList)
        {
            const string startingIdent = "var listviewspells = ";
            const string colorIdent = "\"colors\":";
            const string idIdent = "\"id\":";
            const string nameIdent = "\"name\":";

            spellInfoList = null;

            int spellCount = 0;
            int pos = html.IndexOf(startingIdent);
            if (pos >= 0)
            {
                pos += startingIdent.Length;
                string jscode = html.Substring(pos, html.Length - pos);
                if (jscode.Length <= 0)
                {
                    return -1;
                }

                StringScanner scanner = new StringScanner(jscode);
                scanner.skipWhiteSpaces();

                // Must start with '['
                if (scanner.get() != '[')
                {
                    return -1;
                }

                spellInfoList = new List<SpellInfo>();

                scanner.next();
                scanner.skipWhiteSpaces();

                do
                {
                    // Is end of jscode string?
                    if (scanner.isEof())
                    {
                        break;
                    }

                    // A spell info is start with '{'
                    if (scanner.get() == '{')
                    {
                        scanner.next();
                        scanner.skipWhiteSpaces();

                        int jsonFirst = scanner.Position;
                        int jsonLast = jscode.IndexOf('}', jsonFirst);
                        if (jsonLast > jsonFirst)
                        {
                            string json = jscode.Substring(jsonFirst, jsonLast - jsonFirst);
                            if (json.Length > 0)
                            {
                                int colorPos = json.IndexOf(colorIdent);
                                int idPos = json.IndexOf(idIdent);
                                int namePos = json.IndexOf(nameIdent);
                                if (idPos >= 0 && namePos >= 0)
                                {
                                    string id, name, colors;
                                    id = parseJsonValue(json, idPos + idIdent.Length);
                                    name = parseJsonValue(json, namePos + nameIdent.Length);

                                    if (colorPos >= 0)
                                    {
                                        colors = parseJsonValue(json, colorPos + colorIdent.Length);
                                        spellCount++;
                                    }
                                    else
                                    {
                                        colors = string.Empty;
                                    }

                                    SpellInfo spellInfo = new SpellInfo();
                                    spellInfo.id = id;
                                    spellInfo.name = name;
                                    spellInfo.colors = colors;

                                    spellInfoList.Add(spellInfo);
                                }
                            }

                            // Skip the json value.
                            scanner.next(jsonLast - jsonFirst);

                            // End of a spell info object
                            if (scanner.get() == '}')
                            {
                                scanner.next();
                                scanner.skipWhiteSpaces();
                            }
                        }
                        else
                        {
                            scanner.next();
                        }
                    }
                    if (scanner.get() == ',')
                    {
                        // Skip ','
                        scanner.next();
                        scanner.skipWhiteSpaces();
                    }
                    else
                    {
                        scanner.next();
                    }
                } while (true);
            }
            
            return spellCount;
        }

        public string GetherSpell(Professoion type, out int success, out int failed)
        {
            string logs = string.Empty;
            //
            // WoWHead Spells: https://classic.wowhead.com/alchemy-spells
            //
            string html = HttpGet("https://classic.wowhead.com/alchemy-spells", "", "classic.wowhead.com");
            Debug.Print("html = [\r\n" + html + "\r\n];");

            //html = "var listviewspells = [{\"cat\":11,\"id\":2259,\"learnedat\":9999,\"level\":0,\"name\":\"Alchemy\",\"nskillup\":1,\"rank\":\"Apprentice\",\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":10,popularity:289},{\"cat\":11,\"colors\":[1,55,75,95],\"creates\":[2454,1,1],\"id\":2329,\"learnedat\":1,\"level\":0,\"name\":\"Elixir of Lion's Strength\",\"nskillup\":1,\"reagents\":[[2449,1],[765,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:27},{\"cat\":11,\"colors\":[1,55,75,95],\"creates\":[118,1,1],\"id\":2330,\"learnedat\":1,\"level\":0,\"name\":\"Minor Healing Potion\",\"nskillup\":1,\"reagents\":[[2447,1],[765,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:95},{\"cat\":11,\"colors\":[25,65,85,105],\"creates\":[2455,1,1],\"id\":2331,\"learnedat\":25,\"level\":0,\"name\":\"Minor Mana Potion\",\"nskillup\":1,\"reagents\":[[785,1],[765,1],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":100,\"quality\":1,popularity:50},{\"cat\":11,\"colors\":[40,70,90,110],\"creates\":[2456,1,1],\"id\":2332,\"learnedat\":40,\"level\":0,\"name\":\"Minor Rejuvenation Potion\",\"nskillup\":1,\"reagents\":[[785,2],[2447,1],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":150,\"quality\":1,popularity:23},{\"cat\":11,\"colors\":[140,165,185,205],\"creates\":[3390,1,1],\"id\":2333,\"learnedat\":140,\"level\":0,\"name\":\"Elixir of Lesser Agility\",\"nskillup\":1,\"reagents\":[[3355,1],[2452,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:115},{\"cat\":11,\"colors\":[50,80,100,120],\"creates\":[2458,1,1],\"id\":2334,\"learnedat\":50,\"level\":0,\"name\":\"Elixir of Minor Fortitude\",\"nskillup\":1,\"reagents\":[[2449,2],[2447,1],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":250,\"quality\":1,popularity:37},{\"cat\":11,\"colors\":[60,90,110,130],\"creates\":[2459,1,1],\"id\":2335,\"learnedat\":60,\"level\":0,\"name\":\"Swiftness Potion\",\"nskillup\":1,\"reagents\":[[2452,1],[2450,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:451},{\"cat\":11,\"colors\":[55,85,105,125],\"creates\":[858,1,1],\"id\":2337,\"learnedat\":55,\"level\":0,\"name\":\"Lesser Healing Potion\",\"nskillup\":1,\"reagents\":[[118,1],[2450,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":1000,\"quality\":1,popularity:130},{\"cat\":11,\"id\":3101,\"learnedat\":50,\"level\":0,\"name\":\"Alchemy\",\"nskillup\":1,\"rank\":\"Journeyman\",\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":500,popularity:170},{\"cat\":11,\"colors\":[15,60,80,100],\"creates\":[3382,1,1],\"id\":3170,\"learnedat\":15,\"level\":0,\"name\":\"Weak Troll's Blood Potion\",\"nskillup\":1,\"reagents\":[[2447,1],[2449,2],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":50,\"quality\":1,popularity:23},{\"cat\":11,\"colors\":[90,120,140,160],\"creates\":[3383,1,1],\"id\":3171,\"learnedat\":90,\"level\":0,\"name\":\"Elixir of Wisdom\",\"nskillup\":1,\"reagents\":[[785,1],[2450,2],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":500,\"quality\":1,popularity:105},{\"cat\":11,\"colors\":[110,135,155,175],\"creates\":[3384,1,1],\"id\":3172,\"learnedat\":110,\"level\":0,\"name\":\"Minor Magic Resistance Potion\",\"nskillup\":1,\"reagents\":[[785,3],[3355,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:71},{\"cat\":11,\"colors\":[120,145,165,185],\"creates\":[3385,1,1],\"id\":3173,\"learnedat\":120,\"level\":0,\"name\":\"Lesser Mana Potion\",\"nskillup\":1,\"reagents\":[[785,1],[3820,1],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":1500,\"quality\":1,popularity:123},{\"cat\":11,\"colors\":[120,145,165,185],\"creates\":[3386,1,1],\"id\":3174,\"learnedat\":120,\"level\":0,\"name\":\"Elixir of Poison Resistance\",\"nskillup\":1,\"reagents\":[[1288,1],[2453,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:202},{\"cat\":11,\"colors\":[250,275,295,315],\"creates\":[3387,1,1],\"id\":3175,\"learnedat\":250,\"level\":0,\"name\":\"Limited Invulnerability Potion\",\"nskillup\":1,\"reagents\":[[8839,2],[8845,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:696},{\"cat\":11,\"colors\":[125,150,170,190],\"creates\":[3388,1,1],\"id\":3176,\"learnedat\":125,\"level\":0,\"name\":\"Strong Troll's Blood Potion\",\"nskillup\":1,\"reagents\":[[2453,2],[2450,2],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":1500,\"quality\":1,popularity:160},{\"cat\":11,\"colors\":[130,155,175,195],\"creates\":[3389,1,1],\"id\":3177,\"learnedat\":130,\"level\":0,\"name\":\"Elixir of Defense\",\"nskillup\":1,\"reagents\":[[3355,1],[3820,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":2000,\"quality\":1,popularity:154},{\"cat\":11,\"colors\":[150,175,195,215],\"creates\":[3391,1,1],\"id\":3188,\"learnedat\":150,\"level\":0,\"name\":\"Elixir of Ogre's Strength\",\"nskillup\":1,\"reagents\":[[2449,1],[3356,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:116},{\"cat\":11,\"colors\":[50,80,100,120],\"creates\":[2457,1,1],\"id\":3230,\"learnedat\":50,\"level\":0,\"name\":\"Elixir of Minor Agility\",\"nskillup\":1,\"reagents\":[[2452,1],[765,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:104},{\"cat\":11,\"colors\":[110,135,155,175],\"creates\":[929,1,1],\"id\":3447,\"learnedat\":110,\"level\":0,\"name\":\"Healing Potion\",\"nskillup\":1,\"reagents\":[[2453,1],[2450,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":4000,\"quality\":1,popularity:342},{\"cat\":11,\"colors\":[165,185,205,225],\"creates\":[3823,1,1],\"id\":3448,\"learnedat\":165,\"level\":0,\"name\":\"Lesser Invisibility Potion\",\"nskillup\":1,\"reagents\":[[3818,1],[3355,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":4500,\"quality\":1,popularity:397},{\"cat\":11,\"colors\":[165,190,210,230],\"creates\":[3824,1,1],\"id\":3449,\"learnedat\":165,\"level\":0,\"name\":\"Shadow Oil\",\"nskillup\":1,\"reagents\":[[3818,4],[3369,4],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:1373},{\"cat\":11,\"colors\":[175,195,215,235],\"creates\":[3825,1,1],\"id\":3450,\"learnedat\":175,\"level\":0,\"name\":\"Elixir of Fortitude\",\"nskillup\":1,\"reagents\":[[3355,1],[3821,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":5400,\"quality\":1,popularity:199},{\"cat\":11,\"colors\":[180,200,220,240],\"creates\":[3826,1,1],\"id\":3451,\"learnedat\":180,\"level\":0,\"name\":\"Mighty Troll's Blood Potion\",\"nskillup\":1,\"reagents\":[[3357,1],[2453,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:254},{\"cat\":11,\"colors\":[160,180,200,220],\"creates\":[3827,1,1],\"id\":3452,\"learnedat\":160,\"level\":0,\"name\":\"Mana Potion\",\"nskillup\":1,\"reagents\":[[3820,1],[3356,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":4500,\"quality\":1,popularity:191},{\"cat\":11,\"colors\":[195,215,235,255],\"creates\":[3828,1,1],\"id\":3453,\"learnedat\":195,\"level\":0,\"name\":\"Elixir of Detect Lesser Invisibility\",\"nskillup\":1,\"reagents\":[[3358,1],[3818,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:83},{\"cat\":11,\"colors\":[200,220,240,260],\"creates\":[3829,1,1],\"id\":3454,\"learnedat\":200,\"level\":0,\"name\":\"Frost Oil\",\"nskillup\":1,\"reagents\":[[3358,4],[3819,2],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:537},{\"cat\":11,\"id\":3464,\"learnedat\":125,\"level\":0,\"name\":\"Alchemy\",\"nskillup\":1,\"rank\":\"Expert\",\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":4500,popularity:297},{\"cat\":11,\"colors\":[50,80,100,120],\"creates\":[4596,1,1],\"id\":4508,\"learnedat\":50,\"level\":0,\"name\":\"Discolored Healing Potion\",\"nskillup\":1,\"reagents\":[[3164,1],[2447,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:37},{\"cat\":11,\"colors\":[215,230,250,270],\"creates\":[4623,1,1],\"id\":4942,\"learnedat\":215,\"level\":0,\"name\":\"Lesser Stoneshield Potion\",\"nskillup\":1,\"reagents\":[[3858,1],[3821,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:49},{\"cat\":11,\"colors\":[60,90,110,130],\"creates\":[5631,1,1],\"id\":6617,\"learnedat\":60,\"level\":0,\"name\":\"Rage Potion\",\"nskillup\":1,\"reagents\":[[5635,1],[2450,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:217},{\"cat\":11,\"colors\":[175,195,215,235],\"creates\":[5633,1,1],\"id\":6618,\"learnedat\":175,\"level\":0,\"name\":\"Great Rage Potion\",\"nskillup\":1,\"reagents\":[[5637,1],[3356,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:215},{\"cat\":11,\"colors\":[150,175,195,215],\"creates\":[5634,1,1],\"id\":6624,\"learnedat\":150,\"level\":0,\"name\":\"Free Action Potion\",\"nskillup\":1,\"reagents\":[[6370,2],[3820,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:1345},{\"cat\":11,\"colors\":[90,120,140,160],\"creates\":[5996,1,1],\"id\":7179,\"learnedat\":90,\"level\":0,\"name\":\"Elixir of Water Breathing\",\"nskillup\":1,\"reagents\":[[3820,1],[6370,2],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":450,\"quality\":1,popularity:87},{\"cat\":11,\"colors\":[155,175,195,215],\"creates\":[1710,1,1],\"id\":7181,\"learnedat\":155,\"level\":0,\"name\":\"Greater Healing Potion\",\"nskillup\":1,\"reagents\":[[3357,1],[3356,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":4500,\"quality\":1,popularity:795},{\"cat\":11,\"colors\":[1,55,75,95],\"creates\":[5997,1,1],\"id\":7183,\"learnedat\":1,\"level\":0,\"name\":\"Elixir of Minor Defense\",\"nskillup\":1,\"reagents\":[[765,2],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:24},{\"cat\":11,\"colors\":[100,130,150,170],\"creates\":[6051,1,1],\"id\":7255,\"learnedat\":100,\"level\":0,\"name\":\"Holy Protection Potion\",\"nskillup\":1,\"reagents\":[[2453,1],[2452,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:49},{\"cat\":11,\"colors\":[135,160,180,200],\"creates\":[6048,1,1],\"id\":7256,\"learnedat\":135,\"level\":0,\"name\":\"Shadow Protection Potion\",\"nskillup\":1,\"reagents\":[[3369,1],[3356,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:254},{\"cat\":11,\"colors\":[165,210,230,250],\"creates\":[6049,1,1],\"id\":7257,\"learnedat\":165,\"level\":0,\"name\":\"Fire Protection Potion\",\"nskillup\":1,\"reagents\":[[4402,1],[6371,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:787},{\"cat\":11,\"colors\":[190,205,225,245],\"creates\":[6050,1,1],\"id\":7258,\"learnedat\":190,\"level\":0,\"name\":\"Frost Protection Potion\",\"nskillup\":1,\"reagents\":[[3819,1],[3821,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:126},{\"cat\":11,\"colors\":[190,210,230,250],\"creates\":[6052,1,1],\"id\":7259,\"learnedat\":190,\"level\":0,\"name\":\"Nature Protection Potion\",\"nskillup\":1,\"reagents\":[[3357,1],[3820,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:182},{\"cat\":11,\"colors\":[80,80,90,100],\"creates\":[6370,1,1],\"id\":7836,\"learnedat\":80,\"level\":0,\"name\":\"Blackmouth Oil\",\"nskillup\":1,\"reagents\":[[6358,2],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":250,\"quality\":1,popularity:1779},{\"cat\":11,\"colors\":[130,150,160,170],\"creates\":[6371,1,1],\"id\":7837,\"learnedat\":130,\"level\":0,\"name\":\"Fire Oil\",\"nskillup\":1,\"reagents\":[[6359,2],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":1000,\"quality\":1,popularity:1705},{\"cat\":11,\"colors\":[100,130,150,170],\"creates\":[6372,1,1],\"id\":7841,\"learnedat\":100,\"level\":0,\"name\":\"Swim Speed Potion\",\"nskillup\":1,\"reagents\":[[2452,1],[6370,1],[3371,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":1000,\"quality\":1,popularity:84},{\"cat\":11,\"colors\":[140,165,185,205],\"creates\":[6373,1,1],\"id\":7845,\"learnedat\":140,\"level\":0,\"name\":\"Elixir of Firepower\",\"nskillup\":1,\"reagents\":[[6371,2],[3356,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":3000,\"quality\":1,popularity:145},{\"cat\":11,\"colors\":[90,120,140,160],\"creates\":[6662,1,1],\"id\":8240,\"learnedat\":90,\"level\":0,\"name\":\"Elixir of Giant Growth\",\"nskillup\":1,\"reagents\":[[6522,1],[2449,1],[3371,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:165},{\"cat\":11,\"colors\":[205,220,240,260],\"creates\":[6149,1,1],\"id\":11448,\"learnedat\":205,\"level\":0,\"name\":\"Greater Mana Potion\",\"nskillup\":1,\"reagents\":[[3358,1],[3821,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":8100,\"quality\":1,popularity:152},{\"cat\":11,\"colors\":[185,205,225,245],\"creates\":[8949,1,1],\"id\":11449,\"learnedat\":185,\"level\":0,\"name\":\"Elixir of Agility\",\"nskillup\":1,\"reagents\":[[3820,1],[3821,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":5850,\"quality\":1,popularity:979},{\"cat\":11,\"colors\":[195,215,235,255],\"creates\":[8951,1,1],\"id\":11450,\"learnedat\":195,\"level\":0,\"name\":\"Elixir of Greater Defense\",\"nskillup\":1,\"reagents\":[[3355,1],[3821,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":6750,\"quality\":1,popularity:382},{\"cat\":11,\"colors\":[205,220,240,260],\"creates\":[8956,1,1],\"id\":11451,\"learnedat\":205,\"level\":0,\"name\":\"Oil of Immolation\",\"nskillup\":1,\"reagents\":[[4625,1],[3821,1],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":7200,\"quality\":1,popularity:251},{\"cat\":11,\"colors\":[215,225,245,265],\"creates\":[9030,1,1],\"id\":11452,\"learnedat\":215,\"level\":0,\"name\":\"Restorative Potion\",\"nskillup\":1,\"reagents\":[[7067,1],[3821,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:2509},{\"cat\":11,\"colors\":[210,225,245,265],\"creates\":[9036,1,1],\"id\":11453,\"learnedat\":210,\"level\":0,\"name\":\"Magic Resistance Potion\",\"nskillup\":1,\"reagents\":[[3358,1],[8831,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:379},{\"cat\":11,\"colors\":[210,225,245,265],\"creates\":[9061,1,1],\"id\":11456,\"learnedat\":210,\"level\":0,\"name\":\"Goblin Rocket Fuel\",\"nskillup\":1,\"reagents\":[[4625,1],[9260,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:822},{\"cat\":11,\"colors\":[215,230,250,270],\"creates\":[3928,1,1],\"id\":11457,\"learnedat\":215,\"level\":0,\"name\":\"Superior Healing Potion\",\"nskillup\":1,\"reagents\":[[8838,1],[3358,1],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":9000,\"quality\":1,popularity:745},{\"cat\":11,\"colors\":[225,240,260,280],\"creates\":[9144,1,1],\"id\":11458,\"learnedat\":225,\"level\":0,\"name\":\"Wildvine Potion\",\"nskillup\":1,\"reagents\":[[8153,1],[8831,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:116},{\"cat\":11,\"colors\":[225,240,260,280],\"creates\":[9149,1,1],\"id\":11459,\"learnedat\":225,\"level\":0,\"name\":\"Philosophers' Stone\",\"nskillup\":1,\"reagents\":[[3575,4],[9262,1],[8831,4],[4625,4]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:620},{\"cat\":11,\"colors\":[230,245,265,285],\"creates\":[9154,1,1],\"id\":11460,\"learnedat\":230,\"level\":0,\"name\":\"Elixir of Detect Undead\",\"nskillup\":1,\"reagents\":[[8836,1],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":4500,\"quality\":1,popularity:703},{\"cat\":11,\"colors\":[235,250,270,290],\"creates\":[9155,1,1],\"id\":11461,\"learnedat\":235,\"level\":0,\"name\":\"Arcane Elixir\",\"nskillup\":1,\"reagents\":[[8839,1],[3821,1],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":9000,\"quality\":1,popularity:296},{\"cat\":11,\"colors\":[235,250,270,290],\"creates\":[9172,1,1],\"id\":11464,\"learnedat\":235,\"level\":0,\"name\":\"Invisibility Potion\",\"nskillup\":1,\"reagents\":[[8845,1],[8838,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:327},{\"cat\":11,\"colors\":[235,250,270,290],\"creates\":[9179,1,1],\"id\":11465,\"learnedat\":235,\"level\":0,\"name\":\"Elixir of Greater Intellect\",\"nskillup\":1,\"reagents\":[[8839,1],[3358,1],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":10800,\"quality\":1,popularity:325},{\"cat\":11,\"colors\":[240,255,275,295],\"creates\":[9088,1,1],\"id\":11466,\"learnedat\":240,\"level\":0,\"name\":\"Gift of Arthas\",\"nskillup\":1,\"reagents\":[[8836,1],[8839,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:367},{\"cat\":11,\"colors\":[240,255,275,295],\"creates\":[9187,1,1],\"id\":11467,\"learnedat\":240,\"level\":0,\"name\":\"Elixir of Greater Agility\",\"nskillup\":1,\"reagents\":[[8838,1],[3821,1],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":10800,\"quality\":1,popularity:940},{\"cat\":11,\"colors\":[240,255,275,295],\"creates\":[9197,1,1],\"id\":11468,\"learnedat\":240,\"level\":0,\"name\":\"Elixir of Dream Vision\",\"nskillup\":1,\"reagents\":[[8831,3],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:197},{\"cat\":11,\"colors\":[245,260,280,300],\"creates\":[9206,1,1],\"id\":11472,\"learnedat\":245,\"level\":0,\"name\":\"Elixir of Giants\",\"nskillup\":1,\"reagents\":[[8838,1],[8846,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:338},{\"cat\":11,\"colors\":[245,260,280,300],\"creates\":[9210,1,1],\"id\":11473,\"learnedat\":245,\"level\":0,\"name\":\"Ghost Dye\",\"nskillup\":1,\"reagents\":[[8845,2],[4342,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:546},{\"cat\":11,\"colors\":[250,265,285,305],\"creates\":[9264,1,1],\"id\":11476,\"learnedat\":250,\"level\":0,\"name\":\"Elixir of Shadow Power\",\"nskillup\":1,\"reagents\":[[8845,3],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:463},{\"cat\":11,\"colors\":[250,265,285,305],\"creates\":[9224,1,1],\"id\":11477,\"learnedat\":250,\"level\":0,\"name\":\"Elixir of Demonslaying\",\"nskillup\":1,\"reagents\":[[8846,1],[8845,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:211},{\"cat\":11,\"colors\":[250,265,285,305],\"creates\":[9233,1,1],\"id\":11478,\"learnedat\":250,\"level\":0,\"name\":\"Elixir of Detect Demon\",\"nskillup\":1,\"reagents\":[[8846,2],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":12600,\"quality\":1,popularity:119},{\"cat\":11,\"colors\":[225,240,260,280],\"creates\":[3577,0,0],\"id\":11479,\"learnedat\":225,\"level\":0,\"name\":\"Transmute: Iron to Gold\",\"nskillup\":1,\"reagents\":[[3575,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:80},{\"cat\":11,\"colors\":[225,240,260,280],\"creates\":[6037,0,0],\"id\":11480,\"learnedat\":225,\"level\":0,\"name\":\"Transmute: Mithril to Truesilver\",\"nskillup\":1,\"reagents\":[[3860,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":10000,\"quality\":2,popularity:79},{\"cat\":11,\"id\":11611,\"learnedat\":200,\"level\":0,\"name\":\"Alchemy\",\"nskillup\":1,\"rank\":\"Artisan\",\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":45000,popularity:451},{\"cat\":11,\"colors\":[200,220,240,260],\"creates\":[10592,1,1],\"id\":12609,\"learnedat\":200,\"level\":0,\"name\":\"Catseye Elixir\",\"nskillup\":1,\"reagents\":[[3821,1],[3818,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":7200,\"quality\":1,popularity:108},{\"cat\":11,\"colors\":[230,245,265,285],\"creates\":[12190,1,1],\"id\":15833,\"learnedat\":230,\"level\":0,\"name\":\"Dreamless Sleep Potion\",\"nskillup\":1,\"reagents\":[[8831,3],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":9000,\"quality\":1,popularity:140},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[12360,0,0],\"id\":17187,\"learnedat\":275,\"level\":45,\"name\":\"Transmute: Arcanite\",\"nskillup\":1,\"reagents\":[[12359,1],[12363,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:2954},{\"cat\":11,\"colors\":[250,250,255,260],\"creates\":[13423,1,1],\"id\":17551,\"learnedat\":250,\"level\":0,\"name\":\"Stonescale Oil\",\"nskillup\":1,\"reagents\":[[13422,1],[3372,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":13500,\"quality\":1,popularity:1983},{\"cat\":11,\"colors\":[255,270,290,310],\"creates\":[13442,1,1],\"id\":17552,\"learnedat\":255,\"level\":0,\"name\":\"Mighty Rage Potion\",\"nskillup\":1,\"reagents\":[[8846,3],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:462},{\"cat\":11,\"colors\":[260,275,295,315],\"creates\":[13443,1,1],\"id\":17553,\"learnedat\":260,\"level\":0,\"name\":\"Superior Mana Potion\",\"nskillup\":1,\"reagents\":[[8838,2],[8839,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:776},{\"cat\":11,\"colors\":[265,280,300,320],\"creates\":[13445,1,1],\"id\":17554,\"learnedat\":265,\"level\":0,\"name\":\"Elixir of Superior Defense\",\"nskillup\":1,\"reagents\":[[13423,2],[8838,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:462},{\"cat\":11,\"colors\":[270,285,305,325],\"creates\":[13447,1,1],\"id\":17555,\"learnedat\":270,\"level\":0,\"name\":\"Elixir of the Sages\",\"nskillup\":1,\"reagents\":[[13463,1],[13466,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:507},{\"cat\":11,\"colors\":[275,290,310,330],\"creates\":[13446,1,1],\"id\":17556,\"learnedat\":275,\"level\":0,\"name\":\"Major Healing Potion\",\"nskillup\":1,\"reagents\":[[13464,2],[13465,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:2123},{\"cat\":11,\"colors\":[275,290,310,330],\"creates\":[13453,1,1],\"id\":17557,\"learnedat\":275,\"level\":0,\"name\":\"Elixir of Brute Force\",\"nskillup\":1,\"reagents\":[[8846,2],[13466,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:345},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[7078,0,0],\"id\":17559,\"learnedat\":275,\"level\":45,\"name\":\"Transmute: Air to Fire\",\"nskillup\":1,\"reagents\":[[7082,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:136},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[7076,0,0],\"id\":17560,\"learnedat\":275,\"level\":0,\"name\":\"Transmute: Fire to Earth\",\"nskillup\":1,\"reagents\":[[7078,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:157},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[7080,0,0],\"id\":17561,\"learnedat\":275,\"level\":0,\"name\":\"Transmute: Earth to Water\",\"nskillup\":1,\"reagents\":[[7076,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:529},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[7082,0,0],\"id\":17562,\"learnedat\":275,\"level\":0,\"name\":\"Transmute: Water to Air\",\"nskillup\":1,\"reagents\":[[7080,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:166},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[7080,0,0],\"id\":17563,\"learnedat\":275,\"level\":0,\"name\":\"Transmute: Undeath to Water\",\"nskillup\":1,\"reagents\":[[12808,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:1075},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[12808,0,0],\"id\":17564,\"learnedat\":275,\"level\":0,\"name\":\"Transmute: Water to Undeath\",\"nskillup\":1,\"reagents\":[[7080,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:124},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[7076,0,0],\"id\":17565,\"learnedat\":275,\"level\":0,\"name\":\"Transmute: Life to Earth\",\"nskillup\":1,\"reagents\":[[12803,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:293},{\"cat\":11,\"colors\":[275,275,282,290],\"creates\":[12803,0,0],\"id\":17566,\"learnedat\":275,\"level\":0,\"name\":\"Transmute: Earth to Life\",\"nskillup\":1,\"reagents\":[[7076,1]],\"schools\":1,\"skill\":[171],\"quality\":2,popularity:110},{\"cat\":11,\"colors\":[280,295,315,335],\"creates\":[13455,1,1],\"id\":17570,\"learnedat\":280,\"level\":0,\"name\":\"Greater Stoneshield Potion\",\"nskillup\":1,\"reagents\":[[13423,3],[10620,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:283},{\"cat\":11,\"colors\":[280,295,315,335],\"creates\":[13452,1,1],\"id\":17571,\"learnedat\":280,\"level\":0,\"name\":\"Elixir of the Mongoose\",\"nskillup\":1,\"reagents\":[[13465,2],[13466,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:755},{\"cat\":11,\"colors\":[285,300,320,340],\"creates\":[13462,1,1],\"id\":17572,\"learnedat\":285,\"level\":0,\"name\":\"Purification Potion\",\"nskillup\":1,\"reagents\":[[13467,2],[13466,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:144},{\"cat\":11,\"colors\":[285,300,320,340],\"creates\":[13454,1,1],\"id\":17573,\"learnedat\":285,\"level\":0,\"name\":\"Greater Arcane Elixir\",\"nskillup\":1,\"reagents\":[[13463,3],[13465,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:1129},{\"cat\":11,\"colors\":[290,305,325,345],\"creates\":[13457,1,1],\"id\":17574,\"learnedat\":290,\"level\":0,\"name\":\"Greater Fire Protection Potion\",\"nskillup\":1,\"reagents\":[[7068,1],[13463,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:1387},{\"cat\":11,\"colors\":[290,305,325,345],\"creates\":[13456,1,1],\"id\":17575,\"learnedat\":290,\"level\":0,\"name\":\"Greater Frost Protection Potion\",\"nskillup\":1,\"reagents\":[[7070,1],[13463,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:315},{\"cat\":11,\"colors\":[290,305,325,345],\"creates\":[13458,1,1],\"id\":17576,\"learnedat\":290,\"level\":0,\"name\":\"Greater Nature Protection Potion\",\"nskillup\":1,\"reagents\":[[7067,1],[13463,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:239},{\"cat\":11,\"colors\":[290,305,325,345],\"creates\":[13461,1,1],\"id\":17577,\"learnedat\":290,\"level\":0,\"name\":\"Greater Arcane Protection Potion\",\"nskillup\":1,\"reagents\":[[11176,1],[13463,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:244},{\"cat\":11,\"colors\":[290,305,325,345],\"creates\":[13459,1,1],\"id\":17578,\"learnedat\":290,\"level\":0,\"name\":\"Greater Shadow Protection Potion\",\"nskillup\":1,\"reagents\":[[3824,1],[13463,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:375},{\"cat\":11,\"colors\":[295,310,330,350],\"creates\":[13444,1,1],\"id\":17580,\"learnedat\":295,\"level\":0,\"name\":\"Major Mana Potion\",\"nskillup\":1,\"reagents\":[[13463,3],[13467,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:1024},{\"cat\":11,\"colors\":[300,315,322,330],\"creates\":[13503,1,1],\"id\":17632,\"learnedat\":300,\"level\":0,\"name\":\"Alchemist's Stone\",\"nskillup\":1,\"reagents\":[[7078,8],[7076,8],[7082,8],[7080,8],[12803,8],[9262,2],[13468,4]],\"schools\":1,\"skill\":[171],\"quality\":4,popularity:831},{\"cat\":11,\"colors\":[300,315,322,330],\"creates\":[13506,1,1],\"id\":17634,\"learnedat\":300,\"level\":0,\"name\":\"Flask of Petrification\",\"nskillup\":1,\"reagents\":[[13423,30],[13465,10],[13468,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:219},{\"cat\":11,\"colors\":[300,315,322,330],\"creates\":[13510,1,1],\"id\":17635,\"learnedat\":300,\"level\":0,\"name\":\"Flask of the Titans\",\"nskillup\":1,\"reagents\":[[8846,30],[13423,10],[13468,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:471},{\"cat\":11,\"colors\":[300,315,322,330],\"creates\":[13511,1,1],\"id\":17636,\"learnedat\":300,\"level\":0,\"name\":\"Flask of Distilled Wisdom\",\"nskillup\":1,\"reagents\":[[13463,30],[13467,10],[13468,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:492},{\"cat\":11,\"colors\":[300,315,322,330],\"creates\":[13512,1,1],\"id\":17637,\"learnedat\":300,\"level\":0,\"name\":\"Flask of Supreme Power\",\"nskillup\":1,\"reagents\":[[13463,30],[13465,10],[13468,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:719},{\"cat\":11,\"colors\":[300,315,322,330],\"creates\":[13513,1,1],\"id\":17638,\"learnedat\":300,\"level\":0,\"name\":\"Flask of Chromatic Resistance\",\"nskillup\":1,\"reagents\":[[13467,30],[13465,10],[13468,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:245},{\"cat\":11,\"colors\":[190,210,230,250],\"creates\":[17708,1,1],\"id\":21923,\"learnedat\":190,\"level\":0,\"name\":\"Elixir of Frost Power\",\"nskillup\":1,\"reagents\":[[3819,2],[3358,1],[3372,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:370},{\"cat\":11,\"colors\":[300,310,320,330],\"creates\":[18253,1,1],\"id\":22732,\"learnedat\":300,\"level\":0,\"name\":\"Major Rejuvenation Potion\",\"nskillup\":1,\"reagents\":[[10286,1],[13464,4],[13463,4],[18256,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:404},{\"cat\":11,\"colors\":[215,230,250,270],\"creates\":[18294,1,1],\"id\":22808,\"learnedat\":215,\"level\":0,\"name\":\"Elixir of Greater Water Breathing\",\"nskillup\":1,\"reagents\":[[7972,1],[8831,2],[8925,1]],\"schools\":1,\"skill\":[171],\"source\":[6],\"trainingcost\":9000,\"quality\":1,popularity:92},{\"cat\":11,\"colors\":[0,315,322,330],\"creates\":[19931,3,3],\"id\":24266,\"learnedat\":315,\"level\":0,\"name\":\"Gurubashi Mojo Madness\",\"nskillup\":1,\"reagents\":[[12938,1],[19943,1],[12804,6],[13468,1]],\"schools\":1,\"skill\":[171],\"quality\":3,popularity:852},{\"cat\":11,\"colors\":[275,290,310,330],\"creates\":[20007,1,1],\"id\":24365,\"learnedat\":275,\"level\":0,\"name\":\"Mageblood Potion\",\"nskillup\":1,\"reagents\":[[13463,1],[13466,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:662},{\"cat\":11,\"colors\":[275,290,310,330],\"creates\":[20002,1,1],\"id\":24366,\"learnedat\":275,\"level\":0,\"name\":\"Greater Dreamless Sleep Potion\",\"nskillup\":1,\"reagents\":[[13463,2],[13464,1],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:162,contentPhase:4},{\"cat\":11,\"colors\":[285,300,320,340],\"creates\":[20008,1,1],\"id\":24367,\"learnedat\":285,\"level\":0,\"name\":\"Living Action Potion\",\"nskillup\":1,\"reagents\":[[13467,2],[13465,2],[10286,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:773},{\"cat\":11,\"colors\":[290,305,325,345],\"creates\":[20004,1,1],\"id\":24368,\"learnedat\":290,\"level\":0,\"name\":\"Major Troll's Blood Potion\",\"nskillup\":1,\"reagents\":[[8846,1],[13466,2],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:197},{\"cat\":11,\"colors\":[300,301,305,310],\"creates\":[7068,3,3],\"id\":25146,\"learnedat\":300,\"level\":45,\"name\":\"Transmute: Elemental Fire\",\"nskillup\":1,\"reagents\":[[7077,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:1171},{\"cat\":11,\"colors\":[250,265,285,305],\"creates\":[21546,1,1],\"id\":26277,\"learnedat\":250,\"level\":0,\"name\":\"Elixir of Greater Firepower\",\"nskillup\":1,\"reagents\":[[6371,3],[4625,3],[8925,1]],\"schools\":1,\"skill\":[171],\"quality\":1,popularity:527}]";

            List<SpellInfo> spellInfoList = null;
            int spellCount = parseSpellInfo(html, out spellInfoList);
            if (spellCount > 0)
            {
                foreach (var spellInfo in spellInfoList)
                {
                    logs += string.Format("id = {0}, name = {1}, colors = {2}\r\n", spellInfo.id, spellInfo.name, spellInfo.colors);
                }
                logs += "\r\nSuccess.\r\n";
            }
            else
            {
                logs += "\r\nFailed.\r\n";
            }

            success = 0;
            failed = 0;
            return logs;
        }

        public string Start(Professoion type, out int success, out int failed)
        {
            string logs = GetherSpell(type, out success, out failed);
            return logs;
        }
    }
}
