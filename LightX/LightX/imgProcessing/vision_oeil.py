import os
import cv2
import numpy as np
import matplotlib.pyplot as plt
from datetime import datetime
from numpy.fft import fft, fft2, fftshift, ifft, ifftshift
import sys
import base64 

#############################################################################################################################
# Effectue la transformée de fourier pour obtenir un score de clarté.
# Le score est basé sur sur le nombre de point à haute fréquence qui représente les détails dans l'image.                   
# @param ImgGray : L'image monochrome à traiter                                                                                                                                                   
# @return dataFocus : Score de clarté                                                                          
#############################################################################################################################
def data_focus(imgGray):
    TF = fftshift(fft2(ifftshift(imgGray)))
    magnTF = 20*np.log(np.abs(TF)+1)
    dataFocus = magnTF.sum()
    return dataFocus


##################################################################################################################
#  Effectue un recadrage sur l'iris si elle est détecté sinon effecue un scaling pour augmenter les performances.
#  @param img : Image original à traiter
#  @param cx : Position en x de la pupil si détecté 
#  @param cy : Position en y de la pupil si détecté
#  @param scaling : Grandeur du scaling pour pour augmenter les performances.
#  @return dataFocus : Score de clarté
##################################################################################################################
def blur_score(img, cx=0, cy=0, scaling=256):
    if cx and cy:
        img = img[cy:cy+scaling, cx:cx+scaling]
    else:
        img = cv2.resize(img, (scaling, scaling))
    imgGray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    dataFocus = data_focus(imgGray)
    return dataFocus


#######################################################################################################
# Permet d'extraire des paramètre dans l'image selon leur seuil de valeur HSV. Entre autre permet de trouver la 
# position de la pupil et de la fente dans l'image
#  @param img_hsv : Image à traiter dans le domaine HSV
#  @param parameter_bound : Les seuils définnissants les caractéristiques des paramètre à extraire
#  @param area_thresh : L'aire minimal à considéré pour déclaré la détection du paramètre
#  @return cx, cy : La position en x et en y du centre du paramètre extrait 
#######################################################################################################
def parameter_extractor(img_hsv,parameter_bound,area_thresh=1000):
    lower_bound = np.array(parameter_bound[:3])
    upper_bound = np.array(parameter_bound[3:])
    mask = cv2.inRange(img_hsv, lower_bound, upper_bound)
    contours, _ = cv2.findContours(mask,cv2.RETR_TREE,cv2.CHAIN_APPROX_SIMPLE)
    cx=0
    cy=0
    area_array=[]
    for cnt in contours:
        area=cv2.contourArea(cnt)
        epsilon = 0.01*cv2.arcLength(cnt,True)
        approx = cv2.approxPolyDP(cnt,epsilon,True)
        if area > 1000:
            M = cv2.moments(approx)
            if M['m00']:
                cx = int(M['m10']/M['m00'])
                cy = int(M['m01']/M['m00'])
                pupil_cy=cy
    return cx, cy


#######################################################################################################
# Permet de décoder l'image provenant du program C#, applique la détection de flou, de la pupille 
# et de la fente. 
# Enregistre dans un fichier texte pour retourner la commande au logiciel LightX (C#).
# @param frame_path : Le chemin de la frame enregistrée sous forme de fichier binaire.
#######################################################################################################
def main(frame_path):
    #frame_path = "C:\\Users\\cassa\\Desktop\\LightXGithub\\userinterface\\LightX\\LightX\\LightX\\imgProcessing";
    with open(frame_path, mode='rb') as infile:
        data = infile.read()
    # CV2
    nparr = np.fromstring(data, np.uint8)
    frame = cv2.imdecode(nparr,1)
    pupil_bound = [36, 0, 0, 143, 255, 49]
    fente_bound = [0, 199, 223, 180, 255, 255]
    picture_ready=False
    if not frame is None:
        hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
        pupil_cx, pupil_cy = parameter_extractor(hsv,pupil_bound,area_thresh=500) 
        fente_cx, fente_cy = parameter_extractor(hsv,fente_bound,area_thresh=500)
        dataFocus = blur_score(frame, pupil_cx, pupil_cy)  

        if pupil_cx and pupil_cy:
            picture_ready = True
        if fente_cx:
            fente_cy= pupil_cy if pupil_cy else fente_cy
        
        command_str="{} {} {} {} {}".format(int(dataFocus), pupil_cx, pupil_cy, fente_cx, picture_ready)
        command_file=open("command.txt","w")
        command_file.write(command_str)
        command_file.close()
        print(command_str)


if __name__ == "__main__":
    main(sys.argv[1])