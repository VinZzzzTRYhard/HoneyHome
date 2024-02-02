using HoneyHome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyHome.Model
{
    internal class RoomsItems : IRoom
    {
        public string Name {  get; set; }
        public Int64 RoomId { get; set; }
    }
}
