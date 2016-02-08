using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendMessage
{
    internal static class RussiaLanguage
    {
        /// <summary>
        /// converts rus symbols to UCS2
        /// </summary>
        /// <param name="txtInRus"></param>
        /// <returns></returns>
        internal static string ConvertRusToUCS2(string txtInRus)
        {
            //строка с алфавитом
            String strAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЪЭЮЯабвгдеёжзийклмнопрстуфхцчшщэюяABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'-* :;)(.,!=_";
            //массив для конвертирования русских букв и цифр в UCS2 
            String[] ArrayUCSCode = new String[137]{
             "0410","0411","0412","0413","0414","0415","00A8","0416","0417",
             "0418","0419","041A","041B","041C","041D","041E","041F","0420",
             "0421","0422","0423","0424","0425","0426","0427","0428","0429",
             "042C","042A","042D","042E","042F","0430","0431","0432","0433",
             "0434","0435","00B8","0436","0437","0438","0439","043A","043B",
             "043C","043D","043E","043F","0440","0441","0442","0443","0444",
             "0445","0446","0447","0448","0449","044D","044E","044F","0041",
             "0042","0043","0044","0045","0046","0047","0048","0049","004A",
             "004B","004C","004D","004E","004F","0050","0051","0052","0053",
             "0054","0055","0056","0057","0058","0059","005A","0061","0062",
             "0063","0064","0065","0066","0067","0068","0069","006A","006B",
             "006C","006D","006E","006F","0070","0071","0072","0073","0074",
             "0075","0076","0077","0078","0079","007A","0030","0031","0032",
             "0033","0034","0035","0036","0037","0038","0039","0027","002D",
             "002A","0020","003A","003B","0029","0028","002E","002C","0021",
             "003D","005F"};
            StringBuilder UCS = new StringBuilder(txtInRus.Length);
            Int32 intLetterIndex = 0;
            for (int i = 0; i < txtInRus.Length; i++)
            {
                intLetterIndex = strAlphabet.IndexOf(txtInRus[i]);
                if (intLetterIndex != -1)
                {
                    UCS.Append(ArrayUCSCode[intLetterIndex]);
                }
            }
            return UCS.ToString();
        }
    }
}
