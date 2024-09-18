# **Color Constancy in Virtual Reality (VR) Study**

This Unity project was developed as part of my research into **color constancy** in VR environments. The study aimed to investigate how lighting conditions, specifically variations in lighting temperature, lighting intensity, and the color of reference objects, influence the perception of color consistency, which is crucial in real-world VR applications.

## **Project Overview**

### **Objective**
The core objective of this project was to explore how participants maintain color constancy under varying lighting conditions in a VR environment. This research has practical applications in fields like remote monitoring, collaboration, and training, where accurate color perception is vital for decision-making and task performance.

## **Motivation**
In many VR applications, such as **remote monitoring of construction sites** or **collaborative design work**, maintaining consistent color perception is essential for preventing misjudgments that could lead to costly errors. This project aimed to address this challenge by studying how users perceive color under different lighting conditions in VR environments, thus informing future VR development and color calibration processes.

## **Methodology**
- **Within-Subjects Design:** All participants were exposed to 27 different lighting and color combinations. 
- **Independent Variables:** 
	- **Reference Object Color:** Red, Green, Blue 
	- **Lighting Temperature:** 4000K, 7000K, 10000K 
	- **Lighting Intensity:** Low, Medium, High 
- **Symmetric Color Matching:** Both test and reference objects are viewed simultaneously to eliminate memory recall bias, commonly seen in asymmetric color matching. 
- **Dual Dynamic Staircase:** This method is used to capture both upper and lower bounds of the color constancy threshold by progressively adjusting hue values in two directions—clockwise and counterclockwise—on the color wheel.
- **Data Collection Pipeline:** A custom pipeline was designed to export color values, rendering outputs from Unity's RenderTexture, and process them into the CIELAB color space for further data analysis.


## **Technologies Used**
- **Unity (2021.3)** – VR development platform
- **Universal Render Pipeline (URP)** – Optimized for lightweight rendering, but with future plans to migrate to **HDRP** for improved lighting accuracy.
- **Oculus Quest 2** – Primary headset used for the study

## **How to Run the Project**
1. Clone the repository:
   ```bash
   git clone https://github.com/LauDuncan/Unity-Color-Constancy-VR.git
   ```

2. Open the project in **Unity 2021.3** or higher.
3. Connect an Oculus Quest 2 headset to test the VR scenes.
