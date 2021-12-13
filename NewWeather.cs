using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class NewWeather : IModApi
{

    private static string modname;

    public void InitMod(Mod mod)
    {
        Debug.Log("Loading New Weather Patch: " + GetType().ToString());
        var harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        ModEvents.GameAwake.RegisterHandler(GameAwakeHandler);
        modname = mod.ModInfo.Name?.ToString();
    }

    public static void GameAwakeHandler()
    {
        // Extend enum so game counts correctly?
        // Log.Warning("Game is Awake");
    }


    // Copied from AtmosphereEffects class
    public static ColorSpectrum ColorSpectrumFromResource(string _filename)
    {
        // Just one way to load the resource, might be others
        var dpath = DataLoader.ParseDataPathIdentifier(_filename);
        Texture2D _tex = DataLoader.LoadAsset<Texture2D>(dpath);
        if (_tex == null) return null;
        ColorSpectrum colorSpectrum = new ColorSpectrum(_filename, _tex);
        Resources.UnloadAsset(_tex);
        return colorSpectrum;
    }

    // Copied from AtmosphereEffects class
    public static AtmosphereEffect LoadAtmosphereEffect(string prefix, string suffix = "")
    {
        AtmosphereEffect atmosphereEffect = new AtmosphereEffect();
        atmosphereEffect.spectrums[0] = ColorSpectrumFromResource(prefix + "sky" + suffix);
        atmosphereEffect.spectrums[1] = ColorSpectrumFromResource(prefix + "ambient" + suffix);
        atmosphereEffect.spectrums[2] = ColorSpectrumFromResource(prefix + "sun" + suffix);
        atmosphereEffect.spectrums[3] = ColorSpectrumFromResource(prefix + "moon" + suffix);
        atmosphereEffect.spectrums[4] = ColorSpectrumFromResource(prefix + "fog" + suffix);
        atmosphereEffect.spectrums[5] = ColorSpectrumFromResource(prefix + "fogfade" + suffix);
        return atmosphereEffect;
    }

    [HarmonyPatch(typeof(WeatherManager))]
    [HarmonyPatch("LoadSpectrums")]
    public class WeatherManager_LoadSpectrums
    {
        public static bool Prefix(ref AtmosphereEffect[] ___atmosphereSpectrum)
        {
            if (___atmosphereSpectrum != null) return false;
            // Increase the available effects by 2 (we need to go beyond the "None" entry)
            ___atmosphereSpectrum = new AtmosphereEffect[Enum.GetNames(typeof(SpectrumWeatherType)).Length + 1];
            WeatherManager.ReloadSpectrums();
            return false;
        }
    }

    [HarmonyPatch(typeof (WeatherManager))]
    [HarmonyPatch("ReloadSpectrums")]
    public class WeatherManager_ReloadSpectrums
    {
        public static void Postfix(ref AtmosphereEffect[] ___atmosphereSpectrum)
        {
            // This is mostly copied one to one from the extended method
            // Could use AtmosphereEffect.Load if we put all assets under:
            // `Textures/Environment/Spectrums/` (hardcoded path in loader).
            var loaded = LoadAtmosphereEffect("#@modfolder(" + modname + "):Resources/NewWeather.unity3d?", "");
            // Seems we can't overwrite None without changing the enum
            ___atmosphereSpectrum[6] = loaded; // None
            // But we seem to be able to just go beyond it
            ___atmosphereSpectrum[7] = loaded; // OOB?
        }
    }

    [HarmonyPatch(typeof(ConsoleCmdSpectrum))]
    [HarmonyPatch("Execute")]
    public class ConsoleCmdSpectrum_Execute
    {
        public static void Postfix(List<string> _params)
        {
            if (_params.Count == 1)
            {
                // Seems we can't overwrite None without changing the enum
                if (_params[0].EqualsCaseInsensitive("NewWeatherNone"))
                    WeatherManager.SetForceSpectrum(SpectrumWeatherType.None);
                // But we seem to be able to just go beyond it
                if (_params[0].EqualsCaseInsensitive("NewWeather"))
                    WeatherManager.SetForceSpectrum(SpectrumWeatherType.None + 1);
            }
        }
    }

}
