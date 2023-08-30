using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

public class CustomSpectrums : IModApi
{

    // ####################################################################
    // ####################################################################

    public void InitMod(Mod mod)
    {
        if (GameManager.IsDedicatedServer) return;
        Log.Out("OCB Harmony Patch: " + GetType().ToString());
        Harmony harmony = new Harmony(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    // ####################################################################
    // ####################################################################

    [HarmonyPatch(typeof(ConsoleCmdSpectrum), "Execute")]
    public class ConsoleCmdSpectrum_Execute
    {
        public static void Postfix(List<string> _params)
        {
            if (_params.Count == 1)
            {
                // Check if spectrum was not found be previous code from vanilla
                if (SpectrumWeatherType.None != WeatherManager.forcedSpectrum) return;
                // Let WorldSpectrum do the hard work (we keep track of everything)
                var spectrum = WorldSpectrum.SpectrumName2Type(_params[0]);
                WeatherManager.SetForceSpectrum(WorldSpectrum.SpectrumName2Type(_params[0]));
            }
        }
    }

    // ####################################################################
    // ####################################################################

}
