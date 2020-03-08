using System;
using System.Collections.Generic;
using System.Text;

namespace iChronoMe.Core.Classes
{
    public class FAQ
    {
        private static Dictionary<string, string> _faqList = null;
        public static Dictionary<string, string> FaqList
        {
            get
            {
                if (_faqList == null)
                {
                    _faqList = new Dictionary<string, string>();
                    try
                    {
                        var type = typeof(localize);
                        foreach (var fld in type.GetProperties())
                        {
                            if (fld.Name.StartsWith("faq_item_") && !fld.Name.EndsWith("_value"))
                            {
                                var val = type.GetProperty(fld.Name + "_value");
                                if (val != null)
                                {
                                    _faqList.Add((string)fld.GetValue(null), (string)val.GetValue(null));
                                }
                            }
                        }
                    } catch { }
                }
                return _faqList;
            }
        }
    }
}
