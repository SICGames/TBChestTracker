# TBChestTracker - Total Battle Chest Tracker
![GitHub commit activity](https://img.shields.io/github/commit-activity/t/SICGames/TBChestTracker)
![GitHub contributors](https://img.shields.io/github/contributors/SICGames/TBChestTracker)
[![Github All Releases](https://img.shields.io/github/downloads/SICGames/TBChestTracker/total.svg)]()

## Patreons get a special treat!
- They get a sneak peak at the latest builds available on Discord. Until those latest builds pass with flying colors by a jury of patreons, they will hit Github. And in addition, it speeds up the development when I have instant feedback. 
- Become a VIP Patreon and don't miss out on the latest builds. Or you can wait.

## Description
TotalBattle Chest Tracker was designed to easily track your clan's chest count without worrying about installing various dependancies to get it working. You don't need to install Tesseract OCR, configure your enviroment path to TESSDATA_PATH and figure out how to use a simple program. Total Battle Chest Tracker is install, and boom you're in. Create clan and start counting chests. 

## How does this witch craft work? 
Optical Character Recongition (OCR) looks at a image, figures out where each word is and returns the word from image in text format. A lot more goes into it, though. For a OCR to currectly read results, the image needs to be prepped up first. Grayscaled, have a threshold applied and maybe some other image effects to clean the image up. Then the OCR attempts to read the image. Sometimes you may get incorrect words. Why though? Tesseract OCR does have a load for fonts loaded for it to understand, but I am guessing that Total Battle's Font is not one of them. Hence, why sometimes you get mispelled words. This can be improved upon training Tesseract OCR to understand that font. Sources have said a good training model can be days or weeks to get more accurate results. Tesseract OCR is great but does have weaknesses too. Quality of the image can affect what results you get back.

## The Future Of Total Battle Chest Tracker 
There's a couple of paths this can take. I've been at this since 2023, once it has reached it's stable public release; I'd like to take a break from the project. I'm fancying into machine learning and wanting to experiment a tad bit here and there. There's a chance if a newer version of Total Battle Chest Tracker will be released, it would be written in C++ or Python with GUI. Perhaps a better OCR engine and the ability to become a live chest counter. With a new language, it will be compatible with MacOS, Linux and Windows operating systems. This Total Battle Chest Tracker is only compatible with Windows. Who knows what the future has in store.

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

## Are you stuck on something?
Watch the Youtube videos 
[TotalBattleGuide - Youtube Channel](https://www.youtube.com/@TotalBattleGuide)

## How to install
- Head over to [Releases section](https://github.com/SICGames/TBChestTracker/releases) and download the latest version installation executable.

## Want to contribute? 
Head over to [Contributions section](https://github.com/SICGames/TBChestTracker/blob/master/CONTRIBUTING.md) to contribute to this project.

## How Do I use this?
- In the Start Up Page, click on New Clan and create your clan. 
- You'll be prompted to choose the Ocr Languages you want the OCR to understand and the OCR Studio. Once you're done selecting the Ocr Languages and enter into OCR Studio, you click on the Rectangle icon to draw a Region of Interest you want the OCR to focus on. Then you click on the cross hair icon and place it on the Open button. You can preview what the OCR sees by clicking on the eye icon. When you're happy, click on the Checkmark icon and it will save.  
- Inside the game, head over to Gifts tab and press F9 to start the automation process. Let it count through and once it sees the words, "No gifts" it will stop automation and begin building chests. 
- If you're using chest points, add chest point values via Clan Chest Requirements window under the Chest Points tab.
- When chest period is done, click on File then click on Export and click on the File textbox. Save it as CSV file. CSV file is used to upload to Google Sheets. 


