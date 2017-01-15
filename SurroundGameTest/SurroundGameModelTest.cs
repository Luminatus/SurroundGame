using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SurroundGameWPF.Model;
using SurroundGameWPF.Persistence;

namespace SurroundGameWPF.Test
{
    [TestClass]
    public class SurroundGameModelTest
    {
        private GameModel gameModel;
        [TestInitialize]
        public void Initialize()
        {
            gameModel = new GameModel();
            gameModel.MapGenerated += new EventHandler<MapGeneratedEventArgs>(Model_MapGenerated);
            gameModel.GameEnded += new EventHandler<GameEndedEventArgs>(Model_GameEnded);
        }

        [TestMethod]
        [ExpectedException(typeof(SurroundGameModelException), "Invalid game size!")]
        public void NewGameTestInvalid()
        {
            gameModel.NewGame(3, 3);
        }

        [TestMethod]
        public void NewGameTestValid()
        { 
            Assert.IsTrue(gameModel.IsGameOver);
            gameModel.NewGame(6);
            Assert.IsFalse(gameModel.IsGameOver);
            Assert.AreEqual(gameModel.TilesLeft, 36);
        }

        [TestMethod]
        public void GameStepTest()
        {
            gameModel.NewGame(6, 6);
            Assert.IsTrue(gameModel.isBrickHorizontal);
            gameModel.RotateBrick();
            Assert.IsFalse(gameModel.isBrickHorizontal);
            var startingPlayer = gameModel.CurrentPlayer;

            gameModel.CheckPlacement(6, 0);
            gameModel.MakeStep();
            Assert.AreEqual(gameModel.TilesLeft, 36);
            Assert.AreEqual(startingPlayer, gameModel.CurrentPlayer);

            gameModel.CheckPlacement(0, 0);
            gameModel.MakeStep();
            Assert.IsTrue(gameModel.isBrickHorizontal);
            Assert.AreNotEqual(startingPlayer, gameModel.CurrentPlayer);
            Assert.AreEqual(gameModel.TilesLeft, 34);

            gameModel.CheckPlacement(1, 0);
            gameModel.MakeStep();
            Assert.IsTrue(gameModel.isBrickHorizontal);
            Assert.AreNotEqual(startingPlayer, gameModel.CurrentPlayer);
            Assert.AreEqual(gameModel.TilesLeft, 34);

        }

        [TestMethod]
        public void GamePlayDrawTest()
        {
            gameModel.NewGame(6);
            int tilesToBeLeft = 36;
            for(int i=0; i<gameModel.fieldHeight;i++)
            {
                for (int j=0; j<gameModel.fieldWidth; j+=2)
                {
                    Assert.IsFalse(gameModel.IsGameOver);
                    gameModel.CheckPlacement(i, j);
                    gameModel.MakeStep();
                    tilesToBeLeft -= 2;
                    Assert.AreEqual(gameModel.TilesLeft, tilesToBeLeft);
                }
            }
            Assert.IsTrue(gameModel.IsGameOver);
            Assert.AreEqual(gameModel.TilesLeft, 0);
        }

        [TestMethod]
        public void GamePlayWinTest()
        {
            gameModel.NewGame(6);
            gameModel.MakeStep(0, 0); //Red
            gameModel.MakeStep(1, 1); //Blue
            gameModel.MakeStep(0, 2); //Red
            gameModel.MakeStep(2, 1); //Blue
            gameModel.MakeStep(3, 0); //Red
            gameModel.MakeStep(3, 4); //Blue
            gameModel.MakeStep(3, 2); //Red
            gameModel.MakeStep(4, 4); //Blue
            gameModel.RotateBrick();
            gameModel.MakeStep(1, 0); //Red
            gameModel.MakeStep(5, 4); //Blue
            gameModel.MakeStep(1, 3); //Red
            gameModel.MakeStep(2, 4); //Blue
            gameModel.MakeStep(4, 0); //Red
            gameModel.MakeStep(1, 4); //Blue
            gameModel.MakeStep(4, 1); //Red
            gameModel.MakeStep(0, 4); //Blue
            gameModel.MakeStep(4, 2); //Red
            gameModel.RotateBrick(); 
            gameModel.MakeStep(4, 3); //Blue
            Assert.IsTrue(gameModel.IsGameOver);
            Assert.IsTrue(gameModel.GetPoints(Persistence.Players.Red) > gameModel.GetPoints(Persistence.Players.Blue));
            Assert.AreEqual(gameModel.TilesLeft, 0);
        }

    
        private void Model_MapGenerated(object sender, MapGeneratedEventArgs e)
        {
            Assert.AreEqual(e.PlayerPointDictionary.Count, gameModel.PlayerNames.Length);
            int PointCount = 0;
            int PointDifferenceSum = 0;
            foreach (var playerPoint in e.PlayerPointDictionary)
            {
                Assert.IsTrue(playerPoint.Value.Points >= 0 && playerPoint.Value.Points < gameModel.fieldHeight * gameModel.fieldWidth);
                PointCount += playerPoint.Value.Points;
                PointDifferenceSum += playerPoint.Value.PointDifference;
            }
            Assert.AreEqual(PointCount + gameModel.TilesLeft, gameModel.fieldHeight * gameModel.fieldWidth);
            for(int i=0; i<gameModel.fieldHeight;i++)
            {
                for(int j=0; j<gameModel.fieldWidth; j++)
                {
                    if (gameModel.GetState(i, j) == TileState.Unoccupied && gameModel.GetState(i, j, true) != Persistence.TileState.Unoccupied)
                        PointDifferenceSum--;
                }
            }
            Assert.AreEqual(PointDifferenceSum, 0);
        }

        private void Model_GameEnded(object sender, GameEndedEventArgs e)
        {
            if (e.IsDraw)
                Model_GameDraw(e);
            else
                Model_GameWon(e);
        }
        
        private void Model_GameDraw(GameEndedEventArgs e)
        {
            Assert.IsTrue(gameModel.IsGameOver);
            Assert.IsTrue(e.Players.Count > 1);
            int Points = gameModel.GetPoints(e.Players.First.Value);
            foreach(var player in e.Players)
            {
                Assert.AreEqual(gameModel.GetPoints(player), Points);
            }
        }
        
        private void Model_GameWon(GameEndedEventArgs e)
        {
            Assert.IsTrue(gameModel.IsGameOver);
            Assert.IsTrue(e.Players.Count == 1);
            Persistence.Players winner = e.Players.First.Value;
            int Points = gameModel.GetPoints(winner);
            foreach(var playerName in gameModel.PlayerNames)
            {
                if (playerName != winner)
                    Assert.IsTrue(gameModel.GetPoints(playerName) < Points);
            }
            

        }


    }
}
