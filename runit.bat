del /F /Q KSP-Test\GameData\CivilianPopulationRevamp\
robocopy /E GameData\ KSP-Test\GameData\
copy Source\bin\Release\CivilianPopulationRevamp.dll KSP-Test\GameData\CivilianPopulationRevamp\
KSP-Test\KSP_x64.exe