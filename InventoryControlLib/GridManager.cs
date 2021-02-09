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
        private Guid itemSubscriptionToken;

        List<UpdateGrid> grids;
        
        private static readonly Lazy<GridManager> lazy = new Lazy<GridManager>(() => new GridManager());
        public static GridManager Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public void SetHub(IMessageHub MessageHub)
        {
            logger.Debug($"> SetHub(MessageHub: {MessageHub})");
            if (hub == null)
            {
                hub = MessageHub;
                gridSubscriptionToken = hub.Subscribe<GridAddUpdate>(GridAddUpdate);
                itemSubscriptionToken = hub.Subscribe<ItemPositionUpdate>(ItemPositionUpdate);
                logger.Debug($"Hub set");
            }
            logger.Debug($"< SetHub(MessageHub: {MessageHub})");
        }

        private GridManager()
        {
            logger.Info($"> GridManager()");
            grids = new List<UpdateGrid>();
            logger.Info($"< GridManager()");
        }

        private void GridAddUpdate(GridAddUpdate gridUpdate)
        {
            logger.Info($"> GridAddUpdate(gridUpdate: [{gridUpdate}])");
            grids.Add(gridUpdate.Grid);
            logger.Info($"< GridAddUpdate(gridUpdate: [{gridUpdate}])");
        }
        private void ItemPositionUpdate(ItemPositionUpdate positionUpdate)
        {
            logger.Debug($"> ItemPositionUpdate(positionUpdate: [{positionUpdate}])");
            foreach (var grid in grids)
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
                        var startingX = item.Column * grid.CellSize.Width + parentPoint.X;
                        var startingY = item.Row * grid.CellSize.Height + parentPoint.Y;
                        var p = new Point(startingX, startingY);
                        item.Transform(p);
                }
                else
                {
                    logger.Debug($"Ignored");
                }
            }
            logger.Debug($"< ItemPositionUpdate(positionUpdate: [{positionUpdate}])");
        }
    }
}
