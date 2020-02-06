# TrashfromerBot
Current Features:
    Sound Effects - Custom sounds to be played by your viewers or by mods wIth custom commands for each sound.
    Counters - Custom commands to count stream events such as deaths, yawns, or sneezes.
    Basic Commands - Custom commands with custom text responses  such as !social printing out social media info, or !schedule printing a link   to your schedule.

Features in development

    Currently no features are in development as I am concentrating on Bug fixes with the base bot and it’s three starting features.

Planned Features

     Currently no specific features are planned but you can request some here:  https://forms.gle/UmNmbdu4bk3XMGmLA

Setup Guide

     The latest release of the bot can be downloaded from the release section of this repo, here: 

     After downloading the bot extract it to your location of choice. In the netcoreapp2.1 folder you will find TrashfomerBot.exe, this is the   bot executable.  Running the bot for the first time will generate the configuration file for you to fill out, settings.txt.

Example Configuration

    Channel Name = <Name of the channel to connect to, aka your twitch display name>
    Token = <oauth Token, leave blank for default>
    Username = <Username, Currently semas to do nothing but Twitchlib requires it, leave blank for default>
    RequireModForCommands = <True Or False>
    $command: !twit,<link to your twitter here>
    $sfx: !bong,<Path to your audio file, .wav preferably>
    $counter:  !count,<Text to come before the number>,<Text to come after the number>
