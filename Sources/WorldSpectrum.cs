using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.XPath;
using UnityEngine;

class WorldSpectrum
{

    // Map between spectrum names and effect idx
    private static readonly Dictionary<string, int>
        NameLC2Idx = new Dictionary<string, int>();

    // Complete configuration as parsed from the XML
    // Used to load actual things once everybody is ready
    private static readonly Dictionary<string, List<KeyValuePair<int, ColorSpectrum>>>
        Spectrums = new Dictionary<string, List<KeyValuePair<int, ColorSpectrum>>>();
    
    // Return number of effects
    // This includes built-ins
    public static int Count
    {
        get => NameLC2Idx.Count;
    }

    // Private constructor
    private WorldSpectrum()
    {
        ResetStates();
    }

    // Reset function when we re-read config
    // Not sure if this ever happens though
    private static void ResetStates()
    {
        Spectrums.Clear();
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

    // Load our spectrum config into the array of the built-in class
    // We need to wait until that class filled in its default effects
    // We can only apply our config once that is done to allow overrides
    // Note: the original code doesn't event re-allocate that array!
    public static void ReloadSpectrums(ref AtmosphereEffect[] ___atmosphereSpectrum)
    {
        // Process the complete stored config and load resources here
        // This is mainly done here since the other class does not create
        // it's instance earlier (this was an easy way to work around that)
        foreach (KeyValuePair<string, List<KeyValuePair<int, ColorSpectrum>>> config in Spectrums)
        {
            AtmosphereEffect effect = new AtmosphereEffect();
            // Process and load all light textures for this effect
            // The numbers can vary and unknowns just seem to be empty
            foreach (KeyValuePair<int, ColorSpectrum> tex in config.Value)
                effect.spectrums[tex.Key] = tex.Value;
            // Make sure we have enough space inside the static array
            // Allocate the full expected since once instead of increments
            if (___atmosphereSpectrum.Length < NameLC2Idx.Count)
                Array.Resize(ref ___atmosphereSpectrum, NameLC2Idx.Count);
            // Finally assign the newly loaded effect to the array
            ___atmosphereSpectrum[NameLC2Idx[config.Key.ToLower()]] = effect;
        }
    }

    // Config will just be stored for now
    public static void ReadXML(XDocument _xml)
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

    private static readonly HarmonyFieldProxy<Color[]> ColorSpectrumValues =
        new HarmonyFieldProxy<Color[]>(typeof(ColorSpectrum), "values");

    // Entry point for harmony function
    public static void ParseXML(XDocument _xml)
    {
        // Use `selectNodes` to quickly get to our entry nodes
        // IMO the XML API is a little weird and lacks some features
        foreach (XElement node in _xml.XPathSelectElements("/worldgeneration/biomes/spectrum"))
        {
            // Spectrum must have children, so must be XmlElement!?
            // if (nSpectrum is XElement elSpectrum)
            {
                XAttribute name = node.Attribute("name");
                if (name == null || string.IsNullOrEmpty(name.Value))
                {
                    Log.Error("Spectrum element must have a name!");
                    continue;
                }
                // Create a new list to hold all light textures for current effect
                List<KeyValuePair<int, ColorSpectrum>> configs = new List<KeyValuePair<int, ColorSpectrum>>();
                foreach (XElement grad in node.Elements("gradient"))
                {
                    XAttribute type = grad.Attribute("type");
                    if (type == null || type.Value == "")
                    {
                        Log.Error("Spectrum texture must define a type!");
                        continue;
                    }
                    Dictionary<float, Color> frames = new Dictionary<float, Color>();
                    foreach (XElement frame in grad.Elements("step"))
                    {
                        if (!frame.HasAttribute("time"))
                            Log.Error("No attribute `time` on gradient step");
                        if (!frame.HasAttribute("color"))
                            Log.Error("No attribute `color` on gradient step");
                        frames.Add(NormalizeTime(float.Parse(frame.GetAttribute("time"))),
                            StringParsers.ParseColor32(frame.GetAttribute("color")));
                    }
                    var sorted = frames.ToList();
                    int idx = SpectrumNameToIndex(type.Value);
                    if (idx == -1)
                    {
                        Log.Error("Unknown spectrum texture type " + type.Value);
                    }
                    else
                    {
                        // Sneaky way to avoid going through creating a texture just to throw it away again
                        var spectrum = FormatterServices.GetUninitializedObject(typeof(ColorSpectrum)) as ColorSpectrum;
                        // Set the color values directly on the object (unfortunately all private stuff)
                        ColorSpectrumValues.Set(spectrum, GetGradientColors(sorted));
                        configs.Add(new KeyValuePair<int, ColorSpectrum>(idx, spectrum));
                    }
                }
                foreach (XElement tex in node.Elements("texture"))
                {
                    XAttribute type = tex.Attribute("type");
                    XAttribute path = tex.Attribute("path");
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
                        configs.Add(new KeyValuePair<int, ColorSpectrum>(idx,
                            ColorSpectrumFromResource(path.Value)));
                        // effect.spectrums[idx] = ColorSpectrumFromResource(path.Value);
                    }
                }

                // Only assign new index to new keys
                if (!NameLC2Idx.ContainsKey(name.Value.ToLower()))
                {
                    // Calculate final index including original enum
                    NameLC2Idx[name.Value.ToLower()] = NameLC2Idx.Count;
                }

                // Give a debug message to console (may remove later)
                #if DEBUG
                Log.Warning("Registered custom spectrum {0}", name.Value);
                #endif

                // Store effect config, last item wins
                // This allows to override built-ins
                Spectrums[name.Value] = configs;

            }
        }

        var biomesXPath = "/worldgeneration/biomes/biome";
        foreach (var node in _xml.XPathSelectElements(biomesXPath))
            CustomBiomeSpectrums.ParseBiomeSpectrums(node);

        // UnityEngine.Application.Quit(); ;
    }

    private static float NormalizeTime(float v)
    {
        if (v >= 6) return v - 6f;
        else return v + 18f;
    }

    public static Color[] GetGradientColors(List<KeyValuePair<float, Color>> times)
    {
        // Nothing to work with if nothing passed
        if (times.Count == 0) return null;
        // Create the texture to fill in the colors
        Color[] values = new Color[1024];
        // Sort the time keys nunmmerically
        if (times.Count == 1)
        {
            for(int i  = 0; i < values.Length; i++)
                values[i] = times[0].Value;
        }
        else
        {
            times.Sort((a, b) => a.Key.CompareTo(b.Key));
            AnimationCurve red = new AnimationCurve();
            AnimationCurve green = new AnimationCurve();
            AnimationCurve blue = new AnimationCurve();
            AnimationCurve alpha = new AnimationCurve();

            var from = times.Last();
            var to = times.First();
            // Add the preframe to the animation curve (from last)
            red.AddKey(new Keyframe(from.Key - 24f, from.Value.r));
            green.AddKey(new Keyframe(from.Key - 24f, from.Value.g));
            blue.AddKey(new Keyframe(from.Key - 24f, from.Value.b));
            alpha.AddKey(new Keyframe(from.Key - 24f, from.Value.a));
            // Add all the steps in between to the curve
            for (int i = 0; i < times.Count; i++)
            {
                var step = times[i];
                // Add the postframe to the animation curve
                red.AddKey(new Keyframe(step.Key, step.Value.r));
                green.AddKey(new Keyframe(step.Key, step.Value.g));
                blue.AddKey(new Keyframe(step.Key, step.Value.b));
                alpha.AddKey(new Keyframe(step.Key, step.Value.a));
            }
            // Add the postframe to the animation curve (back to first)
            red.AddKey(new Keyframe(to.Key + 24f, to.Value.r));
            green.AddKey(new Keyframe(to.Key + 24f, to.Value.g));
            blue.AddKey(new Keyframe(to.Key + 24f, to.Value.b));
            alpha.AddKey(new Keyframe(to.Key + 24f, to.Value.a));
            var pixels = new Color[1024 * 16];
            for (int i = 0; i < 1024; i++)
            {
                values[i] = new Color(
                    red.Evaluate(i / 1024f * 24f),
                    green.Evaluate(i / 1024f * 24f),
                    blue.Evaluate(i / 1024f * 24f),
                    alpha.Evaluate(i / 1024f * 24f));
            }
        }
        return values;
    }
}
