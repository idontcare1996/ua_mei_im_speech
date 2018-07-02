using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using mmisharp;
using Newtonsoft.Json;
using multimodal;
using CSGSI;
using CSGSI.Nodes;

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
            
            gamestate.money = 0;
            Console.WriteLine("Listening..." + gamestate.money + "\n\n");
            
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
                t.Speak("A bomba foi plantada, tens 45 segundos para a desligar.");
                gamestate.IsPlanted = true;
            }
            else if (gamestate.IsPlanted && gs.Round.Phase == RoundPhase.FreezeTime)
            {
                gamestate.IsPlanted = false;
            }
        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            Console.WriteLine(e.Message);
            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);
            
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
                            // TERRORISTAS
                            case "TERROR":
                                {
                                    switch (details)
                                    {
                                        // ESTÃO VIVOS
                                        case "ALIVE":
                                            {
                                                // GET ALIVE TERRORISTS

                                                t.Speak(" Estão vivos x terroristas");
                                                break;
                                            }
                                        // ESTÃO MORTOS
                                        case "DEAD":
                                            {
                                                // GET DEAD TERRORISTS
                                                t.Speak(" Estão mortos x terroristas");
                                                break;
                                            }
                                    }
                                    break;
                                }
                            // CONTRA-TERRORISTAS
                            case "CTS":
                                {
                                    switch (details)
                                    {
                                        // ESTÃO VIVOS
                                        case "ALIVE":
                                            {
                                                // GET ALIVE COUNTER-TERRORISTS
                                                t.Speak(" Estão vivos x contra-terroristas");
                                                break;
                                            }
                                        // ESTÃO MORTOS
                                        case "DEAD":
                                            {
                                                // GET DEAD COUNTER-TERRORISTS
                                                t.Speak(" Estão mortos x contra-terroristas");
                                                break;
                                            }
                                    }
                                    break;
                                }
                            // JOGADORES
                            case "PLAYERS":
                                {
                                    switch (details)
                                    {
                                        // ESTÃO VIVOS
                                        case "ALIVE":
                                            {
                                                // GET ALIVE PLAYERS
                                                t.Speak(" Estão vivos x ");
                                                break;
                                            }
                                        // ESTÃO MORTOS
                                        case "DEAD":
                                            {
                                                // GET DEAD PLAYERS
                                                t.Speak(" Estão mortos x ");
                                                break;
                                            }
                                    }
                                    break;
                                }
                            // RONDAS
                            case "ROUNDS":
                                {
                                    switch (details)
                                    {
                                        // FALTAM PARA O FIM DO JOGO
                                        case "LEFT":
                                            {
                                                // GET ROUNDS LEFT UNTIL END OF GAME
                                                t.Speak("Faltam x rondas");
                                                break;
                                            }
                                        // FALTAM PARA O INTERVALO
                                        case "LEFT_HALF":
                                            {
                                                // GET ROUNDS LEFT UNTIL HALFTIME
                                                t.Speak("Faltam x rondas para o intervalo");
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                        break;
                    // QUANTO
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
                                                t.Speak("Tens x dólares.");
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

                                                            t.Speak(" Uma AK-47 custa 2700 dólares");
                                                            break;
                                                        }
                                                    // DEAGLE
                                                    case "DEAGLE":
                                                        {
                                                            t.Speak(" Uma Desert Eagle custa 700 dólares");
                                                            break;
                                                        }
                                                    // AWP
                                                    case "AWP":
                                                        {
                                                            t.Speak(" Uma AWP custa 4750 dólares");
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
            }

                        
        }
    }
}
