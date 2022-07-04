# -*- coding: utf-8 -*-
import cv2, mmap, io
from PIL import Image

class MMAPCamera():
    def __init__(self, tagname):        
        self.mm = mmap.mmap(-1, 1024 * 1024 * 50, tagname)
        
    def read(self):
        self.mm.seek(0)     
        mode = self.mm.read()
        ByteToImg = Image.open(io.BytesIO(mode))
        return ByteToImg
    
    def destroy(self):
        del self.mm
    