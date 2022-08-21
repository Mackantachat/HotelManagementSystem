using System;
using System.Collections.Generic;
using System.Text;

namespace HotelManagementSystem.Model
{
    public class Book
    {
        public string Floor { get; set; }
        public string RoomNumber { get; set; }
        public Customer Customer { get; set; }
        public int KeyCard { get; set; }
    }
}
