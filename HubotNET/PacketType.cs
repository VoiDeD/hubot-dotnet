using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubotNET
{
    enum PacketType
    {
        Invalid = 0,

        Chat,
        Emote,

        Enter,
        Leave,

        Topic
    }
}
