using System;
using System.Collections.Generic;
using System.Text;

namespace HotelManagementSystem.Model
{
    public class Commands
    {
        public string Name { get; set; }
        public Create Create { get; set; }
        public Book Book { get; set; }
        public CheckOut CheckOut { get; set; }
        public GuestInRoom GuestInRoom { get; set; }
    }
}
