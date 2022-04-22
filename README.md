# OCB Spectra Effects Core Mod - 7 Days to Die (A20) Addon

This mod doesn't do anything on its own, it only add
functionality for other modders to add additional lighting
("global weather illumination") spectra.

Demo: https://github.com/OCB7D2D/SpectraGreen/archive/master.zip

![Green Weather](Screens/green-weather.jpg)

To hook up your spectra, you need to reference them in the
weather system in the biomes.xml `worldgeneration` part.

```xml
<append xpath="/worldgeneration/spectra">
	<spectrum name="SpectraGreen">
		<texture type="sky" path="#@modfolder:Resources/SpectraGreen.unity3d?sky"/>
		<texture type="ambient" path="#@modfolder:Resources/SpectraGreen.unity3d?ambient"/>
		<texture type="sun" path="#@modfolder:Resources/SpectraGreen.unity3d?sun"/>
		<texture type="moon" path="#@modfolder:Resources/SpectraGreen.unity3d?moon"/>
		<texture type="fog" path="#@modfolder:Resources/SpectraGreen.unity3d?fog"/>
		<texture type="fogfade" path="#@modfolder:Resources/SpectraGreen.unity3d?fogfade"/>
	</spectrum>
</append>
```

Once you've added your custom spectra, you need to hook them up
into the weather system. You can either add completely new setups
or change the spectrum of an existing biome weather:

```xml
<set xpath="/worldgeneration/biomes/biome[@name='snow']/weather[@name='fog']/spectrum/@name">SpectraGreen</set>
<set xpath="/worldgeneration/biomes/biome[@name='snow']/weather[@name='snow']/spectrum/@name">SpectraGreen</set>
<set xpath="/worldgeneration/biomes/biome[@name='snow']/weather[@name='storm']/spectrum/@name">SpectraGreen</set>
```

Please refer to the [demo repository][1] for some example light
textures that have been taken directly from the game assets. I
haven't figured out much myself, but I'm pretty sure that the
textures are read left to right in regard to day time.

### Download and Install

Simply download here from GitHub and put into your A20 Mods folder:

- https://github.com/OCB7D2D/Spectra0Core/archive/master.zip

## Compatibility

I've developed and tested this Mod against version a20.b218.

[1]: https://github.com/OCB7D2D/SpectraGreen
