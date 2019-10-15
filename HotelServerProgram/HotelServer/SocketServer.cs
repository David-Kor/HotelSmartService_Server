/*
 *  2019.06.10 1학기 프로토타입은 프런트 PC가 1대임을 가정하고 임시로 구현
 *  완성본을 구현할 경우 다수의 프런트PC를 대상으로 모두 따로 통신 가능하도록 해야함.
 *  아두이노도 마찬가지
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HotelServer
{
    class SocketServer
    {
        private System.Windows.Forms.RichTextBox txtMsgBox;
        private Socket sock;
        private Socket front;
        private LinkedList<MySocket> clientLList;
        private bool isClosed;

        private class MySocket
        {
            public Socket socket;
            public byte type;       //0 : UnKnown, 1 : 어플리케이션, 2 : 아두이노, 3 : PC, 4 : CHAPP
            public string ip;
            public int rid = -1;
            public string nid = "";
        }

        public SocketServer(Server _form)
        {
            isClosed = false;
            front = null;
            txtMsgBox = _form.GetRichTextBox();
            clientLList = new LinkedList<MySocket>();
            Thread listenThread = new Thread(new ThreadStart(Listen)); // 서버 시작
            listenThread.Start();
            new Thread(new ThreadStart(InfoRefrash)).Start();
        }


        //프로그램의 창이 닫힐 때 호출(Form1_FormClosed에서)
        public void AllSockClose()
        {
            try
            {
                PrintMessage("=====Server START CLOSING=====");
                isClosed = true;
                //listen용 소켓 닫기
                sock.Close();
                LinkedListNode<MySocket> sockNode = clientLList.First;
                //accept된 소켓들 닫기
                if (sockNode != null)
                {
                    if (sockNode.Value != null) { sockNode.Value.socket.Close(); }

                    while (sockNode.Next != null)
                    {
                        sockNode = sockNode.Next;
                        if (sockNode.Value != null) { sockNode.Value.socket.Close(); }
                    }
                }
            }
            catch (Exception e) { PrintMessage(e.Message); }
            finally { PrintMessage("=====Server CLOSED====="); }
        }


        private void Listen() // 클라이언트와 연결하기
        {
            PrintMessage("=====Server START=====");
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 14334);
                sock.Bind(ep);   //Bind 단계
                sock.Listen(5);   //Listen 단계
                MySocket newSocket;
                while (isClosed == false)
                {
                    newSocket = new MySocket();
                    newSocket.socket = sock.Accept();
                    clientLList.AddLast(newSocket);     //클라이언트 연결 수락하고 해당 클라이언트의 소켓을 리스트에 저장
                    PrintMessage("새로운 클라이언트가 접속했습니다." + "(현재 : " + clientLList.Count + ")");
                    Thread acceptThread = new Thread(new ParameterizedThreadStart(AcceptClient));
                    acceptThread.Start(newSocket);
                }
            }
            catch (Exception e)
            {
                PrintMessage(e.Message);
            }
            finally
            {
                sock.Close();
            }
        }


        private void Listen2()
        {
            PrintMessage("=====Server START=====");
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 14334);
                sock.Bind(ep);   //Bind 단계
                sock.Listen(0);   //Listen 단계
                MySocket newSocket;
                while (isClosed == false)
                {
                    newSocket = new MySocket();
                    newSocket.socket = sock.Accept();
                    clientLList.AddLast(newSocket);     //클라이언트 연결 수락하고 해당 클라이언트의 소켓을 리스트에 저장
                    PrintMessage("새로운 클라이언트가 접속했습니다." + "(현재 : " + clientLList.Count + ")");
                    Thread acceptThread = new Thread(new ParameterizedThreadStart(AcceptClient));
                    acceptThread.Start(newSocket);
                }
            }
            catch (Exception e)
            {
                PrintMessage(e.Message);
            }
            finally
            {
                sock.Close();
            }
        }


        private void InfoRefrash()
        {
            try
            {
                LinkedListNode<MySocket> node = null;
                while (isClosed == false)
                {
                    //객실 아두이노에게 센서 작동명령 전달
                    node = clientLList.First;
                    while (node != null)
                    {
                        if (node.Value != null && node.Value.type == 2)
                        {
                                SendMessage(node.Value.socket, "ROOMINFO");
                        }
                        node = node.Next;
                    }

                    //15초마다 갱신
                    Thread.Sleep(15000);
                }
            }
            catch (Exception e)
            {
                if (isClosed == false)
                {
                    new Thread(new ThreadStart(InfoRefrash)).Start();
                }
            }
        }


        //Accept한 클라이언트와의 통신
        private void AcceptClient(object obj_mySock)
        {
            DBManager db = new DBManager();
            MySocket mySocket = obj_mySock as MySocket;
            Socket clientSock = mySocket.socket;     //현재 스레드의 통신 대상 클라이언트 소켓
            mySocket.ip = (mySocket.socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            mySocket.type = 0;
            byte[] buff = new byte[1024];   //수신 버퍼
            int rcvSize;       //수신 메시지 사이즈
            string rcvMsg, sndMsg;    //수신 메시지, 송신 메시지

            bool isSuccess = false;     //명령 처리 성공여부

            //연결 알림 로그
            PrintMessage("Client IP : " + mySocket.ip);

            try
            {
                while (isClosed == false)
                {
                    if (clientSock.Connected)
                    {
                        //명령어 수신
                        rcvSize = clientSock.Receive(buff);
                    }
                    else { break; }
                    //빈문장이 온 경우
                    if (rcvSize == 0)
                    {
                        //SendMessage(clientSock, "Error : You sended NULL_Message");
                        continue;
                    }
                    rcvMsg = Encoding.UTF8.GetString(buff).TrimEnd('\0');
                    Array.Clear(buff, 0, 1024);

                    //클라이언트 타입 지정
                    if (mySocket.type == 0)
                    {
                        if (rcvMsg.StartsWith("APP"))
                        {
                            LinkedListNode<MySocket> node = clientLList.First;
                            while (node != null)
                            {
                                if (node.Value.type == mySocket.type && node.Value.rid == mySocket.rid && node.Value != mySocket)
                                {
                                    node.Value.socket.Close();
                                    clientLList.Remove(node);
                                    node = clientLList.First;
                                    continue;
                                }
                                node = node.Next;
                            }
                            LinkedListNode<MySocket> ms_node = clientLList.First;
                            while (ms_node != null)
                            {
                                if (ms_node.Value.ip == mySocket.ip && ms_node.Value != mySocket)
                                {
                                    ms_node.Value.socket.Close();
                                    clientLList.Remove(ms_node);
                                }
                                ms_node = ms_node.Next;
                            }
                            //////////
                            mySocket.type = 1;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : Application");
                            mySocket.nid = rcvMsg.Remove(0, "APP".Length);
                            PrintMessage("nid : " + mySocket.nid);
                        }
                        else if (rcvMsg.StartsWith("ARDUINO"))
                        {
                            mySocket.type = 2;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : Arduino");
                            rcvMsg = rcvMsg.Remove(0, "ARDUINO".Length);
                            mySocket.rid = int.Parse(rcvMsg);
                            sndMsg = "RoomInfo";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                            LinkedListNode<MySocket> node = clientLList.First;

                            while (node != null)
                            {
                                if (node.Value.type == mySocket.type && node.Value.rid == mySocket.rid && node.Value != mySocket)
                                {
                                    node.Value.socket.Close();
                                    clientLList.Remove(node);
                                    node = clientLList.First;
                                    continue;
                                }
                                node = node.Next;
                            }
                        }
                        else if (rcvMsg.StartsWith("PC"))
                        {
                            mySocket.type = 3;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : PC");
                            front = clientSock;
                            sndMsg = "Type : PC";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        else if (rcvMsg.StartsWith("CHAPP"))
                        {/*
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : CHAPP");
                            sndMsg = "Type : CHAPP";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                            LinkedListNode<MySocket> node = clientLList.First;

                            while (node != null)
                            {
                                if (node.Value.type == mySocket.type && node.Value.rid == mySocket.rid && node.Value != mySocket)
                                {
                                    node.Value.socket.Close();
                                    clientLList.Remove(node);
                                    node = clientLList.First;
                                    continue;
                                }
                                node = node.Next;
                            }
                            CallChatting(mySocket);*/
                        }
                        else
                        {
                            mySocket.type = 0;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : ???");
                            sndMsg = "알 수 없는 타입";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        continue;
                    }
                    //어플리케이션 통신
                    else if (mySocket.type == 1)
                    {
                        //객실 정보 요청 수신
                        if (rcvMsg == "RoomInfo")
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 객실 정보 전송요청");
                            //DB에서 nid로 Reservation테이블를 검색해 예약한 방을 알아냄
                            List<Reservation> rRecord = db.SelectAllReservationsByNID(mySocket.nid);
                            if (rRecord == null)
                            {
                                //예약한 방이 없는 경우
                                sndMsg = "FAIL";
                                SendMessage(clientSock, sndMsg);
                                PrintMessage("nid : " + mySocket.nid + " -> Reservation 탐색 실패");
                                continue;
                            }
                            //알아낸 방에 대한 정보를 검색 --- 나중에 for문으로 바꿔서 구현할 것
                            Room myRoom = db.SelectRoomByRID(rRecord[0].rid);
                            if (myRoom == null)
                            {
                                //탐색 실패 시
                                sndMsg = "FAIL";
                                SendMessage(clientSock, sndMsg);
                                PrintMessage("rid : " + rRecord[0].rid + " -> Room 탐색 실패");
                            }
                            else
                            {
                                //양식 : "온도\n습도\n먼지농도" + 호실 + 기기 ON/OFF 현황
                                sndMsg = myRoom.temp + "\n" + myRoom.humid + "\n" + myRoom.dust;
                                SendMessage(clientSock, sndMsg);    //저장된 센서 감지값 전송
                            }
                        }
                        //전등 켜기 요청 수신
                        else if (rcvMsg.StartsWith("LampOn"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등 켜기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg.Remove(0, "LampOn".Length), true);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }

                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        //전등 끄기 요청 수신
                        else if (rcvMsg.StartsWith("LampOff"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등 끄기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg.Remove(0, "LampOff".Length), false);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }

                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        else if (rcvMsg == "ALLON")
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등켜기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg, true);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }
                            SendMessage(clientSock, sndMsg);
                        }
                        else if (rcvMsg == "ALLOFF")
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등끄기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg, false);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }
                            SendMessage(clientSock, sndMsg);
                        }
                        //온도 조절 요청 수신
                        else if (rcvMsg.StartsWith("Temp"))
                        {
                            int tempValue = int.Parse(rcvMsg.Split('_')[1]);    //지정 온도 읽기
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 희망온도 변경 : " + tempValue);

                            //nid로 예약 테이블 검색하여 rid를 탐색
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패 시
                            if (reserv == null)
                            {
                                sndMsg = "FAIL";
                            }
                            //탐색 성공 시
                            else
                            {
                                //rid로 객실 정보 검색
                                Room room = db.SelectRoomByRID(reserv[0].rid);
                                //객실 검색 실패 시
                                if (room == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                                    continue;
                                }
                                //희망온도 갱신
                                room.tempSet = tempValue;
                                if (db.UpdateRoom(room))
                                {
                                    sndMsg = "SUCCESS";
                                }
                                //갱신 실패 시
                                else
                                {
                                    sndMsg = "FAIL";
                                }
                                try
                                {
                                    //객실 아두이노에게 센서 작동명령 전달
                                    LinkedListNode<MySocket> node = clientLList.First;
                                    while (node != null)
                                    {
                                        if (node.Value.rid == room.rid)
                                        {
                                            string cmd = "COOLING";
                                            cmd += tempValue;
                                            PrintMessage(room.rid + "호에 전송됨 : " + cmd);
                                            SendMessage(node.Value.socket, cmd);
                                            break;
                                        }
                                        node = node.Next;
                                    }
                                }
                                catch (Exception e) { }
                            }
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        //습도 조절 요청 수신
                        else if (rcvMsg.StartsWith("Humi"))
                        {
                            int humidValue = int.Parse(rcvMsg.Split('_')[1]);    //지정 습도 읽기
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 희망습도 변경 : " + humidValue);

                            //nid로 예약 테이블 검색하여 rid를 탐색
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패 시
                            if (reserv == null)
                            {
                                sndMsg = "FAIL";
                            }
                            //탐색 성공 시
                            else
                            {
                                //rid로 객실 정보 검색
                                Room room = db.SelectRoomByRID(reserv[0].rid);
                                //객실 검색 실패 시
                                if (room == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                                    continue;
                                }
                                //희망온도 갱신
                                room.humidSet = humidValue;
                                if (db.UpdateRoom(room))
                                {
                                    sndMsg = "SUCCESS";
                                }
                                //갱신 실패 시
                                else
                                {
                                    sndMsg = "FAIL";
                                }
                                try
                                {
                                    //객실 아두이노에게 센서 작동명령 전달
                                    LinkedListNode<MySocket> node = clientLList.First;
                                    while (node != null)
                                    {
                                        if (node.Value.rid == room.rid)
                                        {
                                            string cmd = "VENTIL";
                                            cmd += humidValue;
                                            PrintMessage(room.rid + "호에 전송됨 : " + cmd);
                                            SendMessage(node.Value.socket, cmd);
                                            break;
                                        }
                                        node = node.Next;
                                    }
                                }
                                catch (Exception e) { }
                            }
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        else if (rcvMsg.StartsWith("CHAT\n"))
                        {
                            /* 여기도 나중에 다시 구현할 것 */
                            if (front == null) { continue; }
                            CallChatting(mySocket);
                            sndMsg = "CHATFROM" + mySocket.ip.ToString() + "\n";
                            sndMsg += rcvMsg.Split('\n')[1];
                            SendMessage(front, sndMsg);    //채팅 내용 전송
                        }
                        else
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            sndMsg = "알 수 없는 명령어를 수신";
                            SendMessage(clientSock, sndMsg);    //메시지 전송
                        }
                    }
                    //아두이노 통신
                    else if (mySocket.type == 2)
                    {
                        //센서값 수신
                        if (rcvMsg.StartsWith("SensorValues"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            rcvMsg.Replace(" ", "");
                            string[] values = rcvMsg.Remove(0, "SensorValues".Length + 1).Split('\n');
                            float temp = float.Parse(values[0]);
                            float humid = float.Parse(values[1]);
                            float dust = float.Parse(values[2]);

                            Room newRoom = new Room(mySocket.rid, new DateTime(), new DateTime(), 1, -1, -1, (int)temp, (int)humid, (int)dust);
                            isSuccess = db.UpdateRoom(newRoom);
                            if (isSuccess)
                            {
                                PrintMessage("온도 : " + temp + ", \t습도 : " + humid + ", \t먼지농도 : " + dust);
                                sndMsg = "SUCCESS";
                            }
                            else
                            {
                                PrintMessage("[" + mySocket.rid + "]호 객실 정보 갱신 실패");
                                sndMsg = "FAIL";
                            }
                            SendMessage(clientSock, sndMsg);    //완료 메시지 전송
                        }
                        else if (rcvMsg.StartsWith("NFCID"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);

                            string nid = "" + rcvMsg.Remove(0, "NFCID".Length+1).Replace(" ", "");
                            nid = nid.Replace(((char)13).ToString(), "");
                            PrintMessage("Search NID : " + nid);
                            //nid로 예약 테이블 검색
                            List<Reservation> reserv = db.SelectAllReservationsByNID(nid);
                            //검색 실패 시
                            if (reserv == null)
                            {
                                isSuccess = false;
                            }
                            //검색 완료 시
                            else
                            {
                                isSuccess = false;
                                for (int i = 0; i < reserv.Count; i++)
                                {
                                    //검색된 nid와 전송된 nid가 일치한 경우
                                    if (reserv[i].rid == mySocket.rid)
                                    {
                                        isSuccess = true;
                                        break;
                                    }
                                }
                            }
                            if (isSuccess) { sndMsg = "ALLOWNID"; }
                            else { sndMsg = "FAIL"; }
                            SendMessage(clientSock, sndMsg);
                        }
                        else
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            sndMsg = "알 수 없는 명령어를 수신";
                            SendMessage(clientSock, sndMsg);    //에러 메시지 전송
                        }
                    }
                    //프런트와의 통신
                    else if (mySocket.type == 3)
                    {
                        if (rcvMsg == "OK") { continue; }
                        else if (rcvMsg.StartsWith("CHATTO"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string targetIP = rcvMsg.Split('\n')[0].Remove(0, "CHATTO".Length);
                            LinkedListNode<MySocket> node = clientLList.First;
                            while (node != null)
                            {
                                if (node.Value != null && node.Value.ip == targetIP && node.Value.type == 1)
                                {
                                    sndMsg = "CHAT\n";
                                    sndMsg += rcvMsg.Split('\n')[1];
                                    SendMessage(node.Value.socket, sndMsg);    //결과 메시지 전송
                                    break;
                                }
                                node = node.Next;
                            }
                        }
                        else if (rcvMsg.StartsWith("GET"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);

                            string location = rcvMsg.Split('_')[1];
                            if (location == "CUSTOMER")
                            {
                                List<Customer> customerList = db.SelectAllCustomers();
                                List<Reservation> reserv = null;

                                if (customerList == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);
                                }
                                else
                                {
                                    string info;
                                    for (int i = 0; i < customerList.Count; i++)
                                    {
                                        info = "CUSTOMERINFO\n" + customerList[i].nid + "\n" + customerList[i].name + "\n" + customerList[i].phone + "\n" + customerList[i].age + "\n" + customerList[i].address + "\n";
                                        reserv = db.SelectAllReservationsByNID(customerList[i].nid);
                                        if (reserv == null) { continue; }
                                        for (int j = 0; j < reserv.Count; j++)
                                        {
                                            info += reserv[j].rid + ", ";
                                        }
                                        info = info.Remove(info.Length - 2, 2);
                                        int msgSize = Encoding.UTF8.GetByteCount(info);
                                        SendMessage(clientSock, "SIZEOF" + msgSize);
                                        SendMessage(clientSock, info);
                                    }
                                }
                                SendMessage(clientSock,"SUCCESS");
                            }
                            else if (location == "ROOM")
                            {
                                List<Room> roomList = db.SelectAllRooms();
                                if (roomList == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);
                                }
                                else
                                {
                                    string info;
                                    for (int i = 0; i < roomList.Count; i++)
                                    {
                                        info = "ROOMINFO\n" + roomList[i].rid + "\n" + roomList[i].startDate + "\n" + roomList[i].endDate + "\n" + roomList[i].usable + "\n" + roomList[i].temp + "\n" + roomList[i].humid + "\n" + roomList[i].dust;
                                        SendMessage(clientSock, info);
                                        SendMessage(clientSock, "");
                                    }
                                }
                            }
                        }
                        else if (rcvMsg.StartsWith("UPDATE"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string[] customerInfo = rcvMsg.Remove(0, "UPDATE".Length + 1).Split('\n');
                            int i = 0;
                            Customer customer = new Customer();
                            customer.nid = customerInfo[i++];
                            customer.name = customerInfo[i++];
                            customer.phone = customerInfo[i++];
                            customer.age = int.Parse(customerInfo[i++]);
                            customer.address = customerInfo[i++];
                            if (db.UpdateCustomer(customer))
                            {
                                sndMsg = "UPDATESUCCESS";
                                SendMessage(clientSock, sndMsg);
                            }
                            else
                            {
                                sndMsg = "UPATEFAIL";
                                SendMessage(clientSock, sndMsg);
                            }
                        }
                        else if (rcvMsg.StartsWith("INSERT"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string[] customerInfo = rcvMsg.Remove(0, "INSERT".Length + 1).Split('\n');
                            int i = 0;
                            Customer customer = new Customer();
                            customer.nid = customerInfo[i++];
                            customer.name = customerInfo[i++];
                            customer.phone = customerInfo[i++];
                            customer.age = int.Parse(customerInfo[i++]);
                            customer.address = customerInfo[i++];
                            Reservation reserv = new Reservation(int.Parse(customerInfo[i]), customer.nid);
                            if (db.InsertCustomer(customer))
                            {
                                if (db.InsertReservation(reserv.rid, reserv.nid))
                                {
                                    sndMsg = "INSERTSUCCESS";
                                    SendMessage(clientSock, sndMsg);
                                }
                                else
                                {
                                    db.DeleteCustomer(reserv.nid);
                                    sndMsg = "INSERTFAIL";
                                    SendMessage(clientSock, sndMsg);
                                }
                            }
                            else
                            {
                                sndMsg = "INSERTFAIL";
                                SendMessage(clientSock, sndMsg);
                            }
                        }
                        else if (rcvMsg.StartsWith("DELETE"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string delNID = rcvMsg.Remove(0, "DELETE".Length + 1);
                            if (db.DeleteCustomer(delNID))
                            {
                                sndMsg = "DELETESUCCESS";
                                SendMessage(clientSock, sndMsg);
                            }
                            else
                            {
                                sndMsg = "DELETEFAIL";
                                SendMessage(clientSock, sndMsg);
                            }
                        }
                        else
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            sndMsg = "알 수 없는 명령어를 수신";
                            SendMessage(clientSock, sndMsg);    //에러 메시지 전송
                        }
                    }
                    //어플 -> 프런트 채팅 통신
                    else if (mySocket.type == 4)
                    {
                    }
                    else
                    {
                        clientSock.Close();
                        clientLList.Remove(mySocket);
                        PrintMessage("Error : Unknown Type Client\n 연결을 종료합니다.");
                        return;
                    }
                }//while문 끝
            }//try문 끝
            catch (Exception e)
            {
                PrintMessage(e.Message);
                if (clientSock.Connected) { SendMessage(clientSock, "FAIL"); }
            }
            finally
            {
                clientSock.Close();
                clientLList.Remove(mySocket);
                if (mySocket.type == 4)
                {
                    //프런트에 명령어 전달
                    sndMsg = "CLOSECHAT" + mySocket.ip;
                    SendMessage(front, sndMsg);    //메시지 전송
                }
                PrintMessage("클라이언트( " + mySocket.ip.ToString() + " )연결 종료" + "(현재 연결수 : " + clientLList.Count + ")");
            }
        }


        private void Test(object obj_mySock)
        {
            DBManager db = new DBManager();
            MySocket mySocket = obj_mySock as MySocket;
            Socket clientSock = mySocket.socket;     //현재 스레드의 통신 대상 클라이언트 소켓
            mySocket.ip = (mySocket.socket.RemoteEndPoint as IPEndPoint).Address.ToString();
            mySocket.type = 0;
            byte[] buff = new byte[1024];   //수신 버퍼
            int rcvSize;       //수신 메시지 사이즈
            string rcvMsg, sndMsg;    //수신 메시지, 송신 메시지

            bool isSuccess = false;     //명령 처리 성공여부

            //연결 알림 로그
            PrintMessage("Client IP : " + mySocket.ip);

            try
            {
                while (isClosed == false)
                {
                    if (clientSock.Connected)
                    {
                        //명령어 수신
                        rcvSize = clientSock.Receive(buff);
                    }
                    else { break; }
                    //빈문장이 온 경우
                    if (rcvSize == 0)
                    {
                        //SendMessage(clientSock, "Error : You sended NULL_Message");
                        continue;
                    }
                    rcvMsg = Encoding.UTF8.GetString(buff).TrimEnd('\0');
                    Array.Clear(buff, 0, 1024);

                    //클라이언트 타입 지정
                    if (mySocket.type == 0)
                    {
                        if (rcvMsg.StartsWith("APP"))
                        {
                            mySocket.type = 1;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : Application");
                            sndMsg = "Type : Application";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                            mySocket.nid = rcvMsg.Remove(0, "APP".Length);
                            PrintMessage("nid : " + mySocket.nid);
                        }
                        else if (rcvMsg.StartsWith("ARDUINO"))
                        {
                            mySocket.type = 2;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : Arduino");
                            rcvMsg = rcvMsg.Remove(0, "ARDUINO".Length);
                            mySocket.rid = int.Parse(rcvMsg);
                            sndMsg = "RoomInfo";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                            LinkedListNode<MySocket> node = clientLList.First;

                            while (node != null)
                            {
                                if (node.Value.type == mySocket.type && node.Value.rid == mySocket.rid && node.Value != mySocket)
                                {
                                    node.Value.socket.Close();
                                    clientLList.Remove(node);
                                    node = clientLList.First;
                                    continue;
                                }
                                node = node.Next;
                            }
                        }
                        else if (rcvMsg.StartsWith("PC"))
                        {
                            mySocket.type = 3;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : PC");
                            front = clientSock;
                            sndMsg = "Type : PC";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        else if (rcvMsg.StartsWith("CHAPP"))
                        {
                            mySocket.type = 4;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : CHAPP");
                            sndMsg = "Type : CHAPP";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                            LinkedListNode<MySocket> node = clientLList.First;

                            while (node != null)
                            {
                                if (node.Value.type == mySocket.type && node.Value.rid == mySocket.rid && node.Value != mySocket)
                                {
                                    node.Value.socket.Close();
                                    clientLList.Remove(node);
                                    node = clientLList.First;
                                    continue;
                                }
                                node = node.Next;
                            }
                            CallChatting(mySocket);
                        }
                        else
                        {
                            mySocket.type = 0;
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " ->  ClientType : ???");
                            sndMsg = "알 수 없는 타입";
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        continue;
                    }
                    //어플리케이션 통신
                    else if (mySocket.type == 1)
                    {
                        //객실 정보 요청 수신
                        if (rcvMsg == "RoomInfo")
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 객실 정보 전송요청");
                            //DB에서 nid로 Reservation테이블를 검색해 예약한 방을 알아냄
                            List<Reservation> rRecord = db.SelectAllReservationsByNID(mySocket.nid);
                            if (rRecord == null)
                            {
                                //예약한 방이 없는 경우
                                sndMsg = "FAIL";
                                SendMessage(clientSock, sndMsg);
                                PrintMessage("nid : " + mySocket.nid + " -> Reservation 탐색 실패");
                                continue;
                            }
                            //알아낸 방에 대한 정보를 검색 --- 나중에 for문으로 바꿔서 구현할 것
                            Room myRoom = db.SelectRoomByRID(rRecord[0].rid);
                            if (myRoom == null)
                            {
                                //탐색 실패 시
                                sndMsg = "FAIL";
                                SendMessage(clientSock, sndMsg);
                                PrintMessage("rid : " + rRecord[0].rid + " -> Room 탐색 실패");
                            }
                            else
                            {
                                //양식 : "온도\n습도\n먼지농도" + 호실 + 기기 ON/OFF 현황
                                sndMsg = myRoom.temp + "\n" + myRoom.humid + "\n" + myRoom.dust;
                                SendMessage(clientSock, sndMsg);    //저장된 센서 감지값 전송
                            }
                        }
                        //전등 켜기 요청 수신
                        else if (rcvMsg.StartsWith("LampOn"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등 켜기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg.Remove(0, "LampOn".Length), true);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }

                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        //전등 끄기 요청 수신
                        else if (rcvMsg.StartsWith("LampOff"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등 끄기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg.Remove(0, "LampOff".Length), false);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }

                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        else if (rcvMsg == "ALLON")
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등켜기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg, true);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }
                            SendMessage(clientSock, sndMsg);
                        }
                        else if (rcvMsg == "ALLOFF")
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 전등끄기 요청");
                            //예약 테이블을 통해 rid를 구함
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패시
                            if (reserv == null) { sndMsg = "FAIL"; }
                            //탐색 성공시
                            else
                            {
                                isSuccess = LampOnOff(reserv[0].rid, rcvMsg, false);
                                if (isSuccess) { sndMsg = "SUCCESS"; }
                                else { sndMsg = "FAIL"; }
                            }
                            SendMessage(clientSock, sndMsg);
                        }
                        //온도 조절 요청 수신
                        else if (rcvMsg.StartsWith("Temp"))
                        {
                            int tempValue = int.Parse(rcvMsg.Split('_')[1]);    //지정 온도 읽기
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 희망온도 변경 : " + tempValue);

                            //nid로 예약 테이블 검색하여 rid를 탐색
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패 시
                            if (reserv == null)
                            {
                                sndMsg = "FAIL";
                            }
                            //탐색 성공 시
                            else
                            {
                                //rid로 객실 정보 검색
                                Room room = db.SelectRoomByRID(reserv[0].rid);
                                //객실 검색 실패 시
                                if (room == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                                    continue;
                                }
                                //희망온도 갱신
                                room.tempSet = tempValue;
                                if (db.UpdateRoom(room))
                                {
                                    sndMsg = "SUCCESS";
                                }
                                //갱신 실패 시
                                else
                                {
                                    sndMsg = "FAIL";
                                }
                                try
                                {
                                    //객실 아두이노에게 센서 작동명령 전달
                                    LinkedListNode<MySocket> node = clientLList.First;
                                    while (node != null)
                                    {
                                        if (node.Value.rid == room.rid)
                                        {
                                            string cmd = "COOLING";
                                            cmd += tempValue;
                                            PrintMessage(room.rid + "호에 전송됨 : " + cmd);
                                            SendMessage(node.Value.socket, cmd);
                                            break;
                                        }
                                        node = node.Next;
                                    }
                                }
                                catch (Exception e) { }
                            }
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        //습도 조절 요청 수신
                        else if (rcvMsg.StartsWith("Humi"))
                        {
                            int humidValue = int.Parse(rcvMsg.Split('_')[1]);    //지정 습도 읽기
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg + " -> 희망습도 변경 : " + humidValue);

                            //nid로 예약 테이블 검색하여 rid를 탐색
                            List<Reservation> reserv = db.SelectAllReservationsByNID(mySocket.nid);
                            //탐색 실패 시
                            if (reserv == null)
                            {
                                sndMsg = "FAIL";
                            }
                            //탐색 성공 시
                            else
                            {
                                //rid로 객실 정보 검색
                                Room room = db.SelectRoomByRID(reserv[0].rid);
                                //객실 검색 실패 시
                                if (room == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                                    continue;
                                }
                                //희망온도 갱신
                                room.humidSet = humidValue;
                                if (db.UpdateRoom(room))
                                {
                                    sndMsg = "SUCCESS";
                                }
                                //갱신 실패 시
                                else
                                {
                                    sndMsg = "FAIL";
                                }
                                try
                                {
                                    //객실 아두이노에게 센서 작동명령 전달
                                    LinkedListNode<MySocket> node = clientLList.First;
                                    while (node != null)
                                    {
                                        if (node.Value.rid == room.rid)
                                        {
                                            string cmd = "VENTIL";
                                            cmd += humidValue;
                                            PrintMessage(room.rid + "호에 전송됨 : " + cmd);
                                            SendMessage(node.Value.socket, cmd);
                                            break;
                                        }
                                        node = node.Next;
                                    }
                                }
                                catch (Exception e) { }
                            }
                            SendMessage(clientSock, sndMsg);    //응답 메시지 전송
                        }
                        else
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            sndMsg = "알 수 없는 명령어를 수신";
                            SendMessage(clientSock, sndMsg);    //메시지 전송
                        }
                    }
                    //아두이노 통신
                    else if (mySocket.type == 2)
                    {
                        //센서값 수신
                        if (rcvMsg.StartsWith("SensorValues"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            rcvMsg.Replace(" ", "");
                            string[] values = rcvMsg.Remove(0, "SensorValues".Length + 1).Split('\n');
                            float temp = float.Parse(values[0]);
                            float humid = float.Parse(values[1]);
                            float dust = float.Parse(values[2]);

                            Room newRoom = new Room(mySocket.rid, new DateTime(), new DateTime(), 1, -1, -1, (int)temp, (int)humid, (int)dust);
                            isSuccess = db.UpdateRoom(newRoom);
                            if (isSuccess)
                            {
                                PrintMessage("온도 : " + temp + ", \t습도 : " + humid + ", \t먼지농도 : " + dust);
                                sndMsg = "SUCCESS";
                            }
                            else
                            {
                                PrintMessage("[" + mySocket.rid + "]호 객실 정보 갱신 실패");
                                sndMsg = "FAIL";
                            }
                            SendMessage(clientSock, sndMsg);    //완료 메시지 전송
                        }
                        else if (rcvMsg.StartsWith("NFCID"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);

                            string nid = "" + rcvMsg.Remove(0, "NFCID".Length + 1).Replace(" ", "");
                            nid = nid.Replace(((char)13).ToString(), "");
                            PrintMessage("Search NID : " + nid);
                            //nid로 예약 테이블 검색
                            List<Reservation> reserv = db.SelectAllReservationsByNID(nid);
                            //검색 실패 시
                            if (reserv == null)
                            {
                                isSuccess = false;
                            }
                            //검색 완료 시
                            else
                            {
                                isSuccess = false;
                                for (int i = 0; i < reserv.Count; i++)
                                {
                                    //검색된 nid와 전송된 nid가 일치한 경우
                                    if (reserv[i].rid == mySocket.rid)
                                    {
                                        isSuccess = true;
                                        break;
                                    }
                                }
                            }
                            if (isSuccess) { sndMsg = "ALLOWNID"; }
                            else { sndMsg = "FAIL"; }
                            SendMessage(clientSock, sndMsg);
                        }
                        else
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            sndMsg = "알 수 없는 명령어를 수신";
                            SendMessage(clientSock, sndMsg);    //에러 메시지 전송
                        }
                    }
                    //프런트와의 통신
                    else if (mySocket.type == 3)
                    {
                        if (rcvMsg == "OK") { continue; }
                        else if (rcvMsg.StartsWith("CHATTO"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string targetIP = rcvMsg.Split('\n')[0].Remove(0, "CHATTO".Length);
                            LinkedListNode<MySocket> node = clientLList.First;
                            while (node != null)
                            {
                                if (node.Value != null && node.Value.ip == targetIP && node.Value.type == 4)
                                {
                                    sndMsg = "CHAT\n";
                                    sndMsg += rcvMsg.Split('\n')[1];
                                    SendMessage(node.Value.socket, sndMsg);    //결과 메시지 전송
                                    break;
                                }
                                node = node.Next;
                            }
                        }
                        else if (rcvMsg.StartsWith("GET"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);

                            string location = rcvMsg.Split('_')[1];
                            if (location == "CUSTOMER")
                            {
                                List<Customer> customerList = db.SelectAllCustomers();
                                List<Reservation> reserv = null;

                                if (customerList == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);
                                }
                                else
                                {
                                    string info;
                                    for (int i = 0; i < customerList.Count; i++)
                                    {
                                        info = "CUSTOMERINFO\n" + customerList[i].nid + "\n" + customerList[i].name + "\n" + customerList[i].phone + "\n" + customerList[i].age + "\n" + customerList[i].address + "\n";
                                        reserv = db.SelectAllReservationsByNID(customerList[i].nid);
                                        if (reserv == null) { continue; }
                                        for (int j = 0; j < reserv.Count; j++)
                                        {
                                            info += reserv[j].rid + ", ";
                                        }
                                        info = info.Remove(info.Length - 2, 2);
                                        int msgSize = Encoding.UTF8.GetByteCount(info);
                                        SendMessage(clientSock, "SIZEOF" + msgSize);
                                        SendMessage(clientSock, info);
                                    }
                                }
                                SendMessage(clientSock, "SUCCESS");
                            }
                            else if (location == "ROOM")
                            {
                                List<Room> roomList = db.SelectAllRooms();
                                if (roomList == null)
                                {
                                    sndMsg = "FAIL";
                                    SendMessage(clientSock, sndMsg);
                                }
                                else
                                {
                                    string info;
                                    for (int i = 0; i < roomList.Count; i++)
                                    {
                                        info = "ROOMINFO\n" + roomList[i].rid + "\n" + roomList[i].startDate + "\n" + roomList[i].endDate + "\n" + roomList[i].usable + "\n" + roomList[i].temp + "\n" + roomList[i].humid + "\n" + roomList[i].dust;
                                        SendMessage(clientSock, info);
                                        SendMessage(clientSock, "");
                                    }
                                }
                            }
                        }
                        else if (rcvMsg.StartsWith("UPDATE"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string[] customerInfo = rcvMsg.Remove(0, "UPDATE".Length + 1).Split('\n');
                            int i = 0;
                            Customer customer = new Customer();
                            customer.nid = customerInfo[i++];
                            customer.name = customerInfo[i++];
                            customer.phone = customerInfo[i++];
                            customer.age = int.Parse(customerInfo[i++]);
                            customer.address = customerInfo[i++];
                            if (db.UpdateCustomer(customer))
                            {
                                sndMsg = "UPDATESUCCESS";
                                SendMessage(clientSock, sndMsg);
                            }
                            else
                            {
                                sndMsg = "UPATEFAIL";
                                SendMessage(clientSock, sndMsg);
                            }
                        }
                        else if (rcvMsg.StartsWith("INSERT"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string[] customerInfo = rcvMsg.Remove(0, "INSERT".Length + 1).Split('\n');
                            int i = 0;
                            Customer customer = new Customer();
                            customer.nid = customerInfo[i++];
                            customer.name = customerInfo[i++];
                            customer.phone = customerInfo[i++];
                            customer.age = int.Parse(customerInfo[i++]);
                            customer.address = customerInfo[i++];
                            Reservation reserv = new Reservation(int.Parse(customerInfo[i]), customer.nid);
                            if (db.InsertCustomer(customer))
                            {
                                if (db.InsertReservation(reserv.rid, reserv.nid))
                                {
                                    sndMsg = "INSERTSUCCESS";
                                    SendMessage(clientSock, sndMsg);
                                }
                                else
                                {
                                    db.DeleteCustomer(reserv.nid);
                                    sndMsg = "INSERTFAIL";
                                    SendMessage(clientSock, sndMsg);
                                }
                            }
                            else
                            {
                                sndMsg = "INSERTFAIL";
                                SendMessage(clientSock, sndMsg);
                            }
                        }
                        else if (rcvMsg.StartsWith("DELETE"))
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            string delNID = rcvMsg.Remove(0, "DELETE".Length + 1);
                            if (db.DeleteCustomer(delNID))
                            {
                                sndMsg = "DELETESUCCESS";
                                SendMessage(clientSock, sndMsg);
                            }
                            else
                            {
                                sndMsg = "DELETEFAIL";
                                SendMessage(clientSock, sndMsg);
                            }
                        }
                        else
                        {
                            PrintMessage("(" + mySocket.ip.ToString() + ")  " + rcvMsg);
                            sndMsg = "알 수 없는 명령어를 수신";
                            SendMessage(clientSock, sndMsg);    //에러 메시지 전송
                        }
                    }
                    //어플 -> 프런트 채팅 통신
                    else if (mySocket.type == 4)
                    {
                        if (rcvMsg.StartsWith("CHAT\n"))
                        {
                            /* 여기도 나중에 다시 구현할 것 */
                            if (front == null) { continue; }

                            sndMsg = "CHATFROM" + mySocket.ip.ToString() + "\n";
                            sndMsg += rcvMsg.Split('\n')[1];
                            SendMessage(front, sndMsg);    //채팅 내용 전송
                        }
                    }
                    else
                    {
                        clientSock.Close();
                        clientLList.Remove(mySocket);
                        PrintMessage("Error : Unknown Type Client\n 연결을 종료합니다.");
                        return;
                    }
                }//while문 끝
            }//try문 끝
            catch (Exception e)
            {
                PrintMessage(e.Message);
                if (clientSock.Connected) { SendMessage(clientSock, "FAIL"); }
            }
            finally
            {
                clientSock.Close();
                clientLList.Remove(mySocket);
                if (mySocket.type == 4)
                {
                    //프런트에 명령어 전달
                    sndMsg = "CLOSECHAT" + mySocket.ip;
                    SendMessage(front, sndMsg);    //메시지 전송
                }
                PrintMessage("클라이언트( " + mySocket.ip.ToString() + " )연결 종료" + "(현재 연결수 : " + clientLList.Count + ")");
            }
        }


        private void SendMessage(Socket clientSock, string sndMsg)
        {
            try
            {
                if (clientSock != null && clientSock.Connected)
                {
                    clientSock.Send(Encoding.UTF8.GetBytes(sndMsg), SocketFlags.None);    //응답 메시지 송신
                    PrintMessage("전송됨 : " + sndMsg);   //송신한 메시지 출력
                }
            }
            catch (Exception e)
            {
                PrintMessage(e.Message);
                clientSock.Close();
            }
        }

        private void PrintMessage(string str)
        {
            str = "[" + DateTime.Now.ToString() + "]  \t" + str + Environment.NewLine;
            txtMsgBox.AppendText(str);
        }


        private bool LampOnOff(int rid, string location, bool isOn)
        {
            try
            {
                //rid로 소켓들 탐색
                LinkedListNode<MySocket> node = clientLList.First;
                while (node != null)
                {
                    if (node.Value.rid == rid) { break; }
                    node = node.Next;
                }
                //탐색 실패 시
                if (node == null)
                {
                    PrintMessage("RID : " + rid + " 인 객실의 소켓을 찾지 못했습니다.");
                    return false;
                }

                string sndMsg = "LAMP";
                if (isOn) { sndMsg += "ON"; }
                else { sndMsg += "OFF"; }

                if (location.StartsWith("ALL")) {
                    SendMessage(node.Value.socket, location);
                    return true;
                }
                else if (location.EndsWith("Room"))
                {
                    SendMessage(node.Value.socket, sndMsg + "_ROOM");
                    return true;
                }
                else if (location.EndsWith("Toilet"))
                {
                    SendMessage(node.Value.socket, sndMsg + "_TOILET");
                    return true;
                }
                else if (location.EndsWith("Living"))
                {
                    SendMessage(node.Value.socket, sndMsg + "_LIVING");
                    return true;
                }
                else
                {
                    PrintMessage("명령 전송 실패 : 알 수 없는 장소");
                    return false;
                }
            }
            catch (Exception e)
            {
                PrintMessage(e.Message);
                return false;
            }
        }


        private void CallChatting(MySocket mySocket)
        {
            try
            {
                /* 나중에 따로 다시 구현해야 함 */
                if (front == null)
                {
                    SendMessage(mySocket.socket, "FAIL");
                    return;
                }

                string sndMsg;
                byte[] buff = new byte[32];
                //프런트에 명령어 전달
                sndMsg = "CallChat" + mySocket.ip;
                SendMessage(front, sndMsg);
            }
            catch (Exception e)
            {
                PrintMessage(e.Message);
            }
        }
    }
}
