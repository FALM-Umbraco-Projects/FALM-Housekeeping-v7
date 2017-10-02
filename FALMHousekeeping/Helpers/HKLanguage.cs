using System;
using System.IO;
using System.Web;
using FALM.Housekeeping.Constants;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace FALM.Housekeeping.Helpers
{
    /// <summary>
    /// Checks and/or installs language settings on first request
    /// HkLanguageInstaller
    /// </summary>
    public class HkLanguageInstaller
    {
        private static bool _executed;

        /// <summary>
        /// We need to add the text label on the actions otherwise they don't appear on the context menu,
        /// Check each label and if not in the Umbraco langua file, add it to the actions node
        /// </summary>
        public static void CheckAndInstallLanguageActions()
        {
            if (!_executed)
            {
                InstallLanguageKey("sections", HkConstants.Application.Alias, HkConstants.Application.Name);
                _executed = true;
            }
        }

        private static bool KeyMissing(string area, string key)
        {
            //return ui.GetText(area, key) == string.Format("[{0}]", key);
            string keyValue = String.Format("{0}/{1}", area, key);
            return ApplicationContext.Current.Services.TextService.Localize(keyValue) == string.Format("[{0}]", key);
        }

        /// <summary>
        /// Loop through the language config folder and add language nodes to the language files
        /// If the language is not in our language file install the english variant.
        /// </summary>
        private static void InstallLanguageKey(string area, string key, string value)
        {
            if (KeyMissing(area, key))
            {
                var directory = HttpContext.Current.Server.MapPath(FormatUrl("/config/lang"));
                var languageFiles = Directory.GetFiles(directory);

                foreach (var languagefile in languageFiles)
                {
                    try
                    {
                        var langcode = Path.GetFileNameWithoutExtension(languagefile);
                        UpdateActionsForLanguageFile(string.Format("{0}.xml", langcode), area, key, value);

                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<HkLanguageInstaller>("NewPackage Error in language installer", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Update a language file withe the language xml
        /// </summary>
        private static void UpdateActionsForLanguageFile(string languageFile, string area, string key, string value)
        {
            var doc = XmlHelper.OpenAsXmlDocument(string.Format("{0}/config/lang/{1}", GlobalSettings.Path, languageFile));
            var actionNode = doc.SelectSingleNode(string.Format("//area[@alias='{0}']", area));

            if (actionNode != null)
            {
                var node = actionNode.AppendChild(doc.CreateElement("key"));
                if (node.Attributes != null)
                {
                    var att = node.Attributes.Append(doc.CreateAttribute("alias"));
                    att.InnerText = key;
                }
                node.InnerText = value;
            }

            doc.Save(HttpContext.Current.Server.MapPath(string.Format("{0}/config/lang/{1}", GlobalSettings.Path, languageFile)));
        }

        /// <summary>
        /// Returns the url with the correct Umbraco folder
        /// </summary>
        /// <param name="url">url</param>
        /// <returns>string</returns>
        private static string FormatUrl(string url)
        {
            return VirtualPathUtility.ToAbsolute(GlobalSettings.Path + url);
        }

    }
}