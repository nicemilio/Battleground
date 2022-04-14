using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Battleground
{
    internal class board
    {
        public char[,] mBoard { get; set; }
        public bool mReady { get; set; }
        Random rnd = new Random();
        String mEnemyMove = "Empty";
        private int[] schiffe = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
        private int shipCount;

        public board(int rows, int cols, bool autoPlace = false)
        {
            this.shipCount = schiffe.Length;
            mBoard = new char[rows, cols];
            FillBoard();
            if (autoPlace)
            {
                AutoFill();
                PrintBoard();
            }

        }

        /**
* w = wasser
* s = schiff
* o = leer getroffen
* x = schiff getroffen
**/



        void StopClient(TcpClient client)
        {
            client.Client.Shutdown(SocketShutdown.Send);
            //ns.Close();
            client.Close();
            Console.WriteLine("Disconnect from server!");
            Console.ReadKey();
        }
        void AutoFill()
        {
            foreach (int i in this.schiffe)
            {
                bool suc = false;
                while (!suc)
                {
                    int randRow = rnd.Next(0, 10);
                    int randCol = rnd.Next(0, 10);
                    bool randDir = rnd.Next(0, 2) == 0 ? false : true;
                    suc = TryPlace(randRow, randCol, i, randDir);
                }
            }
        }

        void FillBoard()
        {
            for (int i = 0; i < mBoard.GetLength(0); i++)
            {
                for (int j = 0; j < mBoard.GetLength(1); j++)
                {
                    mBoard[i, j] = 'w';
                }
            }
        }

        public void PrintBoard()
        {
            char[] coord = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
            Console.WriteLine("  1 2 3 4 5 6 7 8 9 10");
            for (int i = 0; i < mBoard.GetLength(0); i++)
            {
                Console.Write(coord[i]);
                for (int j = 0; j < mBoard.GetLength(1); j++)
                {
                    Console.Write(" " + mBoard[i, j]);
                }
                Console.Write('\n');
            }
        }

        public char PrintCoord(int theRow, int theCol)
        {
            if (theRow < 0 || theRow > mBoard.GetLength(0) ||
                theCol < 0 || theCol > mBoard.GetLength(1)) throw new IndexOutOfRangeException("Index out of bounds");
            return this.mBoard[theRow, theCol];
        }

        public char PrintCoord(int[] coord)
        {
            return this.PrintCoord(coord[0], coord[1]);
        }
        public char Shoot(int theRow, int theCol)
        {
            if (mBoard[theRow, theCol] == 's')
            {
                mBoard[theRow, theCol] = 'x';
                if (CheckPosition(theRow, theCol, 1, false))
                {
                    this.shipCount -= 1;
                    return this.shipCount == 0 ? 'g' : 'd';
                }
                return 'x';
            }
            else
            {
                mBoard[theCol, theCol] = 'o';
                return 'o';
            }
        }

        //This method is used on enemyBoard to assign hits and misses
        public void AssignChar(int theRow, int theCol, char theChar)
        {
            mBoard[theRow, theCol] = theChar;
        }

        private bool TryPlace(int theRow, int theCol, int theLength, bool isVertical)
        {
            if (CheckPosition(theRow, theCol, theLength, isVertical))
            {
                if (isVertical)
                    for (int i = theRow; i < theRow + theLength; i++) mBoard[i, theCol] = 's';
                else
                    for (int i = theCol; i < theCol + theLength; i++) mBoard[theRow, i] = 's';

                return true;
            }
            return false;
        }

        public bool CheckPosition(int theRow, int theCol, int theLength, bool isVertical, char theChar = 's', bool skipCoord = false)
        {
            int startRow = theRow - 1 < 0 ? 0 : theRow - 1;
            int startCol = theCol - 1 < 0 ? 0 : theCol - 1;
            int lengthRow = theRow + (isVertical ? theLength : 1);
            int lengthCol = theCol + (isVertical ? 1 : theLength);
            int endRow = lengthRow < mBoard.GetLength(0) - 1 ? lengthRow : mBoard.GetLength(0) - 1; //Min(mBoard.GetLength, lengthRow)
            int endCol = lengthCol < mBoard.GetLength(1) - 1 ? lengthCol : mBoard.GetLength(1) - 1;

            if ((isVertical ? theRow : theCol) + theLength <= mBoard.GetLength(isVertical ? 0 : 1)) //is in Board
            {
                for (int i = startRow; i <= endRow; i++)
                {
                    for (int j = startCol; j <= endCol; j++)
                    {
                        if (i == theRow && j == theCol && skipCoord) continue;
                        if (mBoard[i, j] == theChar) return false;

                    }
                }
            }
            else return false;
            return true;
        }
    }
}
