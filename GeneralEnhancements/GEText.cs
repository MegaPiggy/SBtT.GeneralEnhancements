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
            switch(language)
            {
                case Language.FRENCH:
                    return "Passer à la Fusée";
                    break;
                case Language.GERMAN:
                    return "Überspringen zum Schiff";
                    break;
                case Language.ITALIAN:
                    return "Salta sulla Nave";
                    break;
                case Language.JAPANESE:
                    return "探査艇にスキップしてください";
                    break;
                case Language.KOREAN:
                    return "우주선송으로 건너뛰기";
                    break;
                case Language.POLISH:
                    return "Pomiń do Kosmolotnia";
                    break;
                case Language.PORTUGUESE_BR:
                    return "Pular para o Nave";
                    break;
                case Language.RUSSIAN:
                    return "Пропустить к Корабль";
                    break;
                case Language.CHINESE_SIMPLE:
                    return "跳至第宇宙飞船";
                    break;
                case Language.SPANISH_LA:
                    return "Saltar al Astronave";
                    break;
                case Language.TURKISH:
                    return "Gemi'a geç";
                    break;
            }
            
            return "Skip to Ship";
        }
        public static string SkipToStranger()
        {
            switch(language)
            {
                case Language.FRENCH:
                    return "Passer à l'Étranger";
                    break;
                case Language.GERMAN:
                    return "Überspringen zum Der Fremdling";
                    break;
                case Language.ITALIAN:
                    return "Salta sullo Straniero";
                    break;
                case Language.JAPANESE:
                    return "流れ者にスキップしてください";
                    break;
                case Language.KOREAN:
                    return "스트레인저송으로 건너뛰기";
                    break;
                case Language.POLISH:
                    return "Pomiń do Nieznajomy";
                    break;
                case Language.PORTUGUESE_BR:
                    return "Pular para o Desconhecido";
                    break;
                case Language.RUSSIAN:
                    return "Пропустить к Незнакомец";
                    break;
                case Language.CHINESE_SIMPLE:
                    return "跳至第外星站";
                    break;
                case Language.SPANISH_LA:
                    return "Saltar al Forastero";
                    break;
                case Language.TURKISH:
                    return "Yabancı'a geç";
                    break;
            }
            
            return "Skip to Stranger";
        }

    }
}
