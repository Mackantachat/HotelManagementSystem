using System;
using System.Collections.Generic;
using System.Text;

namespace HotelManagementSystem.Model
{
    public class Room
    {
        public string Floor { get; set; }
        public string RoomNumber { get; set; }
        public Customer Customer { get; set; }
        public bool IsAvailable { get; set; }
        public int KeyCardNumber { get; set; }
    }
}
