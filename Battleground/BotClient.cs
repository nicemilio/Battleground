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
        private int[] lastHit = { -1, -1 };
        private Random rnd = new Random();

        public override bool StartClient(string ipString = "127.0.0.1")
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
                    Console.WriteLine("Bot failed to start");
                    return false;
                }
            }
            this.lastShot[0] = 0;
            this.lastShot[1] = 0;
            // if (i > 9) exit; //TODO program exits when the connection doesnt work

            Thread threadReceiveData = new Thread(ReceiveData);
            threadReceiveData.Start();
            Thread shootplayer = new Thread(Shoot);
            shootplayer.Start(); 
            Thread.Sleep(100);
            return true;
        }
        protected override void ReceiveData()
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
                    mData = mData.Substring(0, mData.Length - 1);
                    //    Console.WriteLine ("Bot receives: " + mData);
                    if (mData.Length == 3)
                    {
                        String response = checkEnemyShot(mData);
                        SendData(response);
                        if (response == GAME) break; //End the game here
                        if (!myTurnArray.Contains(response)) Shoot();
                        break;
                    }
                    if (shotResponseArray.Contains(mData))
                        this.enemyBoard.AssignChar(this.lastShot[0], this.lastShot[1], mData == MISS ? 'o' : 'x');
                    if (mData == DESTROY) this.enemyBoard.fillMisses(this.lastShot[0], this.lastShot[1]);
                    //Assign hunting
                    if (mData == HIT)
                    {
                        this.hunting = true;
                        this.lastHit[0] = this.lastShot[0];
                        this.lastHit[1] = this.lastShot[1];
                    }
                    else if (mData == DESTROY) this.hunting = false;
                    if (myTurnArray.Contains(mData)) Shoot();
                }
            }
        }


        public void Shoot ()
        { //TODO Need a thread to be constantly checking if its your turn(?)
            while (true)
            {
                Thread.Sleep(500);
                int nextRow, nextCol;
                if (this.hunting)
                {
                    nextRow = this.lastShot[0];
                    nextCol = this.lastShot[1];
                    if (!this.enemyBoard.CheckPosition(nextRow, nextCol, 1, false, 'x', true))
                    {
                        int dir = 1;
                        bool isHorizontal = this.enemyBoard.checkHorizontal(nextRow, nextCol);

                        while (this.enemyBoard.GetCoord(nextRow, nextCol) != 'w')
                        {
                            if (isHorizontal)
                            {
                                if (nextCol == this.enemyBoard.GetLength(1) - 1 ||
                                    nextCol == 0 ||
                                    enemyBoard.GetCoord(nextRow, Math.Min(nextCol, this.enemyBoard.GetLength(1) - 1)) == 'o')
                                {
                                    if (dir == -1) throw new Exception("Help, I'm stuck in a loop!");
                                    dir = -1;
                                }
                                nextCol += dir;
                            }
                            else
                            {
                                if (nextRow == this.enemyBoard.GetLength(0) - 1 ||
                                    nextRow == 0 ||
                                    enemyBoard.GetCoord(Math.Min(nextRow, this.enemyBoard.GetLength(0) - 1), nextCol) == 'o')
                                {
                                    if (dir == -1) throw new Exception("Help, I'm stuck in a loop!");
                                    dir = -1;
                                }
                                nextRow += dir;
                            }
                        }
                    }
                    else
                    {
                        int choice = chooseDirection(nextRow, nextCol);
                        switch (choice)
                        {
                            case 0:
                                {
                                    nextRow -= 1;
                                    break;
                                }
                            case 1:
                                {
                                    nextRow += 1;
                                    break;
                                }
                            case 2:
                                {
                                    nextCol -= 1;
                                    break;
                                }
                            case 3:
                                {
                                    nextCol += 1;
                                    break;
                                }
                            default: throw new Exception("Something went wrong");
                        }
                    }
                }

                else
                {
                    do
                    {
                        nextRow = rnd.Next(0, 10); //TODO change from 10 to board length
                        nextCol = rnd.Next(0, 10);
                        Console.WriteLine("Bot is trying [" + nextRow + "," + nextCol + "]");
                    } while (this.enemyBoard.GetCoord(nextRow, nextCol) != 'w');
                }
                this.lastShot[0] = nextRow;
                this.lastShot[1] = nextCol;
                SendData(rowColToCoordinate(nextRow, nextCol));
            }
        }

        public override void Shoot (String something = "")
        {
            return;
        }

    private int chooseDirection(int theRow, int theCol)
        {
            //(0, up) (1, down) (2, left) (3. right)
            int[] options = { 0, 1, 2, 3 };


            if (theRow == 0) options = options.Where(val => val != 0).ToArray();
            else if (this.enemyBoard.GetCoord(theRow - 1, theCol) == 'o') options = options.Where(val => val != 0).ToArray();

            if (theRow >= this.enemyBoard.GetLength(0) - 1) options = options.Where(val => val != 1).ToArray();
            else if (this.enemyBoard.GetCoord(theRow + 1, theCol) == 'o') options = options.Where(val => val != 1).ToArray();

            if (theCol == 0) options = options.Where(val => val != 2).ToArray();
            else if (this.enemyBoard.GetCoord(theRow, theCol - 1) == 'o') options = options.Where(val => val != 2).ToArray();

            if (theCol >= this.enemyBoard.GetLength(1) - 1) options = options.Where(val => val != 3).ToArray();
            else if (this.enemyBoard.GetCoord(theRow, theCol + 1) == 'o') options = options.Where(val => val != 3).ToArray();

            if (options.Length == 0) return -1; //Something went wrong
            return options[rnd.Next(options.Length)];
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
