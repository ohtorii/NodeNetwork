using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NodeNetwork.Views
{
    public class NodeCommenterView : NodeView
    {
        private static Rect GetBoundingBox(FrameworkElement element, Visual containerWindow)
        {
            var transform = element.TransformToAncestor(containerWindow);
            var topLeft = transform.Transform(new Point(0, 0));
            var bottomRight = transform.Transform(new Point(element.ActualWidth, element.ActualHeight));
            return new Rect(topLeft, bottomRight);
        }
        bool PrevIsHitTestVisible;
        bool PrevIsReadOnly;
        bool IsEditMode = false;
        IDisposable confirmTextContentDisposable;

        public NodeCommenterView()
        {
            DefaultStyleKey = typeof(NodeCommenterView);
            this.WhenActivated(d => {
                this.Events()
                    .MouseDoubleClick
                    .Subscribe(ToggleLabelEdit)
                    .DisposeWith(d);
                firstActualMinSize = ViewModel.Size;
                //The width of the TextBox in which the string will fit (it would otherwise be undesirable to put a fixed value)
                ViewModel.Size = new Size (170, ViewModel.Size.Height);

                var nodeCommenterVM = this.ViewModel as NodeCommenterViewModel;
                var req = nodeCommenterVM.InitializationRequests;
                if (req == null)
                {
                    SetControlSize(ViewModel.Size);
                } else { 
                    Point newPosition;
                    Size? newSize;
                    CalcPositionAnsSize(out newPosition, out newSize, req);
                    
                    nodeCommenterVM.Position = newPosition;
                    if (newSize == null)
                    {
                        SetControlSize(ViewModel.Size);
                    } else
                    {
                        SetControlSize(newSize.Value);
                    }
                    if (req.NameEditing)
                    {
                        this.StartNameEditing();
                    }
                    //Set to Null to not refer to it in the future
                    nodeCommenterVM.InitializationRequests = null;
                }
#if false
                //
                //Debug. Edit the GroupMode value using the checkbox.
                //
                var cb = new CheckBox();
                //This process is layout-dependent, so changing the layout will throw an exception.
                var grid = WPFUtils.GetVisualAncestorNLevelsUp(this, 2) as Grid;
                WPFUtils.FindDescendantsOfType<StackPanel>(grid, true).First().Children.Add(cb);
                cb.IsChecked = (this.ViewModel as NodeCommenterViewModel).GroupMode;

                cb.Events().Checked
                    .Subscribe(e => 
                    {
                        SetGroupModeValue(true); 
                        e.Handled = true; 
                    }).DisposeWith(d);
                cb.Events().Unchecked
                    .Subscribe(e => 
                    {
                        SetGroupModeValue(false); 
                        e.Handled = true; 
                    }).DisposeWith(d);
#endif
                });
        }
        void CalcPositionAnsSize(out Point newPosition, out Size? newSize, NodeCommenterViewModel.InitialRequest initialRequest)
        {
            if (initialRequest.Size == null)
            {
                newPosition=initialRequest.Position;
                newSize = null;
                return;
            }
            /*
              P = newPosition

              P------------------+  ^
              | Comment          |  |
              +------------------+  |
              |                  |  |
              |                  |  |
              |                  |  |
              |   +----------+   |  |
              |   | Foo node |   |  |
              |   +----------+   |  | newSize.Height
              |   |          |   |  |
              |   |          |   |  |
              |   +----------+   |  |
              |                  |  |
              |                  |  |
              |                  |  |
              +------------------+  V

              <----------------->
                newSize.Width
            */
            var client = CalcClientRect();
       
            Size offset = new Size(
                client.Left,
                client.Top + 4);
            newPosition = new Point(
                initialRequest.Position.X - offset.Width ,
                initialRequest.Position.Y - offset.Height);
            var bottomSpace = this.NameLabel.ActualHeight;
            newSize= new Size(
                initialRequest.Size.Value.Width + offset.Width * 2,
                initialRequest.Size.Value.Height + offset.Height + bottomSpace);
        }
     
        void ToggleLabelEdit(MouseButtonEventArgs e)
        {
            if (ViewModel?.IsSelected == false)
            {
                return;
            }
            if (!GetBoundingBox(HeaderPanel, this).Contains(e.GetPosition(this)))
            {
                return ;
            }

            if (StartNameEditing()){
                e.Handled=true;
            }
        }
        bool StartNameEditing()
        {
            if (IsEditMode)
            {
                return false;
            }

            PrevIsHitTestVisible = NameLabel.IsHitTestVisible;
            PrevIsReadOnly = NameLabel.IsReadOnly;

            NameLabel.IsHitTestVisible = true;
            NameLabel.IsReadOnly = false;
            NameLabel.SelectAll();

            NameLabel.Focus();
            //Keyboard.Focus(NameLabel);

            confirmTextContentDisposable = 
                NameLabel.Events()
                    .PreviewKeyDown
                    .Subscribe(e => 
                    {
                        if ((e.Key == Key.Return) || (e.Key == Key.Enter) || (e.Key==Key.Escape))
                        {
                            e.Handled = true;
                            Focus();
                        }
                    });

            NameLabel.Events()
                .LostFocus
                .Take(1)
                .Subscribe(e =>
                {
                    confirmTextContentDisposable.Dispose();

                    NameLabel.IsHitTestVisible = PrevIsHitTestVisible;
                    NameLabel.IsReadOnly = PrevIsReadOnly;
                    IsEditMode = false;
                    // compositeDisposable.Dispose();
                }) /*.DisposeWith(compositeDisposable)*/ ;

            IsEditMode = true;
            return true;
        }
        void SetGroupModeValue(bool value)
        {
            (this.ViewModel as NodeCommenterViewModel).GroupMode = value;
        }
    }
}
