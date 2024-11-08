# OCB Custom Biome Spectrums Core Mod - 7 Days to Die (V1.0) Addon

This mod doesn't do anything on its own, it only add
functionality for other modders to add additional lighting
("global weather illumination") spectrums.

Demo: https://github.com/OCB7D2D/OcbSpectraGreen/releases

![Green Weather](Screens/green-weather.jpg)

To hook up your spectrum, you need to reference them in the
weather system in the biomes.xml `worldgeneration` part.

```xml
<append xpath="/worldgeneration/biomes">
	<spectrum name="SpectrumGreen">
		<gradient type="sky">
			<!-- times here will only match in-game time if day length is 12h -->
			<!-- will be correctly shifted in-game if setting there is different -->
			<step time="5.00" color="0,0,0,255"/>
			<step time="6.00" color="184,81,255,255"/>
			<step time="11.00" color="107,23,255,255"/>
			<step time="15.00" color="107,23,255,255"/>
			<step time="17.50" color="154,210,255,255"/>
			<step time="19.00" color="0,0,0,255"/>
		</gradient>
		<texture type="ambient" path="#@modfolder:Resources/SpectrumGreen.unity3d?ambient"/>
		<texture type="sun" path="#@modfolder:Resources/SpectrumGreen.unity3d?sun"/>
		<texture type="moon" path="#@modfolder:Resources/SpectrumGreen.unity3d?moon"/>
		<texture type="fog" path="#@modfolder:Resources/SpectrumGreen.unity3d?fog"/>
		<texture type="fogfade" path="#@modfolder:Resources/SpectrumGreen.unity3d?fogfade"/>
	</spectrum>
</append>
```

Once you've added your custom spectrum, you need to hook them up
into the weather system. You can either add completely new setups
or change the spectrum of an existing biome weather:

```xml
<set xpath="/worldgeneration/biomes/biome[@name='snow']/weather[@name='fog']/spectrum/@name">SpectrumGreen</set>
<set xpath="/worldgeneration/biomes/biome[@name='snow']/weather[@name='snow']/spectrum/@name">SpectrumGreen</set>
<set xpath="/worldgeneration/biomes/biome[@name='snow']/weather[@name='storm']/spectrum/@name">SpectrumGreen</set>
```

Please refer to the [demo repository][1] for some example spectrum
textures that have been taken directly from the game assets. I
haven't figured out much myself, but I'm pretty sure that the
textures are read left to right in regard to day time.

### Changing blood moon color (and blood moon cloud tint)

With version 0.6.1 you can also tint the blood moon.

```xml
<append xpath="/worldgeneration">
	<spectrum name="bloodmoon">
		<gradient>
			<step time="18.00" color="148,0,211,255"/>
			<step time="24.00" color="255,0,0,255"/>
			<step time="06.00" color="148,0,211,255"/>
		</gradient>
	</spectrum>
	<spectrum name="bloodcloud">
		<gradient>
			<step time="18.00" color="148,0,211,255"/>
			<step time="24.00" color="255,0,0,255"/>
			<step time="06.00" color="148,0,211,255"/>
		</gradient>
	</spectrum>
</append>
```

### Download and Install

Simply download here from GitHub and put into your Mods folder:

- https://github.com/OCB7D2D/OcbCustomSpectrums/releases

[1]: https://github.com/OCB7D2D/OcbSpectraGreen
