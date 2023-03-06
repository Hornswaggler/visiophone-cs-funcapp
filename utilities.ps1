$localSettingsPath = "C:/projects/visiophone-cs-funcapp/visiophone-cs-funcapp/local.settings.json";
$sampleDataPath = "C:/projects/visiophone-test-data";
.\Utilities\bin\Release\net6.0\Utilities.exe "SETTINGS=$localSettingsPath" "SAMPLE_DATA_PATH=$sampleDataPath" "$args"