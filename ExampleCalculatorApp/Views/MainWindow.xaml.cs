using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using ExampleCalculatorApp.ViewModels;
using ReactiveUI;

namespace ExampleCalculatorApp.Views
{
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel = new MainViewModel();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.ListViewModel, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkViewModel, v => v.viewHost.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.ValueLabel, v => v.valueLabel.Content).DisposeWith(d);
                this.viewHost.Events().KeyUp
                    .Where(x => x.Key == Key.C)
                    .Do(e => e.Handled = true)
                    .Select(_ => Mouse.GetPosition(this.viewHost))
                    .InvokeCommand(ViewModel.NetworkViewModel.SurroundWithCommentNode)
                    .DisposeWith(d);
            });
        }
    }
}