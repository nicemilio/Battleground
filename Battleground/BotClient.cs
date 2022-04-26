using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Battleground
{
    internal class BotClient : PlayerClient
    {
        private bool hunting = false;
        private string previousResult = "";
        private Random rnd = new Random();

        void StartClient(string ipString = "127.0.0.1")
        {
            //"127.0.0.1"
            IPAddress ip = IPAddress.Parse(ipString);
            Console.WriteLine("BotClient started");
            int port = 5000;
            for (int i = 0; i < 10; i++)
            {
                try { this.client.Connect(ip, port); break; }
                catch (SocketException e)
                {
                    Console.WriteLine("Bot failed to start, trying " + (10 - i) + " more times.");
                }
            }
            this.lastShot[0] = 0;
            this.lastShot[1] = 0;
            // if (i > 9) exit; //TODO program exits when the connection doesnt work

            Thread threadReceiveData = new Thread(ReceiveData);
            threadReceiveData.Start();
            Thread.Sleep(100);
        }
        protected override void ReceiveData()
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
                    Console.WriteLine("Bot receives: " + mData);
                    if (mData.Length == 3)
                    {
                        String response = checkEnemyShot(mData);
                        SendData(response);
                        Thread.Sleep(500);
                        // Console.WriteLine("Bot just sent: " + response);
                        this.myTurn = (response != HIT && response != DESTROY);
                        break;
                    }
                    String[] myTurnArray = { FIRST, HIT, DESTROY, RETRY };
                    this.myTurn = myTurnArray.Contains(mData);
                    String[] shotResponseArray = { MISS, HIT, DESTROY };
                    if (shotResponseArray.Contains(mData))
                        this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');
                    this.previousResult = mData;
                }
            }
        }


        public override bool Shoot (String something)
        { //TODO Need a thread to be constantly checking if its your turn(?)
            while (true)
            {
                if (this.myTurn)
                {
                    //  Console.WriteLine("Bot is shooting!");
                    int nextRow = this.lastShot[0];
                    int nextCol = this.lastShot[1];
                    if (this.previousResult == HIT) this.hunting = true;
                    else if (this.previousResult == DESTROY) this.hunting = false;
                    // if previousResult == MISS dont change hunting
                    // Console.WriteLine(hunting ? "Bot is hunting" : "Bot is not hunting");
                    if (this.hunting)
                    {
                        /*
                            check around previous shot
                            if the previous shot has another hit next to it
                                find out if it's north/south or east/west
                                check if the shot is legal (like above)
                            if the prvious shot is surrounded by water/misses (not including edges)
                                choose a random direction and shoot there (or just start with north)
                                checking if the shot is legal (isn't next to *another* ship, not already a miss)

                        */

                        if (!this.enemyBoard.CheckPosition(this.lastShot[0], this.lastShot[1], 1, false, 'x', true))
                        {
                            int dir = 1;

                            bool isVertical = this.enemyBoard.PrintCoord(nextRow, nextCol - 1) == 'x' ||
                                            this.enemyBoard.PrintCoord(nextRow, nextCol + 1) == 'x';
                            try
                            {
                                while (this.enemyBoard.PrintCoord(nextRow, nextCol) != 'w')
                                {
                                    //TODO add check for loop to these if statements
                                    if (this.enemyBoard.PrintCoord(nextRow, nextCol) == 'o') dir *= -1; //Check if the next coordinate was already tried
                                    if (isVertical ? this.enemyBoard.PrintCoord(nextRow, nextCol + (2 * dir)) == 'x' :
                                                     this.enemyBoard.PrintCoord(nextRow + (2 * dir), nextCol) == 'x') dir *= -1; //Check if a ship is 2 spots away
                                                                                                                                 //Move along the ship until you hit water
                                    if (isVertical) nextCol += dir;
                                    else nextRow += dir;
                                }
                            }
                            catch (IndexOutOfRangeException e)
                            {
                                dir *= -1;
                                if (dir == 1) throw new Exception("Help, I'm stuck in a loop"); //TODO more information here
                            }
                        }
                        else
                        {
                            switch (chooseDirection(nextRow, nextCol))
                            {
                                case 0:
                                    {
                                        nextCol -= 1;
                                        break;
                                    }
                                case 1:
                                    {
                                        nextCol += 1;
                                        break;
                                    }
                                case 2:
                                    {
                                        nextRow -= 1;
                                        break;
                                    }
                                case 3:
                                    {
                                        nextRow += 1;
                                        break;
                                    }
                                default: throw new Exception("Something went wrong");
                            }

                        }

                    }
                    else
                    {
                        //Console.WriteLine("The non hunting bot is choosing a coordinate");
                        while (!this.enemyBoard.CheckPosition(nextRow, nextCol, 1, false, 'x') ||
                                this.enemyBoard.PrintCoord(nextRow, nextCol) == 'o')
                        {

                            nextRow = rnd.Next(0, 10); //TODO change from 10 to board length
                            nextCol = rnd.Next(0, 10);
                        }
                    }
                    this.lastShot[0] = nextRow;
                    this.lastShot[1] = nextCol;
                    Console.WriteLine("Bot is shooting " + rowColToCoordinate(nextRow, nextCol));
                    SendData(rowColToCoordinate(nextRow, nextCol));
                    this.myTurn = false;
                }
                return true;
            }
        }

        private int chooseDirection(int theRow, int theCol)
        {
            //TODO refactor for DRY and less exception throwing

            //Up
            try
            {
                if (this.enemyBoard.PrintCoord(theRow, theCol - 1) == 'w') try
                    {
                        if (this.enemyBoard.PrintCoord(theRow, theCol - 2) != 'x') return 0;
                    }
                    catch (IndexOutOfRangeException e) { return 0; }
            }
            catch (IndexOutOfRangeException e) { }

            //Down
            try
            {
                if (this.enemyBoard.PrintCoord(theRow, theCol + 1) == 'w') try
                    {
                        if (this.enemyBoard.PrintCoord(theRow, theCol + 2) != 'x') return 1;
                    }
                    catch (IndexOutOfRangeException e) { return 1; }
            }
            catch (IndexOutOfRangeException e) { }

            //Left
            try
            {
                if (this.enemyBoard.PrintCoord(theRow - 1, theCol) == 'w') try
                    {
                        if (this.enemyBoard.PrintCoord(theRow - 2, theCol) != 'x') return 2;
                    }
                    catch (IndexOutOfRangeException e) { return 2; }
            }
            catch (IndexOutOfRangeException e) { }

            //Right
            try
            {
                if (this.enemyBoard.PrintCoord(theRow + 1, theCol) == 'w') try
                    {
                        if (this.enemyBoard.PrintCoord(theRow + 2, theCol) != 'x') return 3;
                    }
                    catch (IndexOutOfRangeException e) { return 3; }
            }
            catch (IndexOutOfRangeException e) { }
            return -1; //If we get here, something went wrong
        }
        protected override string checkEnemyShot(string shot)
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
                return RETRY;
            }
        }

        private string rowColToCoordinate(int theRow, int theCol)
        {
            string word = "";
            word += (char)(theRow + 65);
            word += (theCol + 1).ToString("00");
            return word;
        }
    }
}
