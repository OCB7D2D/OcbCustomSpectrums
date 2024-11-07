using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using UnityEngine;

public class OcbSpectrum
{

    // ####################################################################
    // ####################################################################

    public AtmosphereEffect.ESpecIdx spec;

    readonly AnimationCurve reds = new AnimationCurve();
    readonly AnimationCurve greens = new AnimationCurve();
    readonly AnimationCurve blues = new AnimationCurve();
    readonly AnimationCurve alphas = new AnimationCurve();

    // ####################################################################
    // ####################################################################

    public OcbSpectrum(AtmosphereEffect.ESpecIdx spec)
    {
        this.spec = spec;
    }

    // ####################################################################
    // ####################################################################

    // Helper to normalize time for regular 12h cycle
    private static float NormalizeTime(float v)
        => v >= 6 ? v - 6f : v + 18f;

    // Helper to parse optional xml attributes
    public static void ParseOptionalFloat(XElement step,
        string name, float def, out float inWeight)
    {
        if (step.TryGetAttribute(name, out string result))
            inWeight = float.Parse(result);
        else inWeight = def;
    }

    // ####################################################################
    // ####################################################################

    public void Parse(IEnumerable<XElement> steps)
    {
        // Only Unity 2022 has clear method
        reds.keys = new Keyframe[0];
        greens.keys = new Keyframe[0];
        blues.keys = new Keyframe[0];
        alphas.keys = new Keyframe[0];
        // Parse each step and add keyframes
        foreach (XElement step in steps)
        {
            // Get the mandatory fields for the animation curve step
            if (!step.HasAttribute("time")) Log.Error("No attribute `time` on gradient step");
            if (!step.HasAttribute("color")) Log.Error("No attribute `color` on gradient step");
            var time = NormalizeTime(float.Parse(step.GetAttribute("time")));
            var color = StringParsers.ParseColor32(step.GetAttribute("color"));
            // Get optional parameters for animation curve steps
            ParseOptionalFloat(step, "in-tangent", 0, out float inTangent);
            ParseOptionalFloat(step, "out-tangent", 0, out float outTangent);
            ParseOptionalFloat(step, "in-weight", 0, out float inWeight);
            ParseOptionalFloat(step, "out-weight", 0, out float outWeight);
            // Create the keyframes for all color channels
            Keyframe red = new Keyframe(time, color.r, inTangent, outTangent);
            Keyframe green = new Keyframe(time, color.g, inTangent, outTangent);
            Keyframe blue = new Keyframe(time, color.b, inTangent, outTangent);
            Keyframe alpha = new Keyframe(time, color.a, inTangent, outTangent);
            // Get weight mode for curve step interpolation
            WeightedMode mode = WeightedMode.None;
            if (inWeight != 0) mode |= WeightedMode.In;
            if (outWeight != 0) mode |= WeightedMode.Out;
            // Set additional config to the keyframe channels
            red.inWeight = green.inWeight = blue.inWeight = alpha.inWeight = inWeight;
            red.outWeight = green.outWeight = blue.outWeight = alpha.outWeight = outWeight;
            red.weightedMode = green.weightedMode = blue.weightedMode = alpha.weightedMode = mode;
            // Add all frames to curves
            reds.AddKey(red);
            greens.AddKey(green);
            blues.AddKey(blue);
            alphas.AddKey(alpha);
        }
        // Make sure curve extends around
        ExtendAnimationCurve(reds);
        ExtendAnimationCurve(greens);
        ExtendAnimationCurve(blues);
        ExtendAnimationCurve(alphas);
    }

    // ####################################################################
    // ####################################################################

    // Helper to ensure curve contains a full 24h cycle
    private void ExtendAnimationCurve(AnimationCurve curve)
    {
        Keyframe min = new Keyframe(float.MaxValue, 0);
        Keyframe max = new Keyframe(float.MinValue, 0);
        foreach (Keyframe step in curve.keys)
        {
            if (step.time < min.time) min = step;
            if (step.time >= max.time) max = step;
        }
        min.time += 24f;
        max.time -= 24f;
        curve.AddKey(min);
        curve.AddKey(max);
    }

    // ####################################################################
    // ####################################################################

    // Return new ColorSpectrum for use by patches
    public ColorSpectrum GetColorSpectrum()
    {
        // Sneaky way to avoid going through creating a texture just to throw it away again
        var spectrum = FormatterServices.GetUninitializedObject(typeof(ColorSpectrum)) as ColorSpectrum;
        // Set the color values directly on the object
        spectrum.values = GetColorSpectrumValues();
        // Return the new color spectrum
        return spectrum;
    }

    // ####################################################################
    // ####################################################################

    // Return array of `samples` from 0 to `max`
    private Color[] GetColorSpectrumValues(
        float max = 24f, int samples = 1024)
    {
        Color[] values = new Color[samples];
        for (int i = 0; i < samples; i++)
        {
            values[i] = new Color(
                // must multiply with float first!
                reds.Evaluate(i * max / samples),
                greens.Evaluate(i * max / samples),
                blues.Evaluate(i * max / samples),
                alphas.Evaluate(i * max / samples));
        }
        return values;
    }

    // ####################################################################
    // ####################################################################

}
