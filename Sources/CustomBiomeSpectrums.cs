using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public static class CustomBiomeSpectrums
{

    // ####################################################################
    // ####################################################################

    public static readonly Dictionary<string, OcbSpectrum[]>
        BiomeSpectra = new Dictionary<string, OcbSpectrum[]>();

    // ####################################################################
    // ####################################################################

    // Unfortunately due to some hard-coded stuff we
    // would not be able to override default biome full
    [HarmonyPatch(typeof(BiomeAtmosphereEffects), "Init")]
    public class BiomeAtmosphereEffectsInitPatch
    {
        public static void Postfix(World _world,
            AtmosphereEffect[] ___worldColorSpectrums)
        {
            // Process all known biomes and check ...
            foreach (var biome in _world.Biomes.GetBiomeMap().Values)
            {
                var name = biome.m_sBiomeName;
                // ... if there is any custom spectrum config
                if (BiomeSpectra.TryGetValue(name, out var spectrums))
                {
                    // Create a new atmosphere effect for all types
                    AtmosphereEffect effect = new AtmosphereEffect();
                    var fallback = ___worldColorSpectrums[0].spectrums;
                    for (int i = 0; i < effect.spectrums.Length; ++i)
                        effect.spectrums[i] = fallback[i];
                    foreach (OcbSpectrum spectra in spectrums)
                    {
                        if (spectra == null) continue;
                        int type = (int)spectra.spec;
                        var spectrum = spectra.GetColorSpectrum();
                        if (spectrum == null) continue;
                        effect.spectrums[type] = spectrum;
                    }
                    // Assign all atmosphere effects to this biome
                   ___worldColorSpectrums[biome.m_Id] = effect;
                }
            }
        }
    }

    // ####################################################################
    // ####################################################################

    public static void ParseBiomeSpectrums(XElement biome)
    {
        var name = biome.GetAttribute("name");
        // Fetch all elements to define custom spectrums
        var spectrums = biome.Elements("biome-spectrum");
        if (spectrums == null || spectrums.Count() == 0) return;
        // Check if config is already known or create new one
        if (!BiomeSpectra.TryGetValue(name, out var spectra))
            BiomeSpectra[name] = spectra = new OcbSpectrum[
                (int)AtmosphereEffect.ESpecIdx.Count];
        // Process all spectrums and assign as needed
        foreach (XElement node in spectrums)
        {
            OcbSpectrum spectrum = SpectrumUtils.ParseSpectrum(node);
            // if (spectrum == null) continue; // skip null overwrite
            spectra[(int)spectrum.spec] = spectrum;
        }
    }

    // ####################################################################
    // ####################################################################

}
