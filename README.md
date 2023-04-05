TLDR;

Build the release version of the Utilities project
run:
   .\utilities reset


Overview:

The utilities.ps1 shell script includes 2 paths:
   - $localSettingsPath: the path to the local.settings.json file to use for the configuration
   - $sampleDataPath: path to the sample data set

The sample data set can be found in the following repository: https://github.com/Hornswaggler/visiophone-test-data

Commands:

delete
   Deletes all data in the database and blob containers
create
   Creates database collections and blob containers
seed
   inserts seed data from referenced directory 


reset
   Runs all of the above commands