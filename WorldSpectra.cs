using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

class WorldSpectra
{

    // Map between spectrum names and effect idx
    private static readonly Dictionary<string, int>
        NameLC2Idx = new Dictionary<string, int>();

    // Complete configuration as parsed from the XML
    // Used to load actual things once everybody is ready
    private static readonly Dictionary<string, List<KeyValuePair<int, string>>>
        Config = new Dictionary<string, List<KeyValuePair<int, string>>>();
    
    // IMPORTANT: singleton initialization must be last
    // NOTE: see clause 15.5.6.2 of the C# specification
    // Singleton Instance is created on ReadXML
    // Without config we should error anyway
    private static readonly WorldSpectra
        Instance = new WorldSpectra();

    // Return number of effects
    // This includes built-ins
    public static int Count
    {
        get => NameLC2Idx.Count;
    }

    // Private constructor
    private WorldSpectra()
    {
        ResetStates();
    }

    // Reset function when we re-read config
    // Not sure if this ever happens though
    private static void ResetStates()
    {
        Config.Clear();
        NameLC2Idx.Clear();
        // Reserved, don't use
        NameLC2Idx.Add("biome", 0);
        NameLC2Idx.Add("snowy", 1);
        NameLC2Idx.Add("stormy", 2);
        NameLC2Idx.Add("rainy", 3);
        NameLC2Idx.Add("foggy", 4);
        NameLC2Idx.Add("bloodmoon", 5);
        // Reserved, don't use
        NameLC2Idx.Add("none", 6);
    }

    // Load our spectra config into the array of the built-in class
    // We need to wait until that class filled in its default effects
    // We can only apply our config once that is done to allow overrides
    // Note: the original code doesn't event re-allocate that array!
    public static void LoadSpectra(ref AtmosphereEffect[] ___atmosphereSpectrum)
    {
        // Process the complete stored config and load resources here
        // This is mainly done here since the other class does not create
        // it's instance earlier (this was an easy way to work around that)
        foreach (KeyValuePair<string, List<KeyValuePair<int, string>>> config in Config)
        {
            AtmosphereEffect effect = new AtmosphereEffect();
            // Process and load all light textures for this effect
            // The numbers can vary and unknowns just seem to be empty
            foreach (KeyValuePair<int, string> tex in config.Value)
            {
                effect.spectrums[tex.Key] = ColorSpectrumFromResource(tex.Value);
            }
            // Make sure we have enough space inside the static array
            // Allocate the full expected since once instead of increments
            if (___atmosphereSpectrum.Length < NameLC2Idx.Count)
            {
                Array.Resize(ref ___atmosphereSpectrum, NameLC2Idx.Count);
            }
            // Finally assign the newly loaded effect to the array
            ___atmosphereSpectrum[NameLC2Idx[config.Key.ToLower()]] = effect;
            // Put a debug message on the console (remove later)
            Log.Out("Loaded custom spectrum " + config.Key +
                " (" + NameLC2Idx[config.Key.ToLower()] + ")");
        }
    }

    // Config will just be stored for now
    public static void ReadXML(XmlDocument _xml)
    {
        // Reset states
        ResetStates();
        // Parse XML document
        ParseXML(_xml);
    }

    // Map name to `SpectrumWeatherType` enumeration
    public static SpectrumWeatherType SpectrumName2Type(string name)
    {
        if (NameLC2Idx.TryGetValue(name.ToLower(), out int type))
        {
            return (SpectrumWeatherType)type;
        }
        return SpectrumWeatherType.None;
    }

    // Map light texture type to effect index
    public static int SpectrumNameToIndex(string name)
    {
        switch (name)
        {
            case "sky":
                return 0;
            case "ambient":
                return 1;
            case "sun":
                return 2;
            case "moon":
                return 3;
            case "fog":
                return 4;
            case "fogfade":
                return 5;
            default:
                return -1;
        }
    }

    // Asset loading copied from AtmosphereEffects class
    // Unsure if we could load directly from images files!?
    public static ColorSpectrum ColorSpectrumFromResource(string _filename)
    {
        // Just one way to load the resource, might be others
        var dpath = DataLoader.ParseDataPathIdentifier(_filename);
        Texture2D _tex = DataLoader.LoadAsset<Texture2D>(dpath);
        if (_tex == null)
        {
            Log.Error("Failed loading asset " + _filename);
            return null;
        }
        ColorSpectrum colorSpectrum = new ColorSpectrum(_filename, _tex);
        Resources.UnloadAsset(_tex);
        return colorSpectrum;
    }

    // Entry point for harmony function
    public static void ParseXML(XmlDocument _xml)
    {
        // Use `selectNodes` to quickly get to our entry nodes
        // IMO the XML API is a little weird and lacks some features
        foreach (XmlNode nSpectrum in _xml.SelectNodes("/worldgeneration/spectra/spectrum"))
        {
            // Spectrum must have children, so must be XmlElement!?
            if (nSpectrum is XmlElement elSpectrum)
            {
                XmlAttribute name = elSpectrum.Attributes["name"];
                if (name == null || name.Value == "")
                {
                    Log.Error("Spectrum element must have a name!");
                    continue;
                }
                // Create a new list to hold all light textures for current effect
                List<KeyValuePair<int, string>> configs = new List<KeyValuePair<int, string>>();
                foreach (XmlNode tex in elSpectrum.GetElementsByTagName("texture"))
                {
                    XmlAttribute type = tex.Attributes["type"];
                    XmlAttribute path = tex.Attributes["path"];
                    if (type == null || type.Value == "")
                    {
                        Log.Error("Spectrum texture must define a type!");
                        continue;
                    }
                    if (path == null || path.Value == "")
                    {
                        Log.Error("Spectrum texture must define a path!");
                        continue;
                    }
                    int idx = SpectrumNameToIndex(type.Value);
                    if (idx == -1)
                    {
                        Log.Error("Unknown spectrum texture type " + type.Value);
                    }
                    else
                    {
                        // Store config for later instantiation when we are ready
                        configs.Add(new KeyValuePair<int, string>(idx, path.Value));
                        // effect.spectrums[idx] = ColorSpectrumFromResource(path.Value);
                    }
                }

                // Only assign new index to new keys
                if (!NameLC2Idx.ContainsKey(name.Value.ToLower()))
                {
                    // Calculate final index including original enum
                    NameLC2Idx[name.Value.ToLower()] = NameLC2Idx.Count;
                }
                // Store effect config, last item wins
                // This allows to override built-ins
                Config[name.Value] = configs;

            }
        }
    }

}
