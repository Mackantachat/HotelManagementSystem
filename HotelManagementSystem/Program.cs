using HotelManagementSystem.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.IO;
using System.Linq;

namespace HotelManagementSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
            StreamReader sr = new StreamReader($@"{path}\Input\input.txt");
            var fileContent = sr.ReadToEnd();
            Console.WriteLine("input");
            Console.WriteLine(fileContent);

            var lstCommands = GetCommandsFromFileName(fileContent);
            List<KeyCard> keyCards = new List<KeyCard>();
            List<Room> rooms = new List<Room>();
            Console.WriteLine("Output");
            foreach (var cmd in lstCommands)
            {
                switch (cmd.Name)
                {
                    case "create_hotel":
                        rooms = GenerateRoom(cmd.Create.Floor, cmd.Create.RoomPerFloor);
                        Console.WriteLine($"Hotel created with {cmd.Create.Floor} floor(s), {cmd.Create.RoomPerFloor} room(s) per floor.");
                        break;
                    case "book":
                        var checkAvailableRooms = CheckAvailableRoom(rooms, cmd.Book.RoomNumber);
                        if (checkAvailableRooms.IsAvailable)
                        {
                            cmd.Book.KeyCard = GenerateKeyCard(ref keyCards);

                            var bookRooms = rooms.Where(r => r.RoomNumber == cmd.Book.RoomNumber).ToList();
                            foreach (var br in bookRooms)
                            {
                                br.Customer = new Customer();
                                br.Customer.Name = cmd.Book.Customer.Name;
                                br.Customer.Age = cmd.Book.Customer.Age;
                                br.IsAvailable = false;
                                br.KeyCardNumber = cmd.Book.KeyCard;
                            }
                            Console.WriteLine($"Room {cmd.Book.RoomNumber} is booked by {cmd.Book.Customer.Name} with keycard number {cmd.Book.KeyCard}.");
                        }
                        else
                        {
                            Console.WriteLine($"Cannot book room {cmd.Book.RoomNumber} for {cmd.Book.Customer.Name}, The room is currently booked by {checkAvailableRooms.Customer.Name}");
                        }

                        break;
                    case "list_available_rooms":
                        string availableRooms = CheckAvailableRoom(rooms);
                        Console.WriteLine(availableRooms);
                        break;
                    case "checkout":
                        var chkOutList = rooms.Where(r => r.KeyCardNumber == cmd.CheckOut.KeyCardNumber && r.Customer.Name.Equals(cmd.CheckOut.CustomerName)).FirstOrDefault();
                        if (chkOutList != null)
                        {
                            CheckOut(rooms , chkOutList);
                            ReturnKeyCard(ref keyCards , cmd.CheckOut.KeyCardNumber);
                            Console.WriteLine($"Room {chkOutList.RoomNumber} is checkout.");
                        }
                        else
                        {
                            var chkKeyCard = rooms.Where(r => r.KeyCardNumber == cmd.CheckOut.KeyCardNumber).FirstOrDefault();
                            Console.WriteLine($"Only {chkKeyCard.Customer.Name} can checkout with keycard number {cmd.CheckOut.KeyCardNumber}.");
                        }
                        break;
                    case "list_guest":
                        string guestName = GetListGuest(rooms);
                        Console.WriteLine(guestName);
                        break;
                    case "get_guest_in_room":
                        string guestInRoom = GetGuestInRoom(rooms, cmd.GuestInRoom.RoomNumber);
                        Console.WriteLine(guestInRoom);
                        break;
                    case "list_guest_by_age":
                        string guestByAge = GetGuestByAge(rooms, cmd.GuestInRoom.Condition, cmd.GuestInRoom.Customer.Age);
                        Console.WriteLine(guestByAge);
                        break;
                    case "list_guest_by_floor":
                        string guestByFloor = GetGuestByFloor(rooms, cmd.GuestInRoom.Floor);
                        Console.WriteLine(guestByFloor);
                        break;
                    case "checkout_guest_by_floor":
                        var chkOutFloor = rooms.Where(r => r.Floor == cmd.CheckOut.Floor && r.IsAvailable == false);
                        string roomNoChkOut = string.Empty;
                        foreach (var r in chkOutFloor)
                        {
                            ReturnKeyCard(ref keyCards, r.KeyCardNumber);
                            CheckOut(rooms, r);
                            roomNoChkOut += r.RoomNumber + ", "; 
                        }
                        roomNoChkOut = roomNoChkOut.Remove(roomNoChkOut.Length - 2);
                        Console.WriteLine($"Room {roomNoChkOut} is checkout.");
                        break;
                    case "book_by_floor":
                        var avaRoom = rooms.Where(r => r.IsAvailable == true && r.Floor.Equals(cmd.Book.Floor));
                        var canBookFloor = rooms.Where(r => r.IsAvailable == false && r.Floor.Equals(cmd.Book.Floor));
                        if (canBookFloor.Count() == 0)
                        {
                            string roomNumbers = string.Empty;
                            string keyCardNumbers = string.Empty;
                            foreach (var br in avaRoom)
                            {
                                int keyCard = GenerateKeyCard(ref keyCards);
                                roomNumbers += br.RoomNumber + ", ";
                                keyCardNumbers += keyCard + ", ";
                                br.Customer = new Customer();
                                br.Customer.Name = cmd.Book.Customer.Name;
                                br.Customer.Age = cmd.Book.Customer.Age;
                                br.IsAvailable = false;
                                br.KeyCardNumber = keyCard;
                            }
                            roomNumbers = roomNumbers.Remove(roomNumbers.Length - 2);
                            keyCardNumbers = keyCardNumbers.Remove(keyCardNumbers.Length - 2);
                            Console.WriteLine($"Room {roomNumbers} are booked with keycard number {keyCardNumbers}");
                        }
                        else
                        {
                            Console.WriteLine($"Cannot book floor {cmd.Book.Floor} for {cmd.Book.Customer.Name}.");

                        }

                        break;

                }
            }
        }

        public static void CheckOut(List<Room> rooms, Room chkOut)
        {
            foreach (var room in rooms)
            {
                if (chkOut.RoomNumber == room.RoomNumber)
                {
                    room.Customer.Name = string.Empty;
                    room.Customer.Age = 0;
                    room.IsAvailable = true;
                    room.KeyCardNumber = 0;
                }
            }
        }

        public static void ReturnKeyCard(ref List<KeyCard> keyCards, int keycard)
        {
            foreach (var keyCard in keyCards)
            {
                if (keyCard.KeyCardNumber == keycard)
                {
                    keyCard.IsActive = false;
                }
            }
        }

        public static string GetGuestByFloor(List<Room> rooms, string floor)
        {
            var lstGuest = rooms.Where(r => r.Floor == floor && r.IsAvailable == false).Distinct().FirstOrDefault();

            return lstGuest.Customer.Name;
        }

        public static string GetGuestByAge(List<Room> rooms, string condition, int Age)
        {
            List<Room> lstGuest = new List<Room>();
            string nameguest = string.Empty;
            if (condition.Equals("<"))
            {
                lstGuest = rooms.Where(r => r.Customer.Age < Age && r.IsAvailable == false).Distinct().ToList();
            }
            else if (condition.Equals(">"))
            {
                lstGuest = rooms.Where(r => r.Customer.Age > Age && r.IsAvailable == false).Distinct().ToList();

            }
            else if (condition.Equals("="))
            {
                lstGuest = rooms.Where(r => r.Customer.Age == Age && r.IsAvailable == false).Distinct().ToList();
            }
            else if (condition.Equals("<="))
            {
                lstGuest = rooms.Where(r => r.Customer.Age <= Age && r.IsAvailable == false).Distinct().ToList();
            }
            else if (condition.Equals(">="))
            {
                lstGuest = rooms.Where(r => r.Customer.Age >= Age && r.IsAvailable == false ).Distinct().ToList();
            }

            if (lstGuest.Count() > 0)
            {
                foreach (var guest in lstGuest)
                {
                    nameguest += guest.Customer.Name + ", ";
                }
                nameguest = nameguest.Remove(nameguest.Length - 2);
            }

            return nameguest;
        }

        public static string GetGuestInRoom(List<Room> rooms, string roomNumber)
        {
            var lstGuest = rooms.Where(r => r.RoomNumber == roomNumber).Distinct().FirstOrDefault();

            return lstGuest.Customer.Name;
        }

        public static string GetListGuest(List<Room> rooms)
        {
            var lstGuest = rooms.Where(r => r.IsAvailable == false).Distinct().OrderBy(o => o.KeyCardNumber).ToList();
            string guestName = string.Empty;
            foreach (var room in lstGuest)
            {
                guestName += room.Customer.Name + ", ";
            }
            guestName = guestName.Remove(guestName.Length - 2);
            return guestName;
        }

        public static List<Room> GenerateRoom(string floor, string roomPerFloor)
        {
            List<Room> rooms = new List<Room>();
            int addFloor = 0;
            int roomNumber = 0;
            for (int i = 1; i <= Convert.ToInt32(floor); i++)
            {
                addFloor = i;
                for (int j = 1; j <= Convert.ToInt32(roomPerFloor); j++)
                {
                    roomNumber = j;

                    rooms.Add(new Room
                    {
                        Floor = addFloor.ToString(),
                        RoomNumber = addFloor.ToString() + roomNumber.ToString().PadLeft(2, '0'),
                        IsAvailable = true
                    });
                }
            }

            return rooms;
        }

        public static int GenerateKeyCard(ref List<KeyCard> keyCards)
        {
            int keyCardNumber = 0;
            if (keyCards.Count() > 0)
            {
                keyCardNumber = keyCards.Where(x => x.IsActive == false).Select(s => s.KeyCardNumber).FirstOrDefault();
                if (keyCardNumber == 0)
                {
                    keyCardNumber = keyCards.Last(x => x.IsActive == true).KeyCardNumber + 1;
                    keyCards.Add(new KeyCard
                    {
                        KeyCardNumber = keyCardNumber,
                        IsActive = true,
                    });
                }
                else
                {
                    foreach (KeyCard keyCard in keyCards)
                    {
                        if (keyCard.KeyCardNumber == keyCardNumber)
                        {
                            keyCard.IsActive = true;
                        }
                    }
                }
               
            }
            else
            {
                keyCards.Add(new KeyCard
                {
                    KeyCardNumber = 1,
                    IsActive = true,
                });
                keyCardNumber = 1;
            }
            return keyCardNumber;
        }

        public static string CheckAvailableRoom(List<Room> rooms)
        {
            var availableRoom = rooms.Where(r => r.IsAvailable == true);
            string listAvailableRooms = string.Empty;
            if (availableRoom.Count() > 0)
            {
                foreach (var room in availableRoom)
                {
                    listAvailableRooms += room.RoomNumber + " ,";
                }
                listAvailableRooms = listAvailableRooms.Remove(listAvailableRooms.Length - 2);
            }
            return listAvailableRooms;
        }

        public static Room CheckAvailableRoom(List<Room> rooms, string roomNumber)
        {
            var availableRoom = rooms.Where(r => r.RoomNumber == roomNumber).FirstOrDefault();

            return availableRoom;
        }

        public static List<Commands> GetCommandsFromFileName(string fileContent)
        {
            List<Commands> commands = new List<Commands>();
            var sFile = fileContent.Split("\n");
            foreach (string file in sFile)
            {
                var sCmd = file.Split(" ");
                sCmd = sCmd.Select(x => { x = x.Trim(); return x; }).ToArray();
                switch (sCmd[0].ToString())
                {
                    case "create_hotel":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            Create = new Create() { Floor = sCmd[1], RoomPerFloor = sCmd[2] }
                        });
                        break;
                    case "book":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            Book = new Book() { RoomNumber = sCmd[1], Customer = new Customer() { Name = sCmd[2], Age = Convert.ToInt32(sCmd[3]) } }
                        });
                        break;
                    case "list_available_rooms":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                        });
                        break;
                    case "checkout":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            CheckOut = new CheckOut() { KeyCardNumber = Convert.ToInt32(sCmd[1]), CustomerName = sCmd[2] }
                        });
                        break;
                    case "list_guest":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                        });
                        break;
                    case "get_guest_in_room":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            GuestInRoom = new GuestInRoom() { RoomNumber = sCmd[1] }
                        });
                        break;
                    case "list_guest_by_age":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            GuestInRoom = new GuestInRoom() { Condition = sCmd[1], Customer = new Customer() { Age = Convert.ToInt32(sCmd[2]) } }
                        });
                        break;
                    case "list_guest_by_floor":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            GuestInRoom = new GuestInRoom() { Floor = sCmd[1]  }
                        });
                        break;
                    case "checkout_guest_by_floor":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            CheckOut = new CheckOut() { Floor = sCmd[1] }
                        });
                        break;
                    case "book_by_floor":
                        commands.Add(new Commands()
                        {
                            Name = sCmd[0],
                            Book = new Book() { Floor = sCmd[1], Customer = new Customer() { Name = sCmd[2], Age = Convert.ToInt32(sCmd[3]) } }
                        });
                        break;
                    default:
                        break;
                }

            }

            return commands;
        }

    }
}
