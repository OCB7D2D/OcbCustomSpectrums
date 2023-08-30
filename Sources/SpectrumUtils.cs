using System.Xml.Linq;

public static class SpectrumUtils
{

    // ####################################################################
    // ####################################################################

    public static OcbSpectrum ParseSpectrum(XElement el)
    {

        var type = el.GetAttribute("type");
        if (string.IsNullOrEmpty(type)) return null;
        var variant = el.GetAttribute("variant");
        if (string.IsNullOrEmpty(variant)) return null;
        // Try to parse enum for valid atmopshere effect type
        var spec = EnumUtils.Parse<AtmosphereEffect.ESpecIdx>(type, true);

        if (variant == "gradient")
        {
            OcbSpectrum spectrum = new OcbSpectrum(spec);
            spectrum.Parse(el.Elements("step"));
            return spectrum;
        }
        else if (variant == "texture")
        {
            // ToDo: implement loading textures directly?
            // Note: not sure if it really adds much value!?
            Log.Error("Spectrum textures not implemented");
            throw new System.NotImplementedException();
        }
        else
        {
            Log.Error("Unknown spectrum variant {0}", variant);
        }
        return null;
    }

    // ####################################################################
    // ####################################################################

}
