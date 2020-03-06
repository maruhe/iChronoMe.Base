using System.Collections.Generic;

namespace iChronoMe.Core.Classes
{
    public static class Contributors
    {
        private static List<ContributorInfo> _allCredits;
        public static List<ContributorInfo> AllCredits
        {
            get
            {
                if (_allCredits == null)
                {
                    _allCredits = new List<ContributorInfo>();
                    _allCredits.Add(new ContributorInfo { 
                        Name = "GitHub", 
                        Description = "pleasure to have the option to code all together as once", 
                        WebLink = "https://github.com" 
                    });

                    _allCredits.Add(new ContributorInfo { 
                        License = "MIT", 
                        Name = "Yort.Ntp", 
                        Description = "our interface to GMT 0", 
                        WebLink = "https://github.com/Yortw/Yort.Ntp" 
                    });
                    /*_allCredits.Add(new ContributorInfo
                    {
                        License = "MIT",
                        Name = "Newtonsoft.Json",
                        Description = "JSON input/output in high-performance",
                        WebLink = "https://github.com/JamesNK/Newtonsoft.Json"
                    });*/
                    //_allCredits.Add(new CreditInfo { License = "MIT", Name = "TimeZoneConverter", Description = "for Windows-Users", WebLink = "https://github.com/mj1856/TimeZoneConverter" });

                    _allCredits.Add(new ContributorInfo { 
                        Name = "icons8.com", 
                        Description = "more icons than I ever could implement", 
                        WebLink = "https://icons8.com" });
                }

                return _allCredits;
            }
        }
    }

    public class ContributorInfo
    {
        public string Name { get; set; }
        public string License { get; set; }
        public string Description { get; set; }
        public string LongInfoText { get; set; }
        public string WebLink { get; set; }
    }
}
