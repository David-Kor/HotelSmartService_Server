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

namespace HotelServer
{
    public partial class Server : Form
    {
        private Socket mv_sockServer;       //서버 메인 소켓
        private IPAddress mv_addrFront;    //호텔 프런트 IP주소객체
        private Dictionary<string, Socket> mv_dicClients;      //클라이언트 소켓들(key: ip주소, value: 소켓)
        private Dictionary<int, string> mv_dicRooms;          //객실 ip들(key: 객실번호[rid], value: ip주소)
        private DBManager mv_db;           //데이터베이스 클래스
        private bool mv_isProgramClose;    //프로그램 종료버튼이 눌림을 알림
        private Thread mv_listenThread;     //소켓 리스너 스레드

        private const string STR_OK = "OK";
        private const string STR_FAIL = "FAIL";

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
        }

        //메인 윈도우 폼이 메모리에 적재될 때 호출
        private void Form1_Load(object sender, EventArgs e)
        {
            PrintMessage("Server Program Start.");
            mv_listenThread = new Thread(new ThreadStart(ServerSocketStart));
            mv_listenThread.Start();
        }

        private object ConvertStringBytes(object objSource)
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
            string strClientIP = (sockClient.RemoteEndPoint as IPEndPoint).Address.ToString();
            byte[] bBuffer = new byte[1024];    //최대 버퍼크기 1024Byte
            int nRcvSize;       //수신 메시지 크기
            string strRcv, strSnd;    //송수신 메시지

            try
            {
                PrintMessage($"Add Client List");
                //클라이언트들 중에 동일 IP가 존재하면 기존에 있던 IP와의 통신을 끊고 Dictionary에서 제거
                if (mv_dicClients.ContainsKey(strClientIP))
                {
                    mv_dicClients[strClientIP].Close();
                    mv_dicClients.Remove(strClientIP);
                }
                //새로운 key,value를 추가
                mv_dicClients.Add(strClientIP, sockClient);
                PrintMessage($"Client Count : {mv_dicClients.Count}");
                //통신 시작
                while (true)
                {
                    if (sockClient == null || sockClient.Connected == false) { break; }
                    //클라이언트 메시지 수신
                    nRcvSize = sockClient.Receive(bBuffer);

                    PrintMessage($"Receive {nRcvSize} Byte From {strClientIP}");
                    //사이즈가 0이하인 경우 무시
                    if (nRcvSize <= 0)
                    {
                        sockClient.Close();
                        mv_dicClients.Remove(strClientIP);
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

                    PrintMessage($"Receive From {strClientIP} : {strRcv}");
                    //버퍼 내용 초기화
                    Array.Clear(bBuffer, 0, bBuffer.Length);
                    //메시지 토큰 분리
                    string[] strSplit = strRcv.Split(':');


                    /* 명령어 처리 구간 */

                    switch (strSplit[0])
                    {
                        //클라이언트가 객실임을 알리는 명령어 (ARDUINO:rid)
                        case "ARDUINO":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : FAIL");
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
                                        PrintMessage($"Send To {strClientIP} : FAIL");
                                        break;
                                    }
                                    //중복된 rid면 이전 rid키 제거
                                    else if (mv_dicRooms.ContainsKey(rid))
                                    {
                                        mv_dicRooms.Remove(rid);
                                    }
                                    //Dictionary에 새 rid, ip 추가
                                    mv_dicRooms.Add(rid, strClientIP);
                                    sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : OK");
                                }
                                //파싱 실패
                                else
                                {
                                    PrintMessage("Parsing Failed.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : FAIL");
                                }
                                break;
                            }
                        //클라이언트가 호텔 프런트임을 알리는 명령어
                        case "PC":
                            {
                                mv_addrFront = (sockClient.RemoteEndPoint as IPEndPoint).Address;
                                sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                PrintMessage($"Send To {strClientIP} : OK");
                                break;
                            }
                        //클라이언트가 호텔 프런트에 채팅을 전송하겠다는 명령어 (CHAT:내용)
                        case "CHAT":
                            {
                                if (strSplit.Length <= 1)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : FAIL");
                                    break;
                                }

                                strSnd = $"CHAT:{strClientIP}:{strSplit[1]}";
                                //채팅 내용중에 콜론(:)이 1번이상 나오는 경우 분할된 내용을 합침
                                for (int i = 2; i < strSplit.Length; i++)
                                {
                                    strSnd += $":{strSplit[i]}";
                                }

                                //프런트에 채팅메시지 전송
                                if (mv_addrFront != null)
                                {
                                    mv_dicClients[mv_addrFront.ToString()].Send(ConvertStringBytes(strSnd) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {mv_addrFront.ToString()} : {strSnd}");
                                    //클라이언트에게 채팅 메시지의 전송에 성공을 알림
                                    sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : OK");
                                }
                                //프런트 ip정보가 없는 경우
                                else
                                {
                                    PrintMessage($"Cannot found Front Address.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : FAIL");
                                }
                                break;
                            }
                        //클라이언트가 다른 사용자에게 채팅을 전송하겠다는 명령어 (CHATIP:IP주소:내용)
                        case "CHATIP":
                            {
                                if (strSplit.Length <= 2)
                                {
                                    PrintMessage("Not enough Args.");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : FAIL");
                                    break;
                                }

                                //해당 ip를 가진 클라이언트가 없는 경우
                                if (mv_dicClients.ContainsKey(strSplit[1]) == false)
                                {
                                    PrintMessage($"Cannot found client ip : .{strSplit[1]}");
                                    sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : FAIL");
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
                                    PrintMessage($"Send To {mv_addrFront.ToString()} : {strSnd}");
                                    //클라이언트에게 채팅 메시지의 전송에 성공을 알림
                                    sockClient.Send(ConvertStringBytes(STR_OK) as byte[], SocketFlags.None);
                                    PrintMessage($"Send To {strClientIP} : OK");
                                }
                                break;
                            }
                    }
                }

                PrintMessage($"Disconnected client : {sockClient.RemoteEndPoint.ToString()}");
            }
            catch (Exception e)
            {
                //예외발생 메시지 출력
                PrintMessage($"Exception occurred in CommunicationToClient() [{strClientIP}] : {e.Message}");
                //아직 연결되어 있으면 FAIL전송
                if (sockClient != null && sockClient.Connected)
                {
                    try
                    {
                        sockClient.Send(ConvertStringBytes(STR_FAIL) as byte[], SocketFlags.None);
                        PrintMessage($"Send To {strClientIP} : FAIL");
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
                    PrintMessage($"Close Socket : {strClientIP}");
                    sockClient.Close();
                    mv_dicClients.Remove(strClientIP);      //클라이언트 Dictionary에서 제거
                }
                PrintMessage($"Client Count : {mv_dicClients.Count}");
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
            str = "[" + DateTime.Now.ToString() + "]  \t" + str + Environment.NewLine;
            display1.AppendText(str);
            using (StreamWriter file = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\공부자료\log.txt", true))
            {
                file.WriteLine(str);
            }
        }
    }
}
