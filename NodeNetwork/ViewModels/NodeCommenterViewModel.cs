using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace NodeNetwork.ViewModels
{
    public class NodeCommenterViewModel : NodeViewModel
    {
        static NodeCommenterViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeCommenterView(), typeof(IViewFor<NodeCommenterViewModel>));
        }

        #region InitialRequest
        /// <summary>
        /// Requests to the View after the node is created. (Processed on the View side)
        /// </summary>
        internal class InitialRequest
        {
            /// <summary>
            /// Whether to change the node name after creating the node
            /// </summary>
            internal bool NameEditing { get; set; }
            /// <summary>
            /// Initial position after node creation
            /// </summary>
            internal Point Position { get; set; }
            /// <summary>
            /// Initial size after node creation
            /// </summary>
            internal Size? Size { get; set; }
        }
        internal InitialRequest /*?*/ InitializationRequests;
        #endregion

        #region GroupMode
        public bool GroupMode
        {
            get => _groupMode;
            set => this.RaiseAndSetIfChanged(ref _groupMode, value);
        }
        private bool _groupMode=true;
        #endregion

        #region Commands
        public ReactiveCommand<bool, Unit> EnableGroupMode { get; }
        #endregion

        #region Drag event
        private static bool IsConsideredPositionMove(ThumbPosition thumbPosition)
        {
            if (thumbPosition==ThumbPosition.None)
            {
                return true;
            }
            return false;
        }
        private void OnDragPositionStarted(NodeViewModel sender, DragPositionStartedEventArg e)
        {
            UpdateIncludedNodeItems();
        }
        private void OnDragPositionDelta(NodeViewModel sender, DragPositionDeltaEventArg e)
        {
            if (GroupMode && IsConsideredPositionMove(e.Position))
            {
                MoveIncludedNodes(Position);
            }
        }
        private void OnDragPositionCompleted(NodeViewModel sender, DragPositionCompletedEventArg e)
        {
            UpdateIncludedNodeItems();
            if (GroupMode)
            {
                MoveIncludedNodes(Position);
            }
        }
        private void OnDragSizeCompleted(NodeViewModel sender, DragSizingCompletedEventArg e)
        {
            UpdateIncludedNodeItems();
        }
        private void OnDragPositionCompletedAddedNode(NodeViewModel sender, DragPositionCompletedEventArg e)
        {
            CommonProcessingForAdditionalNode(sender);
        }
        private void OnDragSizeCompletedAddedNode(NodeViewModel sender, DragSizingCompletedEventArg e)
        {
            CommonProcessingForAdditionalNode(sender);
        }
        #endregion
        private void UpdateIncludedNodeItems()
        {
            var currentIncludedNodes = GatherIncludedNodes();
#if (NET6_0_OR_GREATER)
            var previouslyIncludedNodes = IncludedNodes.ExceptBy(currentIncludedNodes, x => x.Node);
#else
            var previouslyIncludedNodes = new List<Holder>();
            var set = new HashSet<NodeViewModel>(currentIncludedNodes);
            foreach (var element in IncludedNodes)
            {
                if (set.Add(element.Node))
                {
                    previouslyIncludedNodes.Add(element);
                }
            }
#endif
            foreach (var node in previouslyIncludedNodes)
            {
                RemoveIncludedNode(node.Node);
            }
            foreach (var node in currentIncludedNodes)
            {
                UpdateIncludedNode(node);
            }
        }
        private void CommonProcessingForAdditionalNode(NodeViewModel sender)
        {
            if (IsContains(sender))
            {
                UpdateIncludedNode(sender);
            } else
            {
                RemoveIncludedNode(sender);
            }
        }

        private class Holder : IEquatable<Holder>
        /*: IEquatable<Holder>*/
        {
            internal readonly NodeViewModel Node;
            internal Vector Direction;

            internal Holder(NodeViewModel node)
            {
                Node = node;
            }
            internal Holder(NodeViewModel node, Vector direction)
            {
                Node = node;
                Direction =direction;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Holder);
            }

            public bool Equals(Holder other)
            {
                return !(other is null) &&
                       EqualityComparer<NodeViewModel>.Default.Equals(Node, other.Node);
            }

            public override int GetHashCode()
            {
                return Node.GetHashCode();
            }

            public static bool operator ==(Holder left, Holder right)
            {
                return EqualityComparer<Holder>.Default.Equals(left, right);
            }

            public static bool operator !=(Holder left, Holder right)
            {
                return !(left == right);
            }
        }
        private HashSet<Holder> IncludedNodes = new HashSet<Holder>();
        private CompositeDisposable compositeDisposable = new CompositeDisposable();

        //static int instanceNo=0; //Used for debugging purposes only.

        public NodeCommenterViewModel()
        {
#if true
            Name = "Comment";
#else
            //Debug
            var baseName = string.Format("Comment_{0}",instanceNo);
            this.WhenAnyValue(vm => vm.ZIndex).Subscribe(z=>this.Name=String.Format("{0} ZIndex={1}",baseName,z)).DisposeWith(compositeDisposable);
            ++instanceNo;
#endif
            EnableGroupMode = ReactiveCommand.Create((bool enable) => { GroupMode = enable; }); 

            Resizable = ResizeOrientation.HorizontalAndVertical;
            BaseZIndex = Define.ZIndex.CommenterNode;

            this.WhenAnyValue(vm => vm.Parent)
                .Where(parent => parent != null)
                .Subscribe(parent =>
                {
                    parent.Nodes.Connect()
                        .ActOnEveryObject(
                            addedNode   => OnAddedNode(addedNode),
                            removedNode => OnRemovedNodes(removedNode)
                        ).DisposeWith(compositeDisposable);

                    this.WhenAnyValue(vm => vm.GroupMode)
                        .Subscribe(groupMode => 
                        {
                            if (groupMode)
                            {
                                UpdateIncludedNodeItems();
                            } else
                            {
                                IncludedNodes.Clear();
                            }
                        }).DisposeWith(compositeDisposable);

                }).DisposeWith(compositeDisposable);

            this.DragPositionStarted += OnDragPositionStarted;
            this.DragPositionDelta += OnDragPositionDelta;
            this.DragPositionCompleted += OnDragPositionCompleted;
            this.DragSizeCompleted += OnDragSizeCompleted;
        }
        private void OnRemoveThis()
        {
            this.DragPositionStarted -= OnDragPositionStarted;
            this.DragPositionDelta -= OnDragPositionDelta;
            this.DragPositionCompleted -= OnDragPositionCompleted;
            this.DragSizeCompleted -= OnDragSizeCompleted;

            compositeDisposable.Dispose();
            foreach (var node in IncludedNodes)
            {
                RemoveIncludedNode(node.Node);
            }
            IncludedNodes.Clear();
        }
        private void OnAddedNode(NodeViewModel addedNode) 
        {
            if (addedNode == this)
            {
                return;
            }
            addedNode.DragPositionCompleted += OnDragPositionCompletedAddedNode;
            addedNode.DragSizeCompleted += OnDragSizeCompletedAddedNode;
            /* Respond when dropped from NodeListView
            */
            addedNode.WhenAnyValue(vm => vm.Position)
                .Take(1)
                .Subscribe(value =>
                {
                    var debug = String.Format("{0} -> {1}",this.Name, addedNode.Name);
                    if (IsContains(addedNode))
                    {
                        UpdateIncludedNode(addedNode);
                    } else
                    {
                        RemoveIncludedNode(addedNode);
                    }
                })
                /* Take(N) automatically disposes of the subscription.
                .DisposeWith(compositeDisposable)
                */ 
                ;
        }
        private void OnRemovedNodes(NodeViewModel removedNode) 
        {
            removedNode.DragPositionCompleted -= OnDragPositionCompletedAddedNode;
            removedNode.DragSizeCompleted -= OnDragSizeCompletedAddedNode;
            if (removedNode == this)
            {
                OnRemoveThis();
            } else
            {
                RemoveIncludedNode(removedNode);
            }
        }
        bool IsContains(NodeViewModel other) 
        {
            if (this == other)
            {
                return false;
            }
            var thisRect  = new Rect(this.Position, this.Size);
            var otherRect = new Rect(other.Position, other.Size);
            return thisRect.Contains(otherRect);
        }
        void MoveIncludedNodes(Point commentNodePosition)
        {
            Debug.Assert(IncludedNodes.Count <= Parent.Nodes.Count);
            foreach (var containItem in IncludedNodes)
            {
                if (containItem == null)
                {
                    continue;
                }
                containItem.Node.Position = containItem.Direction + commentNodePosition;
            }
        }
        IEnumerable<NodeViewModel> GatherIncludedNodes()
        {
            return Parent.Nodes.Items.Where(otherNode => IsContains(otherNode));
        }
        
        /// <summary>
        ///ZIndex|
        ///------+----------------------------------------------
        ///Z     | [Unselected] Under CommenterNode   ---+
        ///Z+1   | [Selected]   Under CommenterNode      | ZStep
        ///Z+2   | [Unselected] Current CommenterNode <--+
        ///Z+3   | [Selected]   Current CommenterNode
        /// </summary>
        const int ZStep = 2;

        void UpdateIncludedNode(NodeViewModel newNode)
        {
            if (newNode == null)
            {
                return;
            }
            var key = new Holder(newNode);
            var direction = newNode.Position - this.Position;
            if (IncludedNodes.TryGetValue(key, out var existItem))
            {
                existItem.Direction = direction;
            } else
            {   //First time.
                key.Direction = direction;
                if(newNode is NodeCommenterViewModel newCommenterNode)
                {
                    newCommenterNode.BaseZIndex += ZStep;
                }
                IncludedNodes.Add(key);
            }
        }
        void RemoveIncludedNode(NodeViewModel node)
        {
            var key = new Holder(node);
            if (IncludedNodes.TryGetValue(key, out var existItem))
            {
                IncludedNodes.Remove(key);
                if (existItem.Node is NodeCommenterViewModel commenterNode)
                {
                    commenterNode.BaseZIndex -= ZStep;
                }
            }
        }
    }
}
