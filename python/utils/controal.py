import cv2, json
from utils import socket_utils
import numpy as np
import threading

class PIDControal:
    def __init__(self, goal, kp, ki, kd, log=False):
        """
        initialize

        Parameters
        ----------
        goal : float
            goal value.
        kp : float
            P gain.
        ki : float
            I gain.
        kd : float
            D gain.
        log : bool, optional
            Print log or not. The default is False.

        Returns
        -------
        None.

        """
        self.goal = goal
        self.kp = kp
        self.ki = ki
        self.kd = kd
        self.sum = 0
        self.e1 = 0
        self.e2 = 0
        self.e3 = 0    
        self.log = log
        
    def calc(self, x):
        """
        calucurate controal value

        Parameters
        ----------
        x : float
            current value.

        Returns
        -------
        u : float
            Total controal value.
        p_val : float
            P controal value.
        i_val : TYPE
            I controal value.
        d_val : TYPE
            D controal value.
        """
        e = self.goal - x
        self.sum += e
        p_val = self.kp * e
        i_val = self.ki * self.sum
        d_val = self.kd * ((self.e1-e) + (self.e2-e) + (self.e3-e))/3
        u = p_val + i_val + d_val
        if self.log:
            print("[dbg] controal total: {:.3f}, p: {:.3f}, i: {:.3f}, d:{:.3f}".format(u, \
                             p_val, i_val, d_val))
        self.e3 = self.e2
        self.e2 = self.e1
        self.e1 = e
        self.sum *= 0.99
        return u, p_val, i_val, d_val


class CarControal:
    def __init__(self, size, udp_send_port=9000, udp_recv_port=9001, max_st=2,
                 st_param=[1.5, 0, 0], st_th=0.3, mo_param=10.0, max_v=5, 
                 min_v=3):
        """
        initialize

        Parameters
        ----------
        size : list
            image size. [W, H].
        udp_send_port : int, optional
            udp send port. The default is 9000.
        udp_recv_port : int, optional
            udp recv port. The default is 9001.
        max_st : float, optional
            Upper limit of stearing. The default is 2.
        st_param : list, optional
            Parameter for stearing PID controal. The default is [1.5, 0, 0].
        st_th : float, optional
            stearing threshold. The default is 0.3.
        mo_param : float, optional
            acelerator parameter. The default is 10.0.
        max_v : float, optional
            upper limit of verocity. The default is 5.
        min_v : TYPE, optional
            lower limit of verocity. The default is 3.

        Returns
        -------
        None.

        """
        self.udp_send = socket_utils.udpsend(udp_send_port) # send
        self.udp_recv = socket_utils.udprecv(udp_recv_port) # recive
        # Transmission data
        # (mo: acelerator / brake, st: Handle angle)
        self.data = { 
            "mo" :0,
            "st" :0,
            }
        self.v = 0 
        self.x = 0 
        self.y = 0 
        self.z = 0 
        self.time = 0
        self.flg = True
        self.th = None
        self.max_st = max_st
        self.max_v = max_v
        self.print_data = True
        self.size = size 
        self.st_param = st_param
        self.mo_param = mo_param
        self.pid_st = PIDControal(0, 
                                  self.st_param[0], 
                                  self.st_param[1], 
                                  self.st_param[2])
        self.min_v = min_v
        self.st_th = st_th
        
    def send(self):
        """
        send the data
        """
        self.udp_send.send(self.data)
        
    def update_th_start(self, log=False):
        """
        start thred to recive data from unity

        Parameters
        ----------
        log : bool, optional
            print log or not. The default is True.

        Returns
        -------
        None.

        """
        self.log = log
        self.th = threading.Thread(target=self.update)
        self.th.start()
        
    def update(self):
        """
        recive the data
        """
        while self.flg:
            data = self.udp_recv.recv()
            if data is not None:
                d = json.loads(data)
                self.x = d["X"][0]
                self.y = d["Y"][0]
                self.z = d["Z"][0]
                self.v = d["V"][0]
                self.time = d["time"][0]
                
                if self.log:
                    """
                    s = ""
                    for k in d.keys():
                        s += "{}: {:.3f} ".format(k, d[k][0])
                    for k in self.data:
                        s += "{}: {:.3f} ".format(k, self.data[k][0])
                    print(s)
                    """
                    print(d)
                
    def controal(self, x, y):
        """
        controal the car
        This method calculates the Handle angle and accelerator/brake,
        and send them to Unity.        

        Parameters
        ----------
        x : float
            centor point of white lines. x axis.
        y : float
            centor point of white lines. y axis.

        Returns
        -------
        u : float
            values of controal. total.
        p_val : float
            values of controal. P
        i_val : float
            values of controal. I
        d_val : float
            values of controal. D

        """
        x = x - self.size[0]/2
        x = x/self.size[0]
        # Handle angle calculation
        u, p_val, i_val, d_val = self.pid_st.calc(x)
        self.data["st"] = -1 * u
        # Limit steering angle
        if self.data["st"] < -1 * self.max_st:
            self.data["st"] = -1 * self.max_st
        elif self.data["st"] > self.max_st:
            self.data["st"] = self.max_st        
        # If the steering exceeds the threshold, slow down the car.
        if abs(self.data["st"]) > self.st_th and self.v > self.min_v:
            mo = self.min_v - self.v
        else:        
            mo = (self.max_v - self.v)
        self.data["mo"] = mo
        # data send
        self.send()
        return u, p_val, i_val, d_val
                
    def destroy(self):
        """
        close
        """
        # Handle angle and accelerator to 0
        self.data["st"] = 0
        self.data["mo"] = 0
        self.send()
        self.flg = False
        self.th.join()
        self.udp_send.close()
        self.udp_recv.close()
        

class LineDetector:
    def __init__(self, size, x_1 = [0.05, 0.35], x_2 = [0.1, 0.4],
                 x_3 = [0.15, 0.45], y_1 = 0.6, y_2 = 0.5, y_3 = 0.4, drow_img = True):
        """
        initialize

        Parameters
        ----------
        size : list
            image size. [W, H].
        x_1 : list, optional
            scan area range of x. The default is [0.05, 0.35].
        x_2 : list, optional
            scan area range of x. The default is [0.1, 0.4]
        x_3 : list, optional
            scan area range of x. The default is [0.15, 0.45]
        y_1 : float, optional
            scan pos of y. The default is 0.6.
        y_2 : float, optional
            scan pos of y. The default is 0.5.
        y_3 : float, optional
            scan pos of y. The default is 0.4.
        drow_img : bool, optional
            drow line in image or not. The default is True.

        Returns
        -------
        None.

        """
        self.bak_pos = [None]*6 # hist
        self.a = [0, 0]
        self.b = [0, 0]
        self.size = size
        self.x_1 = x_1
        self.x_2 = x_2
        self.x_3 = x_3
        self.y_1 = y_1
        self.y_2 = y_2
        self.y_3 = y_3
        self.img_hist = np.zeros(size).T
        self.drow_img = drow_img
        
    def get_line_pos(self, x1, x2, y, pos, img):
        """
        Get white line posision

        Parameters
        ----------
        x1 : float
            Lower limit of x axis scan range.
        x2 : float
            Upper limit of x axis scan range.
        y : float
            Y of scan position.
        pos : int
            position number.
        img : cv mat
            gray scale image.

        Returns
        -------
        float
            X of line position.

        """
        x1 = int(x1)
        x2 = int(x2)
        y = int(y)
        a = img[y, x1:x2]
        # if hist is None, set centor to hist
        if self.bak_pos[pos] is None:
            self.bak_pos[pos] = int((x1 + x2)/2)
        # if there is no line in scan area, return hist
        if max(a) == 0:
            return self.bak_pos[pos]
        idx = [i for i, x in enumerate(a) if x == max(a)]
        self.bak_pos[pos] = int(np.mean(idx) + x1)
        return self.bak_pos[pos]
    

    def get_line_ab(self, num):
        """
        calcurate slope and intercept of white line.

        Parameters
        ----------
        num : int
            Line number. 0(left) or 1(right).

        Returns
        -------
        a : float
            slope.
        b : float
            intercept.

        """
        x1 = (self.bak_pos[0 + 3*num] + self.bak_pos[1 + 3*num]) / 2
        x2 = (self.bak_pos[1 + 3*num] + self.bak_pos[2 + 3*num]) / 2
        if x1 == x2:
            a = 999
        else:
            tmp = abs(((self.y_1+self.y_2)-(self.y_2+self.y_3))/2*self.size[1])
            a = tmp/(x1 - x2)
        tmp = (self.y_1+self.y_2+self.y_2+self.y_3)/2*self.size[1]
        b = (tmp-a*(x1 + x2))/2
        self.a[num] = a
        self.b[num] = b
        return a, b
    
    def get_cross_point(self):
        """
        get cross point of white line

        Returns
        -------
        x : float
            x axis.
        y : float
            y axis.

        """
        if self.a[0] == self.a[1]:
            x = self.a[0]
        else:
            x = -1*(self.b[0] - self.b[1])/(self.a[0] - self.a[1])
        y = self.a[0]*x + self.b[0]
        return x, y
    
    def get_centor(self):
        """
        get centor of white line

        Returns
        -------
        x : int
            x axis.

        """
        x = 0
        for i in range(int(len(self.bak_pos)/2)):
            x += (self.bak_pos[i] + self.bak_pos[i+3] - self.size[0]) 
        x += self.size[0]/2
        return int(x)
    
    def detect_line(self, img, method = "multiply"):
        """
        calcurate white line position and drow to image.

        Parameters
        ----------
        img : cv mat
            image.
        method : string, optional
            Image processing method for white line detection.
            select from "multiply", "th", "edge" or "hist"
            The default is "multiply".

        Returns
        -------
        x_c : float
            centor position. x axis
        y : float
            centor position. y axis
        img_master : cv mat
            drowed image.

        """
        W, H = self.size
        kernel = np.ones((5,5),np.uint8)
        
        img_cp = img.copy()
        img_cp = cv2.cvtColor(img_cp, cv2.COLOR_BGR2GRAY)
        _, img_th = cv2.threshold(img_cp, 200, 255, cv2.THRESH_BINARY)
        img_ca = cv2.Canny(img_cp, 100, 200)
        img_ca = cv2.dilate(img_ca, kernel,iterations = 1)
        if method == "multiply":
            img_master = cv2.multiply(img_ca, img_th)
        elif method == "th":
            img_master = img_th
        elif method == "edge":
            img_master = img_ca
        elif method == "hist":
            self.img_hist = self.img_hist*0.9 + np.array(img_master)*0.1
            img_master = self.img_hist.astype('uint8')
        
        img = self._get_line(img, img_master, self.x_1, self.y_1, 0)
        img = self._get_line(img, img_master, self.x_2, self.y_2, 1)
        img = self._get_line(img, img_master, self.x_3, self.y_3, 2)
        img = self._get_line_r(img, img_master, self.x_1, self.y_1, 3)
        img = self._get_line_r(img, img_master, self.x_2, self.y_2, 4)
        img = self._get_line_r(img, img_master, self.x_3, self.y_3, 5)
        
        a, b = self.get_line_ab(0)
        if self.drow_img:
            img = cv2.line(img,(0, int(b)),(W, int(W*a+b)),(0, 255, 0),1)

        a, b = self.get_line_ab(1)
        if self.drow_img:
            img = cv2.line(img,(0, int(b)), (W, int(W*a+b)),(0, 255, 0),1)

        x, y = self.get_cross_point()        
        if self.drow_img:
            img = cv2.circle(img, (int(x), int(y)), 5, (255, 0, 255), thickness=-1)
        x_c = self.get_centor()        
        if self.drow_img:
            img = cv2.circle(img, (int(x_c), int(100)), 5, (0, 0, 255), thickness=-1)
        x = np.mean([x, x_c])
        
        return x_c, y, img_master
    
    def _get_line(self, img, img_thresh, x, y, pos_num):
        W, H = self.size
        p = self.get_line_pos(W*x[0], int(W*x[1]), int(H*y), pos_num, img_thresh)
        if self.drow_img:
            img = cv2.line(img,(int(W*x[0]), int(H*y)),(int(W*x[1]), int(H*y)),(0,0, 255),1)
            img = cv2.circle(img, (p, int(H*y)), 5, (255, 0, 0), thickness=-1)
        return img
    
    def _get_line_r(self, img, img_thresh, x, y, pos_num):
        W, H = self.size
        p = self.get_line_pos(W-W*x[1], int((1-x[0])*W), H*y, pos_num, img_thresh)        
        if self.drow_img:
            img = cv2.line(img,(int((1-x[0])*W),int(H*y)),(int(W-W*x[1]),int(H*y)),(0,0, 255),1)
            img = cv2.circle(img, (p, int(H*y)), 5, (255, 0, 0), thickness=-1)
        return img
    