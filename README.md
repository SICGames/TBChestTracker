# TBChestTracker - Total Battle Chest Tracker
![GitHub commit activity](https://img.shields.io/github/commit-activity/t/SICGames/TBChestTracker)
![GitHub contributors](https://img.shields.io/github/contributors/SICGames/TBChestTracker)
[![Github All Releases](https://img.shields.io/github/downloads/SICGames/TBChestTracker/total.svg)]()

## Important Note
This project is no longer being worked on or maintained. This project is being -reworked from the ground up. Meanwhile, working on other projects as well. So, I appreciate everyone's support and all. But I can not continue unarchiving this and re-archieving this. 
I need to do what makes me happier. Honestly, this project been more stressful than I'd like. Complexity of how ocr stuff work is insane, and also depending on others hardware is more insane. Just because i'm running it fine on my system, may not work on others. In addition, the project is overwhemlingly bigger than it started out to be. 
While I did learn a lot creating this, i can apply future knowledge to other projects. Maybe not having many singletons and maybe creating factory interfaces or other types of interfaces. 
No more update gays and gals, i need to work on other projects and slowly take my time on a newer chest counter. I love C# and all, but newer chest counter should be written in another language. 

## Building This Project using Visual Studio 2022
This is a very high dependant program made in C# WPF. See CONTRIBUTE.md for more information. All these can be obtained through my account
Requirements are as follows:
- Visual Studio 2022
- Knowledge in C# and C++
- [CaptainHook](https://github.com/SICGames/CaptainHook) - Obtained through NuGet packages within project. Or it should.
- [libTessy](https://github.com/SICGames/libTessy)
- [Tessy](https://github.com/SICGames/Tessy)
- [KonquestUI](https://github.com/SICGames/KonquestUI)
- [libSnapture](https://github.com/SICGames/libSnapture)
- [Snapster](https://github.com/SICGames/Snapster)
- [Loggio.NET](https://github.com/SICGames/Loggio.NET)
- [ChestBuilder](https://github.com/SICGames/ChestBoxBuilder)
- TB Chest Tracker NodeJS related stuff (can be obtained through installation)
- [CrashBox](https://github.com/SICGames/CrashBox)

## Description
TotalBattle Chest Tracker was designed to easily track your clan's chest count without worrying about installing various dependancies to get it working. You don't need to install Tesseract OCR, configure your enviroment path to TESSDATA_PATH and figure out how to use a simple program. Total Battle Chest Tracker is install, and boom you're in. Create clan and start counting chests. 

## How does this witch craft work? 
Optical Character Recongition (OCR) looks at a image, figures out where each word is and returns the word from image in text format. A lot more goes into it, though. For a OCR to currectly read results, the image needs to be prepped up first. Grayscaled, have a threshold applied and maybe some other image effects to clean the image up. Then the OCR attempts to read the image. Sometimes you may get incorrect words. Why though? Tesseract OCR does have a load for fonts loaded for it to understand, but I am guessing that Total Battle's Font is not one of them. Hence, why sometimes you get mispelled words. This can be improved upon training Tesseract OCR to understand that font. Sources have said a good training model can be days or weeks to get more accurate results. Tesseract OCR is great but does have weaknesses too. Quality of the image can affect what results you get back.

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
 
</details>
<details>
<summary>
Clan Insights
</summary>
  
 * Track clan performance and statistics.
 * Filter clan insights data by name using Quick Filter feature.
 * Filter clan insights data by chest type.
 * Resize Clan insights columns.

</details>

## Are you stuck on something?
Watch the Youtube videos 
[TotalBattleGuide - Youtube Channel](https://www.youtube.com/@TotalBattleGuide)

## How to install
- Head over to [Releases section](https://github.com/SICGames/TBChestTracker/releases) and download the latest version installation executable.

## How Do I use this?
- In the Start Up Page, click on New Clan and create your clan. 
- You'll be prompted to choose the Ocr Languages you want the OCR to understand and the OCR Studio. Once you're done selecting the Ocr Languages and enter into OCR Studio, you click on the Rectangle icon to draw a Region of Interest you want the OCR to focus on. Then you click on the cross hair icon and place it on the Open button. You can preview what the OCR sees by clicking on the eye icon. When you're happy, click on the Checkmark icon and it will save.  
- Inside the game, head over to Gifts tab and press F9 to start the automation process. Let it count through and once it sees the words, "No gifts" it will stop automation and begin building chests. 
- If you're using chest points, add chest point values via Clan Chest Requirements window under the Chest Points tab.
- When chest period is done, click on File then click on Export and click on the File textbox. Save it as CSV file. CSV file is used to upload to Google Sheets. 


