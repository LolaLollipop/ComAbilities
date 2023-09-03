using Exiled.API.Features;
using Exiled.Loader.Features.Configs;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Localizer
{
    /// <summary>
    /// File-based localizer
    /// </summary>
    /// <typeparam name="TTranslation">The translation class to use</typeparam>
    public class Localizer<TTranslation>
        where TTranslation : class, new()
    {
        public TTranslation CurrentLocalization { get; private set; } = new();
        public string Name { get; set; } = "Plugin";

        public static ISerializer Serializer { get; private set;  } = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)

                    //  .WithTypeInspector(inspector => new NewInfoInspector(inspector))
                    .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                    .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                    .Build();

        public static IDeserializer Deserializer { get; private set;  } = new DeserializerBuilder()
                    .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();


        public async void Start(string url, string path, string selectedTranslation, Version version)
        {
            Log.SendRaw($"[INFO] [{Name} Localizer] Attempting to download localizations", ConsoleColor.Blue);
            try
            {
                bool vInfoExists = TryReadFile($"{path}/version.txt", out string? info);
                if (!vInfoExists)
                {
                    await UpdateLocalizations(url, path, version);
                }
                else
                {
                    int versionPosition = info!.IndexOf("$v");
                    string versionText = info.Substring(versionPosition + 2);
                    Log.Debug(versionText);
                    if (!Version.TryParse(versionText, out Version versionInfo) || versionInfo != version)
                    {
                        await UpdateLocalizations(url, path, version);
                    }
                    else
                    {
                        Log.SendRaw($"[INFO] [{Name} Localizer] Localizations are all up to date!", ConsoleColor.DarkGreen);
                    }
                }

            }
            catch (Exception e)
            {
                Log.Debug(e);
                Log.SendRaw($"[WARN] [{Name} Localizer] Downloading or updating localizations FAILED", ConsoleColor.DarkRed);
            }

            if (TryReadFile($"{path}/{selectedTranslation + ".yml"}", out string? result) && result != null)
            {
                // TTranslation translation = JsonSerializer.Deserialize<TTranslation>(result);
                TTranslation translation = Deserializer.Deserialize<TTranslation>(result);
                PropertyInfo[] properties = translation.GetType().GetProperties();
                foreach (PropertyInfo prop in properties)
                {
                    object value = prop.GetValue(translation, null);
                    prop.SetValue(CurrentLocalization, value);
                }
            }
        }

        public bool TryReadFile(string path, out string? result)
        {
            result = default;
            if (!File.Exists(path)) return false;
            result = File.ReadAllText(path);
            return true;
        }
        public bool TryConvertYaml(string text, out TTranslation? translation)
        {
            try
            {
                translation = Deserializer.Deserialize<TTranslation>(text);
                return true;
            }
            catch (Exception e)
            {
                Log.Debug(e);
                translation = default;
                return false;
            }
        }

        private async Task UpdateLocalizations(string url, string path, Version version)
        {
            using HttpClient client = new();
            using Stream stream = await client.GetStreamAsync(url);

            using ZipArchive zipArchive = new(stream);

            foreach (ZipArchiveEntry entry in zipArchive.Entries)
            {
                if (TryReadFile($"{path}/{entry.FullName}", out string? result))
                {
                    StreamReader reader = new(entry.Open(), Encoding.Default);
                    string text = reader.ReadToEnd();

                    if (TryConvertYaml(result!, out TTranslation? oldTrans) && TryConvertYaml(text, out TTranslation? newTrans))
                    {
                        PropertyInfo[] properties = oldTrans!.GetType().GetProperties();
                        foreach (PropertyInfo prop in properties)
                        {
                            object value = prop.GetValue(oldTrans, null);
                            prop.SetValue(newTrans, value);
                        }

                        string yaml = Serializer.Serialize(newTrans!);
                        File.WriteAllText($"{path}/{entry.FullName}", yaml);
                    }
                    else
                    {
                        Log.Warn($"[{Name} Localizer] {entry.FullName} is malformed, skipping");
                        continue;
                    }
                }
                else
                {
                    entry.ExtractToFile($"{path}/{entry.FullName}", true);
                }
            }

            string commentedText =
                "#This file contains information about the\n# latest localization version, reducing\n# useless updates. This file will be\n# regenerated whenever the plugin is run.\n$v";
            File.WriteAllText($"{path}/version.txt", commentedText + version.ToString());

            Log.SendRaw($"[INFO] [{Name} Localizer] Successfully downloaded localizations", ConsoleColor.Yellow);
        }
    }
}