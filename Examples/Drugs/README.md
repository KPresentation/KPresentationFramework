DRUGS EXAMPLE
=================

For executing this example, you need an **internet connection**.

Moreover you must update the system register to **permit the browser emulation of the control**. Using regedit.exe: in 

```
Computer -> HKEY_LOCAL_MACHINE -> SOFTWARE -> Wow6432Node -> Microsoft -> Internet Explorer -> Main -> FeatureControl -> FEATURE_BROWSER_EMULATION
```
you need to add a new DWORD value with 
```
Name "drugs.vshost.exe" and value "11000"
```
and a new DWORD value with 
```
Name "drugs.exe" and valure "11000"
```

For having a better experience, you should **enable the GPU Rendering**. In order to do this, in
```
Computer -> HKEY_LOCAL_MACHINE -> SOFTWARE -> Wow6432Node -> Microsoft -> Internet Explorer -> Main -> FeatureControl -> FEATURE_GPU_RENDERING 
```
add a new DWORD value with 
```
Name "drugs.vshost.exe" and value "1" 
```
and a new DWORD value with 
```
Name "drugs.exe" and valure "1".
```

That's all. Enjoy it!
