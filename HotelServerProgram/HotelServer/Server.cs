using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace HotelServer
{
    public partial class Server : Form
    {
        private Socket mv_sockServer;       //서버 메인 소켓
        private IPAddress mv_addrFront;    //호텔 프런트 IP주소객체
        private Dictionary<string, Socket> mv_dicClients;      //클라이언트 소켓들(key: 식별자(ip or pn), value: 소켓)
        private Dictionary<int, string> mv_dicRooms;          //객실 ip들(key: 객실번호[rid], value: ip주소)
        private DBManager mv_db;           //데이터베이스 클래스
        private bool mv_isProgramClose;    //프로그램 종료버튼이 눌림을 알림
        private Thread mv_listenThread;     //소켓 리스너 스레드
        private Timer mv_timer;        //객실 환경 정보 갱신타이머
        private Mutex mv_mutex;     //스레드 공유자원 보호 (뮤텍스)

        private const int MAX_TEXT_LINE = 512;      //최대 로그 출력 라인(RichTextBox 라인제한)
        private const int INTERVAL_ROOM_UPDATE = 60000;    //객실 정보 갱신 주기(단위 : ms)

        private const string STR_OK = "OK";
        private const string STR_FAIL = "FAIL";

        private enum ClientType { NONE, APP, ROOM, FRONT };

        //생성자
        public Server()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            mv_dicClients = new Dictionary<string, Socket>();
            mv_dicRooms = new Dictionary<int, string>();
            mv_db = new DBManager();
            mv_addrFront = null;
            mv_sockServer = null;
            mv_isProgramClose = false;
            mv_mutex = new Mutex();
        }

        //메인 윈도우 폼이 메모리에 적재될 때 호출
        private void Form1_Load(object sender, EventArgs e)
        {
            PrintMessage("Server Program Start.");
            mv_listenThread = new Thread(new ThreadStart(ServerSocketStart));
            mv_listenThread.Start();
        }

        //string <-> byte[] 변환 함수
        private object ConvertStringBytes(object objSource)
        {
            try
            {
                if (objSource.GetType() == typeof(byte[]))
                {
                    return Encoding.UTF8.GetString(objSource as byte[]).TrimEnd('\0');
                }
                else if (objSource.GetType() == typeof(string))
                {
                    return Encoding.UTF8.GetBytes(objSource as string);
                }
                //전혀 다른 타입인 경우
                else
                {
                    return null;
                }
            }
            //실패 시
            catch (Exception)
            {
                return null;
            }
        }

        //소켓 리스너 스레드 함수
        private void ServerSocketStart()
        {
            try
            {
                PrintMessage("Create New Socket.");
                mv_sockServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, 14334);   //모든 IP에 대하여 14334번 포트로 통신
                PrintMessage($"Local address and port : {ep.ToString()}.");
                PrintMessage("Bind Socket.");
                mv_sockServer.Bind(ep);   //Bind 단계
                PrintMessage("Listen and Loop Start.");
                mv_sockServer.Listen(0);   //Listen 단계
                //일정 시간이 지날 때마다 Timer_Elapsed함수를 호출
                mv_timer = new Timer(INTERVAL_ROOM_UPDATE);
                mv_timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
                mv_timer.Start();
                mv_timer.AutoReset = true;
                PrintMessage("Timer for update rooms start.");

                Socket newSock = null;
                Thread acceptThread = null;

                while (mv_isProgramClose == false)
                {
                    try
                    {
                        newSock = mv_sockServer.Accept();     //클라이언트 연결
                        PrintMessage($"Accept Client : {newSock.RemoteEndPoint.ToString()}");
                        //새 통신 스레드 시작
                        acceptThread = new Thread(new ParameterizedThreadStart(CommunicationToClient));
                        acceptThread.Start(newSock);
                    }
                    catch (Exception e)
                    {
                        PrintMessage($"Exception occurred in ServerSocketStart() while Accept() : {e.Message}");
                    }
                }
                PrintMessage("Loop End");
            }
            catch (Exception e)
            {
                PrintMessage($"Exception occurred in ServerSocketStart() : {e.Message}");
                if (mv_sockServer != null)
                {
                    PrintMessage("Close Socket.");
                    mv_sockServer.Close();
                }
            }
        }

        //클라이언트와의 통신 스레드 함수
        private void CommunicationToClient(object objClientSocket)
        {
            //매개변수가 null값이면 에러 메시지 출력 후 함수 종료
            if (objClientSocket == null)
            {
                PrintMessage("Error in CommunicationToClient() : obj_ClientSocket is null");
                return;
            }

            Socket sockClient = objClientSocket as Socket;
            string strClientID = (sockClient.RemoteEndPoint as IPEndPoint).Address.ToString();
            byte[] bBuffer = new byte[1024];    //최대 버퍼크기 1024Byte
            int nRcvSize;       //수신 메시지 크기
            string strRcv, strSnd;    //송수신 메시지
            ClientType clientType = ClientType.NONE;        //클라이언트 타입

            try
            {
                PrintMessage($"Add Client List");
                //클라이언트들 중에 동일 IP가 존재하면 기존에 있던 IP와의 통신을 끊고 Dictionary에서 제거
                if (mv_dicClients.ContainsKey(strClientID))
                {
                    mv_dicClients[strClientID].Close();
                    DictionaryClientRemove(strClientID);
                }
                //새로운 key,value를 추가
                DictionaryClientAdd(strClientID, sockClient);
                PrintMessage($"Client Count : {mv_dicClients.Count}");
                //통신 시작
                while (true)
                {
                    if (sockClient == null || sockClient.Connected == false) { break; }
                    //클라이언트 메시지 수신
                    nRcvSize = sockClient.Receive(bBuffer);

                    PrintMessage($"Receive {nRcvSize} Byte From {(sockClient.RemoteEndPoint as IPEndPoint).Address}");
                    //사이즈가 0이하인 경우
                    if (nRcvSize <= 0)
                    {
                        sockClient.Close();
                        DictionaryClientRemove(strClientID);
                        PrintMessage($"Closed socket : {sockClient.RemoteEndPoint.ToString()}");
                        break;
                    }
                    //버퍼의 내용을 string으로 변환
                    strRcv = ConvertStringBytes(bBuffer) as string;
                    //결과값이 null이면 무시
                    if (strRcv == null)
                    {
                        PrintMessage("(Warning) strRcv is null.");
                        continue;
                    }

                    PrintMessage($"Receive From ID[{strClientID}]  : {strRcv}");
                    //버퍼 내용 초기화
                    Array.Clear(bBuffer, 0, bBuffer.Length);
                    //메시지 토큰 분리
                    string[] strSplit = strRcv.Split(':');


                    /* 명령어 처리 구간 */
                    // ()안의 내용은 args작성 요령
                    switch (strSplit[0])
                    {
                        //클라이언트가 객실임을 알리는 명령어 (ARDUINO:rid)
                        case "ARDUINO":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }
                                int rid = -1;

                                //파싱 시도
                                if (int.TryParse(strSplit[1], out rid))
                                {
                                    if (rid < 0)
                                    {
                                        PrintMessage("rid Less than 0.");
                                        sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                        PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                        break;
                                    }
                                    //중복된 rid면 이전 rid키 제거
                                    else if (mv_dicRooms.ContainsKey(rid))
                                    {
                                        DictionaryRoomRemove(rid);
                                    }
                                    //소켓의 타입 저장
                                    clientType = ClientType.ROOM;
                                    //Dictionary에 새 rid, ip 추가
                                    DictionaryRoomAdd(rid, strClientID);
                                    PrintMessage($"Add new Room : {rid}");
                                    sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_OK}");
                                    sockClient.Send(ConvertStringBytes("ROOMINFO") as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : ROOMINFO");
                                }
                                //파싱 실패
                                else
                                {
                                    PrintMessage("RID Parsing failed.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                }
                                break;
                            }
                        //클라이언트가 호텔 프런트임을 알리는 명령어
                        case "PC":
                            {
                                mv_addrFront = (sockClient.RemoteEndPoint as IPEndPoint).Address;
                                sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : {STR_OK}");
                                //소켓의 타입 저장
                                clientType = ClientType.FRONT;
                                break;
                            }
                        //사용자 앱의 고유 ID를 등록하는 명령어 (ID:전화번호)
                        case "ID":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                //NullReference 방지
                                if (strSplit[1] == null || strSplit[1].Length == 0)
                                {
                                    PrintMessage("ID is Null");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                //mv_dicClients의 키 중에서 현재 IP키를 제거
                                DictionaryClientRemove(strClientID);
                                PrintMessage($"Remove dictionary key(IP) : {strClientID}");
                                strClientID = strSplit[1];

                                //mv_dicClients에서 ID중복 확인 및 제거
                                if (mv_dicClients.ContainsKey(strClientID) == true)
                                {
                                    if (mv_dicClients[strClientID] != null)
                                    {
                                        mv_dicClients[strClientID].Close();
                                    }
                                    DictionaryClientRemove(strClientID);
                                    PrintMessage($"Remove same dictionary key(ID) : {strClientID}");
                                }
                                //소켓의 타입 저장
                                clientType = ClientType.APP;
                                //mv_dicClients에 ID를 새 키로 추가
                                DictionaryClientAdd(strClientID, sockClient);
                                PrintMessage($"Add new dictionary key : {strClientID}");

                                //프런트에게 연결을 알림
                                if (mv_addrFront != null)
                                {
                                    mv_dicClients[mv_addrFront.ToString()].Send(ConvertStringBytes($"CCON:{strClientID}") as byte[], SocketFlags.None);
                                    PrintMessage($"Sand to {mv_addrFront} : CCON:{strClientID}");
                                }
                                break;
                            }
                        //클라이언트가 호텔 프런트에 채팅을 전송하겠다는 명령어 (CHAT:내용)
                        case "CHAT":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                strSnd = $"CHAT:{strClientID}:{strSplit[1]}";
                                //채팅 내용중에 콜론(:)이 1번이상 나오는 경우 분할된 내용을 합침
                                for (int i = 2; i < strSplit.Length; i++)
                                {
                                    strSnd += $":{strSplit[i]}";
                                }

                                //프런트에 채팅메시지 전송
                                string strFrontAddr = mv_addrFront.ToString();
                                if (mv_addrFront != null && mv_dicClients.ContainsKey(strFrontAddr))
                                {
                                    mv_dicClients[strFrontAddr].Send(ConvertStringBytes($"CHTCALL:{strClientID}") as byte[], SocketFlags.None);
                                    PrintMessage($"Send to {strFrontAddr} : CHTCALL:{strClientID}");
                                    mv_dicClients[strFrontAddr].Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                    PrintMessage($"Send to {strFrontAddr} : {strSnd}");
                                    //클라이언트에게 채팅 메시지의 전송에 성공을 알림
                                    sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                    PrintMessage($"Send to {strClientID} : {STR_OK}");
                                }
                                //프런트 ip정보가 없는 경우
                                else
                                {
                                    PrintMessage($"Cannot found Front socket.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                }
                                break;
                            }
                        //클라이언트가 다른 사용자에게 채팅을 전송하겠다는 명령어 (CHATTO:앱ID:내용)
                        case "CHATTO":
                            {
                                if (strSplit.Length <= 2)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                //해당 id를 가진 클라이언트가 없는 경우
                                if (mv_dicClients.ContainsKey(strSplit[1]) == false)
                                {
                                    PrintMessage($"Cannot found client id : .{strSplit[1]}");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                }
                                else
                                {
                                    strSnd = $"CHAT\n{strSplit[2]}";
                                    //채팅 내용중에 콜론(:)이 1번이상 나오는 경우 분할된 내용을 합침
                                    for (int i = 3; i < strSplit.Length; i++)
                                    {
                                        strSnd += $":{strSplit[i]}";
                                    }

                                    mv_dicClients[strSplit[1]].Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {(mv_dicClients[strSplit[1]].RemoteEndPoint as IPEndPoint).Address.ToString()} : {strSnd}");
                                    //클라이언트에게 채팅 메시지의 전송에 성공을 알림
                                    sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_OK}");
                                }
                                break;
                            }
                        //고객이 객실 환경정보를 요청하는 명령어
                        case "RoomInfo":
                            {
                                string strNID = mv_db.SelectNIDByPhone(strClientID);
                                //고객 정보를 찾을 수 없는 경우
                                if (strNID == null)
                                {
                                    PrintMessage($"Not found nid by app id : {strClientID}");
                                    break;
                                }

                                //DB에서 nid로 Reservation테이블를 검색해 예약한 방을 알아냄
                                List<Reservation> ReservList = mv_db.SelectAllReservationsByNID(strNID);
                                //예약한 방이 없는 경우
                                if (ReservList == null)
                                {
                                    PrintMessage($"Not found reserved room by nid:{strNID}");
                                    break;
                                }

                                //알아낸 방에 대한 정보를 검색
                                Room myRoom = mv_db.SelectRoomByRID(ReservList[0].rid);
                                //탐색 실패 시
                                if (myRoom == null)
                                {
                                    PrintMessage($"Not found rid : {myRoom.rid}");
                                    break;
                                }
                                //저장된 센서 감지값 전송
                                strSnd = $"{myRoom.temp}\n{myRoom.humid}\n{myRoom.dust}";
                                sockClient.Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : {strSnd}");
                                break;
                            }
                        //고객이 객실의 각 전등을 켜는 명령어
                        case "LampOn":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                string strTarget;
                                switch (strSplit[1])
                                {
                                    case "Room":
                                        {
                                            strTarget = "ROOM";
                                            break;
                                        }
                                    case "Toilet":
                                        {
                                            strTarget = "TOILET";
                                            break;
                                        }
                                    case "Living":
                                        {
                                            strTarget = "LIVING";
                                            break;
                                        }
                                    default:
                                        {
                                            strTarget = null;
                                            break;
                                        }
                                }

                                if (strTarget == null)
                                {
                                    PrintMessage("Invalid command : LampOn(arg1)");
                                    break;
                                }

                                string strNID = mv_db.SelectNIDByPhone(strClientID);
                                List<Reservation> reservList = mv_db.SelectAllReservationsByNID(strNID);
                                //nid를 통한 rid검색 실패
                                if (reservList == null)
                                {
                                    PrintMessage($"Not found reserved room by nid:{strNID}");
                                    break;
                                }
                                //객실에 명령어 전송
                                mv_dicClients[mv_dicRooms[reservList[0].rid]].Send(ConvertStringBytes($"LAMPON_{strTarget}") as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : LAMPON_{strTarget}");
                                break;
                            }
                        //고객이 객실의 각 전등을 끄는 명령어
                        case "LampOff":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                string strTarget;
                                switch (strSplit[1])
                                {
                                    case "Room":
                                        {
                                            strTarget = "ROOM";
                                            break;
                                        }
                                    case "Toilet":
                                        {
                                            strTarget = "TOILET";
                                            break;
                                        }
                                    case "Living":
                                        {
                                            strTarget = "LIVING";
                                            break;
                                        }
                                    default:
                                        {
                                            strTarget = null;
                                            break;
                                        }
                                }

                                if (strTarget == null)
                                {
                                    PrintMessage("Invalid command : LampOff(arg1)");
                                    break;
                                }

                                string strNID = mv_db.SelectNIDByPhone(strClientID);
                                List<Reservation> reservList = mv_db.SelectAllReservationsByNID(strNID);
                                //nid를 통한 rid검색 실패
                                if (reservList == null)
                                {
                                    PrintMessage($"Not found reserved room by nid:{strNID}");
                                    break;
                                }
                                //객실에 명령어 전송
                                mv_dicClients[mv_dicRooms[reservList[0].rid]].Send(ConvertStringBytes($"LAMPOFF_{strTarget}") as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : LAMPOFF_{strTarget}");
                                break;
                            }
                        //고객이 객실의 모든 전등을 켜는 명령어
                        case "ALLON":
                            {
                                string strNID = mv_db.SelectNIDByPhone(strClientID);
                                List<Reservation> reservList = mv_db.SelectAllReservationsByNID(strNID);
                                //nid를 통한 rid검색 실패
                                if (reservList == null)
                                {
                                    PrintMessage($"Not found reserved room by nid:{strNID}");
                                    break;
                                }
                                //객실에 명령어 전송
                                mv_dicClients[mv_dicRooms[reservList[0].rid]].Send(ConvertStringBytes("ALLON") as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : ALLON");
                                break;
                            }
                        //고객이 객실의 모든 전등을 끄는 명령어
                        case "ALLOFF":
                            {
                                string strNID = mv_db.SelectNIDByPhone(strClientID);
                                List<Reservation> reservList = mv_db.SelectAllReservationsByNID(strNID);
                                //nid를 통한 rid검색 실패
                                if (reservList == null)
                                {
                                    PrintMessage($"Not found reserved room by nid:{strNID}");
                                    break;
                                }
                                //객실에 명령어 전송
                                mv_dicClients[mv_dicRooms[reservList[0].rid]].Send(ConvertStringBytes("ALLOFF") as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : ALLOFF");
                                break;
                            }
                        //고객이 객실의 희망온도를 설정하는 명령어
                        case "Temp":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                int tempSet;
                                //정수형으로 파싱 시도
                                if (int.TryParse(strSplit[1], out tempSet) == false)
                                {
                                    //파싱 실패
                                    PrintMessage("Temp set Pasing failed");
                                    break;
                                }

                                string strNID = mv_db.SelectNIDByPhone(strClientID);
                                List<Reservation> reservList = mv_db.SelectAllReservationsByNID(strNID);
                                //nid를 통한 rid검색 실패
                                if (reservList == null)
                                {
                                    PrintMessage($"Not found reserved room by nid:{strNID}");
                                    break;
                                }
                                //해당 객실의 온도 가져오기
                                Room room = mv_db.SelectRoomByRID(reservList[0].rid);
                                //rid를 통한 객실 검색 실패
                                if (room == null)
                                {
                                    PrintMessage($"Not found room by rid:{reservList[0].rid}");
                                    break;
                                }

                                if (mv_db.UpdateRoom(reservList[0].rid, tempSet) == false)
                                {
                                    PrintMessage($"Fail tempset update room {reservList[0].rid}");
                                }

                                //현재 온도 > 희망 온도
                                if (room.temp > tempSet)
                                {
                                    strSnd = "COOLING";
                                }
                                //현재 온도 < 희망 온도
                                else if (room.temp < tempSet)
                                {
                                    strSnd = "HEATING";
                                }
                                //현재 온도 == 희망 온도
                                else { break; }

                                //객실에 온도조절 명령어 전송
                                mv_dicClients[mv_dicRooms[reservList[0].rid]].Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                PrintMessage($"Send to {mv_dicRooms[reservList[0].rid]} : {strSnd}");

                                break;
                            }
                        //해당 객실의 온도, 습도, 미세먼지농도 값을 각각 저장하는 명령어 (SensorValues:온도:습도:농도)
                        case "SensorValues":
                            {
                                if (strSplit.Length <= 3)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                float fTemp, fHumid, fDust;
                                //온도 to float
                                if (float.TryParse(strSplit[1], out fTemp) == false)
                                {
                                    PrintMessage("Failed parse : Temperature");
                                    fTemp = -300;
                                }
                                //습도 to float
                                if (float.TryParse(strSplit[2], out fHumid) == false)
                                {
                                    PrintMessage("Failed parse : Humidity");
                                    fHumid = -1;
                                }
                                //농도 to float
                                if (float.TryParse(strSplit[3], out fDust) == false)
                                {
                                    PrintMessage("Failed parse : Dust Concentration");
                                    fDust = -1;
                                }

                                //RID찾기
                                int rid = 0;
                                int[] nRooms = mv_dicRooms.Keys.ToArray();
                                for (int i = 0; i < nRooms.Length; i++)
                                {
                                    if (mv_dicRooms[nRooms[i]] == strClientID)
                                    {
                                        rid = nRooms[i];
                                    }
                                }
                                //방 정보 갱신
                                Room newRoom = new Room(rid, new DateTime(), new DateTime(), 1, -1, -1, (int)fTemp, (int)fHumid, (int)fDust);
                                if (mv_db.UpdateRoom(newRoom) == false)
                                {
                                    PrintMessage($"Failed update room {rid} information");
                                    strSnd = STR_FAIL;
                                    sockClient.Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {strSnd}");
                                    break;
                                }

                                /*
                                 * 해당 객실에 설정된 희망온도를 현재 온도와 비교하여 COOLING / HEATING 결정
                                 */
                                strSnd = STR_OK;
                                sockClient.Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : {strSnd}");

                                break;
                            }
                        //NFC의 ID가 해당 객실에 등록되어 있는지 확인하는 명령어 (NFCID:nid)
                        case "NFCID":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientID} : {STR_FAIL}");
                                    break;
                                }

                                string nid = strSplit[1];
                                PrintMessage($"Search NID : {nid}");
                                //nid로 예약 테이블 검색
                                List<Reservation> reserv = mv_db.SelectAllReservationsByNID(nid);
                                bool isSuccess = false;

                                //검색 실패 시
                                if (reserv == null)
                                {
                                    PrintMessage("Not allowed NID");
                                    isSuccess = false;
                                }
                                //검색 완료 시
                                else
                                {
                                    for (int i = 0; i < reserv.Count; i++)
                                    {
                                        //ip비교
                                        if (mv_dicRooms[reserv[i].rid] == strClientID)
                                        {
                                            isSuccess = true;
                                            break;
                                        }
                                    }
                                }
                                if (isSuccess) { strSnd = "ALLOWNID"; }
                                else { strSnd = STR_FAIL; }
                                sockClient.Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientID} : {strSnd}");
                                break;
                            }
                    }
                }

                PrintMessage($"Disconnected client : {sockClient.RemoteEndPoint.ToString()}");
            }
            catch (Exception e)
            {
                //예외발생 메시지 출력
                PrintMessage($"Exception occurred in CommunicationToClient() [{strClientID}] : {e.Message}");
                //아직 연결되어 있으면 FAIL전송
                if (sockClient != null && sockClient.Connected)
                {
                    try
                    {
                        sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                        PrintMessage($"Send To {(sockClient.RemoteEndPoint as IPEndPoint).Address} : FAIL");
                    }
                    catch (Exception ex)
                    {
                        PrintMessage($"Exception occurred in CommunicationToClient() : {ex.Message}");
                    }
                }
            }
            finally
            {
                if (sockClient != null)
                {
                    try
                    {
                        //APP타입의 통신이 끊기면 프런트에게 알려줌
                        if (clientType == ClientType.APP && mv_addrFront != null)
                        {
                            strSnd = $"CCLOSE:{strClientID}";
                            mv_dicClients[mv_addrFront.ToString()].Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                            PrintMessage($"Send to {mv_addrFront} : {strSnd}");
                        }
                    }
                    catch (Exception)
                    {
                        PrintMessage($"Fail send to {mv_addrFront} : CCLOSE command");
                    }
                    PrintMessage($"Close Socket ID : {strClientID}");
                    sockClient.Close();
                    //여러 스레드가 동시에 remove하면 공유메모리 충돌 가능성 있음. Queue를 이용하여 삭제할 key를 큐에 넣어 하나씩 삭제할 것
                    if (mv_dicClients.ContainsKey(strClientID))
                    {
                        DictionaryClientRemove(strClientID);      //클라이언트 Dictionary에서 제거
                        PrintMessage($"Remove dictionary key : {strClientID}");
                    }

                    //Room타입의 통신이 끊기면 room딕셔너리에서 제거
                    if (clientType == ClientType.ROOM)
                    {
                        int[] rids = mv_dicRooms.Keys.ToArray();
                        for (int i = 0; i < rids.Length; i++)
                        {
                            if (mv_dicRooms[rids[i]] == strClientID)
                            {
                                DictionaryRoomRemove(rids[i]);
                                break;
                            }
                        }
                    }
                }
                //Front타입의 통신이 끊기면 현재 프론트를 null로 설정
                if (clientType == ClientType.FRONT)
                {
                    mv_addrFront = null;
                }
                PrintMessage($"Client Count : {mv_dicClients.Count}");
            }
        }

        //mv_dicClient의 키를 제거하는 함수
        private bool DictionaryClientRemove(string strKey)
        {
            mv_mutex.WaitOne();
            bool rtn = mv_dicClients.Remove(strKey);
            mv_mutex.ReleaseMutex();
            return rtn;
        }

        //mv_dicRoom의 키를 제거하는 함수
        private bool DictionaryRoomRemove(int nKey)
        {
            mv_mutex.WaitOne();
            bool rtn = mv_dicRooms.Remove(nKey);
            mv_mutex.ReleaseMutex();
            return rtn;
        }
        
        //mv_dicClient의 키를 제거하는 함수
        private void DictionaryClientAdd(string strKey, Socket sockValue)
        {
            mv_mutex.WaitOne();
            mv_dicClients.Add(strKey, sockValue);
            mv_mutex.ReleaseMutex();
        }

        //mv_dicRoom의 키를 제거하는 함수
        private void DictionaryRoomAdd(int nKey, string strValue)
        {
            mv_mutex.WaitOne();
            mv_dicRooms.Add(nKey, strValue);
            mv_mutex.ReleaseMutex();
        }

        //객실의 환경 정보를 갱신함
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (mv_dicRooms.Count == 0) { return; }

            try
            {
                int[] nRIDs = mv_dicRooms.Keys.ToArray();

                if (nRIDs == null) { return; }
                PrintMessage($"Update room information");

                for (int i = 0; i < nRIDs.Length; i++)
                {
                    mv_dicClients[mv_dicRooms[nRIDs[i]]].Send(ConvertStringBytes("ROOMINFO") as byte[]);
                }
            }
            catch (Exception except)
            {
                PrintMessage($"Exception in Timer_Elapsed messod : {except.Message}");
            }
        }

        //프로그램의 창이 닫힐 때 호출
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mv_isProgramClose = true;
                //서버 메인 소켓 닫기
                if (mv_sockServer != null)
                {
                    //3초 대기 후 소켓 닫음
                    mv_sockServer.Close(3000);
                }
                //모든 클라이언트 소켓 닫기
                string[] strKeys = mv_dicClients.Keys.ToArray();
                PrintMessage("All client closing.");
                for (int i = 0; i < strKeys.Length; i++)
                {
                    mv_dicClients[strKeys[i]].Close();
                }
                PrintMessage("Server Closing.");
                mv_listenThread.Join();
            }
            catch(Exception except)
            {
                MessageBox.Show($"Exception in FormClosing : {except.Message}");
            }
        }

        //텍스트 변경 이벤트 발생 시 호출
        private void display1_TextChanged(object sender, EventArgs e)
        {
            if (display1.Lines.Length >= MAX_TEXT_LINE)
            {
                display1.Clear();
            }
            //스크롤을 현재 커서 위치(가장 아래)까지 내림
            display1.ScrollToCaret();
        }

        //지우기 버튼 클릭시 호출
        private void btn_clear_Click(object sender, EventArgs e)
        {
            display1.SelectionStart = 0;
            display1.ScrollToCaret();
            display1.Clear();
        }

        //텍스트 로그 출력
        private void PrintMessage(string str)
        {
            mv_mutex.WaitOne();
            str = "[" + DateTime.Now.ToString() + "]  \t" + str + Environment.NewLine;
            display1.AppendText(str);
            using (StreamWriter file = new StreamWriter(Application.StartupPath + @"\log.txt", true))
            {
                file.WriteLine(str);
            }
            mv_mutex.ReleaseMutex();
        }
    }
}
