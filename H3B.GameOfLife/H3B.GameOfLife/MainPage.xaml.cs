using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text; 
using System.Threading.Tasks;
using Xamarin.Forms;

namespace H3B.GameOfLife
{
    public partial class MainPage : ContentPage
    {

        private ViewModels.GameOfLifeViewModel ViewModel => this.BindingContext as ViewModels.GameOfLifeViewModel;
        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new ViewModels.GameOfLifeViewModel();
            this.DrawGrid();
           
        }



        private void DrawGrid()
        {
            var cells = this.ViewModel.Cells;

            var numberOfRows = cells.GetLength(0);
            var numberOfColumns = cells.GetLength(1);

            for (var x = 0; x < numberOfColumns; x++)
            {
                this.CanvasGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (var y = 0; y < numberOfRows; y++)
            {
                this.CanvasGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {


                    var cell = cells[x, y];
                    var itemToAdd = new Frame
                    {
                        Padding = 2,
                        CornerRadius = 0,
                        HasShadow = false,
                        
                    };
                    var cellTapGestureRecognizer = new TapGestureRecognizer();
                    cellTapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, nameof(cell.ToggleCellStateCommand));
                    itemToAdd.GestureRecognizers.Add(cellTapGestureRecognizer);
                    itemToAdd.SetBinding(Frame.BackgroundColorProperty, nameof(cell.BackgroundColor));

                    itemToAdd.BindingContext = cell;

                    this.CanvasGrid.Children.Add(itemToAdd, x, y);
                }
            }


        }





    }
}
