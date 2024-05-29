using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using NodeNetwork.Views.Controls;
using ReactiveUI;
using Splat;

namespace NodeNetwork.Views
{
    [TemplatePart(Name = nameof(CollapseButton), Type = typeof(ArrowToggleButton))]
    [TemplatePart(Name = nameof(NameLabel), Type = typeof(TextBox))]
    [TemplatePart(Name = nameof(HeaderIcon), Type = typeof(Image))]
    [TemplatePart(Name = nameof(HeaderBottomMargin),Type=typeof(Canvas))]
    [TemplatePart(Name = nameof(BottomMargin),Type=typeof(Canvas))]
    [TemplatePart(Name = nameof(InputsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(OutputsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(EndpointGroupsList), Type = typeof(ItemsControl))]
    [TemplatePart(Name = nameof(ResizeVerticalThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeHorizontalThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeDiagonalThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeVerticalTopThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeHorizontalLeftThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeDiagonalBottomLeftThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeDiagonalTopLeftThumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(ResizeDiagonalTopRightThumb), Type = typeof(Thumb))]
    [TemplateVisualState(Name = SelectedState, GroupName = SelectedVisualStatesGroup)]
    [TemplateVisualState(Name = UnselectedState, GroupName = SelectedVisualStatesGroup)]
    [TemplateVisualState(Name = CollapsedState, GroupName = CollapsedVisualStatesGroup)]
    [TemplateVisualState(Name = ExpandedState, GroupName = CollapsedVisualStatesGroup)]
    public class NodeView : Control, IViewFor<NodeViewModel>
    {
        #region SelectedStates
        public const string SelectedVisualStatesGroup = "SelectedStates";
        public const string SelectedState = "Selected";
        public const string UnselectedState = "Unselected";
        #endregion

        #region CollapsedStates
        public const string CollapsedVisualStatesGroup = "CollapsedStates";
        public const string CollapsedState = "Collapsed";
        public const string ExpandedState = "Expanded"; 
        #endregion

        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(NodeViewModel), typeof(NodeView), new PropertyMetadata(null));

        public NodeViewModel ViewModel
        {
            get => (NodeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (NodeViewModel)value;
        }
        #endregion

        #region Properties
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(NodeView));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty ArrowSizeProperty = DependencyProperty.Register(nameof(ArrowSize), typeof(double), typeof(NodeView));
        public double ArrowSize
        {
            get => (double)GetValue(ArrowSizeProperty);
            set => SetValue(ArrowSizeProperty, value);
        }

        public static readonly DependencyProperty TitleFontFamilyProperty = DependencyProperty.Register(nameof(TitleFontFamily), typeof(FontFamily), typeof(NodeView));
        public FontFamily TitleFontFamily
        {
            get => (FontFamily)GetValue(TitleFontFamilyProperty);
            set => SetValue(TitleFontFamilyProperty, value);
        }

        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(nameof(TitleFontSize), typeof(double), typeof(NodeView));
        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty EndpointsStackingOrientationProperty = DependencyProperty.Register(nameof(EndpointsStackingOrientation), typeof(Orientation), typeof(NodeView));
        public Orientation EndpointsStackingOrientation
        {
            get => (Orientation)GetValue(EndpointsStackingOrientationProperty);
            set => SetValue(EndpointsStackingOrientationProperty, value);
        }

        public static readonly DependencyProperty LeadingControlPresenterStyleProperty = DependencyProperty.Register(nameof(LeadingControlPresenterStyle), typeof(Style), typeof(NodeView));
        public Style LeadingControlPresenterStyle
        {
	        get => (Style)GetValue(LeadingControlPresenterStyleProperty);
	        set => SetValue(LeadingControlPresenterStyleProperty, value);
        }

        public static readonly DependencyProperty TrailingControlPresenterStyleProperty = DependencyProperty.Register(nameof(TrailingControlPresenterStyle), typeof(Style), typeof(NodeView));
        public Style TrailingControlPresenterStyle
		{
	        get => (Style)GetValue(TrailingControlPresenterStyleProperty);
	        set => SetValue(TrailingControlPresenterStyleProperty, value);
        }
		#endregion

        private ArrowToggleButton CollapseButton { get; set; }
        protected TextBox NameLabel { get; set; }
        private Image HeaderIcon { get; set; }
        private Canvas HeaderBottomMargin { get; set; }
        private Canvas BottomMargin { get; set; }
        private ItemsControl InputsList { get; set; }
        private ItemsControl OutputsList { get; set; }
        private ItemsControl EndpointGroupsList { get; set; }
        private Thumb ResizeVerticalThumb { get; set; }
        private Thumb ResizeHorizontalThumb { get; set; }
        private Thumb ResizeDiagonalThumb { get; set; }
        private Thumb ResizeVerticalTopThumb { get; set; }
        private Thumb ResizeHorizontalLeftThumb { get; set; }
        private Thumb ResizeDiagonalBottomLeftThumb { get; set; }
        private Thumb ResizeDiagonalTopLeftThumb { get; set; }
        private Thumb ResizeDiagonalTopRightThumb { get; set; } 

        public NodeView()
        {
            DefaultStyleKey = typeof(NodeView);

            SetupBindings();
            SetupEvents();
            SetupVisualStateBindings();
        }

        public override void OnApplyTemplate()
        {
            CollapseButton = GetTemplateChild(nameof(CollapseButton)) as ArrowToggleButton;
            NameLabel = GetTemplateChild(nameof(NameLabel)) as TextBox;
            HeaderIcon = GetTemplateChild(nameof(HeaderIcon)) as Image;
            HeaderBottomMargin = GetTemplateChild(nameof(HeaderBottomMargin)) as Canvas;
            BottomMargin = GetTemplateChild(nameof(BottomMargin)) as Canvas;
            InputsList = GetTemplateChild(nameof(InputsList)) as ItemsControl;
            OutputsList = GetTemplateChild(nameof(OutputsList)) as ItemsControl;
            EndpointGroupsList = GetTemplateChild(nameof(EndpointGroupsList)) as ItemsControl;
            ResizeVerticalThumb = GetTemplateChild(nameof(ResizeVerticalThumb)) as Thumb;
            ResizeHorizontalThumb = GetTemplateChild(nameof(ResizeHorizontalThumb)) as Thumb;
            ResizeDiagonalThumb = GetTemplateChild(nameof(ResizeDiagonalThumb)) as Thumb;
            ResizeVerticalTopThumb = GetTemplateChild(nameof(ResizeVerticalTopThumb)) as Thumb;
            ResizeHorizontalLeftThumb = GetTemplateChild(nameof(ResizeHorizontalLeftThumb)) as Thumb;
            ResizeDiagonalBottomLeftThumb = GetTemplateChild(nameof(ResizeDiagonalBottomLeftThumb)) as Thumb;
            ResizeDiagonalTopLeftThumb = GetTemplateChild(nameof(ResizeDiagonalTopLeftThumb)) as Thumb;
            ResizeDiagonalTopRightThumb = GetTemplateChild(nameof(ResizeDiagonalTopRightThumb)) as Thumb;
            {
                //
                //ResizeVerticalThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.Top;
                ResizeVerticalThumb.DragStarted += (sender, e) => 
                { 
                    OnThumDragStarted(); 
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight)); 
                };
                ResizeVerticalThumb.DragDelta += (sender, e) =>
                {
                    ApplyResize(DragDeltaEventToVector(e), false, true,false);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeVerticalThumb.DragCompleted += (sender, e) => ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
            }

            {
                //
                //ResizeHorizontalThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.Right;
                ResizeHorizontalThumb.DragStarted += (sender, e) => 
                { 
                    OnThumDragStarted(); 
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight)); 
                };
                ResizeHorizontalThumb.DragDelta += (sender, e) =>
                {
                    ApplyResize(DragDeltaEventToVector(e), true, false, false);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeHorizontalThumb.DragCompleted += (sender, e) => ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
            }

            {
                //
                //ResizeDiagonalThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.BottomRight;
                ResizeDiagonalThumb.DragStarted += (sender, e) => 
                { 
                    OnThumDragStarted(); 
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight)); 
                };
                ResizeDiagonalThumb.DragDelta += (sender, e) =>
                {
                    ApplyResize(DragDeltaEventToVector(e), true, true,false);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeDiagonalThumb.DragCompleted += (sender, e) => ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
            }

            {
                //
                //ResizeVerticalTopThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.Top;
                ResizeVerticalTopThumb.DragStarted += (sender, e) =>
                {
                    OnThumDragStarted();
                    ViewModel.NotifyDragPositionStarted(thumbPos);
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeVerticalTopThumb.DragDelta += (sender, e) =>
                {
                    var delta = CalcDragDelta(e, false, true);
                    ViewModel.NotifyDragPositionDelta(thumbPos, delta);
                    ApplyResize(delta, false, true,true);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeVerticalTopThumb.DragCompleted += (sender, e) =>
                {
                    ViewModel.NotifyDragPositionCompleted(thumbPos);
                    ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
                };
            }

            {
                //
                //ResizeHorizontalLeftThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.Left;
                ResizeHorizontalLeftThumb.DragStarted += (sender, e) =>
                {
                    OnThumDragStarted();
                    ViewModel.NotifyDragPositionStarted(thumbPos);
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeHorizontalLeftThumb.DragDelta += (sender, e) =>
                {
                    var delta = CalcDragDelta(e, true, false);
                    ViewModel.NotifyDragPositionDelta(thumbPos, delta);
                    ApplyResize(delta, true, false,true);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeHorizontalLeftThumb.DragCompleted += (sender, e) =>
                {
                    ViewModel.NotifyDragPositionCompleted(thumbPos);
                    ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
                };
            }

            {
                //
                //ResizeDiagonalBottomLeftThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.BottomLeft;
                ResizeDiagonalBottomLeftThumb.DragStarted += (sender, e) =>
                {
                    OnThumDragStarted();
                    ViewModel.NotifyDragPositionStarted(thumbPos);
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeDiagonalBottomLeftThumb.DragDelta += (sender, e) =>
                {
                    var delta = CalcDragDelta(e, true, false);
                    ApplyResize(delta, true, false,true);
                    ApplyResize(new Vector(e.HorizontalChange,e.VerticalChange), false, true,false);
                    ViewModel.NotifyDragPositionDelta(thumbPos, delta);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeDiagonalBottomLeftThumb.DragCompleted += (sender, e) =>
                {
                    ViewModel.NotifyDragPositionCompleted(thumbPos);
                    ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
                };
                
            }

            {
                //
                //ResizeDiagonalTopLeftThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.BottomLeft;
                ResizeDiagonalTopLeftThumb.DragStarted += (sender, e) =>
                {
                    OnThumDragStarted();
                    ViewModel.NotifyDragPositionStarted(thumbPos);
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeDiagonalTopLeftThumb.DragDelta += (sender, e) =>
                {
                    var delta = CalcDragDelta(e, true, true);
                    ApplyResize(delta, true, true,true);
                    ApplyResize(new Vector(e.HorizontalChange, e.VerticalChange), false, false, false);
                    ViewModel.NotifyDragPositionDelta(thumbPos, delta);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeDiagonalTopLeftThumb.DragCompleted += (sender, e) =>
                {
                    ViewModel.NotifyDragPositionCompleted(thumbPos);
                    ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
                };
            }

            {
                //
                //ResizeDiagonalTopRightThumb
                //
                const ThumbPosition thumbPos = ThumbPosition.BottomLeft;
                ResizeDiagonalTopRightThumb.DragStarted += (sender, e) =>
                {
                    OnThumDragStarted();
                    ViewModel.NotifyDragPositionStarted(thumbPos);
                    ViewModel.NotifyDragSizingStarted(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeDiagonalTopRightThumb.DragDelta += (sender, e) =>
                {
                    var delta = CalcDragDelta(e, false, true);
                    ApplyResize(delta, false, true, true);
                    ApplyResize(DragDeltaEventToVector(e), true, false, false);
                    ViewModel.NotifyDragPositionDelta(thumbPos, delta);
                    ViewModel.NotifyDragSizing(thumbPos, new Size(MinWidth, MinHeight));
                };
                ResizeDiagonalTopRightThumb.DragCompleted += (sender, e) =>
                {
                    ViewModel.NotifyDragPositionCompleted(thumbPos);
                    ViewModel.NotifyDragSizingCompleted(thumbPos, new Size(MinWidth, MinHeight));
                };
            }

            VisualStateManager.GoToState(this, ExpandedState, false);
            VisualStateManager.GoToState(this, UnselectedState, false);
        }
   
        private void ApplyResize(Vector change, bool horizontal, bool vertical, bool isPositionMove)
        {
            if (horizontal)
            {
                double newWidth;
                if (isPositionMove)
                {
                    newWidth = MinWidth - change.X;
                } else
                {
                    newWidth = MinWidth + change.X;
                }
                MinWidth = Math.Max(firstActualMinSize.Value.Width, newWidth);
            }
            if (vertical)
            {
                double newHeight;
                if (isPositionMove)
                {
                    newHeight = MinHeight - change.Y;
                } else
                {
                    newHeight = MinHeight + change.Y;
                }
                MinHeight = Math.Max(firstActualMinSize.Value.Height, newHeight);
            }
        }
        private static Vector DragDeltaEventToVector(DragDeltaEventArgs e)
        {
            return new Vector(e.HorizontalChange,e.VerticalChange);
        }

        #region Top and left drag processing.
        private Size? initialActualMinSize;
        private Size? firstActualMinSize;
        private void OnThumDragStarted()
        {
            if (firstActualMinSize==null)
            {
                firstActualMinSize = initialActualMinSize; //new Size(ActualWidth, ActualHeight);
                MinWidth = ActualWidth;
                MinHeight = ActualHeight;
            }
        }
        private Vector CalcDragDelta(DragDeltaEventArgs e, bool horizontal, bool vertical)
        {
            double deltaX=0;
            double deltaY=0;
            if (horizontal)
            {
                var newWidth = MinWidth - e.HorizontalChange;
                if (newWidth <= firstActualMinSize.Value.Width)
                {
                    deltaX = 0;
                } else
                {

                    deltaX = e.HorizontalChange;
                }
            }
            if (vertical)
            {
                var newHeight = MinHeight - e.VerticalChange;
                if (newHeight <= firstActualMinSize.Value.Height)
                {
                    deltaY = 0;
                } else
                {
                    deltaY = e.VerticalChange;
                }
            }
            return new Vector(deltaX, deltaY);
        }
        #endregion
        private void SetupBindings()
        {
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel, vm => vm.IsCollapsed, v => v.CollapseButton.IsChecked).DisposeWith(d);

                this.OneWayBind(ViewModel, vm => vm.Name, v => v.NameLabel.Text).DisposeWith(d);

	            this.BindList(ViewModel, vm => vm.VisibleInputs, v => v.InputsList.ItemsSource).DisposeWith(d);
	            this.BindList(ViewModel, vm => vm.VisibleOutputs, v => v.OutputsList.ItemsSource).DisposeWith(d);
	            this.OneWayBind(ViewModel, vm => vm.VisibleEndpointGroups, v => v.EndpointGroupsList.ItemsSource).DisposeWith(d);

                this.WhenAnyValue(v => v.ActualWidth, v => v.ActualHeight, (width, height) => new Size(width, height))
                    .BindTo(this, v => v.ViewModel.Size).DisposeWith(d);
#if true
                this.WhenAnyValue(v => v.ActualWidth, v => v.ActualHeight, (width, height) => new Size(width, height))
                    .Take(1)
                    .Subscribe(sz => {
                        Debug.Assert(initialActualMinSize == null);
                        initialActualMinSize = sz;
                    }) /*.DisposeWith(d)*/ ;
#endif
                this.OneWayBind(ViewModel, vm => vm.HeaderIcon, v => v.HeaderIcon.Source, img => img?.ToNative()).DisposeWith(d);
            });
        }

        private void SetupEvents()
        {
            this.MouseLeftButtonDown += (sender, args) =>
            {
                this.Focus();

                if (ViewModel == null)
                {
                    return;
                }

                if (ViewModel.IsSelected)
                {
                    return;
                }

                if (ViewModel.Parent != null && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    ViewModel.Parent.ClearSelection();
                }

                ViewModel.IsSelected = true;
            };
        }

        private void SetupVisualStateBindings()
        {
            this.WhenActivated(d =>
            {
                this.WhenAnyValue(v => v.ViewModel.IsCollapsed).Subscribe(isCollapsed =>
                {
                    VisualStateManager.GoToState(this, isCollapsed ? CollapsedState : ExpandedState, true);
                }).DisposeWith(d);

                this.WhenAnyValue(v => v.ViewModel.IsSelected).Subscribe(isSelected =>
                {
                    VisualStateManager.GoToState(this, isSelected ? SelectedState : UnselectedState, true);
                }).DisposeWith(d);
            });
        }
        protected Rect CalcClientRect() {
            var topLeft = HeaderBottomMargin.TranslatePoint(new Point(ResizeHorizontalLeftThumb.Width, HeaderBottomMargin.ActualHeight), this);
            var bottomRight = ResizeDiagonalThumb.TranslatePoint(new Point(0,0),this); 
            return new Rect(topLeft, bottomRight);
        }
    }
}
