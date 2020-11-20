using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace H3B.GameOfLife.ViewModels
{
    public class CellViewModel : BaseViewModel
    {

        public ICommand ToggleCellStateCommand { get; private set; }


        private bool _isAlive;
        public bool IsAlive
        {
            get => _isAlive;
            set
            {
                base.SetProperty(ref _isAlive, value);
                RaisePropertyChanged(() => BackgroundColor);
            }
        }


        public Color BackgroundColor
        {
            get
            {
                if (this.IsAlive)
                {
                    return Color.Black;
                }
                return Color.White;
            }
        }

        public CellViewModel(bool isAlive)
        {
            this.IsAlive = isAlive;

            this.ToggleCellStateCommand = new Command(() =>
            {
                this.IsAlive = !this.IsAlive;
            });
        }


    }
}
