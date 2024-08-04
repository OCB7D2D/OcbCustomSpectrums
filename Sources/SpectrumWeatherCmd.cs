using System.Collections.Generic;
using UnityEngine;

#if (DEBUG)
public class SpectrumWeatherCmd : ConsoleCmdAbstract
{

    // ####################################################################
    // ####################################################################

    public override string[] getCommands() => new string[] { "cspctrm" };

    public override string getDescription() => "force spectrum weather, e.g. fog, rain, storm, etc.";

    // ####################################################################
    // ####################################################################

    // Get pixel valiues from a given spectrum (all info is private)
    private static readonly HarmonyFieldProxy<Color[]> ColorSpectrumValues =
        new HarmonyFieldProxy<Color[]>(typeof(ColorSpectrum), "values");

    // public static Color[] GetColor

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var world = GameManager.Instance.World;
        if (_params.Count == 2)
        {
            if (_params[0] == "weather")
            {
                WeatherManager.Instance.ForceWeather(_params[1], 9999);
                return;
            }
        }
        else if (_params.Count == 4)
        {
            if (_params[0] == "analyse")
            {
                if (_params[1] == "biome")
                {
                    var type = EnumUtils.Parse<AtmosphereEffect.ESpecIdx>(_params[3], true);
                    var effects = world.BiomeAtmosphereEffects.worldColorSpectrums;
                    Log.Out("Analyse {0} => {1}", effects[0], type);
                    var spectrum = effects[0].spectrums[(int)type];
                    AnalyseSpectrum(spectrum);
                    return;
                }
                else if(_params[1] == "effect")
                {
                    // Get the effect type (e.g. Sky, Ambient, Sun, Moon, Fog, etc.)
                    var weather = EnumUtils.Parse<SpectrumWeatherType>(_params[2], true);
                    var type = EnumUtils.Parse<AtmosphereEffect.ESpecIdx>(_params[3], true);
                    Log.Out("Analyse {0} => {1}", weather, type);
                    // WeatherManager.Instance.GetWeatherSpectrum()
                    var effects = WeatherManager.atmosphereSpectrum;
                    var effect = effects[(int)weather];
                    var spectrum = effect.spectrums[(int)type];
                    AnalyseSpectrum(spectrum);
                    return;
                }
            }

        }
        Log.Error("Invalid command or parameters");
    }

    private void AnalyseSpectrum(ColorSpectrum spectrum)
    {
        Log.Out("Analyse the spectrum {0}", spectrum);
        var cols = spectrum.values;
        for (float t = 0; t < 24f; t += 0.25f)
        {
            int p = (int)((t < 6 ? t + 18 : t - 6) / 24f * 1024);
            Log.Out("<step time=\"{0:0.00}\" color=\"{1,3},{2,3},{3,3},{4}\"/>",
                t, (int)(cols[p].r * 255 + .5f), (int)(cols[p].g * 255 + .5f),
                (int)(cols[p].b * 255 + .5f), (int)(cols[p].a * 255 + .5f));
        }
    }

    // ####################################################################
    // ####################################################################

}
#endif