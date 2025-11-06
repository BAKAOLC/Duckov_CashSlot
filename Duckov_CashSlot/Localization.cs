using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov_CashSlot
{
    public static class Localization
    {
        private static readonly object Sync = new();
        private static bool _initialized;

        private static SystemLanguage _currentLanguage = SystemLanguage.English;
        private static Dictionary<string, string> _active = new();
        private static Dictionary<string, string> _en = new();
        private static Dictionary<string, string> _cn = new();

        public static void Initialize()
        {
            if (_initialized) return;
            lock (Sync)
            {
                if (_initialized) return;

                try
                {
                    // Load base fallbacks first
                    _en = LoadLanguageFile(SystemLanguage.English);
                    _cn = LoadLanguageFile(SystemLanguage.ChineseSimplified);
                    _active = _en;

                    // Subscribe to language change and set initial language
                    LocalizationManager.OnSetLanguage += OnSetLanguage;
                    var lang = LocalizationManager.CurrentLanguage;
                    SetLanguage(lang);

                    ModLogger.Log("Localization initialized with JSON files.");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"Localization initialization failed: {ex}");
                }

                _initialized = true;
            }
        }

        public static void Uninitialize()
        {
            if (!_initialized) return;
            lock (Sync)
            {
                try
                {
                    LocalizationManager.OnSetLanguage -= OnSetLanguage;
                }
                catch (Exception ex)
                {
                    ModLogger.LogWarning($"Error while unsubscribing localization events: {ex.Message}");
                }
                finally
                {
                    _initialized = false;
                }
            }
        }

        private static void OnSetLanguage(SystemLanguage language)
        {
            SetLanguage(language);
        }

        public static void SetLanguage(SystemLanguage language)
        {
            if (_currentLanguage == language) return;

            lock (Sync)
            {
                _currentLanguage = language;
                try
                {
                    _active = LoadLanguageFile(language);
                    ModLogger.Log($"Localization language set to {_currentLanguage}; loaded {_active.Count} entries.");
                }
                catch (Exception ex)
                {
                    ModLogger.LogError($"Failed to load localization for {language}: {ex.Message}");
                    _active = new Dictionary<string, string>();
                }
            }
        }

        public static string Tr(string key)
        {
            if (TryGet(_active, key, out var v)) return v;
            if (TryGet(_en, key, out v)) return v;
            if (TryGet(_cn, key, out v)) return v;
            // Return debug string so missing keys stand out
            return $"<<{key}>>";
        }

        public static string TrFormat(string key, params object[] args)
        {
            var format = Tr(key);
            return string.Format(format, args);
        }

        private static bool TryGet(Dictionary<string, string> dict, string key, out string value)
        {
            if (dict.TryGetValue(key, out value)) return true;
            return false;
        }

        private static Dictionary<string, string> LoadLanguageFile(SystemLanguage language)
        {
            var path = GetLanguageFilePath(language);
            try
            {
                if (!File.Exists(path))
                {
                    ModLogger.LogWarning($"Localization file not found: {path}");
                    return new Dictionary<string, string>();
                }

                var json = File.ReadAllText(path);
                var dict = ParseFlatJson(json);
                return dict;
            }
            catch (Exception ex)
            {
                ModLogger.LogError($"Failed to load or parse localization file {path}: {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        private static string GetLanguageFilePath(SystemLanguage language)
        {
            var fileName = language switch
            {
                SystemLanguage.English => "en.json",
                SystemLanguage.Chinese => "cn.json",
                SystemLanguage.ChineseSimplified => "cn.json",
                SystemLanguage.ChineseTraditional => "cn.json",
                _ => "en.json"
            };

            ModLogger.Log($"Loading localization file for language {language}, fileName: {fileName}");

            var asmDir = Path.GetDirectoryName(typeof(ModLoader).Assembly.Location) ?? "";
            return Path.Combine(asmDir, "Localization", fileName);
        }

        // Minimal flat JSON parser: expects an object with string keys and string values.
        private static Dictionary<string, string> ParseFlatJson(string json)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(json)) return dict;

            int i = 0;

            void SkipWs()
            {
                while (i < json.Length && char.IsWhiteSpace(json[i])) i++;
            }

            SkipWs();
            if (i >= json.Length || json[i] != '{') return dict;
            i++; // skip '{'

            while (true)
            {
                SkipWs();
                if (i >= json.Length) break;
                if (json[i] == '}')
                {
                    i++;
                    break;
                }

                // key
                if (json[i] != '"') break;
                var key = ReadJsonString(json, ref i);
                if (key == null) break;

                SkipWs();
                if (i >= json.Length || json[i] != ':') break;
                i++; // skip ':'

                SkipWs();
                if (i >= json.Length) break;
                if (json[i] != '"') break; // only string values supported
                var value = ReadJsonString(json, ref i) ?? string.Empty;

                if (!dict.ContainsKey(key)) dict[key] = value;
                else dict[key] = value;

                SkipWs();
                if (i >= json.Length) break;
                if (json[i] == ',')
                {
                    i++;
                    continue;
                }

                if (json[i] == '}')
                {
                    i++;
                    break;
                }

                // invalid char; stop
                break;
            }

            return dict;
        }

        private static string ReadJsonString(string s, ref int i)
        {
            if (i >= s.Length || s[i] != '"') return "";
            i++; // skip opening quote
            var result = new System.Text.StringBuilder();
            while (i < s.Length)
            {
                var c = s[i++];
                if (c == '"') break;
                if (c == '\\')
                {
                    if (i >= s.Length) break;
                    var e = s[i++];
                    switch (e)
                    {
                        case '"': result.Append('"'); break;
                        case '\\': result.Append('\\'); break;
                        case '/': result.Append('/'); break;
                        case 'b': result.Append('\b'); break;
                        case 'f': result.Append('\f'); break;
                        case 'n': result.Append('\n'); break;
                        case 'r': result.Append('\r'); break;
                        case 't': result.Append('\t'); break;
                        case 'u':
                            if (i + 3 < s.Length)
                            {
                                var hex = s.Substring(i, 4);
                                if (ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null,
                                        out var code))
                                {
                                    result.Append((char)code);
                                    i += 4;
                                    break;
                                }
                            }

                            // invalid, append literally
                            result.Append('u');
                            break;
                        default:
                            result.Append(e);
                            break;
                    }
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}