"""
Will use opencv's built in darknet api to do 2D object detection which will
then get passed into the torch net

source: https://www.pyimagesearch.com/2018/11/12/yolo-object-detection-with-opencv/
"""

import cv2
import numpy as np
import os

class cv_Yolo:

    def __init__(self, yolo_path, confidence=0.1, threshold=.2):
        self.confidence = confidence
        self.threshold = threshold

        labels_path = os.path.sep.join([yolo_path, "coco.names"])
        self.labels = open(labels_path).read().split("\n")

        np.random.seed(42)
        self.colors = np.random.randint(0,255, size=(len(self.labels), 3), dtype="uint8")

        weights_path = os.path.sep.join([yolo_path, "yolov4.weights"])
        cfg_path = os.path.sep.join([yolo_path, "yolov4.cfg"])

        net = cv2.dnn.readNetFromDarknet(cfg_path, weights_path)
        net.setPreferableBackend(cv2.dnn.DNN_BACKEND_CUDA)
        net.setPreferableTarget(cv2.dnn.DNN_TARGET_CUDA_FP16)
        self.net = net

        print("CUDA: ", cv2.cuda.getCudaEnabledDeviceCount())

    def detect(self, image):
        # assert image is opencv
        (H,W) = image.shape[:2]

        ln = self.net.getLayerNames()
        ln = [ln[i[0] - 1] for i in self.net.getUnconnectedOutLayers()]

        # prepare input
        blob = cv2.dnn.blobFromImage(image, 1 / 255.0, (416, 416), swapRB=True, crop=False)

        self.net.setInput(blob)
        output = self.net.forward(ln)

        detections = []

        boxes = []
        confidences = []
        class_ids = []

        for output in output:
            for detection in output:
                scores = detection[5:]
                class_id = np.argmax(scores)
                confidence = scores[class_id]

                if confidence > self.confidence:

                    box = detection[0:4] * np.array([W, H, W, H])
                    (centerX, centerY, width, height) = box.astype("int")

                    # use the center (x, y)-coordinates to derive the top and
                    # and left corner of the bounding box
                    x = int(centerX - (width / 2))
                    y = int(centerY - (height / 2))

                    # update our list of bounding box coordinates, confidences,
                    # and class IDs

                    boxes.append([x, y, int(width), int(height)])
                    confidences.append(float(confidence))
                    class_ids.append(class_id)



        idxs = cv2.dnn.NMSBoxes(boxes, confidences, self.confidence, self.threshold)

        if len(idxs) > 0:
            for i in idxs.flatten():

                top_left = (boxes[i][0], boxes[i][1])
                bottom_right = (top_left[0] + boxes[i][2], top_left[1] + boxes[i][3])

                box_2d = [top_left, bottom_right]
                class_ = self.get_class(class_ids[i])

                detections.append(Detection(box_2d, class_, confidences[i]))

        return detections

    def get_class(self, class_id):
        return self.labels[class_id]



class Detection:
    def __init__(self, box_2d, class_, score):
        self.box_2d = box_2d
        self.detected_class = class_
        self.score = score
