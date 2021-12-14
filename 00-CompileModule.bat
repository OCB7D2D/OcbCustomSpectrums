@echo off

call MC7D2D Spectra0Core.dll /reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" *.cs && ^
echo Successfully compiled Spectra0Core.dll

pause