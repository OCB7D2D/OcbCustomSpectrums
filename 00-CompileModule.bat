@echo off

call MC7D2D NewWeather.dll /reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" *.cs && ^
echo Successfully compiled NewWeather.dll

pause