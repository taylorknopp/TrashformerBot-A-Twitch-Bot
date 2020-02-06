using System;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using System.Net.WebSockets;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Interfaces;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Streams.Effects;
using CSCore.CoreAudioAPI;
using LiteDB.Engine;
using LiteDB;


namespace TwitchBotBecauseIWantTo
{
    class Program
    {
        LiteEngine LiteEngine = new LiteEngine();
        static LiteDB.LiteDatabase db = new LiteDatabase(Directory.GetCurrentDirectory() + @"\counters.db");
        static LiteDB.ILiteCollection<counter> countersColection = db.GetCollection<counter>("counters");
        static List<string> lines = new List<string>() { "Channel Name = ", "Token = ", "Username = " };
        static List<string> countCommands = new List<string>();
        public static List<command> commands = new List<command>();
        public static List<sfx> SFX = new List<sfx>();
        
        static void Main(string[] args)
        {
            string settingsPath = Directory.GetCurrentDirectory() + @"\settings.txt";

            if (!File.Exists(settingsPath))
            {
                
                File.AppendAllLines(settingsPath, lines);
            }

            var settingsFile = File.ReadAllLines(settingsPath);
            var settingsList = new List<string>(settingsFile);
            string channel = null;
            string token;
            string username;
            bool requireMod = true;
            try
            {
                bool.TryParse(settingsList[3].Split("=")[1],out requireMod);
            }
            catch
            {

            }
            try
            {
                channel = settingsList[0].Split('=')[1].Replace(" ", "");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Channel Specified - Press any key to continue");
                Console.ReadKey();
                Environment.Exit(0);

            }
            if(channel.Length <= 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Channel Specified - Press any key to continue");
                Console.ReadKey();
                Environment.Exit(0);
            }
            try
            {
                token = settingsList[1].Split('=')[1].Replace(" ", "");
            }
            catch
            {
                token = "oauth:x64c6vsc9pfxnajwp16slxhusuiy9e";
            }
            try
            {
                username = settingsList[2].Split('=')[1].Replace(" ", "");
            }
            catch
            {
                username = "TrashformerBot";
            }
           
            
            
            int numCommands = 0;
            int numSfx = 0;
            int numCounters = 0;




            foreach (string line in settingsList)
            {
                if (line.Contains("$command:"))
                {
                    string full = line.Split("$command:")[1];
                    string command = full.Split(',')[0].Replace(" ","");
                    string rsp = full.Split(',')[1];
                    commands.Add(new command(command, rsp));
                    numCommands++;


                }
                if (line.Contains("$sfx:"))
                {
                    string full = line.Split("$sfx:")[1];
                    string command = full.Split(',')[0].Replace(" ", "");
                    string rsp = full.Split(',')[1];
                    SFX.Add(new sfx(command, rsp));
                    numSfx++;
                }
                if(line.Contains("$counter:"))
                {
                    List<string> parts = line.Split("$counter:")[1].Split(",").ToList();
                    List<counter> CTs = countersColection.FindAll().ToList();
                    string cmd = parts[0].Replace(" ", "");
                    string partA = "";
                    string partB = "";
                    try
                    {
                        partA = parts[1];
                    }
                    catch
                    {

                    }

                    try
                    {
                        partB = parts[2];
                    }
                    catch
                    {

                    }
                    if(partA == "" && partB == "")
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Malformed Counter In Config: " + cmd + " On Line: " + settingsList.IndexOf(line).ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    if(CTs.Where(counter => counter.commandString == cmd).Count() <= 0)
                    {
                        counter ct = new counter();
                        ct.commandString = cmd;
                        ct.textPartA = partA;
                        ct.textPartB = partB;
                        
                        countersColection.Insert(ct);
                    }
                    else
                    {
                        counter ct = CTs.Where(counter => counter.commandString == cmd).FirstOrDefault();
                        if(ct.textPartA != partA || ct.textPartB != partB)
                        {
                            ct.textPartA = partA;
                            ct.textPartB = partB;
                            countersColection.Update(ct);
                        }
                        

                    }
                    countCommands.Add(cmd);
                    
                    numCounters++;
                    
                   
                    
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(numCommands + " Commands Loaded");
            Console.WriteLine(numSfx + " SFX Loaded");
            Console.WriteLine(numCounters + " Counters Loaded");
            Console.WriteLine("Commands Require Moderator Status Is Set To: " + requireMod);
            Console.WriteLine("--Press Any Key To Continue--");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadKey();

            Bot bot = new Bot(channel,token,username,commands, SFX, countersColection,countCommands,requireMod);
            while(true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("----Menu----");
                Console.WriteLine("1. Reset Counter");
                Console.WriteLine("2. Exit");
                ConsoleKey key = Console.ReadKey().Key;
                Console.Clear();
                if(key == ConsoleKey.D1)
                {
                    Console.WriteLine("Chose What Counter To Reset");
                    List < counter > CTs = countersColection.FindAll().ToList();
                    foreach(counter ct in CTs)
                    {
                        Console.WriteLine(CTs.IndexOf(ct).ToString() + ". " + ct.commandString);
                    }
                    Console.WriteLine("Type the number Of the counter you want to reset and hit enter: ");
                    string keyOfCounterToReset = Console.ReadLine();
                    int index = -1;
                    Int32.TryParse(keyOfCounterToReset, out index);
                    if (index >= 0 && index < CTs.Count())
                    {
                        counter ctToClear = CTs[index];
                        ctToClear.count = 0;
                        countersColection.Update(ctToClear);

                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Invalid Choice");
                        Thread.Sleep(1500);
                    }

                }
                if(key == ConsoleKey.D2)
                {
                    Console.Clear();
                    Console.WriteLine("Shutting Down...");
                    Thread.Sleep(2500);
                    Environment.Exit(0);
                }
            }
           
                



        }
    }
    class Bot
    {
        TwitchClient client;
        public List<command> commands;
        public List<sfx> sfxes;
        public LiteDB.ILiteCollection<counter> counters;
        public List<string> countersList;
        public bool requireMod = true;
         
        public Bot(string channel,string token,string Username, List<command> commandList, List<sfx> sfxList, LiteDB.ILiteCollection<counter> countersCol, List<string> ctList, bool requireModBool)
        {
            countersList = ctList;
            counters = countersCol;
            commands = commandList;
            sfxes = sfxList;
            requireMod = requireModBool;
            ConnectionCredentials credentials = new ConnectionCredentials("TrashformerBot", "oauth:x64c6vsc9pfxnajwp16slxhusuiy9e");

            WebSocketClient customClient = new WebSocketClient();
            client = new TwitchClient(customClient);
            client.Initialize(credentials, channel);

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            //client.OnWhisperReceived += Client_OnWhisperReceived;
            //client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;


            client.Connect();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            //Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            //Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            //Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            //client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.Channel).FirstOrDefault(), "Hey guys! I am a bot connected via TwitchLib!");

        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            bool isMod = e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster;
            bool continueOrStopBecasueNotMod = !requireMod || (requireMod && isMod);
            if(!continueOrStopBecasueNotMod)
            {
                return;
            }
            string message = e.ChatMessage.Message;
            foreach (command cmd in commands)
            {
                if(message.Contains(cmd.commandString))
                {
                    client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), cmd.response);
                }
            }
            foreach (sfx sfx in sfxes)
            {
                if (message.Contains(sfx.commandString))
                {
                    sfx.play();
                }
            }
            foreach(string ct in countersList)
            {
                if (message.Contains(ct))
                {
                    List<counter> CTs = counters.FindAll().ToList();
                    counter ctOBJ = CTs.Where(counter => message.Contains(counter.commandString)).FirstOrDefault();
                    string count = ctOBJ.increment().ToString();
                    counters.Update(ctOBJ);
                    client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), ctOBJ.textPartA + " " + count  + " " + ctOBJ.textPartB);
                }
            }
            if (e.ChatMessage.Message.Contains("!dadJoke"))
            {

                client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), "JOKE: ");
            }

        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            if (e.WhisperMessage.Username == "my_friend")
                client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            else
                client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
        }
    }
    class command
    {
        public string commandString;
        public string response;
        public command(string cmd, string rsp)
        {
            commandString = cmd;
            response = rsp;
        }



    }

    class sfx
    {


        
        private ISoundOut _soundOut;
        private IWaveSource _source;
        
       

        public string commandString;
        public string sfxPath;
        public sfx(string cmd, string path)
        {
            commandString = cmd;
            sfxPath = path;
        }
        public void play()
        {
            ISampleSource source = CodecFactory.Instance.GetCodec(this.sfxPath).ToSampleSource();

            var notificationSource = new SingleBlockNotificationStream(source);



            _source = notificationSource.ToWaveSource(16);

           

            //play the audio
            _soundOut = new WasapiOut();
            _soundOut.Initialize(_source);
            _soundOut.Play();

        }
       


    }
    class counter
    {
        public int _id { get; set; }
        public int count { get; set; }
        public string textPartA { get; set; }
        public string textPartB { get; set; }
        public string commandString { get; set; }



        
        public int increment()
        {

            this.count++;
          
            return this.count;
            
        }
    }
}

