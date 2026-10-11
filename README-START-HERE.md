# ابدأ من هنا

## الحزمة التي تختارها

استخدم الحزمة:

`GeoLocalCAD_v0.6.0-source-validation.zip`

لكنها ليست DLL نهائية قابلة للتحميل داخل Civil 3D؛ يجب أولًا تشغيل `build-final-windows.ps1` على Windows مثبت عليه Civil 3D 2020.

## الخطوات

1. ثبّت Visual Studio Community مع workload: **.NET desktop development** و **.NET Framework 4.8 Developer Pack**.
2. ثبّت Civil 3D 2020 وGDAL Windows.
3. فك ضغط الحزمة أو استنسخ المشروع الكامل من GitHub.
4. غيّر امتداد `build-final-windows.txt` إلى `build-final-windows.ps1` إذا احتجت النسخة النصية.
5. افتح PowerShell من جذر المشروع وشغّل:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build-final-windows.ps1 `
  -AutodeskManagedApiPath "C:\Program Files\Autodesk\AutoCAD 2020" `
  -Civil3DManagedApiPath "C:\Program Files\Autodesk\AutoCAD 2020" `
  -GdalRoot "C:\GeoLocalCAD\GDAL" `
  -InstallBundle
```

6. بعد نجاح البناء، اختبر `GEOLOCAL` و`GEODEM` داخل Civil 3D.

## رابط Visual Studio الرسمي

https://visualstudio.microsoft.com/vs/community/

اختر **Visual Studio Community** للاستخدام الفردي، ثم ثبّت workload: **.NET desktop development**.
