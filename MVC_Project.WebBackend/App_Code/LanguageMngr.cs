﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Linq;
using System.Web;
using MVC_Project.Resources;

namespace MVC_Project.WebBackend.App_Code
{
    public class LanguageMngr
    {
        public static List<Language> AvailableLanguages = new List<Language> {
            new Language {
                LanguageFullName = "Español", LanguageCultureName = "es-MX"
            },
            new Language {
                LanguageFullName = "English", LanguageCultureName = "en"
            }
        };
        public static bool IsLanguageAvailable(string lang)
        {
            return AvailableLanguages.Where(a => a.LanguageCultureName.Equals(lang)).FirstOrDefault() != null ? true : false;
        }
        public static string GetDefaultLanguage()
        {
            return AvailableLanguages[0].LanguageCultureName;
        }
        public static void SetDefaultLanguage()
        {
            SetLanguage(GetDefaultLanguage());
        }
        public static void SetLanguage(string lang)
        {
            try
            {
                if (!IsLanguageAvailable(lang)) lang = GetDefaultLanguage();
                var cultureInfo = new CultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
                ViewLabels.Culture = cultureInfo;
                HttpCookie langCookie = new HttpCookie("culture", lang);
                langCookie.Expires = DateTime.Now.AddYears(1);
                HttpContext.Current.Response.Cookies.Add(langCookie);
            }
            catch (Exception) { }
        }
    }
    public class Language
    {
        public string LanguageFullName
        {
            get;
            set;
        }
        public string LanguageCultureName
        {
            get;
            set;
        }
    }
}