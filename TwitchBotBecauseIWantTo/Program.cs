using System;
using System.Linq;
using System.Runtime.InteropServices;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication;
using TwitchLib.Communication.Events;
using System.Net.WebSockets;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Interfaces;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Runtime;
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
using RestSharp;

namespace TwitchBotBecauseIWantTo
{
    class Program
    {
        LiteEngine LiteEngine = new LiteEngine();
        static LiteDB.LiteDatabase db = new LiteDatabase(Directory.GetCurrentDirectory() + @"\counters.db");
        static LiteDB.ILiteCollection<counter> countersColection = db.GetCollection<counter>("counters");
        static LiteDB.ILiteCollection<swearJarAcc> sjAccClection = db.GetCollection<swearJarAcc>("sjAccClection");
        static List<string> lines = new List<string>() { "Channel Name = ", "Token = ", "Username = ",  "requireMod = ", "quote = "};
        static List<string> countCommands = new List<string>();
        public static List<command> commands = new List<command>();
        public static List<sfx> SFX = new List<sfx>();
        public static bool quote = false;
        public static bool pretty = false;
        public static bool dice = false;
        public static bool coin = false;
        public static int prettyVal = 100;
        public static bool swearJar = false;
        public static float denom = 1.00F; 
        
        static void Main(string[] args)
        {
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            string settingsPath = "";

            if (isWindows)
            {
                settingsPath = Directory.GetCurrentDirectory() + @"\settings.txt";
            }
            else
            {
                settingsPath = Directory.GetCurrentDirectory() + @"/settings.txt";
            }
            

            if (!File.Exists(settingsPath))
            {
                
                File.AppendAllLines(settingsPath, lines);
            }
            Console.WriteLine(settingsPath);
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
                bool.TryParse(settingsList[4].Split("=")[1], out quote);
            }
            catch
            {

            }
            try
            {
                bool.TryParse(settingsList[5].Split("=")[1], out pretty);
            }
            catch
            {

            }
            try
            {
                Int32.TryParse(settingsList[6].Split("=")[1], out prettyVal);
            }
            catch
            {

            }
            try
            {
                bool.TryParse(settingsList[7].Split("=")[1], out dice);
            }
            catch
            {

            }
            try
            {
                bool.TryParse(settingsList[8].Split("=")[1], out coin);
            }
            catch
            {

            }
            try
            {
                bool.TryParse(settingsList[9].Split("=")[1], out swearJar);
            }
            catch
            {

            }
            try
            {
                float.TryParse(settingsList[10].Split("=")[1], out denom);
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
                token = "whh64pi7gjbmljju7z5mw1awmbx7cv";
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
                    commands.Add(new command(command.ToLower(), rsp));
                    numCommands++;


                }
                if (line.Contains("$sfx:"))
                {
                    string full = line.Split("$sfx:")[1];
                    string command = full.Split(',')[0].Replace(" ", "");
                    string rsp = full.Split(',')[1];
                    SFX.Add(new sfx(command.ToLower(), rsp));
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
                    if(CTs.Where(counter => counter.commandString == cmd.ToLower()).Count() <= 0)
                    {
                        counter ct = new counter();
                        ct.commandString = cmd.ToLower();
                        ct.textPartA = partA;
                        ct.textPartB = partB;
                        
                        countersColection.Insert(ct);
                    }
                    else
                    {
                        counter ct = CTs.Where(counter => counter.commandString == cmd.ToLower()).FirstOrDefault();
                        if(ct.textPartA != partA || ct.textPartB != partB)
                        {
                            ct.textPartA = partA;
                            ct.textPartB = partB;
                            countersColection.Update(ct);
                        }
                        

                    }
                    countCommands.Add(cmd.ToLower()); ;
                    
                    numCounters++;
                    
                   
                    
                }
            }
           /* foreach (counter ct in countersColection.FindAll())
            {
                if(ct.commandString.Any(char.IsUpper))
                {
                    ct.commandString = ct.commandString.ToLower();
                    countersColection.Update(ct);
                }
            }*/
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(numCommands + " Commands Loaded");
            Console.WriteLine(numSfx + " SFX Loaded");
            Console.WriteLine(numCounters + " Counters Loaded");
            Console.WriteLine("Commands Require Moderator Status Is Set To: " + requireMod);
            Console.WriteLine("--Press Any Key To Continue--");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.ReadKey();

            Bot bot = new Bot(channel,token,username,commands, SFX, countersColection, sjAccClection, countCommands,requireMod,quote,pretty,coin,dice,prettyVal,swearJar,denom);
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
        public LiteDB.ILiteCollection<swearJarAcc> accounts;
        public List<string> countersList;
        public bool requireMod = true;
        public bool quote = false;
        public bool pretty = false;
        public bool dice = false;
        public bool coin = false;
        public int pretyIntVal = 100;
        public bool swearJar = false;
        public float denomination = 1.00F;

        public Bot(string channel,string token,string Username, List<command> commandList, List<sfx> sfxList, LiteDB.ILiteCollection<counter> countersCol, LiteDB.ILiteCollection<swearJarAcc> accountsColl, List<string> ctList, bool requireModBool, bool quoteBool, bool isPrettyBool, bool coinBool, bool diceBool,int pretyInt,bool sjBool,float sjDenom)
        {
            
            countersList = ctList;
            counters = countersCol;
            commands = commandList;
            sfxes = sfxList;
            quote = quoteBool;
            requireMod = requireModBool;
            pretty = isPrettyBool;
            dice = diceBool;
            coin = coinBool;
            swearJar = sjBool;
            pretyIntVal = pretyInt;
            accounts = accountsColl;

            denomination = sjDenom;
            if (Username == "")
            {
                Username = "TrashformerBot";
            }
            if (token == "")
            {
                token = "oauth:x64c6vsc9pfxnajwp16slxhusuiy9e";
            }
            ConnectionCredentials credentials = new ConnectionCredentials(Username, token);

            WebSocketClient customClient = new WebSocketClient();
            client = new TwitchClient(customClient);
            client.Initialize(credentials, channel);

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            //client.OnWhisperReceived += Client_OnWhisperReceived;
            //client.OnNewSubscriber += Client_OnNewSubscriber;
            client.OnConnected += Client_OnConnected;
            client.OnDisconnected += Client_OnDisconnected;


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

        private void Client_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            Console.WriteLine("DISCONECTED!!!");
        }
        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            //client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.Channel).FirstOrDefault(), "Hey guys! I am a TrashformerBot!");

        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Random rand = new Random();
            bool isMod = e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster;
            bool continueOrStopBecasueNotMod = !requireMod || (requireMod && isMod);
            if(pretty)
            {
                if (rand.Next(0, pretyIntVal) == pretyIntVal - 1)
                {
                    client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), e.ChatMessage.Username + " you so pretty!");
                }
            }
            



            if (!continueOrStopBecasueNotMod)
            {
                return;
            }
            string message = e.ChatMessage.Message.ToLower();
            foreach (command cmd in commands)
            {
                if(message.ToLower().Contains(cmd.commandString.ToLower()))
                {
                    client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), cmd.response);
                }
            }
            foreach (sfx sfx in sfxes)
            {
                if (message.ToLower().Contains(sfx.commandString))
                {
                    sfx.play();
                }
            }
            foreach(string ct in countersList)
            {
                if (message.ToLower().Contains(ct.ToLower()))
                {
                    List<counter> CTs = counters.FindAll().ToList();
                    counter ctOBJ = CTs.Where(counter => message.Contains(counter.commandString.ToLower())).FirstOrDefault();
                    string count = ctOBJ.increment().ToString();
                    counters.Update(ctOBJ);
                    client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), ctOBJ.textPartA + " " + count  + " " + ctOBJ.textPartB);
                }
            }
            if (e.ChatMessage.Message.ToLower().Contains("!stats"))
            {
                string count = "";

                foreach (string ctCommand in countersList)
                {
                    List<counter> CTs = counters.FindAll().ToList();
                    counter ctOBJ = CTs.Where(counter => ctCommand.Contains(counter.commandString.ToLower())).FirstOrDefault();
                     count = count + ctOBJ.textPartA + " " + ctOBJ.count.ToString() + " " + ctOBJ.textPartB + "\r\n\r\n";
                }
                client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), count );
            }
            if(e.ChatMessage.Message.ToLower().Contains("!quote") && quote)
            {
                var restClient = new RestClient("https://rapidapi.p.rapidapi.com/ai-quotes/0");
                var request = new RestRequest(Method.GET);
                request.AddHeader("x-rapidapi-key", "5881a49a26mshe388135102c2cebp1a0796jsn99699e24584c");
                request.AddHeader("x-rapidapi-host", "quoteai.p.rapidapi.com");
                IRestResponse response = restClient.Execute(request);
                List<string> quoteParts = response.Content.Replace("{", "").Split(",").ToList();
                string quote = quoteParts[0].Remove(0, quoteParts[0].IndexOf(":")) + " -- " + quoteParts.Find(String => String.Contains("quote")).Remove(0, quoteParts.Find(String => String.Contains("quote")).IndexOf(":"));
                quote = quote.Replace(":", "").Replace("\"","");
                client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), quote);
            }
            if(e.ChatMessage.Message.ToLower().Contains("!temp"))
            {
                string tempString = e.ChatMessage.Message.ToLower().Replace("!temp","").Replace(" ","");
                if (tempString.Contains("c"))
                {
                    Int32 T = 0;
                    Int32.TryParse(tempString.Replace("c", ""), out T);
                    T = (int)((T * 1.8) + 32);
                    
                    client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), T + "F");
                }
                else
                {
                    Int32 T = 0;
                    Int32.TryParse(tempString.Replace("f", ""), out T);
                    T = (int)((T - 32) / 1.8);
                    client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), T + "C");
                }
            }
            if(e.ChatMessage.Message.ToLower().Contains("!roll") && dice)
            {
                
                int result = rand.Next(1, 7);
                client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), result.ToString());
            }
            if (e.ChatMessage.Message.ToLower().Contains("!flip") && coin)
            {
                
                string[] msg = new string[] { "Heads", "Tails" };
                int result = rand.Next(0, 2);
                client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), msg[result]);
            }
            if (message.ToLower().Contains("!sj") && !message.ToLower().Contains("!sjreset") && swearJar)
            {
                List<swearJarAcc> accountsList = accounts.FindAll().ToList();
                string userNameToFind = e.ChatMessage.Message.Remove(0,3).Replace(" ","");
                swearJarAcc acc = accountsList.Where(swearJarAcc => swearJarAcc.useranme == userNameToFind).FirstOrDefault();
                string owings = "";
                if (acc == null)
                {
                    acc = new swearJarAcc();
                    acc.useranme = userNameToFind;
                    acc.owings = 0;
                    owings = acc.increment(denomination).ToString();
                    accounts.Insert(acc);
                }
                else
                {
                    owings = acc.increment(denomination).ToString();
                    accounts.Update(acc);
                }



                
                
                client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), userNameToFind + " ows " + owings + "$ to the swear jar!");
            }
            if (message.ToLower().Contains("!sjreset") && swearJar)
            {
                List<swearJarAcc> accountsList = accounts.FindAll().ToList();
                string userNameToFind = e.ChatMessage.Message.Remove(0, 8).Replace(" ", "");
                swearJarAcc acc = accountsList.Where(swearJarAcc => swearJarAcc.useranme == userNameToFind).FirstOrDefault();
                
                if (acc == null)
                {
                    return;
                }
                else
                {
                    acc.reset();
                    accounts.Update(acc);
                }





                client.SendMessage(client.JoinedChannels.Where(JoinedChannel => JoinedChannel.Channel == e.ChatMessage.Channel).FirstOrDefault(), userNameToFind + " ows nothing now!");
            }

        }

        private void Client_OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            if (e.WhisperMessage.Username == "my_friend")
                client.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }


        private void Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime);
            //client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points! So kind of you to use your Twitch Prime on this channel!");
            //else
                //client.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the substers! You just earned 500 points!");
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

    class swearJarAcc
    {
        public int _id { get; set; }
        public float owings { get; set; }
        public string useranme { get; set; } 
        public void reset()
        {
            this.owings = 0F;
        }


        public float increment(float denomination)
        {

            this.owings += denomination;

            return this.owings;

        }
    }

}

