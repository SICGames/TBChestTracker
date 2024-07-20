# Localalization
locale folder holds all the localization files necessary. Would like to implemend localizaiton strings for ui under a seperate file. 
- ChestNames.csv
- ChestSources.csv
- Strings.csv

# Directory Format 

- locale (directory)
  - language short code (en-US)
	- file (Strings.csv)

locale\en-US\Strings.csv

locale\pl-PL\Strings.csv

# Strings.csv
Will be a dictionary list.

{English Name},{Translated Value}

Hello,Guten Tag 'locale\de-DE\Strings.csv'

Hello,Hello 'locale\en-US\Strings.csv'


# Localization Folder installation 

Should be in ProgramData\SICGames\TotalBattleChestTracker\

Sometimes accessing Program Files directory under windows makes Windows not happy. Hence why settings.json were moved to the ProgramData folder. I can be stored in localapp folder if needed to. 