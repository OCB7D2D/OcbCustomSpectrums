@echo off

call MC7D2D CustomSpectrums.dll Harmony\*cs Sources\*.cs ^
	/reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" && ^
echo Successfully compiled CustomSpectrums.dll

pause