﻿using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using multimodal;
using CSGSI;
using CSGSI.Nodes;
using WindowsInput.Native;
using WindowsInput;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AppGui
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public struct Gamestate
    {
        public int money;
        public bool IsPlanted;
        public int round_number;
        public int bullets;
        public int max_bullets;
        public int health;
        public int armour;
        public int roundkills;
        public int total_kills;
        public int total_deaths;
        public int spectators;

    }
    static class Prices
    {
        public const int ak47 = 2700;
        public const int awp = 4750;
        public const int deagle = 700;

    }
    

    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;
        private Tts t = new Tts();
        private static GameStateListener gsl;
        public Gamestate gamestate = new Gamestate();
        

       
        public MainWindow()
        {
            InitializeComponent();
            gsl = new GameStateListener(3000);
            gsl.NewGameState += new NewGameStateHandler(OnNewGameState);
            if (!gsl.Start())
            {
                Environment.Exit(0);
            }
            
            gamestate.money = 800;
            Console.WriteLine("Listening... n\n");
            
            //t.Speak("Por favor, espere um pouco enquanto tentamos conectar...");
            mmiC = new MmiCommunication("localhost",8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;
            mmiC.Start();

        }
       
        void OnNewGameState(GameState gs)
        {
            if (!gamestate.IsPlanted &&
               gs.Round.Phase == RoundPhase.Live &&
               gs.Round.Bomb == BombState.Planted &&
               gs.Previously.Round.Bomb == BombState.Undefined)
            {
                Console.WriteLine("Bomb has been planted.");
                t.Speak("A bomba foi plantada, explodirá em 45 segundos");
                gamestate.IsPlanted = true;
            }
            else if (gamestate.IsPlanted && gs.Round.Phase == RoundPhase.FreezeTime)
            {
                gamestate.IsPlanted = false;
            }
            gamestate.money = gs.Player.State.Money;
            gamestate.round_number = gs.Map.Round;
            gamestate.bullets = gs.Player.Weapons.ActiveWeapon.AmmoClip;
            gamestate.max_bullets = gs.Player.Weapons.ActiveWeapon.AmmoClipMax;
            gamestate.health = gs.Player.State.Health;
            gamestate.armour = gs.Player.State.Armor;
            gamestate.roundkills = gs.Player.State.RoundKills;
            gamestate.total_kills = gs.Player.MatchStats.Kills;
            gamestate.total_deaths = gs.Player.MatchStats.Deaths;
            gamestate.spectators = gs.Map.CurrentSpectators;


        }
       

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);

            InputSimulator inputsim = new InputSimulator();
            
            double confidence = Double.Parse((string)json.recognized[0].ToString());
            
            String command = (string)json.recognized[1].ToString();
           
            String command2 = (string)json.recognized[2].ToString();
            
            String details = (string)json.recognized[3].ToString();

            String weapon = (string)json.recognized[4].ToString();


            if (t.getSpeech() == true)
            {                
                return;
            }

            if (confidence < 0.3)
            {
                t.Speak("Desculpe, pode repetir?");
            }
                       
            else
            {
                switch (command)
                {
                    // QUANTOS/QUANTAS
                    case "HOWMANY":
                        switch(command2)
                        {                            
                            // RONDAS
                            case "ROUNDS":
                                {
                                    switch (details)
                                    {
                                        // FALTAM PARA O FIM DO JOGO
                                        case "LEFT":
                                            {
                                                // GET ROUNDS LEFT UNTIL END OF GAME
                                                
                                                t.Speak("Faltam " + (30-gamestate.round_number) + " rondas");
                                                break;
                                            }
                                        // FALTAM PARA O INTERVALO
                                        case "LEFT_HALF":
                                            {
                                                // GET ROUNDS LEFT UNTIL HALFTIME
                                                if (gamestate.round_number >= 15)
                                                    t.Speak(" Já estás na segunda parte do jogo!");
                                                else if (gamestate.round_number == 0)
                                                    t.Speak(" O jogo ainda não começou!");
                                                else
                                                    t.Speak("Faltam " + (15 - gamestate.round_number) + " rondas para o intervalo");
                                                break;
                                            }
                                    }
                                    break;
                                }
                            // BALAS TENHO
                            case "BULLETS":
                                {
                                    t.Speak(" Tens " + gamestate.bullets + " balas restantes");
                                    if (gamestate.bullets <= (0.1 * gamestate.max_bullets))
                                        t.Speak(" Aconselho-te a recarregar.");
                                    break;
                                }
                            // MORTES NUMA RONDA
                            case "KILLS":
                                {
                                    switch(details)
                                    {
                                        case "HAVE_ROUND":
                                            {
                                                t.Speak("Mataste " + gamestate.roundkills + " nesta ronda");
                                                break;
                                            }
                                        case "HAVE":
                                            {
                                                t.Speak("Tens um total de " + gamestate.total_kills + " mortes");
                                                break;
                                            }
                                    }

                                 break;
                                }
                            // TOTAL DE OBITOS
                            case "DEATHS":
                                {
                                    t.Speak("Morreste " + gamestate.total_deaths);
                                    break;
                                }
                            // ESPETADORES 
                            case "SPECTATORES":
                                {
                                    t.Speak("Temos no total " + gamestate.spectators + "espetadores");
                                    break;
                                }
                        }
                        break;
                    // QUANTO/QUANTA
                    case "HOWMUCH":
                        switch(command2)
                        {
                            // DINHEIRO
                            case "MONEY":
                                {
                                    switch(details)
                                    {
                                        // TENHO
                                        case "HAVE":
                                            {
                                                t.Speak("Tens "+gamestate.money+" dólares.");
                                                break;
                                            }
                                        // CUSTA UM/UMA
                                        case "COST":
                                            {
                                                switch(weapon)
                                                {
                                                    // AK-47
                                                    case "AK47":
                                                        {
                                                            t.Speak(" Uma AK-47 custa " + Prices.ak47 + " dólares");
                                                            break;
                                                        }
                                                    // DEAGLE
                                                    case "DEAGLE":
                                                        {
                                                            t.Speak(" Uma Desert Eagle custa " + Prices.deagle + " dólares");
                                                            break;
                                                        }
                                                    // AWP
                                                    case "AWP":
                                                        {
                                                            t.Speak(" Uma AWP custa " + Prices.awp + " dólares");
                                                            break;
                                                        }                                                    
                                                }
                                                break;
                                            }
                                        // FALTA-ME PARA COMPRAR UMA
                                        case "LEFT":
                                            {
                                                switch (weapon)
                                                {
                                                    // AK-47
                                                    case "AK47":
                                                        {
                                                            if (gamestate.money < Prices.ak47)
                                                                t.Speak(" Faltam-te " + (Prices.ak47 - gamestate.money) + " dólares");
                                                            else
                                                                t.Speak(" Consegues comprar uma A K 47 e sobrar " + (gamestate.money - Prices.ak47) + " dólares.");
                                                            break;
                                                        }
                                                    // DEAGLE
                                                    case "DEAGLE":
                                                        {
                                                            if (gamestate.money < Prices.deagle)
                                                                t.Speak(" Faltam-te " + (Prices.deagle - gamestate.money) + " dólares");
                                                            else
                                                                t.Speak(" Consegues comprar uma Deagle e sobrar " + (gamestate.money - Prices.deagle) + " dólares.");
                                                            break;
                                                        }
                                                    // AWP
                                                    case "AWP":
                                                        {
                                                            if (gamestate.money < Prices.awp)
                                                                t.Speak(" Faltam-te " + (Prices.awp - gamestate.money) + " dólares");
                                                            else
                                                                t.Speak(" Consegues comprar uma A W P e sobrar " + (gamestate.money - Prices.awp) + " dólares.");
                                                            break;
                                                        }
                                                }
                                                break;
                                            }
                                    }
                                    break;
                                }
                            // VIDA
                            case "HEALTH":
                                {
                                    switch(details)
                                    {
                                        // TENHO
                                        case "HAVE":
                                            {
                                                t.Speak("Tens " + gamestate.health + "pontos de vida");
                                                break;
                                            }
                                    }
                                    break;
                                }
                            // ARMADURA
                            case "ARMOUR":
                                {
                                    switch (details)
                                    {
                                        // TENHO
                                        case "HAVE":
                                            {
                                                t.Speak("Tens " + gamestate.armour + "pontos de armadura");
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                        break;
                    // TENHO
                    case "HAVE":
                        {
                            switch(command2)
                            {
                                //DINHEIRO
                                case "MONEY":
                                    {
                                        switch(details)
                                        {
                                            //SUFICIENTE PARA COMPRAR UM/UMA
                                            case "ENOUGH_FOR":
                                                {
                                                    switch (weapon)
                                                    {
                                                        // AK-47
                                                        case "AK47":
                                                            {
                                                                if (gamestate.money < Prices.ak47)
                                                                    t.Speak(" Não, faltam-te" + (Prices.ak47 - gamestate.money) + " dólares");
                                                                else
                                                                    t.Speak(" Tens dinehiro para comprar uma AK-47 e sobrar " + (gamestate.money - Prices.ak47) + " dólares.");
                                                                break;
                                                            }
                                                        // DEAGLE
                                                        case "DEAGLE":
                                                            {
                                                                if (gamestate.money < Prices.deagle)
                                                                    t.Speak(" Não, faltam-te " + (Prices.deagle - gamestate.money) + " dólares");
                                                                else
                                                                    t.Speak(" Tens dinehiro para comprar uma Deagle e sobrar " + (gamestate.money - Prices.deagle) + " dólares.");
                                                                break;
                                                            }
                                                        // AWP
                                                        case "AWP":
                                                            {
                                                                if (gamestate.money < Prices.awp)
                                                                    t.Speak("Não, faltam-te " + (Prices.awp - gamestate.money) + " dólares");
                                                                else
                                                                    t.Speak(" Tens dinehiro para comprar uma AWP e sobrar " + (gamestate.money - Prices.awp) + " dólares.");
                                                                break;
                                                            }
                                                    }
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    // COMPRA-ME UM/UMA
                    case "BUY_ME":
                        {
                            switch (weapon)
                            {
                                // AK-47
                                case "AK47":
                                    {
                                        if (gamestate.money < Prices.ak47)
                                            t.Speak(" Não consigo, faltam-te" + (Prices.ak47 - gamestate.money) + " dólares");
                                        else
                                        {
                                            // CÓDIGO PARA DAR INPUT DA TECLA: VK_6 ( Tecla 6 do teclado )
                                            inputsim.Keyboard.KeyPress(VirtualKeyCode.VK_6);

                                            t.Speak(" Compraste uma A K 47 e sobraram " + (gamestate.money - Prices.ak47) + " dólares.");
                                        }
                                        break;
                                    }
                                // DEAGLE
                                case "DEAGLE":
                                    {
                                        if (gamestate.money < Prices.deagle)
                                            t.Speak(" Não consigo, faltam-te " + (Prices.deagle - gamestate.money) + " dólares");
                                        else
                                            t.Speak(" Compraste uma Deagle e sobraram " + (gamestate.money - Prices.deagle) + " dólares.");
                                        break;
                                    }
                                // AWP
                                case "AWP":
                                    {
                                        if (gamestate.money < Prices.awp)
                                            t.Speak("Não consigo, faltam-te " + (Prices.awp - gamestate.money) + " dólares");
                                        else
                                            t.Speak(" Compraste uma A W P e sobraram " + (gamestate.money - Prices.awp) + " dólares.");
                                        break;
                                    }
                            }
                            break;
                        }
                    // ABRE-ME O CSGO
                    case "OPEN_CSGO":
                        {
                            t.Speak(" Abrindo o Counter Strike ");
                            Process.Start(@"C:\Program Files (x86)\Steam\Steam.exe", "-applaunch 730 -console +exec autoexec +exec autoexec");
                            break;
                        }
                }
            }                        
        }
    }
}
