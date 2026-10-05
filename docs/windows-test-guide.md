# Windows Build and Civil 3D 2020 Test Guide

## 1. الهدف والحدود

هذا الدليل يبني الإضافة مقابل Civil 3D/AutoCAD 2020 المثبت والمرخص، ثم يختبر `NETLOAD` و`GEOLOCAL` و`GEOSETTINGS` و`GEOIMPORT` و`GEOEXPORT`. لا تنسخ Autodesk DLLs إلى Git أو إلى حزمة التوزيع؛ المشروع يربطها من تثبيت Autodesk المحلي فقط.

## 2. المتطلبات

استخدم Windows 10/11 64-bit مع Civil 3D 2020 محدثًا إلى آخر تحديث متاح في بيئة الاختبار، وVisual Studio أو Build Tools تحتوي على:

- .NET Framework 4.8 Developer Pack/Targeting Pack.
- MSBuild.
- أدوات C# وDesktop development عند استخدام Visual Studio.
- Git.
- حساب GitHub بصلاحية قراءة المستودع.

لا تستخدم DLLs من Civil 3D 2021/2022 أو AutoCAD إصدار مختلف. يجب أن تتطابق كل Autodesk Managed APIs مع إصدار 2020 الذي سيجري عليه اختبار NETLOAD.

## 3. جلب المصدر

افتح **Developer PowerShell for Visual Studio** أو **Developer Command Prompt**:

```powershell
mkdir C:\src -ErrorAction SilentlyContinue
cd C:\src
git clone https://github.com/Hshawqi2026/GeoLocal-CAD.git
cd GeoLocal-CAD
git checkout v0.3.0
```

يفضل اختبار Tag ثابت بدل `main` حتى تكون نتيجة الاختبار قابلة للتكرار.

## 4. تحديد مجلد Autodesk API

تحقق من مكان تثبيت Civil 3D/AutoCAD 2020. المسار الشائع هو:

```powershell
$autodesk = "C:\Program Files\Autodesk\AutoCAD 2020"
```

إذا كان تثبيت Civil 3D عند مسار مختلف، استخدم ذلك المسار. تحقق من الملفات قبل البناء:

```powershell
$required = @("AcMgd.dll", "AcDbMgd.dll", "AcCoreMgd.dll", "AdWindows.dll")
$required | ForEach-Object {
    $p = Join-Path $autodesk $_
    [PSCustomObject]@{ File = $p; Exists = Test-Path $p }
}
```

لا تتابع إذا كانت إحدى القيم `False`. ابحث عن DLL داخل مجلد البرنامج المثبت، ولا تنسخها إلى المستودع.

## 5. بناء Release

```powershell
msbuild .\GeoLocalCAD.sln /t:Rebuild `
  /p:Configuration=Release `
  /p:Platform="Any CPU" `
  /p:AutodeskManagedApiPath="$autodesk"
```

تحقق من المخرجات:

```powershell
Get-ChildItem .\src\GeoLocalCAD.Plugin\bin\Release\
```

يجب أن توجد، بحسب إعدادات MSBuild، الملفات التالية في مجلد المخرجات أو مجلد staging:

```text
GeoLocalCAD.dll
GeoLocalCAD.Core.dll
GeoLocalCAD.UI.dll
GeoLocalCAD.CRS.dll
GeoLocalCAD.Vector.dll
```

يجب ألا توجد الملفات التالية في staging أو ZIP:

```text
AcMgd.dll
AcDbMgd.dll
AcCoreMgd.dll
AdWindows.dll
```

أنشئ مجلد اختبار محليًا وانسخ إليه فقط DLLs GeoLocal الناتجة وملفات التوثيق:

```powershell
$staging = "C:\GeoLocalCAD\v0.3.0-test"
New-Item -ItemType Directory -Force $staging | Out-Null
Copy-Item .\src\GeoLocalCAD.Plugin\bin\Release\GeoLocalCAD*.dll $staging
Copy-Item .\README-INSTALL.md, .\CHANGELOG.md, .\LICENSE $staging
Get-ChildItem $staging
```

## 6. تثبيت PROJ offline للتحويلات

`GEOIMPORT` لا يحتاج PROJ عندما يكون Source CRS مساويًا لـ Drawing CRS. عند اختلافهما، ثبّت نسخة Windows x64 من PROJ من مصدر مؤسسي موثوق، وتأكد من وجود `cs2cs.exe` وملف قاعدة البيانات `proj.db` وبيانات PROJ اللازمة.

لا تستخدم Web API أثناء التشغيل. عيّن المتغير في جلسة الاختبار:

```powershell
$env:GEOLOCAL_PROJ_CS2CS = "C:\GeoLocalCAD\PROJ\bin\cs2cs.exe"
Test-Path $env:GEOLOCAL_PROJ_CS2CS
```

إذا كانت نسخة PROJ تعتمد على DLLs مجاورة، أبقها في حزمة PROJ نفسها، ولا تخلطها مع Autodesk DLLs. اختبر المسار قبل تشغيل Civil 3D.

## 7. Trusted Location وUnblock

أنشئ مجلدًا محليًا مثل `C:\GeoLocalCAD\v0.3.0-test`، ثم أضفه إلى **Options → Files → Trusted Locations** في AutoCAD/Civil 3D. لا تستخدم مجلد Network share في الاختبار الأول.

إذا تم تنزيل ZIP من الإنترنت، فك الضغط ثم أزل Mark-of-the-Web من ملفات الحزمة المحلية فقط:

```powershell
Get-ChildItem $staging -File | Unblock-File
```

توصي Autodesk باستخدام مجلد موثوق أو إلغاء حظر الملفات المحلية عند ظهور خطأ `Operation is not supported` أثناء NETLOAD. المصدر: [Autodesk — How to autoload DLLs with AutoCAD](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/How-to-autoload-DLLs-with-AutoCAD.html) و[Autodesk — NETLOAD Operation is not supported](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Cannot-load-assembly-Operation-is-not-supported-when-loading-a-DLL-file-via-NETLOAD-in-AutoCAD.html).

## 8. اختبار NETLOAD

1. أغلق أي جلسة Civil 3D قديمة.
2. شغّل Civil 3D 2020.
3. افتح رسمًا جديدًا.
4. اكتب في سطر الأوامر:

```text
NETLOAD
```

5. اختر:

```text
C:\GeoLocalCAD\v0.3.0-test\GeoLocalCAD.dll
```

6. تحقق من عدم ظهور Assembly load error.
7. شغّل:

```text
GEOLOCAL
```

8. تحقق من ظهور رقم الإصدار `0.3.0` ومسار السجل.
9. تحقق من ظهور تبويب Ribbon باسم `GeoLocal`.
10. شغّل `GEOSETTINGS` وتأكد من فتح WPF وحفظ إعدادات Offline mode وDrawing CRS.

افحص السجل:

```text
%LOCALAPPDATA%\GeoLocalCAD\Logs\GeoLocalCAD.log
```

## 9. إنشاء ملف GeoJSON تجريبي

احفظ النص التالي في:

```text
C:\GeoLocalCAD\samples\phase3-test.geojson
```

```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "properties": { "id": "P-001", "source": "phase3-test" },
      "geometry": { "type": "Point", "coordinates": [44.2, 15.3, 100.0] }
    },
    {
      "type": "Feature",
      "properties": { "id": "L-001" },
      "geometry": { "type": "LineString", "coordinates": [[44.2, 15.3, 0], [44.3, 15.4, 0]] }
    },
    {
      "type": "Feature",
      "properties": { "id": "A-001" },
      "geometry": { "type": "Polygon", "coordinates": [[[44.2, 15.3, 0], [44.3, 15.3, 0], [44.3, 15.4, 0], [44.2, 15.3, 0]]] }
    }
  ]
}
```

هذا الملف لا يحتوي على CRS رسمي داخل GeoJSON. لذلك يجب إدخال CRS في الأمر؛ لا تستنتج CRS من الأرقام أو من موقع المشروع.

## 10. اختبار GEOIMPORT دون تحويل

للاختبار الأول استخدم نفس CRS للمصدر والرسم:

1. اكتب `GEOIMPORT`.
2. أدخل:

```text
C:\GeoLocalCAD\samples\phase3-test.geojson
```

3. أدخل Source CRS:

```text
EPSG:4326
```

4. أدخل Drawing CRS:

```text
EPSG:4326
```

5. تحقق من إنشاء Point وPolyline مغلقة للمضلع، ومن بقاء الإحداثيات كما هي.
6. تحقق من رسالة العدد في سطر الأوامر والسجل.

## 11. اختبار GEOIMPORT مع تحويل PROJ

لاختبار التحويل استخدم ملفًا مصدره WGS84 `EPSG:4326` واجعل Drawing CRS:

```text
EPSG:32638
```

مع تعيين `GEOLOCAL_PROJ_CS2CS` مسبقًا. تحقق من أن:

- العملية تنجح فقط عند وجود PROJ صالح.
- الإحداثيات المستوردة تقع في نطاق Easting/Northing المتوقع لـ UTM Zone 38N.
- حذف المتغير أو وضع مسار غير صالح يوقف العملية برسالة ظاهرة، ولا يضع الكيانات في إحداثيات غير محولة.

## 12. اختبار GEOEXPORT

1. أنشئ أو استخدم Point/Polyline داخل الرسم.
2. اكتب `GEOEXPORT`.
3. حدد الكيانات واضغط Enter.
4. أدخل مسارًا مثل:

```text
C:\GeoLocalCAD\samples\exported.geojson
```

5. أدخل Drawing CRS صريحًا، مثل `EPSG:32638`.
6. افتح الناتج في محرر نصي وتحقق من:
   - `type: FeatureCollection`.
   - `geolocalCrs: EPSG:32638`.
   - الإحداثيات والـ Z values.
   - عدم فقدان الميزات المدعومة.

## 13. معيار نجاح الاختبار

سجّل لكل اختبار: التاريخ، إصدار Civil 3D، مسار DLL، CRS المصدر والهدف، اسم الملف، عدد الميزات، نتيجة الأمر، ورسالة الخطأ إن وجدت. لا تغلق Phase 3 أو تبدأ Phase 4 على أنها مستقرة حتى تمر اختبارات NETLOAD وGEOIMPORT وGEOEXPORT داخل Civil 3D 2020 فعليًا.

## 14. جمع تقرير خطأ

عند الفشل، أرسل:

- نص سطر الأوامر كاملًا.
- نسخة من `%LOCALAPPDATA%\GeoLocalCAD\Logs\GeoLocalCAD.log`.
- إصدار Civil 3D وتحديثه.
- نتيجة `Get-ChildItem $staging`.
- قيم Source CRS وDrawing CRS.
- هل كان `GEOLOCAL_PROJ_CS2CS` مضبوطًا.
- الملف GeoJSON المستخدم، بعد إزالة أي بيانات سرية.

لا ترسل Autodesk DLLs إلى GitHub أو إلى التقرير إلا إذا طلب Autodesk ذلك رسميًا عبر قناة دعم مناسبة.
