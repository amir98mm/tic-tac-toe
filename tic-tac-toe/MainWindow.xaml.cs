using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace tic_tac_toe
{
    public partial class MainWindow : Window
    {
        private enum DifficultyLevel
        {
            Easy,
            Medium,
            Hard
        }

        private DifficultyLevel selectedDifficulty = DifficultyLevel.Easy;
        private char currentPlayer;
        private char[,] board;
        private bool isVsAI = false;

        public MainWindow()
        {
            InitializeComponent();
            ResetGame();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Content == null && (currentPlayer == 'X' || !isVsAI))
            {
                // Player's move
                MakeMove(button, currentPlayer);

                if (CheckWin(currentPlayer))
                {
                    if (isVsAI)
                        MessageBox.Show("You won the game :)");
                    else 
                        MessageBox.Show($"{currentPlayer} won the game!");
                    ResetGame();
                }
                else if (IsBoardFull())
                {
                    MessageBox.Show("It's a draw!");
                    ResetGame();
                }
                else
                {
                    // Switch player
                    currentPlayer = currentPlayer == 'X' ? 'O' : 'X';

                    // If it's AI's turn and playing against AI
                    if (isVsAI && currentPlayer == 'O') // If it's AI's turn
                    {
                        MakeAIMove();
                    }
                }
            }
        }

        private void MakeMove(Button button, char player)
        {
            button.Content = player;
            int index = GameGrid.Children.IndexOf(button);
            int row = index / 3;
            int column = index % 3;
            board[row, column] = player;
        }

        private void MakeEasyMove()
        {
            Random random = new Random();
            List<Button> availableMoves = new List<Button>();

            // Find all available buttons (empty cells)
            foreach (var child in GameGrid.Children)
            {
                if (child is Button button && button.Content == null)
                {
                    availableMoves.Add(button);
                }
            }

            if (availableMoves.Count > 0)
            {
                int randomIndex = random.Next(availableMoves.Count);
                Button selectedButton = availableMoves[randomIndex];
                MakeMove(selectedButton, 'O');
            }
        }

        private void MakeMediumMove()
        {
            // Check if AI can win in the next move
            var winningMove = MakeWinningMove('O');
            if (winningMove != null)
            {
                // Commit the winning move
                MakeMove(GetButtonAt(winningMove.Value.row, winningMove.Value.col), 'O');
                return;
            }

            // Check if the player can win in the next move and block them
            var blockingMove = MakeWinningMove('X');
            if (blockingMove != null)
            {
                // Block the player
                MakeMove(GetButtonAt(blockingMove.Value.row, blockingMove.Value.col), 'O');
                return;
            }

            // If no immediate moves, make a random move
            MakeEasyMove(); // You can reuse the easy move function here
        }

        private void MakeAIMove()
        {
            // Check if AI can win in the next move
            var winningMove = MakeWinningMove('O');
            if (winningMove != null)
            {
                // Commit the winning move
                MakeMove(GetButtonAt(winningMove.Value.row, winningMove.Value.col), 'O');
            }

            else
            {
                // Play based on selected difficulty
                if (selectedDifficulty == DifficultyLevel.Easy)
                {
                    MakeEasyMove();
                }
                else if (selectedDifficulty == DifficultyLevel.Medium)
                {
                    // Random move if no winning or blocking move is found
                    MakeMediumMove();
                }
                else if (selectedDifficulty == DifficultyLevel.Hard)
                {
                    // Call Minimax to find the best move for 'O'
                    (int bestRow, int bestCol) = FindBestMove();

                    // Make the best move found
                    MakeMove(GetButtonAt(bestRow, bestCol), 'O');
                }
            }

            // Check win or draw conditions
            if (CheckWin('O'))
            {
                MessageBox.Show("BOT won the game!");
                ResetGame();
            }
            else if (IsBoardFull())
            {
                MessageBox.Show("It's a draw!");
                ResetGame();
            }
            else
            {
                currentPlayer = 'X'; // Switch back to player
            }
        }

        private void MakeHardMove()
        {
            int bestScore = int.MinValue;
            Button bestButton = null;

            foreach (var button in GameGrid.Children.OfType<Button>())
            {
                if (button.Content == null) // If the button is empty
                {
                    // Simulate the move
                    button.Content = 'O'; // AI's marker
                    int score = Minimax(board, 0, false); // Call minimax
                    button.Content = null; // Undo the move

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestButton = button; // Track the best move
                    }
                }
            }

            // Make the best move found
            if (bestButton != null)
            {
                MakeMove(bestButton, 'O'); // Place AI's marker
            }
        }


        private (int row, int col) FindBestMove()
        {
            int bestScore = int.MinValue;
            int bestRow = -1;
            int bestCol = -1;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == '\0') // If the cell is empty
                    {
                        board[i, j] = 'O'; // Simulate AI move
                        int score = Minimax(board, 0, false); // Call Minimax
                        board[i, j] = '\0'; // Undo move

                        if (score > bestScore)
                        {
                            bestScore = score; // Update best score
                            bestRow = i; // Track best row
                            bestCol = j; // Track best column
                        }
                    }
                }
            }

            return (bestRow, bestCol); // Return best move found
        }

        private int Minimax(char[,] board, int depth, bool isMaximizing)
        {
            if (CheckWin('O')) return 10 - depth; // AI wins
            if (CheckWin('X')) return depth - 10; // Player wins
            if (IsBoardFull()) return 0; // Draw

            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == '\0')
                        {
                            board[i, j] = 'O'; // AI's move
                            int score = Minimax(board, depth + 1, false);
                            board[i, j] = '\0'; // Undo move
                            bestScore = Math.Max(score, bestScore);
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (board[i, j] == '\0')
                        {
                            board[i, j] = 'X'; // Player's move
                            int score = Minimax(board, depth + 1, true);
                            board[i, j] = '\0'; // Undo move
                            bestScore = Math.Min(score, bestScore);
                        }
                    }
                }
                return bestScore;
            }
        }


        private (int row, int col)? MakeWinningMove(char player)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == '\0') // Check for an empty spot
                    {
                        board[i, j] = player; // Temporarily place player's move
                        if (CheckWin(player))
                        {
                            // Return the position of the winning move instead of placing it
                            return (i, j);
                        }
                        board[i, j] = '\0'; // Undo move if no win
                    }
                }
            }
            return null; // No winning move found
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            ResetGame();
        }

        private void ResetGame()
        {
            currentPlayer = 'X'; // Player X always starts
            board = new char[3, 3];

            foreach (var child in GameGrid.Children)
            {
                if (child is Button button)
                {
                    button.Content = null;
                    Storyboard storyboard = (Storyboard)FindResource("WinningAnimation");
                    storyboard.Stop(button);  // Stop any animations
                    button.RenderTransform = new ScaleTransform(1, 1);  // Reset button scale
                }
            }
        }

        private bool CheckWin(char player)
        {
            // Check rows and columns for a win
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player)
                {
                    return true;
                }
                if (board[0, i] == player && board[1, i] == player && board[2, i] == player)
                {
                    return true;
                }
            }

            // Check diagonals
            if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player)
            {
                return true;
            }
            if (board[0, 2] == player && board[1, 1] == player && board[2, 0] == player)
            {
                return true;
            }

            return false;
        }

        private Button GetButtonAt(int row, int column)
        {
            int index = row * 3 + column;
            return GameGrid.Children[index] as Button;
        }

        private bool IsBoardFull()
        {
            foreach (char cell in board)
            {
                if (cell == '\0') return false;
            }
            return true;
        }

        private void GameModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            ComboBoxItem selectedMode = comboBox.SelectedItem as ComboBoxItem;

            if (selectedMode != null)
            {
                string mode = selectedMode.Content.ToString();
                isVsAI = mode == "1 Player";

                if (GameGrid != null)
                {
                    ResetGame();  // Reset the game when the mode changes
                }
            }
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            ComboBoxItem selectedDifficultyItem = comboBox.SelectedItem as ComboBoxItem;

            if (selectedDifficultyItem != null)
            {
                string difficulty = selectedDifficultyItem.Content.ToString();
                selectedDifficulty = difficulty switch
                {
                    "Easy" => DifficultyLevel.Easy,
                    "Medium" => DifficultyLevel.Medium,
                    "Hard" => DifficultyLevel.Hard,
                    _ => DifficultyLevel.Easy
                };
            }
        }
    }
}
