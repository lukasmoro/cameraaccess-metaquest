import cv2
from ultralytics import YOLO
import numpy as np
import socket
import json
import torch

def setup_server(server_ip, server_port):
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind((server_ip, server_port))
    server_socket.listen(1)
    print("Waiting for a connection...")
    return server_socket

def handle_client(client_socket, cap, model, class_names, device):
    while True:
        ret, frame = cap.read()
        if not ret:
            break

        h, w, _ = frame.shape
        min_dim = min(h, w)
        start_x = (w - min_dim) // 2
        start_y = (h - min_dim) // 2
        cropped_frame = frame[start_y:start_y+min_dim, start_x:start_x+min_dim]

        resized_frame = cv2.resize(cropped_frame, (1024, 1024))

        results = model(resized_frame, device=device)
        result = results[0]
        bboxes = np.array(result.boxes.xyxy.cpu(), dtype="int")
        classes = np.array(result.boxes.cls.cpu(), dtype="int")
        detection_data = []

        for cls, bbox in zip(classes, bboxes):
            x, y, x2, y2 = bbox
            class_name = class_names[cls]
            detection_data.append({"class_name": class_name, "bbox": [int(x), int(y), int(x2), int(y2)]})

            cv2.rectangle(resized_frame, (x, y), (x2, y2), (0, 0, 255), 2)
            cv2.putText(resized_frame, class_name, (x, y - 5), cv2.FONT_HERSHEY_PLAIN, 2, (255, 255, 255), 2)

        try:
            wrapped_data = json.dumps({"detections": detection_data})
            client_socket.send(wrapped_data.encode('utf-8'))
            client_socket.send(b'\n')
        except BrokenPipeError:
            print("Client disconnected.")
            break

        cv2.imshow("img", resized_frame)
        key = cv2.waitKey(1)

def main():
    server_ip = "0.0.0.0"
    server_port = 8080

    with open("classes.txt", "r") as f:
        class_names = [line.strip() for line in f.readlines()]

    cap = cv2.VideoCapture(1)

    model = YOLO("yolov8m.pt")

    if torch.cuda.is_available():
        device = "cuda"
    elif torch.backends.mps.is_available():
        device = "mps"
    else:
        device = "cpu"

    print(f"Using device: {device}")

    server_socket = setup_server(server_ip, server_port)

    while True:
        client_socket, address = server_socket.accept()
        print(f"Connection from {address} has been established.")
        handle_client(client_socket, cap, model, class_names, device)
        client_socket.close()
        print("Connection closed. Waiting for new connection...")

    cap.release()
    cv2.destroyAllWindows()
    server_socket.close()

if __name__ == "__main__":
    main()
