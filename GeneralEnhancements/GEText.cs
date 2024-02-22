using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneralEnhancements
{
    public static class GEText
    {
        static TextTranslation.Language language => PlayerData.GetSavedLanguage();
        public static string SkipToShip()
        {
            return "Skip to Ship";
        }
        public static string SkipToStranger()
        {
            return "Skip to Stranger";
        }

    }
}