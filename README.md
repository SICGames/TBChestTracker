# TBChestTracker - Total Battle Chest Tracker
![GitHub commit activity](https://img.shields.io/github/commit-activity/t/SICGames/TBChestTracker)
![GitHub contributors](https://img.shields.io/github/contributors/SICGames/TBChestTracker)
[![Github All Releases](https://img.shields.io/github/downloads/SICGames/TBChestTracker/total.svg)]()

## Patreons get a special treat!
- They get a sneak peak at the latest builds available on Discord. Until those latest builds pass with flying colors by a jury of patreons, they will hit Github. And in addition, it speeds up the development when I have instant feedback. 
- Become a VIP Patreon and don't miss out on the latest builds. Or you can wait.
## 2.6.6. Known Issues
- Index Out Of Range exception occurs with ChestBoxBuilder.exe. This has been patched in 2.6.7. Unfortunately, I am placing in a new logger and placing in a newer version of Tesseract.
- Mispelled clan names are result from EMGU.cv tesseract's version. Unable to load Tesseract's Best Trained Models without causing issues. Newer Tesseract version, I can load Best Trained Models containing LSTM.
- With the new tesseract version, filtering out the image and cleaning up may be removed. Since accuracy has improved significally.

## Description
TotalBattle Chest Tracker was designed to easily track your clan's chest count. 

## Features  
<details><summary>
  Clan management
</summary>
  
 * Manage Multiple Clans.
  
</details>
<details>
<summary>
  OCR
</summary>
  
 * Choose multiple languages for the OCR to understand.
 * Select Region for OCR to extract text from with OCR Wizard.
 * OCR detects unknown clan mates and adds them to database.

</details>
<details>
<summary>
Clan Insights
</summary>
  
 * Track clan performance and statistics.
 * Future implendation is to allow clans to create goals to achieve. 
 * Future implendation is to allow simulate what they'd need to do to get higher clan wealth.
 * Filter clan insights data by name using Quick Filter feature.
 * Filter clan insights data by chest type.

</details>
<details>
<summary>
Clanmate Management
</summary>
  
 * Add clanmates via text file.
 * Add clanmates via selection rectangle.
 * Clanmate search box filters a clan mate. 
 * Create clanmate aliases by selecting parent clanmate name then their known aliases. In case OCR misreads their name. 
 * Remove multiple clanmate names.
 
</details>

## Are you stuck on something?
Watch the Youtube videos 
[TotalBattleGuide - Youtube Channel](https://www.youtube.com/@TotalBattleGuide)

## How to install
- Head over to [Releases section](https://github.com/SICGames/TBChestTracker/releases) and download the latest version installation executable.

## Want to contribute? 
Head over to [Contributions section](https://github.com/SICGames/TBChestTracker/blob/master/CONTRIBUTING.md) to contribute to this project.

## Launched Program and now what?
- In the Start Up Page, click on New Clan and create your clan. 
- Follow the OCR Wizard. 
- Begin adding new clan mates. 
- If you're using chest points, add chest point values via Clan Chest Requirements window under the Chest Points tab.
- When you're ready to begin the automation process, press F9 to start automation procecss and F10 to stop automation process.
- Everything is saved when creating new clan and when new clanmate is added, in addition when automation is stopped.
- When chest period is done, click on File then click on Export and click on the File textbox. Save it as CSV file. CSV file is used to upload to Google Sheets. 


