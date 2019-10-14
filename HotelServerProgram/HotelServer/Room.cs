using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServer
{
    class Room
    {
        public int rid;
        public DateTime startDate;
        public DateTime endDate;
        public byte usable;
        public int tempSet;
        public int humidSet;
        public int temp;
        public int humid;
        public int dust;

        public Room() { }
        public Room(Room room)
        {
            rid = room.rid;
            startDate = room.startDate;
            endDate = room.endDate;
            usable = room.usable;
            tempSet = room.tempSet;
            humidSet = room.humidSet;
            temp = room.temp;
            humid = room.humid;
            dust = room.dust;
        }
        public Room(int rid, DateTime startDate, DateTime endDate, byte usable,
                        int tempSet, int humidSet, int temp, int humid, int dust)
        {

            this.rid = rid;
            this.startDate = startDate;
            this.endDate = endDate;
            this.usable = usable;
            this.tempSet = tempSet;
            this.humidSet = humidSet;
            this.temp = temp;
            this.humid = humid;
            this.dust = dust;
        }
    }
}
