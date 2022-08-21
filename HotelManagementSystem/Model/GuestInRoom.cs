using System;
using System.Collections.Generic;
using System.Text;

namespace HotelManagementSystem.Model
{
    public class GuestInRoom
    {
        public string RoomNumber { get; set; }
        public Customer Customer { get; set; }
        public string Condition { get; set; }
        public string Floor { get; set; }
    }
}
