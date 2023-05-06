using System;

namespace Utilities.Sockets.EventSocket.Messages
{
    [Serializable]
    public class ItemAddedMessage : Message
    {
        public string ItemId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string BackpackId { get; set; }

        public ItemAddedMessage(string itemId, int x, int y, string backpackId)
        {
            ItemId = itemId;
            X = x;
            Y = y;
            BackpackId = backpackId;
        }
    }
}
