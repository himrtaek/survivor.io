using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private CancellationTokenSource _cancellationTokenSource = new();
    
    // Start is called before the first frame update
    void Start()
    {
        RunAsyncSocketClient(_cancellationTokenSource.Token);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = null;
    }

    async void RunAsyncSocketClient(CancellationToken token)
    {
        int MAX_SIZE = 1024;  // 가정
        var buff = new byte[MAX_SIZE];
        
        // (1) 소켓 객체 생성 (TCP 소켓)
        Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
 
        // (2) 서버에 연결
        var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
        await clientSock.ConnectAsync(ep);
        if (token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            int count = 0;
            while (true)
            {
                var msg = count.ToString();
                buff = Encoding.UTF8.GetBytes(msg);

                // (3) 서버에 데이타 전송
                await Task.Factory.FromAsync(
                    clientSock.BeginSend(buff, 0, buff.Length, SocketFlags.None, null, clientSock),
                    clientSock.EndSend);
                if (token.IsCancellationRequested)
                {
                    break;
                }

                Debug.Log("Send => " + msg);

                // (4) 서버에서 데이타 수신
                var n = await Task.Factory.FromAsync<int>(
                    clientSock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, null, clientSock),
                    clientSock.EndReceive);
                if (token.IsCancellationRequested)
                {
                    break;
                }

                msg = Encoding.UTF8.GetString(buff, 0, n);
                Debug.Log("Receive => " + msg);

                count = int.Parse(msg);
                count++;

                await Task.Delay(1000, token);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        finally
        {
            // (5) 소켓 닫기
            clientSock.Close();   
        }
    }
}
