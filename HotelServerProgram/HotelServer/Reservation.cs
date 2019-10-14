using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServer
{
    class Reservation
    {
        public int rid;
        public string nid;
        public Reservation() { }
        public Reservation(Reservation reservation)
        {
            rid = reservation.rid;
            nid = reservation.nid;
        }
        public Reservation(int rid, string nid)
        {
            this.rid = rid;
            this.nid = nid;
        }
    }
}
