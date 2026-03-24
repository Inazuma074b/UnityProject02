using UnityEngine;
using Newtonsoft.Json;

public class DataManager : MonoBehaviorSingleton<DataManager>
{
    private static GameSettings gameSettings;


    public static SystemLanguage GetLanguage() { return gameSettings.Language; }
    public static string GetLocale() { return gameSettings.Locale; }        // Localization string, ex: fr-FR, es-ES, de-DE, it-IT
    public static bool GetEnableBGM() { return gameSettings.EnableBGM; }    // Enable/Disable background music.
    public static bool GetEnableSFX() { return gameSettings.EnableSFX; }    // Enable/Disable sound effects.
    public static float GetBGMVolume() { return gameSettings.BGMVolume; }   // Volume of the background music; between 0f ~ 1.0f.
    public static float GetSFXVolume() { return gameSettings.SFXVolume; }   // Volume of the sound effects; between 0f ~ 1.0f.




    #region GameSettings
    public static void LoadGameSettings()
    {
        Debug.Log("[DataManager] Load Game Settings");
        GameSettings settings = new GameSettings();
        if (!PlayerPrefs.HasKey(Constants.Key_GameSettings))
        {
            settings = InitGameSettings();
            SaveGameSettings(settings);
        }
        else
        {
            string json = PlayerPrefs.GetString(Constants.Key_GameSettings);
            settings = JsonConvert.DeserializeObject<GameSettings>(json);
            if (settings == null)
            {
                settings = InitGameSettings();
                SaveGameSettings(settings);
            }
            else
            {
                ChangeGameSettings(settings);
            }
        }
    }

    public static void SaveGameSettings(GameSettings settings)
    {
        ChangeGameSettings(settings);
        string value = JsonConvert.SerializeObject(settings);
        PlayerPrefs.SetString(Constants.Key_GameSettings, value);
    }

    private static void ChangeGameSettings(GameSettings settings)
    {
        gameSettings = settings;
        /*
         
         do something...
         
         */
    }

    private static GameSettings InitGameSettings()
    {
        GameSettings settings = new GameSettings();
        //get system language, default ChineseTraditional
        SystemLanguage local = Application.systemLanguage;
        SystemLanguage language = local switch
        {
            SystemLanguage.ChineseTraditional => SystemLanguage.ChineseTraditional,
            SystemLanguage.English => SystemLanguage.English,
            SystemLanguage.Japanese => SystemLanguage.Japanese,
            SystemLanguage.French => SystemLanguage.French,
            SystemLanguage.German => SystemLanguage.German,
            SystemLanguage.Italian => SystemLanguage.Italian,
            SystemLanguage.Spanish => SystemLanguage.Spanish,
            _ => SystemLanguage.ChineseTraditional,
        } ;
        settings.Language = language;
        settings.EnableBGM = true;
        settings.BGMVolume = 0.1f;
        settings.EnableSFX = true;
        settings.SFXVolume = 1.0f;
        return settings;
    }
    #endregion
}

public class GameSettings
{
    public SystemLanguage Language;
    public string Locale;                       // Localization string, ex: fr-FR, es-ES, de-DE, it-IT
    public bool EnableBGM;                      // Enable/Disable background music.
    public bool EnableSFX;                    // Enable/Disable sound effects.
    public float BGMVolume;                     // Volume of the background music; between 0f ~ 1.0f.
    public float SFXVolume;                     // Volume of the sound effects; between 0f ~ 1.0f.
}
