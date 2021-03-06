using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bogus.Vendor
{
    internal class UserAgentGenerator
    {
        private readonly Func<Randomizer> random;

        internal UserAgentGenerator(Func<Randomizer> random)
        {
            this.random = random;
        }

        private Randomizer Random => random();

        internal string VersionString(string type, string delim = ".")
        {
            if (type == "net")
            {
                return $"{this.Random.Number(1, 4)}.{this.Random.Number(0, 9)}.{this.Random.Number(10000, 99999)}.{this.Random.Number(0, 9)}";
            }
            if (type == "nt")
            {
                return $"{this.Random.Number(5, 6)}.{this.Random.Number(0, 3)}";
            }
            if (type == "trident")
            {
                return $"{this.Random.Number(3, 7)}.{this.Random.Number(0, 1)}";
            }
            if (type == "osx")
            {
                return $"10{delim}{this.Random.Number(5, 10)}{delim}{this.Random.Number(0, 9)}";
            }
            if (type == "chrome")
            {
                return $"{this.Random.Number(13, 39)}.0.{this.Random.Number(800, 899)}.0";
            }
            if (type == "presto")
            {
                return $"2.9.{this.Random.Number(160, 190)}";
            }
            if (type == "presto2")
            {
                return $"{this.Random.Number(10, 12)}.00";
            }
            if (type == "safari")
            {
                return $"{this.Random.Number(531, 538)}.{this.Random.Number(0, 2)}.{this.Random.Number(0, 2)}";
            }
            throw new ArgumentOutOfRangeException($"{nameof(type)} is not valid.");
        }

        internal string RandomRevision(int dots)
        {
            var ver = string.Empty;
            for (int i = 0; i < dots; i++)
            {
                ver += "." + this.Random.Number(0, 9);
            }
            return ver;
        }

        private string RandomLanguage()
        {
            var languages = new[]
                {
                    "AB", "AF", "AN", "AR", "AS", "AZ", "BE", "BG", "BN", "BO", "BR", "BS", "CA", "CE", "CO", "CS",
                    "CU", "CY", "DA", "DE", "EL", "EN", "EO", "ES", "ET", "EU", "FA", "FI", "FJ", "FO", "FR", "FY",
                    "GA", "GD", "GL", "GV", "HE", "HI", "HR", "HT", "HU", "HY", "ID", "IS", "IT", "JA", "JV", "KA",
                    "KG", "KO", "KU", "KW", "KY", "LA", "LB", "LI", "LN", "LT", "LV", "MG", "MK", "MN", "MO", "MS",
                    "MT", "MY", "NB", "NE", "NL", "NN", "NO", "OC", "PL", "PT", "RM", "RO", "RU", "SC", "SE", "SK",
                    "SL", "SO", "SQ", "SR", "SV", "SW", "TK", "TR", "TY", "UK", "UR", "UZ", "VI", "VO", "YI", "ZH"
                };

            return this.Random.ArrayElement(languages);
        }


        internal string RandomBrowser()
        {
            return this.Random.WeightedRandom(BrowserNames, BrowserWeights);
        }

        private const string BrowserUsageJson =
            @"
    {
        'chrome':    0.45132810566,
        'iexplorer': 0.27477061836,
        'firefox':   0.19384170608,
        'safari':    0.06186781118,
        'opera':     0.01574236955
    }";

        private static readonly JObject BrowserUsage = JObject.Parse(BrowserUsageJson);
        private static readonly string[] BrowserNames = BrowserUsage.Properties().Select(p => p.Name).ToArray();
        private static readonly float[] BrowserWeights = BrowserUsage.PropertyValues().Select(pv => pv.Value<float>()).ToArray();

        private const string BrowserOsUsageJson =
            @"
{
        'chrome':  {'win': 0.89, 'mac': 0.09, 'lin': 0.02},
        'firefox': {'win': 0.83, 'mac': 0.16, 'lin': 0.01},
        'opera':   {'win': 0.91, 'mac': 0.03, 'lin': 0.06},
        'safari':  {'win': 0.04, 'mac': 0.96  },
        'iexplorer':  {'win': 1}
}
";
        private static readonly JObject BrowserOsUsage = JObject.Parse(BrowserOsUsageJson);

        internal string RandomOS(string browser)
        {
            //return random OS weighted by browser.

            var osList = BrowserOsUsage[browser] as JObject;

            var osNames = osList.Properties().Select(p => p.Name).ToArray();

            var weights = osList.PropertyValues().Select(pv => pv.Value<float>()).ToArray();

            return this.Random.WeightedRandom(osNames, weights);
        }

        private const string ProcJson = @"
    {
        lin: {'i686':   0.50, 'x86_64':  0.50 },
        mac: {'Intel':  0.48, 'PPC':     0.01, 'U; Intel':  0.48, 'U; PPC':  0.01},
        win: {'':       0.33, 'WOW64':   0.33, 'Win64; x64':0.33}
    }
";
        private static readonly JObject Proc = JObject.Parse(ProcJson);

        internal string RandomProc(string os)
        {
            var procList = Proc[os] as JObject;

            var procNames = procList.Properties().Select(p => p.Name).ToArray();
            var procWeights = procList.PropertyValues().Select(pv => pv.Value<float>()).ToArray();

            return this.Random.WeightedRandom(procNames, procWeights);
        }

        internal string BrowserAgent(string browser, string arch)
        {
            if (browser == "firefox")
            {
                var firefox_ver = this.Random.Number(5, 15) + RandomRevision(2);
                var gecko_ver = "Gecko/20100101 Firefox/" + firefox_ver;
                var proc = RandomProc(arch);
                var os_ver = (arch == "win")
                    ? "(Windows NT " + VersionString("nt") + (proc != "" ? "; " + proc : "")
                    : (arch == "mac")
                        ? "(Macintosh; " + proc + " Mac OS X " + VersionString("osx")
                        : "(X11; Linux " + proc;


                return "Mozilla/5.0 " + os_ver + "; rv:" + firefox_ver.Substring(0, firefox_ver.Length - 2) + ") " + gecko_ver;
            }
            if( browser == "chrome" )
            {
                var safari = VersionString("safari");
                var os_ver = (arch == "mac")
                    ? "(Macintosh; " + RandomProc("mac") + " Mac OS X " + VersionString("osx", "_") + ") "
                    : (arch == "win")
                        ? "(Windows; U; Windows NT " + VersionString("nt") + ")"
                        : "(X11; Linux " + RandomProc(arch);

                return "Mozilla/5.0 " + os_ver + " AppleWebKit/" + safari + " (KHTML, like Gecko) Chrome/" + VersionString("chrome") + " Safari/" + safari;
            }
            if( browser == "opera" )
            {
                var presto_ver = " Presto/" + VersionString("presto") + " Version/" + VersionString("presto2") + ")";
                var os_ver = (arch == "win")
                    ? "(Windows NT " + VersionString("nt") + "; U; " + RandomLanguage() + presto_ver
                    : (arch == "lin")
                        ? "(X11; Linux " + RandomProc(arch) + "; U; " + RandomLanguage() + presto_ver
                        : "(Macintosh; Intel Mac OS X " + VersionString("osx") + " U; " + RandomLanguage() + " Presto/" +
                          VersionString("presto") + " Version/" + VersionString("presto2") + ")";

                return "Opera/" + this.Random.Number(9, 14) + "." + this.Random.Number(0, 99) + " " + os_ver;
            }
            if( browser == "safari" )
            {
                var safari = VersionString("safari");
                var ver = this.Random.Number(4, 7) + "." + this.Random.Number(0, 1) + "." + this.Random.Number(0, 10);
                var os_ver = (arch == "mac")
                    ? "(Macintosh; " + RandomProc("mac") + " Mac OS X " + VersionString("osx", "_") + " rv:" + this.Random.Number(2, 6) + ".0; " + RandomLanguage() + ") "
                    : "(Windows; U; Windows NT " + VersionString("nt") + ")";

                return "Mozilla/5.0 " + os_ver + "AppleWebKit/" + safari + " (KHTML, like Gecko) Version/" + ver + " Safari/" + safari;
            }
            if( browser == "iexplorer" )
            {
                var ver = this.Random.Number(7, 11);

                if (ver >= 11)
                {
                    //http://msdn.microsoft.com/en-us/library/ie/hh869301(v=vs.85).aspx
                    return "Mozilla/5.0 (Windows NT 6." + this.Random.Number(1, 3) + "; Trident/7.0; " + this.Random.ArrayElement(new[] {"Touch; ", ""}) +"rv:11.0) like Gecko";
                }

                //http://msdn.microsoft.com/en-us/library/ie/ms537503(v=vs.85).aspx
                return "Mozilla/5.0 (compatible; MSIE " + ver + ".0; Windows NT " + VersionString("nt") + "; Trident/" +
                       VersionString("trident")+ ((this.Random.Number(0, 1) == 1) ? "; .NET CLR " + VersionString("net") : "") + ")";
            }
            return string.Empty;
        }




        public string Generate()
        {
            var browser = RandomBrowser();
            var os = RandomOS(browser);

            return BrowserAgent(browser, os);
        }
    }
}