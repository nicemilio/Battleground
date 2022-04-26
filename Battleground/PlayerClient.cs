using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Battleground
{
    internal class PlayerClient
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
        protected bool myTurn = false;
        protected int[] lastShot = new int[2]; //{theRow, theCol}


        public PlayerClient()
        {
            this.client = new TcpClient();
            this.myBoard = new board(10, 10, true);
            this.enemyBoard = new board(10, 10, false);
            this.myTurn = false;
            Output ("Starting client");
            Output ("Press the connect button to connect to a server");
            //StartClient(ipString);
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
            while (true)
            {


                while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
                {
                    mData = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);
                    mData = mData.Substring(0, mData.Length - 1);
                    this.myTurn = false;
                    if (mData.Length == 3)
                    {
                        String response = checkEnemyShot(mData);
                        SendData(response);
                        this.myTurn = (response != HIT && response != DESTROY);
                        if (this.myTurn) refreshConsole("Your opponent shot (" + mData + ") and missed!");
                        else refreshConsole("Your opponent hit (" + mData + ") and is taking another turn");
                        break;
                    }
                    String[] myTurnArray = { FIRST, HIT, DESTROY, RETRY };
                    this.myTurn = myTurnArray.Contains(mData);
                    String[] shotResponseArray = { MISS, HIT, DESTROY };
                    if (shotResponseArray.Contains(mData))
                        this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');

                }
            }
        }


        virtual public bool Shoot(string theCoords)
        {
            while (true)
            {
                if (this.myTurn)
                {
                    SendData(theCoords);
                    this.lastShot[0] = coordinateToRowCol(theCoords.Substring(0, 1));
                    this.lastShot[1] = coordinateToRowCol(theCoords.Substring(1, 2));
                    this.myTurn = false;
                    return true;
                }
                else
                    return false;
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
            try
            {
                int theRow = coordinateToRowCol(mData.Substring(0, 1)); //First character in the string
                int theCol = coordinateToRowCol(mData.Substring(1, 2)); //Second and Thid characters in the string
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
                        (window as MainWindow).Console.Text += theMsg + "\r\n" ;
                    }
                }
            }
        }

        private void refreshConsole(String message = "")
        {
            Output (message);
        }
        //Helper method to conver battleshipe coordinates (A10) to our integers
        protected int coordinateToRowCol(string co)
        {
            switch (co)
            {
                case ("01"):
                case ("A"):
                case ("a"): return 0;
                case ("02"):
                case ("B"):
                case ("b"): return 1;
                case ("03"):
                case ("C"):
                case ("c"): return 2;
                case ("04"):
                case ("D"):
                case ("d"): return 3;
                case ("05"):
                case ("E"):
                case ("e"): return 4;
                case ("06"):
                case ("F"):
                case ("f"): return 5;
                case ("07"):
                case ("G"):
                case ("g"): return 6;
                case ("08"):
                case ("H"):
                case ("h"): return 7;
                case ("09"):
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
