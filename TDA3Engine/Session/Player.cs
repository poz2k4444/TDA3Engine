using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TDA3Engine
{
    public class Player
    {
        public uint Money
        {
            get;
            set;
        }

        public List<Tower> PlacedTowers
        {
            get;
            set;
        }
    }
}
