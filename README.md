# About

Linon SDK is drive simulator​ sample. The purpose of it is to create an open source simulation environment.

# Summary

Linon SDK consists of Controllor and Simurator (Agent and Enviroment). It provides base of Controlor and Agent. Enviroment expected to be made by User.

Simurator is made by unity. Controllor can be use any language.

The communication method is defined as follows for ease of use.
- UDP communication is used for continuous information that needs to be conveyed in real time.
- TCP communication is used for Event signals.
- MMAP(memory-mapped file) is used to send and receive images.

![summary](/doc/img/summary.png) 

# Quick start

## Simurator (Unity)

1. Install Unity. https://unity3d.com/jp/get-unity/download
2. Create 3D Project.
3. Install Package.
   1. Open Project
   2. Assets -> Import package -> custom package
   3. Select /unity/package/AICarSet.unitypackage and Base.unitypackage, and import them.
4. Create Rode (Option)
   1. Install EasyRoads3D https://assetstore.unity.com/packages/tools/terrain/easyroads3d-pro-v3-469?locale=ja-JP
   2. Create Road
5. Add car
   1. From Project window, drug and drop Assets -> AICar -> Prehub -> AICarSet.
   2. Adjust car position, move on the road and rotate its direction.

## Controllor (Python)

1. Install Miniconda https://docs.conda.io/en/latest/miniconda.html
2. Open Miniconda and create vitual env.
3. Install some librallies.
```
$ pip install matplotlib pandas numpy
$ conda install -c conda-forge opencv
# option if you want to use AI
$ conda install pytorch==1.11.0 torchvision==0.12.0 torchaudio==0.11.0 cudatoolkit=11.3 -c pytorch
```
4. Install Jupyter Notebook.
```
$ pip install jupyter
```
5. Start Jupyter Notebook.
```
$ jupyter notebook
```
6. Open Sample_Car_controal.ipynb.


