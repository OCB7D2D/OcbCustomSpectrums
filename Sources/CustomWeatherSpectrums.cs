using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using UnityEngine;

public class CustomWeatherSpectrums
{

    // ####################################################################
    // ####################################################################

    // Patch to load our XML from biomes too
    [HarmonyPatch(typeof(WorldBiomes))]
    [HarmonyPatch("readXML")]
    public class WorldBiomes_ReadXML
    {
        public static void Prefix(XDocument _xml)
        {
            WorldSpectrum.ReadXML(_xml);
        }
    }

    [HarmonyPatch(typeof(WeatherManager))]
    [HarmonyPatch("LoadSpectrums")]
    public class WeatherManager_LoadSpectrums
    {
        // We overload the complete original function since it's simple enough
        public static bool Prefix(ref AtmosphereEffect[] ___atmosphereSpectrum)
        {
            if (___atmosphereSpectrum != null) return false;
            ___atmosphereSpectrum = new AtmosphereEffect[WorldSpectrum.Count];
            WeatherManager.ReloadSpectrums();
            return false;
        }
    }

    [HarmonyPatch(typeof(WeatherManager))]
    [HarmonyPatch("ReloadSpectrums")]
    public class WeatherManager_ReloadSpectrums
    {
        public static void Postfix(ref AtmosphereEffect[] ___atmosphereSpectrum)
        {
            // Let WorldSpectrum do the hard work
            WorldSpectrum.ReloadSpectrums(ref ___atmosphereSpectrum);
        }
    }

    // ####################################################################
    // ####################################################################

    // Generic harmony patch since we need to
    // patch a templated/generic function
    [HarmonyPatch()]
    public class EnumUtils_Parse
    {

        // Function telling harmony what to patch
        // Allows us to do a more elaborate search
        // Couldn't get this with annotations only
        static MethodBase TargetMethod()
        {
            foreach (MethodInfo m in AccessTools.GetDeclaredMethods(typeof(EnumUtils)))
            {
                if (m.Name == "Parse")
                {
                    string fn = m.ToString();
                    // Best heuristic I could come up with
                    // If TEnum is also inside the parameters
                    if (fn.LastIndexOf("TEnum") > fn.IndexOf("("))
                    {
                        // We only want to overload it for a specific type
                        return m.MakeGenericMethod(typeof(SpectrumWeatherType));
                    }
                }
            }
            return null;
        }

        // Patch the EnumUtils.Parse function for SpectrumWeatherType to include our custom effects
        public static bool Prefix(string _name, ref SpectrumWeatherType __result)
        {
            // We only support case-insensitive matching
            // Having both doesn't really make much sense
            __result = WorldSpectrum.SpectrumName2Type(_name);
            return false;
        }
    }

    // ####################################################################
    // ####################################################################

    // Unfortunately due to some hard-coded stuff we
    // would not be able to override default biome fully
    [HarmonyPatch(typeof(WeatherManager))]
    [HarmonyPatch("GetWeatherSpectrum")]
    public class WeatherManager_Execute
    {
        public static bool Prefix(AtmosphereEffect[] ___atmosphereSpectrum,
            Color regularSpectrum, AtmosphereEffect.ESpecIdx type, float dayTimeScalar,
            SpectrumWeatherType ___spectrumSourceType, SpectrumWeatherType ___spectrumTargetType,
            bool ___isGameModeNormal, float ___spectrumBlend, ref Color __result)
        {
            // Use regular code-path for forced condition
            if (WeatherManager.forcedSpectrum != SpectrumWeatherType.None) return true;
            // Rest is mostly copied one to one
            Color color1 = regularSpectrum;
            Color color2 = regularSpectrum;
            if (___isGameModeNormal)
            {
                // Never ignore what we have in store for you
                // Before this check would explicitly skip Biome
                if (___atmosphereSpectrum[(int)___spectrumSourceType] != null)
                {
                    ColorSpectrum spectrum = ___atmosphereSpectrum[(int)___spectrumSourceType].spectrums[(int)type];
                    if (spectrum != null) color1 = spectrum.GetValue(dayTimeScalar);
                }
                if (___atmosphereSpectrum[(int)___spectrumTargetType] != null)
                {
                    ColorSpectrum spectrum = ___atmosphereSpectrum[(int)___spectrumTargetType].spectrums[(int)type];
                    if (spectrum != null) color2 = spectrum.GetValue(dayTimeScalar);
                }
            }
            // Interpolate between the two colors (why not try lerping?)
            __result = color1 * (1f - ___spectrumBlend) + color2 * ___spectrumBlend;
            // We did all the work
            return false;
        }
    }

    // ####################################################################
    // ####################################################################

}
