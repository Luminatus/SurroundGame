using System;

namespace SurroundGameWPF.Persistence
{
    public class SurroundGameTable
    {
        TileState[,] gameField;
        public int FieldHeight
        {
            get { return gameField.GetLength(0); }
        }
        public int FieldWidth
        {
            get { return gameField.GetLength(1); }
        }

        public int tilesLeft
        { get; private set; }

        public SurroundGameTable(int size) : this(size, size) { }
        public SurroundGameTable(int row, int col)
        {
            gameField = new TileState[row, col];
            for(int rowiterator = 0; rowiterator<FieldHeight; rowiterator++ )
            {
                for (int coliterator = 0; coliterator < FieldWidth; coliterator++)
                {
                    gameField[rowiterator, coliterator] = TileState.Unoccupied;
                }
            }
            tilesLeft = FieldHeight * FieldWidth;
        }

        public SurroundGameTable(SurroundGameTable paramTable)
        {
            gameField = (TileState[,])paramTable.gameField.Clone();
            tilesLeft = paramTable.tilesLeft;
        }

        public TileState this[int row, int col]
        {
            get
            {
                return GetState(row, col);
            }
            set
            {
                SetState(row, col, value);
            }
        }

        public TileState GetState(int row, int col)
        {
            string exceptionString = String.Empty;
            if (!CheckRange(row, col, ref exceptionString))
                throw new ArgumentOutOfRangeException(exceptionString);
            return gameField[row, col];
        }

        public void SetState(int row, int col, TileState state)
        {
            string exceptionString=String.Empty;
            if (!CheckRange(row, col, ref exceptionString))
                throw new ArgumentOutOfRangeException(exceptionString);
            if (gameField[row, col] == TileState.Unoccupied && state != TileState.Unoccupied)
                tilesLeft--;
            else if (gameField[row, col] != TileState.Unoccupied && state == TileState.Unoccupied)
                tilesLeft++;
            gameField[row, col] = state;
        }

        private bool CheckRange(int row, int col)
        {
            string dummystring="";
            return CheckRange(row, col, ref dummystring );
        }

        private bool CheckRange(int row, int col, ref string returnString)
        {
            if (row < 0 || row >= FieldHeight)
            {
                returnString = String.Format("The given row number {0} is out of the game field's range ({1})", row, FieldHeight);
                return false;
            }
            if (col < 0 || col >= FieldWidth)
            {
                returnString = String.Format("The given column number {0} is out of the game field's range ({1})", col, FieldWidth);
                return false;
            }
                return true;
        }

        public bool CheckTile( int row, int column, TileState state = TileState.Unoccupied)
        {
            return CheckTile( row, column, new TileState[] { state });
        }

        public bool CheckTile(int row, int column, TileState[] state)
        {
            bool isValid=false;
            if (CheckRange(row, column))
            {
                bool containsState = false;
                for (int i = 0; i < state.Length && !containsState; i++)
                    containsState = gameField[row, column] == state[i];
                isValid = containsState;
            }

            return isValid;
        }

        public TileState[,] ToArray()
        {
            return (TileState[,])gameField.Clone();
        }
    }
}
