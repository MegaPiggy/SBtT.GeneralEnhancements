using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TextTranslation;

namespace GeneralEnhancements
{
    public static class GEText
    {
        static TextTranslation.Language language => PlayerData.GetSavedLanguage();
        public static string SkipToShip()
        {
            switch (language)
            {
                case Language.FRENCH:
                    return "Passer à la Fusée";
                case Language.GERMAN:
                    return "Überspringen zum Schiff";
                case Language.ITALIAN:
                    return "Salta sulla Nave";
                case Language.JAPANESE:
                    return "探査艇にスキップしてください";
                case Language.KOREAN:
                    return "우주선송으로 건너뛰기";
                case Language.POLISH:
                    return "Pomiń do Kosmolotnia";
                case Language.PORTUGUESE_BR:
                    return "Pular para o Nave";
                case Language.RUSSIAN:
                    return "Пропустить к Корабль";
                case Language.CHINESE_SIMPLE:
                    return "跳至第宇宙飞船";
                case Language.SPANISH_LA:
                    return "Saltar al Astronave";
                case Language.TURKISH:
                    return "Gemi'a geç";
                default:
                    switch (language.ToString())
                    {
                        case "Íslenska":
                            return "Sleppa í Skip";
                        case "Czech":
                            return "Zrychlit na Loď";
                        case "Andalûh":
                            return "Çartâh al Âttronabe";
                        case "Euskara":
                            return "Saltatu ontzira";
                        default:
                            return "Skip to Ship";
                    }
            }
        }
        public static string SkipToStranger()
        {
            switch (language)
            {
                case Language.FRENCH:
                    return "Passer à l'Étranger";
                case Language.GERMAN:
                    return "Überspringen zum Der Fremdling";
                case Language.ITALIAN:
                    return "Salta sullo Straniero";
                case Language.JAPANESE:
                    return "流れ者にスキップしてください";
                case Language.KOREAN:
                    return "스트레인저송으로 건너뛰기";
                case Language.POLISH:
                    return "Pomiń do Nieznajomy";
                case Language.PORTUGUESE_BR:
                    return "Pular para o Desconhecido";
                case Language.RUSSIAN:
                    return "Пропустить к Незнакомец";
                case Language.CHINESE_SIMPLE:
                    return "跳至第外星站";
                case Language.SPANISH_LA:
                    return "Saltar al Forastero";
                case Language.TURKISH:
                    return "Yabancı'a geç";
                default:
                    switch (language.ToString())
                    {
                        case "Íslenska":
                            return "Sleppa í Gesturinn";
                        case "Czech":
                            return "Zrychlit na Cizinec";
                        case "Andalûh":
                            return "Çartâh ar Forâttero";
                        case "Euskara":
                            return "Saltatu kanpotarrarengana";
                        default:
                            return "Skip to Stranger";
                    }
            }
        }
    }
}
