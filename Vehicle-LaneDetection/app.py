import matplotlib.pyplot as plt
import matplotlib.image as mpimg
import pickle
from PIL import Image

import os
import time
import sys
import numpy as np
import cv2

import torch
import torch.nn as nn
from torch_lib import Model, ClassAverages
from yolo import cv_Yolo
from torch.autograd import Variable
from torchvision.models import vgg

import argparse

from shapely.geometry import Point
from shapely.geometry.polygon import Polygon

import json

from grabscreen import grab_screen
from sort import Sort
from line_fit import LineFit

import queue 
import threading
import copy

class ADAS:

    def __init__(self):
        self.yolo = cv_Yolo('weights/')        

        self.mot_tracker = Sort()

        self.width = 960
        self.height = 480
        self.height_offset = 200

        src = np.float32([[0, self.height], [self.width, self.height ], [self.width, 0], [0, 0]])
        dst = np.float32([[self.width / 2.5, self.height], [self.width - self.width / 2.5, self.height], [self.width, 0], [0, 0]])
        # src = np.float32([[590, 440],
        #             [690, 440],
        #             [200, 640],
        #             [1000, 640]])

        # # Window to be shown
        # dst = np.float32([[200, 0],
        #                 [1200, 0],
        #                 [200, 710],
        #                 [1200, 710]])
        self.projection_matrix = cv2.getPerspectiveTransform(src, dst) # The transformation matrix
        self.projection_matrix_inverse = cv2.getPerspectiveTransform(dst, src)

        self.lane_lines = LineFit(self.width, self.height - self.height_offset)

        self.detected_objects = []
        self.vehicle_offset = 0.0 

        self.run = False           
        self.t1 = threading.Thread(target=self.lane_detect_thread, args=())
        self.t2 = threading.Thread(target=self.vehicle_detect_thread, args=())

        self.lane_queue = queue.Queue(1)
        self.vehicle_queue = queue.Queue(1)    

        with open('calibrate_camera.p', 'rb') as f:
            save_dict = pickle.load(f)
            self.distortion_matrix = save_dict['mtx']
            self.distortion = save_dict['dist']    

    def load_image(self, img_path):
        # image loading
        img = cv2.imread(img_path)
        img = cv2.resize(img, None, fx=0.4, fy=0.4)
        height, width, channels = img.shape
        return img, height, width, channels


    #classes = ['person', 'bicycle', 'car', 'motorbike', 'bus', 'train', 'truck', 'traffic light', 'stop sign', ]
    def draw_box_labes(self, image, detections):    
        d = []
        detected_objects = []

        for idx, detection in enumerate(detections):            
            x1, y1 = detection.box_2d[0]
            x2, y2 = detection.box_2d[1]
            label_index = self.yolo.labels.index(detection.detected_class)
            d.append([x1, y1, x2, y2, detection.score, label_index])

        if len(d) > 0:
            d = np.array(d)
            tracked_objects = self.mot_tracker.update(d)

            for idx, (x1, y1, x2, y2, obj_id, label_index) in enumerate(tracked_objects):
                # if abs(x2 - x1) < 20 or abs(y2 - y1) < 20:
                #     continue                

                x1, y1, x2, y2, obj_id, label_index = int(x1), int(y1), int(x2), int(y2), int(obj_id), int(label_index)
                center_x, center_y = int((x1 + x2) / 2), int(y2)

                if abs(x1 - x2) < self.width / 30 or abs(y2 - y1) < self.height / 30:
                    continue

                color = tuple([int(i) for i in self.yolo.colors[0]])

                cv2.circle(image, (center_x, y2), 3, (255, 100, 100), -1)
                cv2.rectangle(image, (x1, y1), (x2, y2), color, 2)                        
                cv2.putText(image, f"{self.yolo.labels[label_index]} {obj_id}", (x1, y1 + 10), cv2.FONT_HERSHEY_SIMPLEX, .5, (255, 255, 255), 1)
                                
                detected_objects.append({
                    'id': obj_id,
                    'class_label': self.yolo.labels[label_index],
                    'center_x': center_x / self.width,
                    'center_y': center_y / (self.height - self.height_offset),
                })

        self.detected_objects = detected_objects
    

    def project_point(self, point, height_offset):        
        pts = np.array([[point[0], point[1]-height_offset]], dtype = "float32")
        pts = np.array([pts])

        p_out = cv2.perspectiveTransform(pts, self.projection_matrix)
        return (int(p_out[0][0][0]), int(p_out[0][0][1]))


    def project_birds_eye_view(self, img):        
        height_offset = 150
        img = img[height_offset:(height_offset+self.height), 0:self.width] # ROI Crop
        warped_img = cv2.warpPerspective(img, self.projection_matrix, (self.width, self.height-height_offset)) # Image warping

        return warped_img

    def prepare_data(self, detections):        
        for detection in detections:
            x1, y1 = detection.box_2d[0]
            x2, y2 = detection.box_2d[1]

            center_x, center_y = int((x1 + x2) / 2), int(y2)

    def image_detect(self, image): 
                
        #image = cv2.resize(frame, (480, 480), interpolation=cv2.INTER_LINEAR)           
        #image = image[self.height_offset:(self.height_offset+self.height), 0:self.width] # ROI Crop        

        detections = self.yolo.detect(image)           
        self.draw_box_labes(image, detections) #{'left': roi_left, 'center': roi_center, 'right': roi_right}         
        return image
        #self.prepare_data(detections)

        #warped_image = self.project_birds_eye_view(image, detections)
        #cv2.circle(image, (288, 288-150), 10, (100, 100, 255), -1)
        #cv2.imshow("Image Warped", warped_image)


    def mask_image(self, image):
        
        roi_left = np.array([[(0,0), (width / 2, height/3), (width / 4, height),  (0, height)]], dtype=np.int32) #left
        roi_center = np.array([[(width / 4, height), (width / 2, height / 3), (width * 3 / 4, height)]], dtype=np.int32) # center
        roi_right = np.array([[(width, 0), (width, height), (width * 3 / 4, height), (width / 2, height/3)]], dtype=np.int32) #right

        mask = np.zeros(image.shape, dtype=np.uint8)
        white = (255, 255, 255)
        red = (255, 0, 0)
        blue = (0, 255, 0)
        
        cv2.fillPoly(mask, roi_left, red)
        cv2.fillPoly(mask, roi_center, white)
        cv2.fillPoly(mask, roi_right, blue)

        # apply the mask
        masked_image = cv2.bitwise_and(image, mask)

        cv2.imshow("Image", masked_image)

    
    def update_image_frame(self, image):
        image = image[self.height_offset:(self.height_offset+self.height), 0:self.width]
        self.image = cv2.undistort(image, self.distortion_matrix, self.distortion, None, self.distortion_matrix)        
        #self.display("Initial", self.image)

        try:
            self.display("Lanes", self.lanes_image)
            self.display("Vehicles", self.vehicles_image)
            #print(json.dumps({'objects': self.detected_objects, 'vehicle_offset': self.vehicle_offset}), flush=True)
        except:
            pass

        # try:
        #     title, image = self.vehicle_queue.get(False) #doesn't block
        #     print(title)
        #     self.display(title, image)
        # except queue.Empty: #raised when queue is empty
        #     pass

        # try:
        #     title, image = self.lane_queue.get(False) #doesn't block
        #     print(title)
        #     self.display(title, image)
        # except queue.Empty: #raised when queue is empty
        #     pass

    def inference(self, image):
        #image = cv2.resize(image, (1280, 720))        
        image = image[self.height_offset:(self.height_offset+self.height), 0:self.width]
        image = cv2.undistort(image, self.distortion_matrix, self.distortion, None, self.distortion_matrix)
        image, self.vehicle_offset = self.lane_lines.lane_detect(image)
        image = self.image_detect(image)
        #image = self.lane_lines.lane_detect(image)        

        print(json.dumps({'objects': self.detected_objects, 'vehicle_offset': self.vehicle_offset}), flush=True)
        self.display("Result", image)        

    def output_result(self):
        print(json.dumps({'objects': self.detected_objects, 'vehicle_offset': self.vehicle_offset}), flush=True)

    def lane_detect_thread(self):
        while self.run:
            try:                
                start = time.time()
                image = copy.deepcopy(self.image)
                image, self.vehicle_offset = self.lane_lines.lane_detect(image)

                end = 1 / (time.time() - start)
                label_str = 'FPS: %.1f' % end
                image = cv2.putText(image, label_str, (0, 20), 0, .5, (255,255,255), 1, cv2.LINE_AA)
                #print(f"Lane FPS: {str( 1 / (time.time() - start) )}")
                #self.display("Lanes", image)
                #self.lane_queue.put((title, image))

                self.output_result()

                self.lanes_image = image
            except:
                pass

    def vehicle_detect_thread(self):
        while self.run:
            try:
                start = time.time()

                image = copy.deepcopy(self.image)
                image = self.image_detect(image)                

                end = 1 / (time.time() - start)
                label_str = 'FPS: %.1f' % end
                image = cv2.putText(image, label_str, (0, 20), 0, .5, (255,255,255), 1, cv2.LINE_AA)
                #print(f"Vehicle FPS: {str( 1 / (time.time() - start) )}")
                #self.display("Vehicles", image)
                #self.vehicle_queue.put((title, image))
                self.output_result()

                self.vehicles_image = image
            except:
                pass

    def display(self, title, image):                    
        #image = cv2.resize(image, (640, 360 - ))
        cv2.imshow(title, image)
        #cv2.imshow('Display', canny)

    def start_threads(self):
        self.run = True
        self.t1.start()
        self.t2.start()

    def stop_threads(self):
        if self.run == True:
            self.run = False
            self.t1.join()
            self.t2.join()

#image, height, width, channels = load_image('road.jpg')

adas = ADAS()
adas.start_threads()

while True:	

    try:
        start = time.time()
        x_offset = 0
        y_offset = 250
        image = grab_screen((x_offset, y_offset, x_offset + adas.width, y_offset + adas.height))
        #image = cv2.resize(image, (288, 288))
        adas.update_image_frame(image)
        #adas.inference(image)
        #print(f"FPS: {str( 1 / (time.time() - start) )}")

        if cv2.waitKey(1) & 0xFF == ord("x"):
            adas.stop_threads()
            cv2.destroyAllWindows()
            break

    except KeyboardInterrupt:
        adas.stop_threads()
        cv2.destroyAllWindows()
        break