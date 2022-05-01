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

        public char GetCoord(int theRow, int theCol)
        {
            if (theRow < 0 || theRow >= mBoard.GetLength(0) ||
                theCol < 0 || theCol >= mBoard.GetLength(1)) throw new IndexOutOfRangeException("Index out of bounds");
            return this.mBoard[theRow, theCol];
        }
        public int GetLength(int dimension)
        {
            return this.mBoard.GetLength(dimension);
        }

        public char Shoot(int theRow, int theCol)
        {
            if (mBoard[theRow, theCol] == 'o') return 'o';
            if (mBoard[theRow, theCol] == 'x') return 'x';

            if (mBoard[theRow, theCol] == 's')
            {
                mBoard[theRow, theCol] = 'x';
                if (CheckDestroyed(theRow, theCol))
                {
                    fillMisses(theRow, theCol);
                    this.shipCount -= 1;
                    return this.shipCount == 0 ? 'g' : 'd';
                }
                return 'x';
            }
            else
            {
                mBoard[theRow, theCol] = 'o';
                return 'o';
            }
        }

        private bool CheckDestroyed(int theRow, int theCol)
        {
            char[] water = { 'w', 'o' };
            Console.WriteLine("Checking if the ship is destroyed");
            if (CheckPosition(theRow, theCol) && CheckPosition(theRow, theCol, 1, true, 'x', true)) return true;
            Console.WriteLine("The Ship was not a single ship");
            bool isHorizontal = checkHorizontal(theRow, theCol);
            int dir = 1;
            Console.WriteLine("The ship is " + (isHorizontal ? "horizontal" : "vertical"));
            while (GetCoord(theRow, theCol) != 's')
            {
                if (isHorizontal)
                {
                    theCol += dir;
                    if (theCol >= mBoard.GetLength(1) - 1 ||
                        theCol <= 0 ||
                        water.Contains(mBoard[theRow, Math.Min(theCol, mBoard.GetLength(1) - 1)]))
                    { //Avoid IndexOutOfBoundsError
                        if (dir == -1) return true;
                        dir = -1;
                        theCol += dir;
                    }


                }
                else
                {
                    theRow += dir;
                    if (theRow >= mBoard.GetLength(0) - 1 ||
                        theRow <= 0 ||
                        water.Contains(mBoard[Math.Min(theRow, mBoard.GetLength(0) - 1), theCol]))
                    {
                        if (dir == -1) return true;
                        dir = -1;
                        theRow += dir;
                    }

                }
                Console.WriteLine("Checking shot [" + theRow + "," + theCol + "]");
            }
            return false;
        }

        public void fillMisses(int theRow, int theCol)
        {
            int startRow, startCol, endRow, endCol;

            if (!CheckPosition(theRow, theCol, 1, true, 'x', true))
            {
                bool isHorizontal = checkHorizontal(theRow, theCol);
                startRow = theRow;
                startCol = theCol;
                endRow = theRow;
                endCol = theCol;

                if (isHorizontal)
                {
                    while (mBoard[startRow, startCol] == 'x')
                    {
                        startCol -= 1;
                        if (startCol == 0) break;
                    }
                    while (mBoard[endRow, endCol] == 'x')
                    {
                        endCol += 1;
                        if (endCol == mBoard.GetLength(1) - 1) break;
                    }
                    startRow = Math.Max(0, theRow - 1);
                    endRow = Math.Min(mBoard.GetLength(0) - 1, theRow + 1);
                }
                else
                {
                    while (mBoard[startRow, startCol] == 'x')
                    {
                        startRow -= 1;
                        if (startRow == 0) break;
                    }
                    while (mBoard[endRow, endCol] == 'x')
                    {
                        endRow += 1;
                        if (endRow == mBoard.GetLength(0) - 1) break;
                    }
                    startCol = Math.Max(0, theCol - 1);
                    endCol = Math.Min(mBoard.GetLength(1) - 1, theCol + 1);
                }

            }
            else
            {
                //    Console.WriteLine("The ship was a single ship");
                startRow = Math.Max(0, theRow - 1);
                startCol = Math.Max(0, theCol - 1);
                endRow = Math.Min(mBoard.GetLength(0) - 1, theRow + 1);
                endCol = Math.Min(mBoard.GetLength(1) - 1, theCol + 1);
            }

            // Console.WriteLine("startRow: " + startRow + ", startCol: " + startCol + ", endRow: " + endRow + ", endCol: " + endCol);
            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    if (mBoard[row, col] == 'w') mBoard[row, col] = 'o';
                    //    else Console.WriteLine(mBoard[row, col]);
                }
            }
        }

        public bool checkHorizontal(int theRow, int theCol)
        {
            char[] ship = { 's', 'x' };
            if (theCol == 0) return ship.Contains(GetCoord(theRow, theCol + 1));
            if (theCol >= mBoard.GetLength(1) - 1) return ship.Contains(GetCoord(theRow, theCol - 1));
            return ship.Contains(GetCoord(theRow, theCol + 1)) || ship.Contains(GetCoord(theRow, theCol - 1));
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

        public bool CheckPosition(int theRow, int theCol, int theLength = 1, bool isVertical = true, char theChar = 's', bool skipCoord = false)
        {
            int startRow = Math.Max(0, theRow - 1);
            int startCol = Math.Max(0, theCol - 1);
            int endRow = Math.Min(mBoard.GetLength(0) - 1, theRow + (isVertical ? theLength : 1));
            int endCol = Math.Min(mBoard.GetLength(1) - 1, theCol + (!isVertical ? theLength : 1));

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
