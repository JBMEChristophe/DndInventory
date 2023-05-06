using System;

namespace Utilities.Sockets.EventSocket.Messages
{
    [Serializable]
    public class ItemMovedMessage : Message
    {
        public string ItemId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string FromBackpackId { get; set; }
        public string ToBackpackId { get; set; }

        public ItemMovedMessage(string itemId, int x, int y, string fromBackpackId, string toBackpackId) : base()
        {
            X = x;
            Y = y;
            FromBackpackId = fromBackpackId;
            ToBackpackId = toBackpackId;
            ItemId = itemId;
        }

        public ItemMovedMessage(string itemId, int x, int y, string fromBackpackId) : this(itemId, x, y, fromBackpackId, fromBackpackId) { }
    }
}
