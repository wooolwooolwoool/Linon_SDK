# -*- coding: utf-8 -*-
from socket import *
import json
import select
import threading
import traceback

## UDP送信クラス
class udpsend():
    def __init__(self, port, ip="127.0.0.1"):
        SrcIP = "127.0.0.1"
        SrcPort = 11111
        self.SrcAddr = (SrcIP,SrcPort)
        DstIP = ip
        DstPort = port
        self.DstAddr = (DstIP,DstPort)
        self.udpClntSock = socket(AF_INET, SOCK_DGRAM)
        self.udpClntSock.bind(self.SrcAddr)

    def send(self, data):
        data = json.dumps(data)
        data = data.encode('utf-8')
        self.udpClntSock.sendto(data, self.DstAddr)

    def close(self):
        self.udpClntSock.close()
        del self.udpClntSock


## UDP受信クラス
class udprecv():
    def __init__(self, port, ip="127.0.0.1", timeout=3):
        self.SrcAddr = (ip, port)                  
        self.BUFSIZE = 4096                             
        self.udpServSock = socket(AF_INET, SOCK_DGRAM)  
        self.udpServSock.bind(self.SrcAddr)             
        self.udpServSock.setblocking(0)
        self.udpServSock.settimeout(timeout)

    def recv(self):
        try:
            data = self.udpServSock.recv(self.BUFSIZE).decode()
        except:
            data = None
        return data

    def close(self):
        self.udpServSock.close()
        del self.udpServSock

## TCP送受信クラス       
class TCPboth():
    def __init__(self, port, ip="127.0.0.1", timeout=3):
        self.SrcAddr = (ip, port)
        self.BUFSIZE = 4096
        self.conn = None
        self.flg = True
        self.timeout = timeout
        self.onrecv_fn = {}
        self.th = None

    def start_server(self):
        # Start data receve Server
        self.th = threading.Thread(target=self._server_th)
        self.th.start()

    def _server_th(self):
        if self.conn is None:
            self.server = create_server(self.SrcAddr)
            self.server.listen()
            # wait connection
            self.conn, self.addr = self.server.accept()
        # set timeout
        self.conn.settimeout(self.timeout)
        while self.flg:
            try:
                s = self.conn.recv(self.BUFSIZE)
                self.OnRecv(s)
            except timeout:
                pass
            except:
                self.flg = False
                self.conn.close()
                self.conn = None
                print("server closed")

    def create_connection(self, port, ip="127.0.0.1"):
        if self.conn is not None:
            self.conn.close()
            self.conn = None
        self.conn = socket(AF_INET, SOCK_STREAM)
        self.conn.connect((ip, port))

    def add_onrecv(self, key, f):
        self.onrecv_fn[key] = f

    def OnRecv(self, s):
        d = json.loads(s.decode(encoding='utf-8'))
        try:
            self.onrecv_fn[d["type"]](d["data"])
        except:
            print(traceback.format_exc())
            print("ignore", d)

    def send(self, s):
        if self.conn is not None:
            self.conn.send(s.encode("UTF-8"))
            print("send OK")
            data = self.conn.recv(self.BUFSIZE).decode()
            print(data)
        else:
            print("No connection!!")

    def close(self):
        self.flg = False
        if self.conn is not None:
            self.conn.close()
        if self.th is not None:
            self.th.join()

# 送受信テスト
if __name__=="__main__":
    print("start test")
    s1 = TCPboth(9000)
    s1.start_server()
    st = ""
    while st != "000":
        st = input(">>")
        s1.send(st)

    s1.close()
    print("OK")
    del s1

