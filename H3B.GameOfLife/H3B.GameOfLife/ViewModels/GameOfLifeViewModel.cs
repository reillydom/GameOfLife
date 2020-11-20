
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace H3B.GameOfLife.ViewModels
{
    public class GameOfLifeViewModel : BaseViewModel
    {

        private CellViewModel[,] _cells;
        public CellViewModel[,] Cells // Current/next generation.
        {
            get => _cells;
            set => base.SetProperty(ref _cells, value);
        }

        private int _currentGeneration = 0;
        private int CurrentGeneration
        {
            get => _currentGeneration;
            set
            {
                base.SetProperty(ref _currentGeneration, value);
                base.RaisePropertyChanged(() => CurrentGenerationText);
            }

        }

        private bool _isGameLoopRunning;
        public bool IsGameLoopRunning
        {
            get => _isGameLoopRunning;
            set
            {
                base.SetProperty(ref _isGameLoopRunning, value);
                base.RaisePropertyChanged(() => this.StartStopGameLoopText);
            }
        }


        public string CurrentGenerationText
        {
            get => $"Current Generation: {_currentGeneration}";
        }

        public string StartStopGameLoopText
        {
            get => (this.IsGameLoopRunning) ? "Stop" : "Start";
        }
        public ICommand LoadNextGenerationCommand { private set; get; }

        public ICommand StopStartGameLoopCommand { private set; get; }
        // TODO: VM needs a ExecuteLoadCommand which will generate the grid size and cells.


        private const double SecondsBetweenTicks = 0.5;
        private const int Rows = 10;
        private const int Columns = 10;

        public GameOfLifeViewModel()
        {

            this.Cells = new CellViewModel[Columns, Rows];
            for (var x = 0; x < Columns; x++)
            {
                for (var y = 0; y < Rows; y++)
                {

                    var isAlive = this.NextBool(_random); // 50% chance of true or false.
                    this.Cells[x, y] = new CellViewModel(isAlive);
                }
            }
            this.LoadNextGenerationCommand = new Command(() => this.LoadNextGeneration());
            this.StopStartGameLoopCommand = new Command(() =>
            {
                this.IsGameLoopRunning = !this.IsGameLoopRunning;
                //if (this.IsGameLoopRunning)
                //{
                //    this.IsGameLoopRunning = false;
                //}
                //else
                //{
                //    this.IsGameLoopRunning = true;
                //}

                Device.StartTimer(TimeSpan.FromSeconds(SecondsBetweenTicks), () =>
                {
                    this.LoadNextGenerationCommand.Execute(this);
                    return this.IsGameLoopRunning;
                });
            });

        }


        public void LoadNextGeneration()
        {
            if (this.IsBusy)
            {
                return;
            }
            this.IsBusy = true;

            lock (_cells.SyncRoot)
            {
                // Rules:
                // Any live cell with fewer than two live neighbours dies, as if by underpopulation.
                // Any live cell with two or three live neighbours lives on to the next generation.
                // Any live cell with more than three live neighbours dies, as if by overpopulation.
                // Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
                this.UpdateAllCellsState();

                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
                {
                    this.CurrentGeneration += 1;
                    this.IsBusy = false;
                });

            }
        }





        #region Private methods

        private static readonly Random _random = new Random();

        private bool NextBool(Random r, int truePercentage = 50)
        {
            return r.NextDouble() < truePercentage / 100.0;
        }


        private void UpdateAllCellsState()
        {
            var isCurrentThreadHolderOfCellLock = Monitor.IsEntered(_cells);
            var previousCells = (CellViewModel[,])Cells.Clone();

            Parallel.For(0, Columns, x =>
            {
                Parallel.For(0, Rows, y =>
                    {
                        int numberOfAliveNeighbours = 0;

                        for (int xi = x - 1; xi <= x + 1; xi++)
                            for (int yi = y - 1; yi <= y + 1; yi++)
                            {

                                if (xi >= 0 && yi >= 0 && xi < Columns && yi < Rows)
                                {
                                    // Ensure it's not our current cell that we're checking. 
                                    if (!((y == yi) && (x == xi)))
                                    {
                                        numberOfAliveNeighbours += previousCells[xi, yi].IsAlive ? 1 : 0;
                                    }
                                }
                            }


                        var cell = _cells[x, y];
                        var isAlive = false;

                        if (cell.IsAlive)
                        {
                            // Determine if the cell should live or die. 
                            if (numberOfAliveNeighbours < 2)
                            {
                                // Rule 1 - underpopulation
                                // Don't reassign the 'isAlive' variable as we initialise as false by default.
                                isAlive = false;
                            }
                            else if (numberOfAliveNeighbours > 3)
                            {
                                // Rule 3 - overpopulation
                                // Don't reassign the 'isAlive' variable as we initialise as false by default.
                                isAlive = false;
                            }
                            else
                            {
                                // Rule 2 - lives on to the next generation
                                //cell.IsAlive = true;
                                isAlive = true;
                            }
                        }
                        else
                        {
                            if (numberOfAliveNeighbours == 3)
                            {
                                // Rule 4 - reproduction
                                isAlive = true;
                            }
                        }

                        // Xamarin does not guarantee that a property changed will be raised on the main-thread 
                        Device.BeginInvokeOnMainThread(() => cell.IsAlive = isAlive);
                    });

            });


            #endregion

        }
    }
}
