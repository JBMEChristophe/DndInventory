﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Sockets.SocketFileTransfer
{
    public enum Headers : byte
    {
        Queue,
        Start,
        Stop,
        Pause,
        Chunk
    }
}
