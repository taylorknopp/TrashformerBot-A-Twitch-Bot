# TrashfromerBot
Default Config and Default Features:

    Channel Name = <Channel Name>
    Token = <Token To Use, leave blank for default>
    Username = <Username, Currently seams to do nothing but Twitchlib requires it, leave blank for default>
    requireMod = <True/False> - Is mod required for all bot commands
    quote = <True/False> - Enable AI quotes command !quote
    pretty = <True/False> - Enable randomly calling chatters pretty
    prettyVal = 1000 - How likely is a chat to set off the pretty command. 1/X where this value is X. SO here it would be 1/1000 chance every time someone sends a chat.
    dice = <True/False> - Enable D6 Rolling with !roll
    coin = <True/False> - enable Coin flip with !flip
    swearJar = <True/False> - Enable the sear Jar Feature !sj
    swearJarDonmination = <2 decimal place float> - How much to charge someone when they swear
    dad = <True/False> Enable dad joke command !dad

    Sound Effects - Custom sounds to be played by your viewers or by mods with custom commands for each sound.
    Counters - Custom commands to count stream events such as deaths, yawns, or sneezes.
    Basic Commands - Custom commands with custom text responses such as !social printing out social media info, or !schedule printing a link to your schedule.
    !temp <A temperature in either Celsius or Fahrenheit followed by the corresponding unity C/F> - Converts the temp to the other unit.
    !quote - returns a random quote.
    !stats - lists stats for all counters.

## Features in development

    Currently no features are in development as I am concentrating on 
    Bug fixes with the base bot and its starting features.

## Planned Features

   Currently no specific features are planned but you can request some here, [Bot Feature Request](https://forms.gle/12LwgnkpPPTFqica7 "Google Forms") 
     
## Setup Guide

   The latest release of the bot can be downloaded from the release section of this repo, here, [Releases](https://github.com/taylorknopp/TrashformerBot-A-Twitch-Bot/releases "Releases")

     After downloading the bot extract it to your location of choice.
     In the netcoreapp2.1 folder you will find TrashfomerBot.exe,
     this is the bot executable. Running the bot for the first time will 
     generate the configuration file for you to fill out, settings.txt.

## Example Configuration

Channel Name = <Channel Name>
Token =
Username =
requireMod = false
quote = false
pretty = true
prettyVal = 1000
dice = true
coin = true
swearJar = true
swearJarDonmination = 0.1
dad = true
$command: !twit,<link to your twitter here>
$sfx: !bong,<Path to your audio file, .wav preferably>
$counter:  !count,<Text to come before the number>,<Text to come after the number>
    
## Default Account
   The default account the bot will use is TrashformerBot on Twitch
   
   Default Account Logo Art by [AllTheseVertices](https://www.twitch.tv/allthesevertices "AllTheseVertices On Twitch")
