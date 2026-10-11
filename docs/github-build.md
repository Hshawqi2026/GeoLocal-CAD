# GitHub build workflow

## ما الذي يبنيه GitHub دون جهاز خاص؟

Job `core-validation` يعمل على GitHub Hosted Ubuntu Runner ويتحقق من Core وCRS وRaster وDEM وGDAL sampler. لا يحتاج Autodesk.

## لماذا يحتاج البناء النهائي إلى Self-hosted Runner؟

GitHub Hosted Runners لا تحتوي على Civil 3D 2020 أو ملفات Autodesk المرخصة مثل `AeccDbMgd.dll`. لذلك يستخدم Job `civil3d-windows` جهاز Windows يملكه المستخدم ومثبت عليه:

- Civil 3D 2020.
- Visual Studio Build Tools و.NET Framework 4.8 Developer Pack.
- GDAL Windows.
- GitHub Actions self-hosted runner.

لا تُرفع Autodesk DLLs إلى GitHub؛ تبقى على الجهاز المحلي ويستخدمها MSBuild أثناء البناء فقط.

## إعداد الجهاز

بعد تثبيت GitHub Actions Runner من:

```text
Repository → Settings → Actions → Runners → New self-hosted runner → Windows x64
```

شغّل runner كخدمة أو من PowerShell. أضف labels:

```text
self-hosted, Windows, x64, civil3d-2020
```

أنشئ متغيرات بيئة على جهاز runner، ثم أعد تشغيل خدمة runner:

```powershell
[Environment]::SetEnvironmentVariable('GEOLOCAL_AUTODESK_PATH', 'C:\Program Files\Autodesk\AutoCAD 2020', 'Machine')
[Environment]::SetEnvironmentVariable('GEOLOCAL_CIVIL3D_PATH', 'C:\Program Files\Autodesk\AutoCAD 2020', 'Machine')
[Environment]::SetEnvironmentVariable('GEOLOCAL_GDAL_ROOT', 'C:\GeoLocalCAD\GDAL', 'Machine')
```

يجب أن توجد:

```text
%GEOLOCAL_AUTODESK_PATH%\AcMgd.dll
%GEOLOCAL_AUTODESK_PATH%\AcDbMgd.dll
%GEOLOCAL_AUTODESK_PATH%\AcCoreMgd.dll
%GEOLOCAL_AUTODESK_PATH%\AdWindows.dll
%GEOLOCAL_CIVIL3D_PATH%\AeccDbMgd.dll
%GEOLOCAL_GDAL_ROOT%\bin\gdal_translate.exe
%GEOLOCAL_GDAL_ROOT%\bin\gdalinfo.exe
```

## التشغيل

بعد تسجيل runner، ادفع Commit أو شغّل Workflow يدويًا من تبويب Actions. سيقوم Workflow بـ:

1. بناء Core والتحقق منه على GitHub Hosted Ubuntu.
2. استخدام Windows runner المحلي لبناء `GeoLocalCAD.dll`.
3. تشغيل الاختبارات.
4. إنشاء ZIP.
5. التحقق من عدم وجود Autodesk DLLs في ZIP.
6. رفع ZIP كـ Artifact إلى صفحة تشغيل Workflow.

## حدود الأتمتة

Workflow لا يشغّل Civil 3D بواجهة تفاعلية ولا ينفذ `NETLOAD` أو يحفظ DWG تلقائيًا. بعد رفع Artifact يجب اختبار `GEODEM` داخل Civil 3D يدويًا، ثم اعتماد الإصدار النهائي فقط بعد نجاح إنشاء TinSurface وإعادة فتح DWG.
