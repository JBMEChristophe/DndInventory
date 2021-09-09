using Easy.MessageHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InventoryControlLib
{
    public sealed class GridManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IMessageHub hub;
        private Guid gridSubscriptionToken;
        private Guid deleteGridSubscriptionToken;
        private Guid itemSubscriptionToken;

        public List<UpdateGrid> Grids { get; }

        private static readonly Lazy<GridManager> lazy = new Lazy<GridManager>(() => new GridManager());
        public static GridManager Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public Guid GroundId { get; set; }
        public UpdateGrid GroundGrid { get { return Grids.Where((e) => e.Id == GroundId).First(); } }

        public void SetHub(IMessageHub MessageHub)
        {
            logger.Debug($"> SetHub(MessageHub: {MessageHub})");
            if (hub == null)
            {
                hub = MessageHub;
                gridSubscriptionToken = hub.Subscribe<UpdateGrid>(GridUpdate);
                deleteGridSubscriptionToken = hub.Subscribe<DeleteGrid>(DeleteGrid);
                itemSubscriptionToken = hub.Subscribe<ItemPositionUpdate>(ItemPositionUpdate);
                logger.Debug($"Hub set");
            }
            logger.Debug($"< SetHub(MessageHub: {MessageHub})");
        }

        private GridManager()
        {
            logger.Info($"> GridManager()");
            Grids = new List<UpdateGrid>();
            logger.Info($"< GridManager()");
        }

        private void GridUpdate(UpdateGrid gridUpdate)
        {
            logger.Info($"(GridManager)> GridUpdate(gridUpdate: [{gridUpdate}])"); 
            int index = Grids.FindIndex(m => m.Id == gridUpdate.Id);
            if (index >= 0)
            { 
                Grids[index] = gridUpdate;
            }
            else
            {
                Grids.Add(gridUpdate);
            }
            logger.Info($"(GridManager)< GridUpdate(gridUpdate: [{gridUpdate}])");
        }

        private void DeleteGrid(DeleteGrid grid)
        {
            logger.Info($"(GridManager)> DeleteGrid(DeleteGrid: [{grid}])");
            int index = Grids.FindIndex(m => m.Id == grid.Id);
            if (index >= 0)
            {
                Grids.RemoveAt(index);
            }
            else
            {
                logger.Debug($"No grid found with guid: {grid.Id}");
            }
            logger.Info($"(GridManager)< DeleteGrid(DeleteGrid: [{grid}])");
        }

        private void ItemPositionUpdate(ItemPositionUpdate positionUpdate)
        {
            logger.Debug($"(GridManager)> ItemPositionUpdate(positionUpdate: [{positionUpdate}])");
            foreach (var grid in Grids)
            {
                var item = positionUpdate.Item;
                var releasePoint = positionUpdate.Position;

                var width = grid.Size.Width;
                var height = grid.Size.Height;
                var screenPoint = grid.Grid.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

                if (!(releasePoint.X < screenPoint.X + width && releasePoint.Y < screenPoint.Y + height
                    && releasePoint.X > screenPoint.X && releasePoint.Y > screenPoint.Y))
                {
                        var parentPoint = item.GridParent.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
                        var startingX = item.Model.CellX * grid.CellSize.Width + parentPoint.X;
                        var startingY = item.Model.CellY * grid.CellSize.Height + parentPoint.Y;
                        var p = new Point(startingX, startingY);
                        item.Transform(p);
                }
                else
                {
                    logger.Debug($"Ignored");
                }
            }
            logger.Debug($"(GridManager)< ItemPositionUpdate(positionUpdate: [{positionUpdate}])");
        }
    }
}
