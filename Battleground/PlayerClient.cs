using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Battleground
{
    internal class PlayerClient : ISubject
    {
        protected const string MISS = "miss!";
        protected const string HIT = "hit!";
        protected const string DESTROY = "destroyed!";
        protected const string GAME = "Game over, you win!";
        protected const string RETRY = "Bad input, shoot again";
        protected const string FIRST = "first!";
        protected const string SECOND = "second!";
        public board myBoard { get; set; }
        public board enemyBoard { get; set; }
        protected TcpClient client;
        protected string mData = "";
        protected Boolean myTurn = false;
        protected int[] lastShot = new int[2]; //{theRow, theCol}
        protected List<IObserver> _observers = new List<IObserver>();
        delegate void Update_TextBox_callback (String theMsg);


        public PlayerClient()
        {
            this.client = new TcpClient();
            this.myBoard = new board(10, 10, true);
            this.enemyBoard = new board(10, 10, false);
            Output ("Starting client");
            Output ("Press the connect button to connect to a server");
            //StartClient(ipString);
        }

        public void Register(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Unregister(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify()
        {
            _observers.ForEach(o => o.BoardChanged(this.myTurn));
        }

        public virtual bool StartClient(string ipString)
        {
            //"127.0.0.1"
            IPAddress ip = IPAddress.Parse(ipString);
            int port = 5000;
            string keepTrying = "y";
            while (keepTrying == "y")
            {
                try { this.client.Connect(ip, port); keepTrying = "n"; }
                catch (SocketException e)
                {
                    Output ("Connection refused. Please try again");
                    return false;
                }
            }

            Output ("Application connected to server!");
            Thread threadReceiveData = new Thread(ReceiveData);
            threadReceiveData.Start();
            return true;
        }
        protected virtual void ReceiveData()
        {
            NetworkStream ns = this.client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;
            String[] myTurnArray = { FIRST, HIT, DESTROY, RETRY };
            String[] shotResponseArray = { MISS, HIT, DESTROY };
            while (true)
            {
                while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
                {
                    mData = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                    mData = mData.Substring(0, mData.Length - 2);
                    this.myTurn = false;
                    if (mData.Length <= 3)
                    {
                        String response = checkEnemyShot(mData);
                        Notify();
                        // MessageBox.Show(response);
                        SendData(response);
                        if (response == GAME)
                        {
                            //TODO Offer rematch
                        }
                        else if (myTurnArray.Contains(response))
                        {
                        }
                        else
                        {
                            this.myTurn = true;
                        }
                        break;

                        
                    }
                    if (shotResponseArray.Contains(mData))
                        this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');
                    if (mData == DESTROY) 
                        this.enemyBoard.fillMisses(this.lastShot[0], this.lastShot[1]);
                    if (myTurnArray.Contains(mData)) 
                        this.myTurn = true;
                    Notify();
                }
            }
        }


        virtual public void Shoot(string theCoords)
        {
            Output(myTurn.ToString ());
            if (this.myTurn)
            {
                SendData(theCoords);
                this.lastShot[0] = coordinateToRowCol(theCoords.Substring(0, 1));
                this.lastShot[1] = coordinateToRowCol(theCoords.Substring(1, theCoords.Length - 1));
                this.myTurn = false;
            }
            
        }
        protected void SendData(String theMessage)
        {
            NetworkStream ns = this.client.GetStream();
            byte[] buffer = Encoding.ASCII.GetBytes(theMessage);
            ns.Write(buffer, 0, buffer.Length);
        }

        protected virtual string checkEnemyShot(string shot)
        {
            //MessageBox.Show(shot);
            try
            {
                int theRow = coordinateToRowCol(shot.Substring(0, 1)); //First character in the string
                int theCol = coordinateToRowCol(shot.Substring(1, shot.Length - 1)); //Second and Thid characters in the string
                switch (this.myBoard.Shoot(theRow, theCol))
                {
                    case ('o'): return MISS;
                    case ('x'): return HIT;
                    case ('d'): return DESTROY;
                    case ('g'): return GAME;
                    default: throw new Exception("Something went wrong");
                }
            }
            catch (Exception e)
            {
                Output ("Your opponent entered bad coordinates, they are trying again");
                return RETRY;
            }
        }

        public void Output (String theMsg)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    String text = (window as MainWindow).Console.Text;
                    int lines = text.Count(f => f == '\n');
                    if (lines > 3)
                    {
                        String newText = text.Substring(text.IndexOf('\n') + 1);
                        (window as MainWindow).Console.Text = newText + theMsg + "\r\n";
                    }
                    else
                    {
                        (window as MainWindow).Console.Text += theMsg + "\r\n";
                    }
                    
                }
            }
        }


        //Helper method to convert battleshipe coordinates (A10) to our integers
        protected int coordinateToRowCol(string co)
        {
            switch (co)
            {
                case ("1"):
                case ("A"):
                case ("a"): return 0;
                case ("2"):
                case ("B"):
                case ("b"): return 1;
                case ("3"):
                case ("C"):
                case ("c"): return 2;
                case ("4"):
                case ("D"):
                case ("d"): return 3;
                case ("5"):
                case ("E"):
                case ("e"): return 4;
                case ("6"):
                case ("F"):
                case ("f"): return 5;
                case ("7"):
                case ("G"):
                case ("g"): return 6;
                case ("8"):
                case ("H"):
                case ("h"): return 7;
                case ("9"):
                case ("I"):
                case ("i"): return 8;
                case ("10"):
                case ("J"):
                case ("j"): return 9;
                default: throw new Exception("Not a valid coordinate");
            }
        }

       
    }
}
