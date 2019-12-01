using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace HotelServer
{
    //데이터베이스 관리(질의) 클래스
    class DBManager
    {
        const string strConn = "Data Source = 27.117.199.42,1433; database = HotelDB; uid = sa; pwd = 9d8a7v7i8d2-@";
        private SqlConnection con;  //DB 컨넥터

        //생성자
        public DBManager()
        {
            try
            {
                con = new SqlConnection(strConn);
                con.Open();
            }
            catch (Exception)
            {
            }
            finally
            {
                con.Close();
            }
        }

        //전화번호로 nid를 검색
        public string SelectNIDByPhone(string strPhnNmbr)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand($"SELECT nid FROM Customer WHERE PhoneNumber = '{strPhnNmbr}'", con);
                SqlDataReader rd = cmd.ExecuteReader();

                //검색 결과가 없으면 null반환
                if (rd.HasRows == false)
                {
                    //DB닫기
                    con.Close();
                    return null;
                }

                //데이터를 읽어들임
                rd.Read();
                string nid = rd.GetString(0);

                con.Close();
                return nid;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //nid로 고객 검색
        public Customer SelectCustomerByNID(string nid)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Customer WHERE nid = '" + nid + "'", con);
                SqlDataReader rd = cmd.ExecuteReader();

                //검색 결과가 없으면 null반환
                if (rd.HasRows == false)
                {
                    //DB닫기
                    con.Close();
                    return null;
                }

                Customer customer = new Customer();
                //데이터를 읽어들임
                rd.Read();
                customer.nid = rd.GetString(0);
                customer.name = rd.IsDBNull(1) ? "" : rd.GetString(1);
                customer.phone = rd.IsDBNull(2) ? "" : rd.GetString(2);
                customer.age = rd.IsDBNull(3) ? int.MinValue : rd.GetInt32(3);
                customer.address = rd.IsDBNull(4) ? "" : rd.GetString(4);

                con.Close();
                return customer;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }


        //전체 고객 목록
        public List<Customer> SelectAllCustomers()
        {
            try
            {
                List<Customer> customerList = new List<Customer>();
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Customer", con);
                SqlDataReader rd = cmd.ExecuteReader();
                Customer customer;

                while (rd.Read())
                {
                    customer = new Customer();
                    //데이터를 읽어들임
                    customer.nid = rd.GetString(0);
                    customer.name = rd.IsDBNull(1) ? null : rd.GetString(1);
                    customer.phone = rd.IsDBNull(2) ? null : rd.GetString(2);
                    customer.age = rd.IsDBNull(3) ? int.MinValue : rd.GetInt32(3);
                    customer.address = rd.IsDBNull(4) ? null : rd.GetString(4);
                    customerList.Add(customer);
                }

                //DB닫기
                con.Close();
                //검색 결과가 없으면 null반환
                if (customerList.Count == 0) { return null; }

                return customerList;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //rid로 객실 검색
        public Room SelectRoomByRID(int rid)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Room WHERE rid = " + rid, con);
                SqlDataReader rd = cmd.ExecuteReader();

                //검색 결과가 없으면 null반환
                if (rd.HasRows == false)
                {
                    //DB닫기
                    con.Close();
                    return null;
                }

                Room room = new Room();
                //Room 레코드를 읽어들임
                rd.Read();
                room.rid = rd.GetInt32(0);
                room.startDate = rd.IsDBNull(1) ? DateTime.MinValue : rd.GetDateTime(1);
                room.endDate = rd.IsDBNull(2) ? DateTime.MinValue : rd.GetDateTime(2);
                if (rd.IsDBNull(3)) { room.usable = 1; }
                else { room.usable = rd.GetBoolean(3) ? (byte)1 : (byte)0; }
                room.tempSet = rd.IsDBNull(4) ? 26 : rd.GetInt32(4);
                room.humidSet = rd.IsDBNull(5) ? 50 : rd.GetInt32(5);
                room.temp = rd.IsDBNull(6) ? int.MinValue : rd.GetInt32(6);
                room.humid = rd.IsDBNull(7) ? int.MinValue : rd.GetInt32(7);
                room.dust = rd.IsDBNull(8) ? int.MinValue : rd.GetInt32(8);

                con.Close();
                return room;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //객실 전체 검색
        public List<Room> SelectAllRooms()
        {
            try
            {
                List<Room> customerList = new List<Room>();
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Room", con);
                SqlDataReader rd = cmd.ExecuteReader();
                Room room;

                while (rd.Read())
                {
                    //Room 레코드를 읽어들임
                    room = new Room();
                    room.rid = rd.GetInt32(0);
                    room.startDate = rd.IsDBNull(1) ? DateTime.MinValue : rd.GetDateTime(1);
                    room.endDate = rd.IsDBNull(2) ? DateTime.MinValue : rd.GetDateTime(2);
                    if (rd.IsDBNull(3)) { room.usable = 1; }
                    else { room.usable = rd.GetBoolean(3) ? (byte)1 : (byte)0; }
                    room.tempSet = rd.IsDBNull(4) ? 26 : rd.GetInt32(4);
                    room.humidSet = rd.IsDBNull(5) ? 50 : rd.GetInt32(5);
                    room.temp = rd.IsDBNull(6) ? int.MinValue : rd.GetInt32(6);
                    room.humid = rd.IsDBNull(7) ? int.MinValue : rd.GetInt32(7);
                    room.dust = rd.IsDBNull(8) ? int.MinValue : rd.GetInt32(8);

                    customerList.Add(room);
                }

                //DB닫기
                con.Close();
                //검색 결과가 없으면 null반환
                if (customerList.Count == 0) { return null; }

                return customerList;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //rid, nid로 예약 검색 [rid, nid]
        public Reservation SelectReservation(int rid, string nid)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Reservation WHERE rid = " + rid + " AND nid = '" + nid + "'", con);
                SqlDataReader rd = cmd.ExecuteReader();

                Reservation reserv = new Reservation();
                rd.Read();
                //검색결과가 없거나 Null이 포함된 경우
                if (rd.HasRows == false ||
                    rd.IsDBNull(0) == true || rd.IsDBNull(1) == true)
                {
                    //DB닫기
                    con.Close();
                    return null;
                }

                reserv.rid = rd.GetInt32(0);
                reserv.nid = rd.GetString(1);

                //DB닫기
                con.Close();
                return reserv;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //rid로 예약 전체 검색 [rid, nid]
        public List<Reservation> SelectAllReservationsByRID(int rid)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Reservation WHERE rid = " + rid, con);
                SqlDataReader rd = cmd.ExecuteReader();
                List<Reservation> reservList = new List<Reservation>();

                Reservation reserv;

                while (rd.Read())
                {
                    reserv = new Reservation();
                    //데이터가 Null값이면 -1, 아니면 데이터를 받아옴
                    reserv.rid = rd.IsDBNull(0) ? -1 : rd.GetInt32(0);
                    reserv.nid = rd.IsDBNull(1) ? null : rd.GetString(1);

                    //Null값인 경우 무시
                    if (reserv.rid < 0 || reserv.nid == null) { continue; }
                    reservList.Add(reserv);
                }
                //DB닫기
                con.Close();
                //레코드를 찾지 못한 경우 null반환
                if (reservList.Count == 0) { return null; }

                return reservList;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //rid로 예약 전체 검색 [rid, nid]
        public List<Reservation> SelectAllReservationsByNID(string nid)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Reservation WHERE nid = '" + nid + "'", con);
                SqlDataReader rd = cmd.ExecuteReader();
                List<Reservation> reservList = new List<Reservation>();
                Reservation reserv;

                while (rd.Read())
                {
                    reserv = new Reservation();
                    //데이터가 Null값이면 -1, 아니면 데이터를 받아옴
                    reserv.rid = rd.IsDBNull(0) ? -1 : rd.GetInt32(0);
                    reserv.nid = rd.IsDBNull(1) ? null : rd.GetString(1);

                    //Null값인 경우 무시
                    if (reserv.rid < 0 || reserv.nid == null) { continue; }
                    reservList.Add(reserv);
                }

                //DB닫기
                con.Close();
                //레코드를 찾지 못한 경우 null반환
                if (reservList.Count == 0) { return null; }

                return reservList;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //예약 전체 검색 [rid, nid]
        public List<Reservation> SelectAllReservations()
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Reservation", con);
                SqlDataReader rd = cmd.ExecuteReader();
                List<Reservation> reservList = new List<Reservation>();

                Reservation reserv;

                while (rd.Read())
                {

                    reserv = new Reservation();
                    //데이터가 Null값이면 -1, 아니면 데이터를 받아옴
                    reserv.rid = rd.IsDBNull(0) ? -1 : rd.GetInt32(0);
                    reserv.nid = rd.IsDBNull(1) ? null : rd.GetString(1);

                    //Null값인 경우 무시
                    if (reserv.rid < 0 || reserv.nid == null) { continue; }
                    reservList.Add(reserv);
                }

                //레코드를 찾지 못한 경우 null반환
                if (reservList.Count == 0)
                {
                    //DB닫기
                    con.Close();
                    return null;
                }

                //DB닫기
                con.Close();
                return reservList;
            }
            catch (Exception e)
            {
                con.Close();
                return null;
            }
        }

        //프로시저를 이용한 고객 추가
        public bool InsertCustomer(Customer customer)
        {
            try
            {
                //DB 열기
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Customer VALUES (", con);
                //Nid
                if (customer.nid != null && customer.nid.Length > 0)
                {
                    cmd.CommandText += " '" + customer.nid + "', ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Name
                if (customer.name != null && customer.name.Length > 0)
                {
                    cmd.CommandText += " '" + customer.name + "', ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //PhoneNumber
                if (customer.phone != null && customer.phone.Length > 0)
                {
                    cmd.CommandText += " '" + customer.phone + "', ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Age
                if (customer.age >= 0)
                {
                    cmd.CommandText += customer.age + ", ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Address
                if (customer.address != null && customer.address.Length > 0)
                {
                    cmd.CommandText += " '" + customer.address + "'";
                }
                else
                {
                    cmd.CommandText += "NULL";
                }

                //실행
                cmd.CommandText += ")";
                cmd.ExecuteNonQuery();
                //DB닫기
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                con.Close();
                return false;
            }
        }

        //프로시저를 이용한 객실 추가
        public bool InsertRoom(Room room)
        {
            try
            {
                //DB 열기
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Room VALUES (");
                //Rid
                cmd.CommandText += room.rid + ", ";
                //StartDate
                if (room.startDate.Year > 2000)
                {
                    cmd.CommandText += " '" + room.startDate.ToString("MM/dd/yyyy HH:mm:ss") + "', ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //EndDate
                if (room.endDate.Year > 2000)
                {
                    cmd.CommandText += " '" + room.endDate.ToString("MM/dd/yyyy HH:mm:ss") + "', ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Usable
                if (room.usable == 0) { cmd.CommandText += "0, "; }
                else { cmd.CommandText += "1, "; }
                //TempSet
                if (room.tempSet >= 0)
                {
                    cmd.CommandText += room.tempSet + ", ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //HumidSet
                if (room.humidSet >= 0)
                {
                    cmd.CommandText += room.humidSet + ", ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Temp
                if (room.temp >= 0)
                {
                    cmd.CommandText += room.temp + ", ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Humid
                if (room.humid >= 0)
                {
                    cmd.CommandText += room.humid + ", ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Dust
                if (room.dust >= 0)
                {
                    cmd.CommandText += room.dust;
                }
                else
                {
                    cmd.CommandText += "NULL";
                }

                //실행
                cmd.CommandText += ")";
                cmd.ExecuteNonQuery();
                //DB닫기
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                con.Close();
                return false;
            }
        }

        //프로시저를 이용한 예약 추가
        public bool InsertReservation(int rid, string nid)
        {
            try
            {
                //DB 열기
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Reservation VALUES (", con);
                //Rid
                if (rid >= 0)
                {
                    cmd.CommandText += rid + ", ";
                }
                else
                {
                    cmd.CommandText += "NULL, ";
                }
                //Nid
                if (nid != null && nid.Length > 0)
                {
                    cmd.CommandText += " '" + nid + "' ";
                }
                else
                {
                    cmd.CommandText += "NULL";
                }

                //실행
                cmd.CommandText += ")";
                cmd.ExecuteNonQuery();
                //DB닫기
                con.Close();
                return true;
            }
            catch (Exception e)
            {
                con.Close();
                return false;
            }
        }

        //고객 정보 수정
        public bool UpdateCustomer(Customer customer)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Customer SET ", con);
                if (customer.name != null && customer.name.Length > 0)
                {
                    cmd.CommandText += "name = '" + customer.name + "',";
                }
                if (customer.phone != null && customer.phone.Length > 0)
                {
                    cmd.CommandText += "phonenumber = '" + customer.phone + "',";
                }
                if (customer.age >= 0)
                {
                    cmd.CommandText += "age = " + customer.age + ",";
                }
                if (customer.address != null && customer.address.Length > 0)
                {
                    cmd.CommandText += "address = '" + customer.address + "',";
                }

                //마지막 쉼표 제거
                if (cmd.CommandText.EndsWith(","))
                {
                    cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 1, 1);
                }
                //WHERE절 추가
                cmd.CommandText += "WHERE nid = '" + customer.nid + "'";
                //쿼리문 실행
                int count = cmd.ExecuteNonQuery();
                //DB닫기
                con.Close();

                if (count > 0) { return true; } //성공(갱신된 레코드가 1개 이상
                else { return false; }  //실패
            }
            catch (Exception e)
            {
                con.Close();
                return false;
            }
        }

        //객실 정보 수정
        public bool UpdateRoom(Room room)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Room SET ", con);
                if (room.startDate.Year > 2000)
                {
                    cmd.CommandText += "startdate = '" + room.startDate.ToString("MM/dd/yyyy HH:mm:ss") + "',";
                }
                if (room.endDate.Year > 2000)
                {
                    cmd.CommandText += "enddate = '" + room.endDate.ToString("MM/dd/yyyy HH:mm:ss") + "',";
                }
                if (room.usable == 0) { cmd.CommandText += "usable = 0,"; }
                else if (room.usable == 1) { cmd.CommandText += "usable = 1,"; }
                if (room.tempSet >= 0)
                {
                    cmd.CommandText += "tempset = " + room.tempSet + ",";
                }
                if (room.humidSet >= 0)
                {
                    cmd.CommandText += "humidset = " + room.humidSet + ",";
                }
                if (room.temp >= -273)
                {
                    cmd.CommandText += "temp = " + room.temp + ",";
                }
                if (room.humid >= 0)
                {
                    cmd.CommandText += "humid = " + room.humid + ",";
                }
                if (room.dust >= 0)
                {
                    cmd.CommandText += "dust = " + room.dust + ",";
                }

                //마지막 쉼표 제거
                if (cmd.CommandText.EndsWith(","))
                {
                    cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 1, 1);
                }
                //WHERE절 추가
                cmd.CommandText += "WHERE rid = " + room.rid;
                //쿼리문 실행
                int count = cmd.ExecuteNonQuery();
                //DB닫기
                con.Close();

                if (count > 0) { return true; } //성공(갱신된 레코드가 1개 이상
                else { return false; }  //실패
            }
            catch (Exception e)
            {
                con.Close();
                return false;
            }
        }
        //tempset만 수정
        public bool UpdateRoom(int nRID, int nTempSet)
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Room SET ", con);
                
                if (nTempSet >= 0)
                {
                    cmd.CommandText += "tempset = " + nTempSet;
                }

                //WHERE절 추가
                cmd.CommandText += "WHERE rid = " + nRID;
                //쿼리문 실행
                int count = cmd.ExecuteNonQuery();
                //DB닫기
                con.Close();

                if (count > 0) { return true; } //성공(갱신된 레코드가 1개 이상
                else { return false; }  //실패
            }
            catch (Exception e)
            {
                con.Close();
                return false;
            }
        }

        //고객 정보 삭제
        public bool DeleteCustomer(string nid)
        {
            {
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand($"DELETE FROM Customer WHERE nid = '{ nid }'", con);
                    //쿼리문 실행
                    int count = cmd.ExecuteNonQuery();
                    //DB닫기
                    con.Close();

                    if (count > 0) { return true; } //성공(갱신된 레코드가 1개 이상
                    else { return false; }  //실패
                }
                catch (Exception e)
                {
                    con.Close();
                    return false;
                }
            }
        }
    }
}
