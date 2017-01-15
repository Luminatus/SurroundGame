using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using SurroundGameWPF.Model;

namespace SurroundGameWPF.Test
{
    [TestClass]
    public class SurroundGameTrackTest
    {
        GameModel gameModel;
        [TestInitialize]
        public void Initialize()
        {
            gameModel = new GameModel();
        }

        [TestMethod]
        public void TestIntegrity()
        {
            GameTracker.Load(true);
            for (int i=0; i<GameTracker.CachedTracks; i++)
            {                
                Console.WriteLine(i + ". Game:");
                Assert.IsTrue(gameModel.NewPlayback(i));
                int j = 0;
                while(!gameModel.IsGameOver)
                {
                    Console.Write("Step " + j + ":");
                    gameModel.MakeStep();
                    if (!gameModel.IsGameOver)
                    {
                        Persistence.TileState[,] tiles = gameModel.GameField;
                        Assert.IsTrue(tiles.Cast<Persistence.TileState>().SequenceEqual(gameModel.CurrentAction.Table.Cast<Persistence.TileState>()));
                        Console.WriteLine(" OK");
                    }
                }
            }
        }
    }
}
